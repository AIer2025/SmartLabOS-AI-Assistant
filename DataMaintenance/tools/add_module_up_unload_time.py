# -*- coding: utf-8 -*-
"""
一次性数据迁移：为所有 Module 卡片在 front-matter 中、`platform_compatibility:`
之前插入新分组：

    module_up_unload_time:
      up_unload_time: 60s

- 仅在 front-matter（首尾 `---` 之间）内操作；正文不动。
- 无损保留 UTF-8 BOM 与 CRLF 行尾。
- 幂等：已存在 `module_up_unload_time:` 的文件会被跳过。

用法:
    python add_module_up_unload_time.py            # 实际写入
    python add_module_up_unload_time.py --dry-run  # 只报告，不写文件
"""
import os
import sys
import argparse

HERE = os.path.dirname(os.path.abspath(__file__))
PROJECT_ROOT = os.path.abspath(os.path.join(HERE, "..", ".."))
MODULES_DIR = os.path.join(PROJECT_ROOT, "references", "01-modules")

GROUP_KEY = "module_up_unload_time"
FIELD_KEY = "up_unload_time"
DEFAULT_VALUE = "60s"
ANCHOR = "platform_compatibility:"


def process_text(text, eol):
    """在 front-matter 内、platform_compatibility 之前插入新分组。
    返回 (new_text, status)；status ∈ inserted/skipped-exists/skipped-no-anchor/skipped-no-frontmatter。"""
    lines = text.split(eol)

    # 定位 front-matter 区间 [fm_start+1, fm_end)
    if not lines or lines[0].strip() != "---":
        return text, "skipped-no-frontmatter"
    fm_end = None
    for i in range(1, len(lines)):
        if lines[i].strip() == "---":
            fm_end = i
            break
    if fm_end is None:
        return text, "skipped-no-frontmatter"

    fm_range = range(1, fm_end)
    # 幂等：已存在则跳过
    for i in fm_range:
        if lines[i].strip() == GROUP_KEY + ":":
            return text, "skipped-exists"

    # 锚点：front-matter 内顶层（无缩进）的 platform_compatibility:
    anchor_idx = None
    for i in fm_range:
        if lines[i] == ANCHOR or lines[i].rstrip() == ANCHOR:
            anchor_idx = i
            break
    if anchor_idx is None:
        return text, "skipped-no-anchor"

    block = [
        "%s:" % GROUP_KEY,
        "  %s: %s" % (FIELD_KEY, DEFAULT_VALUE),
    ]
    new_lines = lines[:anchor_idx] + block + lines[anchor_idx:]
    return eol.join(new_lines), "inserted"


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--dry-run", action="store_true")
    args = ap.parse_args()

    files = sorted(f for f in os.listdir(MODULES_DIR)
                   if f.lower().endswith(".md") and not f.startswith("_"))
    counts = {}
    for fn in files:
        path = os.path.join(MODULES_DIR, fn)
        with open(path, "rb") as f:
            raw = f.read()
        has_bom = raw.startswith(b"\xef\xbb\xbf")
        body = raw[3:] if has_bom else raw
        # 行尾判定：有 CRLF 则用 CRLF，否则 LF
        eol = "\r\n" if b"\r\n" in body else "\n"
        text = body.decode("utf-8")

        new_text, status = process_text(text, eol)
        counts[status] = counts.get(status, 0) + 1
        if status != "inserted":
            print("  [%s] %s" % (status, fn))
            continue

        if not args.dry_run:
            out = new_text.encode("utf-8")
            if has_bom:
                out = b"\xef\xbb\xbf" + out
            with open(path, "wb") as f:
                f.write(out)

    print("\n汇总（%d 个文件）：" % len(files))
    for k in sorted(counts):
        print("  %-22s %d" % (k, counts[k]))
    if args.dry_run:
        print("\n[DRY-RUN] 未写入任何文件。")


if __name__ == "__main__":
    main()
