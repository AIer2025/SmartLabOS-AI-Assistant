#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
validate_solution.py — 工艺方案卡片合法性校验

作用:读入一份 SOL-XXX.md 工艺方案卡片,根据它引用的平台卡片(PLT-XXX.md)
和模块卡片(MOD-XXX.md),自动检查方案的合法性。

校验项目:
  1. 平台卡片存在性
  2. 模块卡片存在性
  3. 模块-平台兼容性(双向)
  4. 模块位占用 ≤ 平台 max_modules
  5. 托盘位占用 ≤ 平台 max_trays
  6. 峰值功率累加 ≤ 平台 power_kva_peak
  7. 加热类模块数 vs 通风规则一致性
  8. 公用工程需求(压缩空气/水/排液)汇总一致性
  9. resource_summary 字段与实际汇总一致性
 10. workflow 引用的模块都在 modules 列表中

使用:
    python validate_solution.py <path_to_solution.md> [--kb-root <kb_root>]

示例:
    python validate_solution.py references/solutions/SOL-QUECHERS-MID.md
    python validate_solution.py SOL-QUECHERS-MID.md --kb-root C:/TestClaude/SmartLabOS-AI-Assistant/references

返回值(供 CI/git hook 使用):
    0 = 全部通过(可能有 WARN)
    1 = 至少一项 FAIL
    2 = 脚本本身出错(文件找不到、格式错等)

依赖:
    pip install pyyaml
