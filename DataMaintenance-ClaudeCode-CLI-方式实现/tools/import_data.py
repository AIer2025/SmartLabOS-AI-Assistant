# -*- coding: utf-8 -*-
"""
SmartLabOS 主数据导入器
=========================
读取 references/ 下的 YAML-MD 卡片文件，解析 front-matter + Markdown 正文，
按 "关键字段列 + 全量保真(JSON/原文)" 的结构 UPSERT 进 MySQL 五张表。

数据源 -> 目标表:
    references/01-modules      -> `SmartLabOS-Module`
    references/02-platforms    -> `SmartLabOS-PlatformBase`
    references/03-workstation  -> `SmartLabOS-WorkStation`
    references/04-solutions    -> `SmartLabOS-Solution`
    references/05-Project      -> `SmartLabOS-Project`

连接配置: 读取 ../db.config.json (独立于程序之外，可自由设定)。
依赖: pip install pyyaml pymysql

用法:
    python import_data.py                # 增量 UPSERT (默认)
    python import_data.py --truncate     # 先清空各表再导入
    python import_data.py --dry-run      # 只解析不写库，打印将导入的条数
"""
import os
import sys
import json
import argparse

try:
    import yaml
except ImportError:
    sys.exit("缺少依赖 pyyaml，请执行: pip install pyyaml")
try:
    import pymysql
except ImportError:
    sys.exit("缺少依赖 pymysql，请执行: pip install pymysql")


HERE = os.path.dirname(os.path.abspath(__file__))
PROJECT_ROOT = os.path.abspath(os.path.join(HERE, "..", ".."))   # SmartLabOS-AI-Assistant
REFERENCES = os.path.join(PROJECT_ROOT, "references")
CONFIG_PATH = os.path.abspath(os.path.join(HERE, "..", "db.config.json"))

# 目标表定义: (子目录, 表名, 该表除公共列外的专有列 -> front-matter 取值路径)
# 取值路径用 "/" 分隔, 例如 "identification_version/category"
COMMON_PATHS = {
    "name":         "identification_version/name",
    "version":      "identification_version/version",
    "status":       "identification_version/status",
    "owner":        "identification_version/owner",
    "last_updated": "identification_version/last_updated",
}

TABLES = [
    {
        "dir": "01-modules",
        "table": "SmartLabOS-Module",
        "extra": {
            "module_part_no": "identification_version/module_part_no",
            "category":       "identification_version/category",
        },
    },
    {
        "dir": "02-platforms",
        "table": "SmartLabOS-PlatformBase",
        "extra": {
            "platform_part_no": "identification_version/platform_part_no",
        },
    },
    {
        "dir": "03-workstation",
        "table": "SmartLabOS-WorkStation",
        "extra": {
            "platform_id": "hardware_config/platform_id",
            "maturity":    "identification_version/maturity",
        },
    },
    {
        "dir": "04-solutions",
        "table": "SmartLabOS-Solution",
        "extra": {
            "proposal_type": "identification_version/proposal_type",
            "maturity":      "identification_version/maturity",
        },
    },
    {
        "dir": "05-Project",
        "table": "SmartLabOS-Project",
        "extra": {
            "customer":      "identification_version/customer",
            "proposal_type": "identification_version/proposal_type",
            "stage":         "identification_version/stage",
        },
    },
]


def load_config():
    with open(CONFIG_PATH, "r", encoding="utf-8") as f:
        cfg = json.load(f)
    return cfg


def split_frontmatter(text):
    """返回 (frontmatter_str, body_str)。容忍无 front-matter 的文件。"""
    lines = text.splitlines()
    if not lines or lines[0].strip() != "---":
        return "", text
    end = None
    for i in range(1, len(lines)):
        if lines[i].strip() == "---":
            end = i
            break
    if end is None:
        return "", text
    fm = "\n".join(lines[1:end])
    body = "\n".join(lines[end + 1:]).lstrip("\n")
    return fm, body


def dig(data, path):
    """按 a/b/c 路径取值；任一级缺失返回 None。"""
    cur = data
    for key in path.split("/"):
        if isinstance(cur, dict) and key in cur:
            cur = cur[key]
        else:
            return None
    return cur


def to_scalar(v):
    """将取值规整为可写入 VARCHAR 的字符串/None。"""
    if v is None:
        return None
    if isinstance(v, (str, int, float, bool)):
        s = str(v).strip()
        return s if s != "" else None
    if isinstance(v, (list, dict)):
        return json.dumps(v, ensure_ascii=False, default=str)
    return str(v)


