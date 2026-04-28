SmartLabOS AI Assistant - Migration Kit
========================================

Directory convention:
  Project root  : C:\TestClaude\SmartLabOS-AI-Assistant
  Ubuntu staging: C:\TestClaude\SmartLabOS-AI-Assistant\FromUbuntu

Files in this kit:
  1. CLAUDE_CODE_SETUP_GUIDE.md  - Full setup guide (read this first)
  2. export_openclaw_kb.sh       - Ubuntu-side export script
  3. setup.ps1                   - Windows-side one-shot setup script
                                   (UTF-8 with BOM, pure ASCII content -
                                    safe on Windows PowerShell 5.1 and 7+)

Usage order:
  Step 1 [Ubuntu]  Copy export_openclaw_kb.sh to Ubuntu and run:
                     bash export_openclaw_kb.sh
                   Output: ~/FromUbuntu.tar.gz
  Step 2 [Windows] Put the tarball into
                   C:\TestClaude\SmartLabOS-AI-Assistant\FromUbuntu\
                   and extract:
                     tar -xzf FromUbuntu.tar.gz --strip-components=1
  Step 3 [Windows] Put setup.ps1 into C:\TestClaude\SmartLabOS-AI-Assistant\
                   and run (in PowerShell):
                     PowerShell -ExecutionPolicy Bypass -File .\setup.ps1
  Step 4 [VSCode]  Open C:\TestClaude\SmartLabOS-AI-Assistant\
                   Start Claude Code, first ask:
                     "List all files under references/ and summarize each"

For full details see CLAUDE_CODE_SETUP_GUIDE.md
