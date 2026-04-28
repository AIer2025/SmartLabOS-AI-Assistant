#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
recompute_resource_summary.py — 工艺方案 resource_summary 字段自动重算

作用:读入一份 SOL-XXX.md 工艺方案卡片,根据它引用的所有模块卡片,
自动计算 resource_summary 各字段的实际值,并回写到 frontmatter。

避免人手填 resource_summary 时出错(validate_solution.py 会抓出不一致,
但抓到了人还得算一遍再填,这里直接帮你算)。

设计原则:
  · 只修改 frontmatter 中 resource_summary 这一段,其他字段、注释、正文完全不动
  · 默认 dry-run(只显示差异,不写盘),加 --write 才实际修改
  · 修改前自动备份为 <原文件>.bak

使用:
    # 预览,不修改文件
    python recompute_resource_summary.py <solution.md>

    # 实际写入
    python recompute_resource_summary.py <solution.md> --write

    # 批量处理整个目录
    python recompute_resource_summary.py references/solutions/ --write

    # 不要备份
    python recompute_resource_summary.py <solution.md> --write --no-backup

返回值:
    0 = 成功(可能有修改也可能无变化)
    1 = 至少一份方案处理失败
    2 = 脚本本身出错(参数错、依赖缺失等)

依赖:
    pip install pyyaml