def extract_tags(data):
    """tags 可能位于顶层 tags 或 tags_meta/tags。"""
    for path in ("tags_meta/tags", "tags"):
        v = dig(data, path)
        if v is not None:
            if isinstance(v, list):
                return "，".join(str(x) for x in v)
            return str(v)
    return None


def parse_file(path):
    # utf-8-sig: 源文件带 UTF-8 BOM，需剥离否则首行 "---" 无法识别为 front-matter 分隔
    with open(path, "r", encoding="utf-8-sig") as f:
        text = f.read()
    fm_str, body = split_frontmatter(text)
    parsed = None
    if fm_str.strip():
        try:
            parsed = yaml.safe_load(fm_str)
        except Exception as e:
            print("    [WARN] YAML 解析失败，仅保留原文: %s (%s)" % (os.path.basename(path), str(e)[:60]))
            parsed = None
    if not isinstance(parsed, dict):
        parsed = {}
    return parsed, fm_str, body


def build_row(spec, parsed, fm_str, body, filename):
    row = {}
    # id: 优先 front-matter 的 id，否则用文件名(去扩展名)
    fid = dig(parsed, "identification_version/id") or os.path.splitext(filename)[0]
    row["id"] = str(fid).strip()
    for col, path in COMMON_PATHS.items():
        row[col] = to_scalar(dig(parsed, path))
    for col, path in spec["extra"].items():
        row[col] = to_scalar(dig(parsed, path))
    row["tags"] = extract_tags(parsed)
    # default=str: 兼容 YAML 自动解析出的 date/datetime 等非原生 JSON 类型
    row["data_json"] = json.dumps(parsed, ensure_ascii=False, default=str) if parsed else None
    row["raw_frontmatter"] = fm_str
    row["body_markdown"] = body
    row["source_file"] = filename
    return row


def upsert(cursor, table, row):
    cols = list(row.keys())
    col_sql = ", ".join("`%s`" % c for c in cols)
    ph = ", ".join(["%s"] * len(cols))
    updates = ", ".join("`%s`=VALUES(`%s`)" % (c, c) for c in cols if c != "id")
    sql = "INSERT INTO `%s` (%s) VALUES (%s) ON DUPLICATE KEY UPDATE %s" % (
        table, col_sql, ph, updates
    )
    cursor.execute(sql, [row[c] for c in cols])


def main():
    ap = argparse.ArgumentParser(description="SmartLabOS 主数据导入器")
    ap.add_argument("--truncate", action="store_true", help="导入前清空各表")
    ap.add_argument("--dry-run", action="store_true", help="仅解析，不写库")
    args = ap.parse_args()

    cfg = load_config()
    print("连接 MySQL: %s@%s:%s/%s" % (cfg["user"], cfg["host"], cfg.get("port", 3306), cfg["database"]))

    conn = None
    if not args.dry_run:
        conn = pymysql.connect(
            host=cfg["host"], port=int(cfg.get("port", 3306)),
            user=cfg["user"], password=cfg["password"],
            database=cfg["database"], charset="utf8mb4",
            autocommit=False,
        )

    total = 0
    try:
        cur = conn.cursor() if conn else None
        for spec in TABLES:
            src = os.path.join(REFERENCES, spec["dir"])
            if not os.path.isdir(src):
                print("[SKIP] 目录不存在: %s" % src)
                continue
            files = sorted(f for f in os.listdir(src)
                           if f.lower().endswith(".md") and not f.startswith("_"))
            print("\n== %s  ->  `%s`  (%d 个文件)" % (spec["dir"], spec["table"], len(files)))

            if cur and args.truncate:
                cur.execute("TRUNCATE TABLE `%s`" % spec["table"])
                print("   已清空表 `%s`" % spec["table"])

            count = 0
            for fn in files:
                parsed, fm_str, body = parse_file(os.path.join(src, fn))
                row = build_row(spec, parsed, fm_str, body, fn)
                if not row["id"]:
                    print("   [WARN] 无法确定 id，跳过: %s" % fn)
                    continue
                if cur:
                    upsert(cur, spec["table"], row)
                count += 1
            total += count
            print("   导入 %d 条" % count)

        if conn:
            conn.commit()
            print("\n已提交事务。合计导入/更新 %d 条记录。" % total)
        else:
            print("\n[DRY-RUN] 解析完成，合计 %d 条记录(未写库)。" % total)
    except Exception:
        if conn:
            conn.rollback()
            print("\n发生错误，已回滚事务。")
        raise
    finally:
        if conn:
            conn.close()


if __name__ == "__main__":
    main()
