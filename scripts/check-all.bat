@echo off
REM SmartLabOS 卡片库一键体检
REM 1. 重建所有索引
REM 2. 校验所有工艺方案
REM
REM 双击运行,或在命令行 cd 到项目根目录后执行

cd /d %~dp0\..

echo ========================================
echo SmartLabOS 卡片库一键体检
echo ========================================
echo.

REM 检查 python
where python >nul 2>&1
if %errorlevel% neq 0 (
    echo [错误] 找不到 python,请先安装 Python 3.9+ 并加入 PATH
    pause
    exit /b 1
)

REM 检查 pyyaml
python -c "import yaml" >nul 2>&1
if %errorlevel% neq 0 (
    echo [提示] 缺少 pyyaml,正在安装...
    pip install pyyaml
    if %errorlevel% neq 0 (
        echo [错误] pyyaml 安装失败
        pause
        exit /b 1
    )
)

echo [步骤 1/2] 重建索引...
echo ----------------------------------------
python tools\build_indexes.py
if %errorlevel% neq 0 (
    echo [警告] 索引生成出错,但继续后续步骤
)
echo.

echo [步骤 2/2] 校验所有方案...
echo ----------------------------------------
set FAIL_COUNT=0
for %%f in (references\solutions\SOL-*.md) do (
    echo.
    echo ^>^>^> 校验 %%f
    python tools\validate_solution.py "%%f"
    if errorlevel 1 set /a FAIL_COUNT+=1
)

echo.
echo ========================================
if %FAIL_COUNT% gtr 0 (
    echo 完成: %FAIL_COUNT% 份方案校验失败,请逐一修复
) else (
    echo 完成: 全部方案校验通过
)
echo ========================================
pause
