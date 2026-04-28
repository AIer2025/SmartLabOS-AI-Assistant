# =============================================================================
# setup.ps1 - SmartLabOS Claude Code Project Setup Script
# -----------------------------------------------------------------------------
# Purpose: Move files from FromUbuntu\ staging dir to Claude Code layout.
# Prereq : Extract the Ubuntu export tarball into .\FromUbuntu\ first.
# Usage  : In the project root, run:
#            PowerShell -ExecutionPolicy Bypass -File .\setup.ps1
#
# Project root: C:\TestClaude\SmartLabOS-AI-Assistant
# =============================================================================

$ErrorActionPreference = "Stop"

# Force UTF-8 for console I/O so any non-ASCII content in source files
# (Chinese in AGENTS.md / SOUL.md / SKILL.md) reads and writes correctly
# on both Windows PowerShell 5.1 and PowerShell 7+.
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

$ProjectRoot = "C:\TestClaude\SmartLabOS-AI-Assistant"
$FromUbuntu  = Join-Path $ProjectRoot "FromUbuntu"

if ((Get-Location).Path -ne $ProjectRoot) {
    Write-Host "Switching to project root: $ProjectRoot"
    Set-Location $ProjectRoot
}

if (-not (Test-Path $FromUbuntu)) {
    Write-Host "ERROR: $FromUbuntu not found" -ForegroundColor Red
    Write-Host "       Please extract the Ubuntu tarball into this directory first." -ForegroundColor Red
    exit 1
}

Write-Host "=========================================="
Write-Host "SmartLabOS Claude Code Project Setup"
Write-Host "=========================================="
Write-Host "Project root : $ProjectRoot"
Write-Host "Staging dir  : $FromUbuntu"
Write-Host ""

# Helper: read a file as UTF-8 text (handles BOM and non-BOM)
function Read-Utf8File {
    param([string]$Path)
    if (-not (Test-Path $Path)) { return "" }
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    return [System.Text.Encoding]::UTF8.GetString($bytes)
}

# Helper: write text as UTF-8 WITHOUT BOM (Claude Code prefers this)
function Write-Utf8File {
    param([string]$Path, [string]$Content)
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($Path, $Content, $utf8NoBom)
}

# ---- 1) Create directory skeleton ---------------------------------------
Write-Host "[1/6] Creating directory skeleton..."
$dirs = @(".\.claude\skills", ".\references", ".\projects", ".\samples")
foreach ($d in $dirs) {
    New-Item -ItemType Directory -Force -Path $d | Out-Null
}
New-Item -ItemType File -Force -Path ".\projects\.gitkeep" | Out-Null
Write-Host "    OK  .claude\skills\  references\  projects\  samples\"

# ---- 2) Copy references knowledge base ----------------------------------
Write-Host "[2/6] Copying references knowledge base..."
$refSrc = Join-Path $FromUbuntu "references"
if (Test-Path $refSrc) {
    Copy-Item "$refSrc\*.md" ".\references\" -Force
    $count = (Get-ChildItem ".\references\*.md" -ErrorAction SilentlyContinue).Count
    Write-Host "    OK  $count knowledge base file(s) copied"
} else {
    Write-Host "    WARN: $refSrc not found" -ForegroundColor Yellow
}

# ---- 3) Copy Skills ------------------------------------------------------
Write-Host "[3/6] Copying Skills..."
$skillsSrc = Join-Path $FromUbuntu "skills"
if (Test-Path $skillsSrc) {
    Copy-Item "$skillsSrc\*" ".\.claude\skills\" -Recurse -Force
    $count = (Get-ChildItem ".\.claude\skills" -Filter "SKILL.md" -Recurse -ErrorAction SilentlyContinue).Count
    Write-Host "    OK  $count Skill(s) copied"
} else {
    Write-Host "    WARN: $skillsSrc not found" -ForegroundColor Yellow
}

# ---- 4) Merge AGENTS.md + SOUL.md into CLAUDE.md ------------------------
Write-Host "[4/6] Generating CLAUDE.md from AGENTS.md + SOUL.md..."
$agentsFile = Join-Path $FromUbuntu "persona\AGENTS.md"
$soulFile   = Join-Path $FromUbuntu "persona\SOUL.md"
$agents = Read-Utf8File $agentsFile
$soul   = Read-Utf8File $soulFile

# Rewrite legacy OpenClaw paths to Claude Code paths
$agents = $agents -replace 'workspace/references/', 'references/'
$agents = $agents -replace 'workspace/projects/',   'projects/'
$agents = $agents -replace '~/\.openclaw/workspace/', ''
$soul   = $soul   -replace 'workspace/references/', 'references/'
$soul   = $soul   -replace 'workspace/projects/',   'projects/'