"""

from __future__ import annotations

import argparse
import sys
from dataclasses import dataclass, field
from pathlib import Path
from typing import Any

try:
    import yaml
except ImportError:
    print("错误: 需要安装 pyyaml,请运行: pip install pyyaml", file=sys.stderr)
    sys.exit(2)


# ============================================================
# 数据结构
# ============================================================

@dataclass
class CheckResult:
    """单项校验结果"""
    level: str           # PASS | FAIL | WARN | INFO
    rule: str            # 规则名,简短
    message: str         # 详细说明

    def render(self) -> str:
        symbol = {"PASS": "✓", "FAIL": "✗", "WARN": "!", "INFO": "·"}.get(self.level, "?")
        return f"  [{self.level:4}] {symbol} {self.rule}: {self.message}"


@dataclass
class ValidationReport:
    """整体校验报告"""
    solution_id: str
    results: list[CheckResult] = field(default_factory=list)

    def add(self, level: str, rule: str, message: str) -> None:
        self.results.append(CheckResult(level, rule, message))

    @property
    def pass_count(self) -> int:
        return sum(1 for r in self.results if r.level == "PASS")

    @property
    def fail_count(self) -> int:
        return sum(1 for r in self.results if r.level == "FAIL")

    @property
    def warn_count(self) -> int:
        return sum(1 for r in self.results if r.level == "WARN")

    @property
    def has_failures(self) -> bool:
        return self.fail_count > 0

    def render(self) -> str:
        lines = [
            "=" * 60,
            f"校验报告: {self.solution_id}",
            "=" * 60,
        ]
        for r in self.results:
            lines.append(r.render())
        lines.append("")
        lines.append(
            f"小结: {self.pass_count} 通过, "
            f"{self.fail_count} 失败, "
            f"{self.warn_count} 警告"
        )
        lines.append("=" * 60)
        return "\n".join(lines)


# ============================================================
# Frontmatter 解析
# ============================================================

def load_card(path: Path) -> dict[str, Any]:
    """读 .md 文件,解析 YAML frontmatter,返回 dict"""
    if not path.exists():
        raise FileNotFoundError(f"卡片文件不存在: {path}")

    text = path.read_text(encoding="utf-8")
    if not text.startswith("---"):
        raise ValueError(f"卡片缺少 YAML frontmatter: {path}")

    # 切出 frontmatter 部分(两个 --- 之间)
    parts = text.split("---", 2)
    if len(parts) < 3:
        raise ValueError(f"卡片 frontmatter 格式不完整: {path}")

    frontmatter_text = parts[1]
    try:
        data = yaml.safe_load(frontmatter_text)
    except yaml.YAMLError as e:
        raise ValueError(f"YAML 解析失败 ({path}): {e}") from e

    if not isinstance(data, dict):
        raise ValueError(f"frontmatter 必须是字典格式: {path}")

    return data


def find_card_by_id(kb_root: Path, prefix: str, card_id: str) -> Path | None:
    """
    在知识库目录树下递归查找指定 ID 的卡片文件。
    prefix: 'PLT' / 'MOD' / 'SOL'
    card_id: 完整 ID,如 'PLT-1400'

    优先匹配 <id>.md 文件名,找不到则在 frontmatter 里查 id 字段。
    """
    # 按子目录分别搜索,提高效率
    subdir_map = {"PLT": "platforms", "MOD": "modules", "SOL": "solutions"}
    subdir = kb_root / subdir_map.get(prefix, "")
    search_root = subdir if subdir.exists() else kb_root

    # 第一轮:按文件名直接命中
    direct = list(search_root.rglob(f"{card_id}.md"))
    if direct:
        return direct[0]

    # 第二轮:扫描所有 .md 文件,匹配 frontmatter 里的 id
    for md_file in search_root.rglob("*.md"):
        if md_file.name.startswith("_"):  # 跳过 _index.md 等
            continue
        try:
            data = load_card(md_file)
            if data.get("id") == card_id:
                return md_file
        except (ValueError, FileNotFoundError):
            continue

    return None


# ============================================================
# 字段安全读取
# ============================================================

def safe_get(d: dict, *keys, default=None):
    """安全嵌套取值: safe_get(d, 'a', 'b', 'c') 等价 d['a']['b']['c'],缺则 default"""
    cur = d
    for k in keys:
        if not isinstance(cur, dict) or k not in cur:
            return default
        cur = cur[k]
    return cur


def to_float(v, default=0.0) -> float:
    """安全转 float"""
    try:
        return float(v) if v is not None else default
    except (TypeError, ValueError):
        return default


def to_int(v, default=0) -> int:
    """安全转 int"""
    try:
        return int(v) if v is not None else default
    except (TypeError, ValueError):
        return default


# ============================================================
# 校验规则
# ============================================================

def validate(solution_path: Path, kb_root: Path) -> ValidationReport:
    """主校验函数"""
    sol = load_card(solution_path)
    sol_id = sol.get("id", solution_path.stem)
    report = ValidationReport(solution_id=sol_id)

    # ----- 0. 基础元数据检查 -----
    if not sol.get("id"):
        report.add("FAIL", "metadata", "方案卡片缺少 id 字段")
        return report  # 没 ID 没法继续

    maturity = sol.get("maturity", "未知")
    status = sol.get("status", "未知")
    report.add("INFO", "metadata", f"方案状态: status={status}, maturity={maturity}")

    # ----- 1. 平台 ID 引用检查 -----
    platform_id = safe_get(sol, "platform", "id")
    if not platform_id:
        report.add("FAIL", "platform.id", "方案未指定 platform.id")
        return report

    platform_path = find_card_by_id(kb_root, "PLT", platform_id)
    if not platform_path:
        report.add("FAIL", "platform.exists",
                   f"平台 {platform_id} 在知识库中找不到")
        platform = {}  # 空字典,后续容量检查会被跳过但模块检查继续跑
    else:
        platform = load_card(platform_path)
        report.add("PASS", "platform.exists",
                   f"平台 {platform_id} 存在 ({platform_path.name})")

    # ----- 2. 模块 ID 引用与加载 -----
    module_entries = sol.get("modules", []) or []
    if not module_entries:
        report.add("WARN", "modules", "方案未引用任何模块")

    loaded_modules: list[tuple[dict, dict]] = []  # (entry, module_card)
    missing_module_ids: list[str] = []

    for entry in module_entries:
        mid = entry.get("id") if isinstance(entry, dict) else None
        if not mid:
            report.add("FAIL", "modules.entry", f"模块条目缺少 id: {entry}")
            continue
        mpath = find_card_by_id(kb_root, "MOD", mid)
        if not mpath:
            missing_module_ids.append(mid)
            continue
        try:
            mcard = load_card(mpath)
            loaded_modules.append((entry, mcard))
        except (ValueError, FileNotFoundError) as e:
            report.add("FAIL", "modules.load", f"模块 {mid} 解析失败: {e}")

    if missing_module_ids:
        report.add("FAIL", "modules.exists",
                   f"以下模块在知识库中找不到: {', '.join(missing_module_ids)}")
    else:
        report.add("PASS", "modules.exists",
                   f"全部 {len(loaded_modules)} 个模块均已找到")

    # ----- 3. 模块-平台兼容性 -----
    incompatible = []
    for entry, mcard in loaded_modules:
        compat = mcard.get("compatible_platforms") or []
        incompat = mcard.get("incompatible_platforms") or []
        mid = mcard.get("id", "?")
        if platform_id in incompat:
            incompatible.append(f"{mid}(明确禁止)")
        elif compat and platform_id not in compat:
            incompatible.append(f"{mid}(不在兼容列表)")

    if incompatible:
        report.add("FAIL", "compatibility",
                   f"以下模块不兼容平台 {platform_id}: {', '.join(incompatible)}")
    elif loaded_modules:
        report.add("PASS", "compatibility",
                   f"所有模块均兼容平台 {platform_id}")

    # ----- 4. 模块位占用 -----
    total_module_slots = 0
    total_tray_slots = 0
    total_power_peak = 0.0
    heating_count = 0
    needs_air = False
    needs_water = False
    needs_drain = False
    needs_ventilation_modules = False  # 模块自身要求通风

    for entry, mcard in loaded_modules:
        qty = to_int(entry.get("quantity", 1), 1)
        total_module_slots += to_int(mcard.get("module_slots", 0)) * qty
        total_tray_slots += to_int(mcard.get("tray_slots", 0)) * qty
        total_power_peak += to_float(mcard.get("power_kva_peak", 0)) * qty
        if mcard.get("heat_generating"):
            heating_count += qty
        if mcard.get("requires_compressed_air"):
            needs_air = True
        if mcard.get("requires_water"):
            needs_water = True
        if mcard.get("requires_drain"):
            needs_drain = True
        if mcard.get("requires_ventilation"):
            needs_ventilation_modules = True

    max_modules = to_int(platform.get("max_modules", 0))
    if not platform:
        report.add("INFO", "capacity.modules",
                   f"平台数据缺失,跳过容量检查(实际占用 {total_module_slots})")
    elif total_module_slots > max_modules:
        report.add("FAIL", "capacity.modules",
                   f"模块位占用 {total_module_slots} 超出平台上限 {max_modules}")
    else:
        report.add("PASS", "capacity.modules",
                   f"模块位占用 {total_module_slots}/{max_modules}")

    # ----- 5. 托盘位占用 -----
    max_trays = to_int(platform.get("max_trays", 0))
    if not platform:
        report.add("INFO", "capacity.trays",
                   f"平台数据缺失,跳过容量检查(实际占用 {total_tray_slots})")
    elif total_tray_slots > max_trays:
        report.add("FAIL", "capacity.trays",
                   f"托盘位占用 {total_tray_slots} 超出平台上限 {max_trays}")
    else:
        report.add("PASS", "capacity.trays",
                   f"托盘位占用 {total_tray_slots}/{max_trays}")

    # ----- 6. 功率累加 -----
    platform_power_peak = to_float(platform.get("power_kva_peak", 0))
    if not platform:
        report.add("INFO", "capacity.power",
                   f"平台数据缺失,跳过功率检查(实际 {total_power_peak:.2f} kVA)")
    elif total_power_peak > platform_power_peak:
        report.add("FAIL", "capacity.power",
                   f"峰值功率累加 {total_power_peak:.2f} kVA "
                   f"超出平台上限 {platform_power_peak:.2f} kVA")
    elif platform_power_peak > 0 and total_power_peak / platform_power_peak > 0.9:
        report.add("WARN", "capacity.power",
                   f"峰值功率累加 {total_power_peak:.2f} kVA "
                   f"接近平台上限 {platform_power_peak:.2f} kVA(>90%),建议核对")
    else:
        report.add("PASS", "capacity.power",
                   f"峰值功率 {total_power_peak:.2f}/{platform_power_peak:.2f} kVA")

    # ----- 7. 加热类模块 vs 通风规则 -----
    max_heating = to_int(safe_get(platform, "constraints", "max_heating_modules", default=999))
    sol_ventilation = bool(safe_get(sol, "resource_summary", "ventilation_required", default=False))

    if heating_count > max_heating:
        if not sol_ventilation and not needs_ventilation_modules:
            report.add("WARN", "ventilation",
                       f"加热类模块 {heating_count} 个,超过平台限制 {max_heating},"
                       f"但 resource_summary.ventilation_required 未设为 true")
        else:
            report.add("PASS", "ventilation",
                       f"加热模块 {heating_count} 个超过 {max_heating},已正确启用通风")
    else:
        report.add("INFO", "ventilation",
                   f"加热类模块 {heating_count} 个,平台限制 {max_heating}")

    # ----- 8. 公用工程需求一致性 -----
    declared_air = bool(safe_get(sol, "resource_summary", "compressed_air_required", default=False))
    declared_water = bool(safe_get(sol, "resource_summary", "water_required", default=False))

    if needs_air and not declared_air:
        report.add("WARN", "utilities.air",
                   "至少一个模块需要压缩空气,但 resource_summary.compressed_air_required 未声明")
    elif needs_air:
        report.add("PASS", "utilities.air", "压缩空气需求已正确声明")

    if needs_water and not declared_water:
        report.add("WARN", "utilities.water",
                   "至少一个模块需要水,但 resource_summary.water_required 未声明")
    elif needs_water:
        report.add("PASS", "utilities.water", "水路需求已正确声明")

    if needs_drain:
        report.add("INFO", "utilities.drain", "至少一个模块需要排液,请确认现场具备排液条件")

    # ----- 9. resource_summary 字段一致性 -----
    declared_total_modules = to_int(safe_get(sol, "resource_summary", "total_modules_used", default=-1))
    declared_total_trays = to_int(safe_get(sol, "resource_summary", "total_trays_used", default=-1))
    declared_total_power = to_float(safe_get(sol, "resource_summary", "total_power_kva_peak", default=-1))

    if declared_total_modules >= 0 and declared_total_modules != total_module_slots:
        report.add("WARN", "resource_summary",
                   f"resource_summary.total_modules_used={declared_total_modules} "
                   f"与实际汇总 {total_module_slots} 不一致")
    if declared_total_trays >= 0 and declared_total_trays != total_tray_slots:
        report.add("WARN", "resource_summary",
                   f"resource_summary.total_trays_used={declared_total_trays} "
                   f"与实际汇总 {total_tray_slots} 不一致")
    if declared_total_power >= 0 and abs(declared_total_power - total_power_peak) > 0.05:
        report.add("WARN", "resource_summary",
                   f"resource_summary.total_power_kva_peak={declared_total_power:.2f} "
                   f"与实际汇总 {total_power_peak:.2f} 不一致")

    # ----- 10. workflow 引用一致性 -----
    workflow = sol.get("workflow", []) or []
    module_ids_in_solution = {e.get("id") for e in module_entries if isinstance(e, dict)}
    workflow_orphans = []
    for step in workflow:
        if not isinstance(step, dict):
            continue
        wmid = step.get("module_id")
        if wmid and wmid not in module_ids_in_solution:
            workflow_orphans.append(f"step {step.get('step', '?')}({wmid})")
    if workflow_orphans:
        report.add("FAIL", "workflow.refs",
                   f"workflow 引用了未在 modules 列表声明的模块: {', '.join(workflow_orphans)}")
    elif workflow:
        report.add("PASS", "workflow.refs",
                   f"workflow {len(workflow)} 个步骤引用一致")

    # ----- 11. 平台功能集合检查(软规则) -----
    platform_funcs = set(platform.get("supported_functions") or [])
    missing_funcs = []
    for entry, mcard in loaded_modules:
        f = mcard.get("function")
        if f and platform_funcs and f not in platform_funcs:
            missing_funcs.append(f"{mcard.get('id')}({f})")
    if missing_funcs:
        report.add("WARN", "functions",
                   f"以下模块功能不在平台 supported_functions 中: {', '.join(missing_funcs)}")

    return report


# ============================================================
# CLI
# ============================================================

def main() -> int:
    parser = argparse.ArgumentParser(
        description="工艺方案卡片合法性校验",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    parser.add_argument("solution", help="待校验的 SOL-XXX.md 文件路径")
    parser.add_argument(
        "--kb-root",
        default=None,
        help="知识库根目录(包含 platforms/ modules/ solutions/),"
             "默认从方案文件位置向上推断"
    )
    args = parser.parse_args()

    sol_path = Path(args.solution).resolve()
    if not sol_path.exists():
        print(f"错误: 找不到方案文件 {sol_path}", file=sys.stderr)
        return 2

    # 推断 kb_root: 默认从方案文件向上找,直到找到包含 platforms/ 的目录
    if args.kb_root:
        kb_root = Path(args.kb_root).resolve()
    else:
        kb_root = sol_path.parent
        for _ in range(5):  # 最多上溯 5 层
            if (kb_root / "platforms").is_dir() and (kb_root / "modules").is_dir():
                break
            kb_root = kb_root.parent
        else:
            print(f"警告: 未自动定位到知识库根目录,请用 --kb-root 显式指定", file=sys.stderr)
            kb_root = sol_path.parent

    if not kb_root.exists():
        print(f"错误: 知识库根目录不存在 {kb_root}", file=sys.stderr)
        return 2

    try:
        report = validate(sol_path, kb_root)
    except (ValueError, FileNotFoundError) as e:
        print(f"错误: {e}", file=sys.stderr)
        return 2

    print(report.render())

    return 1 if report.has_failures else 0


if __name__ == "__main__":
    sys.exit(main())