"""

from __future__ import annotations

import argparse
import shutil
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Any

try:
    import yaml
except ImportError:
    print("错误: 需要安装 pyyaml,请运行: pip install pyyaml", file=sys.stderr)
    sys.exit(2)


# ============================================================
# Frontmatter 处理
# ============================================================

def split_frontmatter(text: str) -> tuple[str, str, str] | None:
    """
    把整份 .md 文件切成三段: 头部分隔符前 / frontmatter 内容 / 后续正文。
    返回 (lead, fm, body) 或 None(格式不对)。

    例如:
        ---
        id: SOL-XXX
        ---

        # 标题
        正文...

    切完后 lead='---\\n', fm='id: SOL-XXX\\n', body='---\\n\\n# 标题\\n正文...'
    保留尾部 --- 在 body 开头,这样后续拼接时分隔符天然存在。
    """
    if not text.startswith("---"):
        return None
    # 找两个 --- 行(不能用简单 split,因为正文里可能也有 ---)
    lines = text.split("\n")
    if not lines[0].strip() == "---":
        return None
    fm_end = None
    for i in range(1, len(lines)):
        if lines[i].strip() == "---":
            fm_end = i
            break
    if fm_end is None:
        return None
    lead = lines[0] + "\n"
    fm = "\n".join(lines[1:fm_end]) + ("\n" if fm_end > 1 else "")
    body = "\n".join(lines[fm_end:])
    return lead, fm, body


def load_frontmatter_dict(path: Path) -> dict[str, Any] | None:
    """读 .md 解析 frontmatter 为 dict"""
    try:
        text = path.read_text(encoding="utf-8")
    except (IOError, UnicodeDecodeError):
        return None
    parts = split_frontmatter(text)
    if not parts:
        return None
    try:
        data = yaml.safe_load(parts[1])
        return data if isinstance(data, dict) else None
    except yaml.YAMLError:
        return None


# ============================================================
# 计算 resource_summary
# ============================================================

@dataclass
class ComputedSummary:
    """从模块汇总出的实际资源占用"""
    total_modules_used: int
    total_trays_used: int
    total_power_kva_peak: float
    ventilation_required: bool
    compressed_air_required: bool
    water_required: bool

    def to_dict(self) -> dict[str, Any]:
        return {
            "total_modules_used": self.total_modules_used,
            "total_trays_used": self.total_trays_used,
            "total_power_kva_peak": round(self.total_power_kva_peak, 2),
            "ventilation_required": self.ventilation_required,
            "compressed_air_required": self.compressed_air_required,
            "water_required": self.water_required,
        }


def to_int(v, default=0) -> int:
    try:
        return int(v) if v is not None else default
    except (TypeError, ValueError):
        return default


def to_float(v, default=0.0) -> float:
    try:
        return float(v) if v is not None else default
    except (TypeError, ValueError):
        return default


def find_card_by_id(kb_root: Path, prefix: str, card_id: str) -> Path | None:
    """
    按 ID 在知识库中查找卡片文件。
    优先匹配 <id>.md 文件名,找不到则扫 frontmatter id 字段。
    """
    subdir_map = {"PLT": "platforms", "MOD": "modules", "SOL": "solutions"}
    subdir = kb_root / subdir_map.get(prefix, "")
    search_root = subdir if subdir.exists() else kb_root

    direct = list(search_root.rglob(f"{card_id}.md"))
    if direct:
        return direct[0]

    for md_file in search_root.rglob("*.md"):
        if md_file.name.startswith("_"):
            continue
        data = load_frontmatter_dict(md_file)
        if data and data.get("id") == card_id:
            return md_file
    return None


def compute_summary(sol: dict, kb_root: Path) -> tuple[ComputedSummary, list[str]]:
    """
    根据方案的 modules 列表 + 平台 constraints,算出实际 resource_summary。
    返回 (computed_summary, warnings)。warnings 是处理过程中跳过的提示。
    """
    warnings: list[str] = []
    module_entries = sol.get("modules") or []

    total_module_slots = 0
    total_tray_slots = 0
    total_power_peak = 0.0
    heating_count = 0
    needs_air = False
    needs_water = False
    needs_ventilation_self = False  # 模块自身要求通风

    for entry in module_entries:
        if not isinstance(entry, dict):
            continue
        mid = entry.get("id")
        qty = to_int(entry.get("quantity", 1), 1)
        if not mid:
            warnings.append(f"模块条目缺少 id 字段,已跳过: {entry}")
            continue

        mpath = find_card_by_id(kb_root, "MOD", mid)
        if not mpath:
            warnings.append(f"模块 {mid} 在知识库中找不到,已按 0 计入")
            continue

        mcard = load_frontmatter_dict(mpath)
        if not mcard:
            warnings.append(f"模块 {mid} 解析失败,已按 0 计入")
            continue

        total_module_slots += to_int(mcard.get("module_slots", 0)) * qty
        total_tray_slots += to_int(mcard.get("tray_slots", 0)) * qty
        total_power_peak += to_float(mcard.get("power_kva_peak", 0)) * qty
        if mcard.get("heat_generating"):
            heating_count += qty
        if mcard.get("requires_compressed_air"):
            needs_air = True
        if mcard.get("requires_water"):
            needs_water = True
        if mcard.get("requires_ventilation"):
            needs_ventilation_self = True

    # 通风规则: 加热模块超出平台限制 → 需要通风
    needs_ventilation = needs_ventilation_self
    platform_id = (sol.get("platform") or {}).get("id")
    if platform_id:
        ppath = find_card_by_id(kb_root, "PLT", platform_id)
        if ppath:
            pcard = load_frontmatter_dict(ppath) or {}
            max_heating = to_int(
                ((pcard.get("constraints") or {}).get("max_heating_modules", 999)),
                999
            )
            if heating_count > max_heating:
                needs_ventilation = True

    return ComputedSummary(
        total_modules_used=total_module_slots,
        total_trays_used=total_tray_slots,
        total_power_kva_peak=total_power_peak,
        ventilation_required=needs_ventilation,
        compressed_air_required=needs_air,
        water_required=needs_water,
    ), warnings


# ============================================================
# Frontmatter 文本片段替换(保留注释和格式)
# ============================================================

def find_block_range(fm_text: str, key: str) -> tuple[int, int] | None:
    """
    在 frontmatter 文本中找到 key 块的起止行号(0-indexed,半开区间)。
    一个"块"是从 `key:` 行开始,到下一个顶层 key(列开头有字符的非空行)
    或文件末尾为止。

    例如对于:
        platform:
          id: PLT-1400
        resource_summary:
          total_modules_used: 6
          total_power_kva_peak: 4.1
        price_rmb:
          ...

    find_block_range(fm, "resource_summary") 会返回 (2, 5)。
    """
    lines = fm_text.split("\n")
    start = None
    for i, line in enumerate(lines):
        # 顶层 key 的特征: 行首非空白且包含冒号
        if not line or line[0].isspace() or line.lstrip().startswith("#"):
            continue
        # 取冒号前的部分判断 key 名
        head = line.split(":", 1)[0].strip()
        if head == key:
            start = i
            break

    if start is None:
        return None

    # 找下一个顶层 key 或文件末尾;空行不算块内容,块到最后一个有内容的子行为止
    end = start + 1
    last_content = start
    for j in range(start + 1, len(lines)):
        line = lines[j]
        if not line:
            # 空行先记录但不立即扩展块
            continue
        if line[0].isspace() or line.lstrip().startswith("#"):
            # 缩进行或注释行,属于本块
            last_content = j
            continue
        # 顶层 key,块到此结束
        break
    end = last_content + 1
    return start, end


def render_resource_summary(summary: ComputedSummary) -> str:
    """
    把 ComputedSummary 渲染成保留注释的 frontmatter 片段。
    注释列对齐到统一位置,人看着舒服。
    """
    d = summary.to_dict()
    fields = [
        ("total_modules_used", d["total_modules_used"]),
        ("total_trays_used", d["total_trays_used"]),
        ("total_power_kva_peak", d["total_power_kva_peak"]),
        ("ventilation_required", str(d["ventilation_required"]).lower()),
        ("compressed_air_required", str(d["compressed_air_required"]).lower()),
        ("water_required", str(d["water_required"]).lower()),
    ]
    # 把每行 "  key: value" 拼好,然后把注释列对齐到第 42 列
    lines = ["resource_summary:"]
    for k, v in fields:
        prefix = f"  {k}: {v}"
        padded = prefix + " " * max(2, 42 - len(prefix))
        lines.append(padded + "# 自动核算")
    return "\n".join(lines)


def replace_resource_summary(text: str, summary: ComputedSummary) -> str:
    """
    在整份 .md 文件文本中,替换或插入 resource_summary 块。
    保持其他所有内容不变。
    """
    parts = split_frontmatter(text)
    if not parts:
        raise ValueError("frontmatter 格式不正确,无法处理")
    lead, fm, body = parts

    new_block = render_resource_summary(summary)

    rng = find_block_range(fm, "resource_summary")
    fm_lines = fm.split("\n") if fm else []
    new_block_lines = new_block.split("\n")

    if rng is None:
        # 不存在,追加到 frontmatter 末尾
        # 处理末尾换行,确保格式整齐
        while fm_lines and fm_lines[-1] == "":
            fm_lines.pop()
        fm_lines.append("")
        fm_lines.append("# ========== 资源占用汇总(自动核算) ==========")
        fm_lines.extend(new_block_lines)
        fm_lines.append("")
    else:
        start, end = rng
        # 保留 start 之前(含)的内容,替换 [start, end) 区间
        fm_lines = fm_lines[:start] + new_block_lines + fm_lines[end:]

    new_fm = "\n".join(fm_lines)
    if not new_fm.endswith("\n"):
        new_fm += "\n"

    return lead + new_fm + body


# ============================================================
# 单文件处理
# ============================================================

@dataclass
class ProcessResult:
    path: Path
    changed: bool
    old_summary: dict | None
    new_summary: dict
    warnings: list[str]
    error: str | None = None


def process_one(path: Path, kb_root: Path, *, write: bool, backup: bool) -> ProcessResult:
    """处理一份方案卡片"""
    try:
        text = path.read_text(encoding="utf-8")
    except (IOError, UnicodeDecodeError) as e:
        return ProcessResult(path, False, None, {}, [], error=f"读取失败: {e}")

    sol = load_frontmatter_dict(path)
    if not sol:
        return ProcessResult(path, False, None, {}, [], error="frontmatter 解析失败")

    if not sol.get("id", "").startswith("SOL"):
        # 不是方案卡片,跳过
        return ProcessResult(path, False, None, {}, [],
                             error="不是 SOL- 开头的方案卡片,已跳过")

    summary, warnings = compute_summary(sol, kb_root)
    new_dict = summary.to_dict()
    old_dict = sol.get("resource_summary") or {}

    # 比较是否需要变更(只比较脚本管理的几个字段)
    managed_keys = set(new_dict.keys())
    old_managed = {k: old_dict.get(k) for k in managed_keys}
    # 类型规范化比较: 把 None/缺失 视为不同
    needs_change = False
    for k in managed_keys:
        ov = old_managed.get(k)
        nv = new_dict[k]
        if isinstance(nv, float) and isinstance(ov, (int, float)):
            if abs(float(ov) - nv) > 0.005:
                needs_change = True
                break
        elif ov != nv:
            needs_change = True
            break

    if not needs_change:
        return ProcessResult(path, False, old_managed, new_dict, warnings)

    if write:
        try:
            new_text = replace_resource_summary(text, summary)
        except ValueError as e:
            return ProcessResult(path, False, old_managed, new_dict, warnings,
                                 error=str(e))
        if backup:
            shutil.copy2(path, path.with_suffix(path.suffix + ".bak"))
        path.write_text(new_text, encoding="utf-8")

    return ProcessResult(path, True, old_managed, new_dict, warnings)


# ============================================================
# 输出渲染
# ============================================================

def render_diff(r: ProcessResult) -> str:
    """渲染单份处理结果"""
    lines = [f"\n📄 {r.path}"]
    if r.error:
        lines.append(f"  ❌ {r.error}")
        return "\n".join(lines)

    if not r.changed:
        lines.append("  ✓ 已是最新,无需修改")
        for w in r.warnings:
            lines.append(f"  ! {w}")
        return "\n".join(lines)

    lines.append("  字段差异:")
    for k, new_v in r.new_summary.items():
        old_v = (r.old_summary or {}).get(k, "(未填)")
        if old_v != new_v and not (
            isinstance(new_v, float) and isinstance(old_v, (int, float))
            and abs(float(old_v) - new_v) <= 0.005
        ):
            lines.append(f"    {k}: {old_v}  →  {new_v}")
        else:
            lines.append(f"    {k}: {new_v}  (无变化)")
    for w in r.warnings:
        lines.append(f"  ! {w}")
    return "\n".join(lines)


# ============================================================
# CLI
# ============================================================

def find_kb_root(start: Path) -> Path | None:
    """从给定位置向上找包含 platforms/ 和 modules/ 的目录"""
    cur = start.resolve()
    if cur.is_file():
        cur = cur.parent
    for _ in range(6):
        if (cur / "platforms").is_dir() and (cur / "modules").is_dir():
            return cur
        if cur == cur.parent:
            break
        cur = cur.parent
    return None


def collect_targets(target: Path) -> list[Path]:
    """收集要处理的方案文件路径"""
    if target.is_file():
        return [target]
    if target.is_dir():
        return sorted(
            p for p in target.rglob("*.md")
            if not p.name.startswith("_")
        )
    return []


def main() -> int:
    parser = argparse.ArgumentParser(
        description="工艺方案 resource_summary 字段自动重算",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    parser.add_argument("target", help="方案文件 .md 或包含方案文件的目录")
    parser.add_argument("--kb-root", default=None,
                        help="知识库根目录(包含 platforms/ modules/ solutions/),默认自动探测")
    parser.add_argument("--write", action="store_true",
                        help="实际写入文件;不加此参数则为 dry-run(只显示差异)")
    parser.add_argument("--no-backup", action="store_true",
                        help="--write 时不要创建 .bak 备份(默认会创建)")
    args = parser.parse_args()

    target = Path(args.target).resolve()
    if not target.exists():
        print(f"错误: 路径不存在 {target}", file=sys.stderr)
        return 2

    if args.kb_root:
        kb_root = Path(args.kb_root).resolve()
    else:
        kb_root = find_kb_root(target)
        if not kb_root:
            print("错误: 未找到知识库根目录,请用 --kb-root 显式指定", file=sys.stderr)
            return 2

    targets = collect_targets(target)
    if not targets:
        print(f"错误: 在 {target} 下未找到任何 .md 文件", file=sys.stderr)
        return 2

    print(f"知识库根目录: {kb_root}")
    print(f"模式: {'实际写入' if args.write else 'DRY-RUN(预览,不修改)'}")
    print(f"待处理: {len(targets)} 份方案文件")

    results: list[ProcessResult] = []
    for t in targets:
        r = process_one(t, kb_root,
                        write=args.write,
                        backup=not args.no_backup)
        results.append(r)
        print(render_diff(r))

    changed = sum(1 for r in results if r.changed)
    errors = sum(1 for r in results if r.error)
    unchanged = len(results) - changed - errors

    print("\n" + "=" * 50)
    print(f"总计: {len(results)} 份处理,"
          f"{changed} 份{'已修改' if args.write else '需修改'},"
          f"{unchanged} 份无变化,"
          f"{errors} 份失败")
    if not args.write and changed > 0:
        print("\n提示: 加 --write 参数实际写入修改")
    print("=" * 50)

    return 1 if errors > 0 else 0


if __name__ == "__main__":
    sys.exit(main())
