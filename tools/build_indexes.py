#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
build_indexes.py — 自动生成各卡片库的 _index.md

作用:扫描 references/platforms/、references/modules/、references/solutions/
三个目录下的所有卡片(.md 文件),解析每份卡片的 frontmatter,把关键字段
拍平成 Markdown 表格,生成对应的 _index.md。

让 Claude 一次读索引就能掌握全库,而不必扫所有卡片。
让人类打开索引 5 分钟就看清全貌。

使用:
    python build_indexes.py [--kb-root <kb_root>] [--quiet]

示例:
    python build_indexes.py
    python build_indexes.py --kb-root C:/TestClaude/SmartLabOS-AI-Assistant/references

返回值:
    0 = 全部成功
    1 = 部分卡片解析失败(已跳过)
    2 = 脚本本身出错(目录不存在等)

依赖:
    pip install pyyaml
"""

from __future__ import annotations

import argparse
import sys
from collections import defaultdict
from datetime import datetime
from pathlib import Path
from typing import Any

try:
    import yaml
except ImportError:
    print("错误: 需要安装 pyyaml,请运行: pip install pyyaml", file=sys.stderr)
    sys.exit(2)


# ============================================================
# Frontmatter 解析(与 validate_solution.py 同款)
# ============================================================

def load_frontmatter(path: Path) -> dict[str, Any] | None:
    """读 .md 文件,解析 YAML frontmatter,失败返回 None"""
    try:
        text = path.read_text(encoding="utf-8")
    except (IOError, UnicodeDecodeError):
        return None

    if not text.startswith("---"):
        return None

    parts = text.split("---", 2)
    if len(parts) < 3:
        return None

    try:
        data = yaml.safe_load(parts[1])
        if isinstance(data, dict):
            return data
    except yaml.YAMLError:
        return None

    return None


def collect_cards(card_dir: Path) -> tuple[list[dict], list[Path]]:
    """
    扫描指定目录下所有 .md 文件,解析 frontmatter。
    返回 (成功解析的卡片列表, 解析失败的文件路径列表)。

    跳过下划线开头的文件(_index.md / _categories.md 等基础设施文件)。
    """
    cards = []
    errors = []
    if not card_dir.exists():
        return cards, errors

    for md_file in sorted(card_dir.rglob("*.md")):
        if md_file.name.startswith("_"):
            continue
        data = load_frontmatter(md_file)
        if data is None:
            errors.append(md_file)
            continue
        # 附加文件相对路径,便于索引中给出链接
        data["_file_path"] = md_file.relative_to(card_dir).as_posix()
        cards.append(data)
    return cards, errors


# ============================================================
# 渲染辅助
# ============================================================

def fmt_list(v: Any, max_items: int = 5) -> str:
    """把 list/字符串/None 渲染成简短的 Markdown 单元格内容"""
    if v is None:
        return "—"
    if isinstance(v, list):
        if not v:
            return "—"
        items = [str(x) for x in v[:max_items]]
        if len(v) > max_items:
            items.append(f"... ({len(v)} 项)")
        return ", ".join(items)
    return str(v)


def fmt_status(status: str | None) -> str:
    """状态字段加颜色标记"""
    if status is None:
        return "—"
    icons = {"active": "🟢 active", "draft": "🟡 draft", "deprecated": "⚪ deprecated"}
    return icons.get(status, status)


def fmt_maturity(maturity: str | None) -> str:
    """方案成熟度图标"""
    if maturity is None:
        return "—"
    icons = {
        "概念": "💭 概念",
        "验证中": "🔬 验证中",
        "已交付": "✅ 已交付",
        "标准化": "⭐ 标准化",
    }
    return icons.get(maturity, maturity)


def md_link(text: str, href: str) -> str:
    """生成 Markdown 链接"""
    return f"[{text}]({href})"


def render_header(title: str, subtitle: str, count: int) -> str:
    """通用索引头"""
    now = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    return (
        f"# {title}\n\n"
        f"> {subtitle}\n"
        f"> 由 build_indexes.py 自动生成于 {now}\n"
        f"> 共收录 **{count}** 份卡片\n\n"
        f"⚠️ 本文件由脚本自动生成,**请勿手工编辑**;改动卡片后重新运行 build_indexes.py。\n\n"
    )


# ============================================================
# 平台索引
# ============================================================

def render_platform_index(cards: list[dict]) -> str:
    """生成 platforms/_index.md"""
    out = render_header(
        title="平台卡片索引",
        subtitle="SmartLabOS 平台型号清单",
        count=len(cards),
    )

    if not cards:
        out += "_(暂无平台卡片)_\n"
        return out

    out += "## 平台一览\n\n"
    out += "| ID | 名称 | 状态 | 尺寸(L×W×H mm) | 模块/托盘上限 | 峰值功率 | 标签 |\n"
    out += "|----|------|------|----------------|--------------|---------|------|\n"

    for c in sorted(cards, key=lambda x: x.get("id", "")):
        cid = c.get("id", "—")
        name = c.get("name", "—")
        status = fmt_status(c.get("status"))
        dim = c.get("dimensions_mm") or {}
        dim_s = f"{dim.get('length','?')}×{dim.get('width','?')}×{dim.get('height','?')}" \
                if isinstance(dim, dict) else "—"
        max_mod = c.get("max_modules", "—")
        max_tray = c.get("max_trays", "—")
        cap_s = f"{max_mod}M / {max_tray}T"
        power = c.get("power_kva_peak")
        power_s = f"{power} kVA" if power else "—"
        tags = fmt_list(c.get("tags"), max_items=3)
        link = md_link(cid, c.get("_file_path", "#"))
        out += f"| {link} | {name} | {status} | {dim_s} | {cap_s} | {power_s} | {tags} |\n"

    return out


# ============================================================
# 模块索引(按 category 分组)
# ============================================================

def render_module_index(cards: list[dict]) -> str:
    """生成 modules/_index.md,按 category 分组"""
    out = render_header(
        title="模块卡片索引",
        subtitle="SmartLabOS 模块清单(按类别分组)",
        count=len(cards),
    )

    if not cards:
        out += "_(暂无模块卡片)_\n"
        return out

    # 按 category 聚合
    by_cat: dict[str, list[dict]] = defaultdict(list)
    for c in cards:
        cat = c.get("category", "未分类")
        by_cat[cat].append(c)

    # 类别概览
    out += "## 类别概览\n\n"
    out += "| 类别 | 数量 |\n|------|------|\n"
    for cat in sorted(by_cat.keys()):
        out += f"| {cat} | {len(by_cat[cat])} |\n"
    out += "\n"

    # 每个类别一个表
    for cat in sorted(by_cat.keys()):
        items = by_cat[cat]
        out += f"## {cat} ({len(items)} 个)\n\n"
        out += ("| ID | 名称 | 状态 | 通量 | 兼容平台 | 功率(峰值) | "
                "需通风/气/水 | 标签 |\n")
        out += ("|----|------|------|------|---------|----------|"
                "--------------|------|\n")
        for c in sorted(items, key=lambda x: x.get("id", "")):
            cid = c.get("id", "—")
            name = c.get("name", "—")
            status = fmt_status(c.get("status"))
            tput = c.get("throughput") or {}
            tput_s = (
                f"{tput.get('capacity','?')} {tput.get('unit','')}/"
                f"{tput.get('cycle_time_min','?')}min"
                if isinstance(tput, dict) and tput else "—"
            )
            compat = fmt_list(c.get("compatible_platforms"), max_items=4)
            power = c.get("power_kva_peak")
            power_s = f"{power} kVA" if power else "—"
            flags = []
            if c.get("requires_ventilation"):
                flags.append("通风")
            if c.get("requires_compressed_air"):
                flags.append("气")
            if c.get("requires_water"):
                flags.append("水")
            flags_s = "/".join(flags) if flags else "—"
            tags = fmt_list(c.get("tags"), max_items=3)
            link = md_link(cid, c.get("_file_path", "#"))
            out += (f"| {link} | {name} | {status} | {tput_s} | {compat} | "
                    f"{power_s} | {flags_s} | {tags} |\n")
        out += "\n"

    return out


# ============================================================
# 方案索引
# ============================================================

def render_solution_index(cards: list[dict]) -> str:
    """生成 solutions/_index.md"""
    out = render_header(
        title="工艺方案卡片索引",
        subtitle="SmartLabOS 可复用方案清单",
        count=len(cards),
    )

    if not cards:
        out += "_(暂无方案卡片)_\n"
        return out

    # 按成熟度分组,已交付/标准化排前面
    maturity_order = {"标准化": 0, "已交付": 1, "验证中": 2, "概念": 3}
    sorted_cards = sorted(
        cards,
        key=lambda x: (maturity_order.get(x.get("maturity", ""), 99), x.get("id", ""))
    )

    out += "## 方案一览\n\n"
    out += ("| ID | 名称 | 成熟度 | 行业 | 通量(样/天) | 平台 | 模块数 | "
            "标签 |\n")
    out += ("|----|------|-------|------|------------|------|--------|"
            "------|\n")

    for c in sorted_cards:
        cid = c.get("id", "—")
        name = c.get("name", "—")
        maturity = fmt_maturity(c.get("maturity"))
        industry = fmt_list(c.get("target_industry"), max_items=2)
        tput = c.get("throughput_target") or {}
        tput_s = str(tput.get("samples_per_day", "—")) if isinstance(tput, dict) else "—"
        plat = c.get("platform") or {}
        plat_s = plat.get("id", "—") if isinstance(plat, dict) else "—"
        modules = c.get("modules") or []
        mod_count = len(modules) if isinstance(modules, list) else 0
        tags = fmt_list(c.get("tags"), max_items=3)
        link = md_link(cid, c.get("_file_path", "#"))
        out += (f"| {link} | {name} | {maturity} | {industry} | {tput_s} | "
                f"{plat_s} | {mod_count} | {tags} |\n")

    # 适用场景反向索引
    out += "\n## 按行业反查\n\n"
    by_industry: dict[str, list[str]] = defaultdict(list)
    for c in cards:
        for ind in (c.get("target_industry") or []):
            by_industry[ind].append(c.get("id", "?"))
    if by_industry:
        for ind in sorted(by_industry.keys()):
            ids = ", ".join(sorted(by_industry[ind]))
            out += f"- **{ind}**: {ids}\n"
    else:
        out += "_(尚无行业标签)_\n"

    return out


# ============================================================
# 主流程
# ============================================================

def find_kb_root(start: Path) -> Path | None:
    """从给定目录向上找包含 platforms/ 和 modules/ 的目录"""
    cur = start.resolve()
    for _ in range(6):
        if (cur / "platforms").is_dir() and (cur / "modules").is_dir():
            return cur
        if cur == cur.parent:
            break
        cur = cur.parent
    return None


def main() -> int:
    parser = argparse.ArgumentParser(
        description="自动生成卡片库索引",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    parser.add_argument(
        "--kb-root",
        default=None,
        help="知识库根目录(包含 platforms/ modules/ solutions/),默认自动探测",
    )
    parser.add_argument("--quiet", action="store_true", help="只打印警告与错误")
    args = parser.parse_args()

    if args.kb_root:
        kb_root = Path(args.kb_root).resolve()
    else:
        kb_root = find_kb_root(Path.cwd())
        if not kb_root:
            print("错误: 未找到知识库根目录,请用 --kb-root 显式指定", file=sys.stderr)
            return 2

    if not kb_root.exists():
        print(f"错误: 目录不存在 {kb_root}", file=sys.stderr)
        return 2

    log = (lambda msg: None) if args.quiet else print
    log(f"知识库根目录: {kb_root}")

    has_errors = False

    # 三个子目录分别处理
    targets = [
        ("platforms", render_platform_index),
        ("modules", render_module_index),
        ("solutions", render_solution_index),
    ]

    for subdir_name, renderer in targets:
        subdir = kb_root / subdir_name
        if not subdir.exists():
            log(f"  [跳过] {subdir_name}/ 目录不存在")
            continue

        cards, errors = collect_cards(subdir)
        if errors:
            has_errors = True
            print(f"  [警告] {subdir_name}/ 中以下文件解析失败,已跳过:",
                  file=sys.stderr)
            for e in errors:
                print(f"    - {e}", file=sys.stderr)

        index_path = subdir / "_index.md"
        content = renderer(cards)
        index_path.write_text(content, encoding="utf-8")
        log(f"  [生成] {index_path.relative_to(kb_root)} "
            f"(收录 {len(cards)} 份卡片)")

    log("\n完成。")
    return 1 if has_errors else 0


if __name__ == "__main__":
    sys.exit(main())