# Build CLAUDE.md body. Using plain text concat to avoid any here-string
# parsing issues with backticks / colons on old PowerShell versions.
$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine("# SmartLabOS Proposal Generation Agent - Project Instructions")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("> This file is loaded into the system prompt at the start of every Claude Code session.")
[void]$sb.AppendLine("> Merged from OpenClaw's AGENTS.md and SOUL.md, with paths rewritten for Claude Code.")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("## From AGENTS.md")
[void]$sb.AppendLine("")
[void]$sb.AppendLine($agents)
[void]$sb.AppendLine("")
[void]$sb.AppendLine("---")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("## From SOUL.md")
[void]$sb.AppendLine("")
[void]$sb.AppendLine($soul)
[void]$sb.AppendLine("")
[void]$sb.AppendLine("---")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("## Claude Code Specific Conventions (appended)")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("- Project root : C:\TestClaude\SmartLabOS-AI-Assistant\")
[void]$sb.AppendLine("- Knowledge base is under the project-root references/ folder")
[void]$sb.AppendLine("- Proposal output goes to projects/<customer-or-project-name>/")
[void]$sb.AppendLine("- Skills live under .claude/skills/ (auto-loaded by Claude Code)")
[void]$sb.AppendLine("- All technical parameters MUST be read from references/. Never fabricate values.")
[void]$sb.AppendLine("  If a value is missing, say so explicitly and stop instead of guessing.")
[void]$sb.AppendLine("- For pricing questions, always reply: 'Please contact the sales team'. No numbers.")
[void]$sb.AppendLine("- Only handle SmartLabOS-related proposals. Decline unrelated questions.")

Write-Utf8File ".\CLAUDE.md" $sb.ToString()
Write-Host "    OK  CLAUDE.md generated (paths rewritten: workspace/references/ -> references/)"

# ---- 5) Copy historical projects as samples -----------------------------
Write-Host "[5/6] Copying historical projects as samples..."
$archSrc = Join-Path $FromUbuntu "projects-archive"
if ((Test-Path $archSrc) -and (Get-ChildItem $archSrc -ErrorAction SilentlyContinue)) {
    Copy-Item "$archSrc\*" ".\samples\" -Recurse -Force
    $count = (Get-ChildItem ".\samples" -Filter "*.md" -Recurse -ErrorAction SilentlyContinue).Count
    Write-Host "    OK  $count historical .md file(s) copied"
} else {
    Write-Host "    INFO: no historical projects, skipped"
}

# ---- 6) Rewrite paths inside Skill files --------------------------------
Write-Host "[6/6] Rewriting OpenClaw paths inside Skill files..."
$skillMds = Get-ChildItem ".\.claude\skills" -Filter "*.md" -Recurse -ErrorAction SilentlyContinue
$modified = 0
foreach ($f in $skillMds) {
    $content  = Read-Utf8File $f.FullName
    $original = $content
    $content = $content -replace 'workspace/references/', 'references/'
    $content = $content -replace 'workspace/projects/',   'projects/'
    $content = $content -replace '~/\.openclaw/workspace/', ''
    if ($content -ne $original) {
        Write-Utf8File $f.FullName $content
        $modified++
    }
}
Write-Host "    OK  $modified Skill file(s) rewritten"

# ---- Generate .gitignore if missing -------------------------------------
if (-not (Test-Path ".\.gitignore")) {
    $gitignore = @'
# Generated proposal output
projects/**/*.md
!projects/.gitkeep

# Claude Code local state
.claude/settings.local.json

# Ubuntu staging dir - do not commit
FromUbuntu/

# OS junk
Thumbs.db
.DS_Store
'@
    Write-Utf8File ".\.gitignore" $gitignore
    Write-Host "    OK  .gitignore generated"
}

# ---- Generate README.md if missing --------------------------------------
if (-not (Test-Path ".\README.md")) {
    $readme = @'
# SmartLabOS AI Assistant (Claude Code edition)

Claude Code + Claude Max based pre-sales technical proposal generator.

## Quick start

1. Open C:\TestClaude\SmartLabOS-AI-Assistant in VS Code
2. Launch Claude Code (`claude` in terminal, or the VS Code extension)
3. First, verify: ask Claude to list all files under references/ and summarize each
4. Real run: ask Claude "Create proposal for <customer> <project>" and step through the 5 stages

## Directory layout

- CLAUDE.md          - Agent role and constraints (auto-loaded each session)
- .claude/skills/    - The 5-stage workflow Skills
- references/        - Industry standards, hardware specs, SOPs, reference data
- projects/<name>/   - One directory per customer, holds 6-stage artifacts
- samples/           - Historical project references
- FromUbuntu/        - Raw export from Ubuntu OpenClaw (safe to delete after setup)
'@
    Write-Utf8File ".\README.md" $readme
    Write-Host "    OK  README.md generated"
}

Write-Host ""
Write-Host "=========================================="
Write-Host "Setup complete" -ForegroundColor Green
Write-Host "=========================================="
Write-Host "Next steps:"
Write-Host "  1. Open $ProjectRoot in VS Code"
Write-Host "  2. Run 'claude' in the terminal (or start the VS Code extension)"
Write-Host "  3. Ask: 'List all files under references/ and summarize each one'"
Write-Host "  4. Then: 'Create proposal for <customer> <project>' to start the real flow"
Write-Host ""
Write-Host "Once everything works, you can delete the staging dir to save space:"
Write-Host "  Remove-Item -Recurse -Force $FromUbuntu"
