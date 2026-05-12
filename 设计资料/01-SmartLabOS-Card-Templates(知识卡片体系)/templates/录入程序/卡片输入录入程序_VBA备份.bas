Attribute VB_Name = "modCardExport"
Option Explicit

' ============================================================
' SmartLabOS 卡片录入程序 - VBA 核心
' 三个按钮的入口：
'   ClearCurrentInput   清除当前选中行的输入项
'   ClearAllInputs      清除当前 Sheet 所有输入项
'   ExportToMD          按当前 Sheet 类型导出 MD 卡片
' ============================================================

Private Const OUTPUT_DIR As String = "C:\TestClaude\SmartLabOS-AI-Assistant\设计资料\01-SmartLabOS-Card-Templates(知识卡片体系)\examples"

' 静默模式：True 时不弹窗，所有 Yes/No 默认 Yes（用于自动化测试）
Public g_silent As Boolean
Private Const COL_SECTION As Long = 1   ' A
Private Const COL_LABEL As Long = 2     ' B
Private Const COL_INPUT As Long = 3     ' C
Private Const COL_TIP As Long = 4       ' D
Private Const COL_KEY As Long = 5       ' E (隐藏)
Private Const COL_TYPE As Long = 6      ' F (隐藏)
Private Const FIELD_START_ROW As Long = 5

' 静默 MsgBox：g_silent=True 时不弹窗
Private Sub Notify(msg As String, Optional opts As VbMsgBoxStyle = vbInformation)
    If Not g_silent Then MsgBox msg, opts
End Sub

' 静默确认：g_silent=True 时直接返回 vbYes
Private Function Confirm(msg As String, Optional opts As VbMsgBoxStyle = vbYesNo + vbQuestion, Optional title As String = "") As VbMsgBoxResult
    If g_silent Then
        Confirm = vbYes
    Else
        Confirm = MsgBox(msg, opts, title)
    End If
End Function

' 用于自动化测试：将弹窗与 Yes/No 切换为静默（默认 Yes）
Public Sub SetSilent(b As Boolean)
    g_silent = b
End Sub

' ------------------------------------------------------------
' 入口：清除当前选中行的输入项
Public Sub ClearCurrentInput()
    Dim ws As Worksheet
    Set ws = ActiveSheet
    If Not IsInputSheet(ws) Then
        Notify "请在 卡片输入 Sheet 上使用此按钮。", vbExclamation
        Exit Sub
    End If
    Dim r As Long
    r = ActiveCell.Row
    If r < FIELD_START_ROW Then
        Notify "请先选中要清除的字段所在行（第 " & FIELD_START_ROW & " 行起）。", vbExclamation
        Exit Sub
    End If
    If Trim$(CStr(ws.Cells(r, COL_KEY).Value)) = "" Then
        Notify "当前行不是字段输入行。", vbExclamation
        Exit Sub
    End If
    ws.Cells(r, COL_INPUT).ClearContents
    ws.Cells(r, COL_INPUT).Select
End Sub

' ------------------------------------------------------------
' 入口：清除当前 Sheet 所有输入项
Public Sub ClearAllInputs()
    Dim ws As Worksheet
    Set ws = ActiveSheet
    If Not IsInputSheet(ws) Then
        Notify "请在 卡片输入 Sheet 上使用此按钮。", vbExclamation
        Exit Sub
    End If
    If Confirm("确认清除 [" & ws.Name & "] 所有输入项？此操作不可撤销。", vbYesNo + vbQuestion, "确认清除") <> vbYes Then Exit Sub
    Dim lastRow As Long, r As Long
    lastRow = ws.Cells(ws.Rows.Count, COL_KEY).End(xlUp).Row
    Application.ScreenUpdating = False
    For r = FIELD_START_ROW To lastRow
        If Trim$(CStr(ws.Cells(r, COL_KEY).Value)) <> "" Then
            ws.Cells(r, COL_INPUT).ClearContents
        End If
    Next r
    Application.ScreenUpdating = True
    Notify "已清除全部输入。"
End Sub

' ------------------------------------------------------------
' 入口：导出当前 Sheet 为 MD 卡片
Public Sub ExportToMD()
    On Error GoTo ErrHandler
    Dim ws As Worksheet
    Set ws = ActiveSheet
    If Not IsInputSheet(ws) Then
        Notify "请在 卡片输入 Sheet 上使用此按钮。", vbExclamation
        Exit Sub
    End If

    Dim cardType As String, prefix As String
    cardType = SheetCardType(ws)
    prefix = SheetFilenamePrefix(ws)
    If cardType = "" Then
        Notify "无法识别 Sheet 类型。请使用 01/02/03 输入页。", vbCritical
        Exit Sub
    End If

    ' 收集 key->value 字典
    Dim dict As Object
    Set dict = CreateObject("Scripting.Dictionary")
    Dim typeDict As Object
    Set typeDict = CreateObject("Scripting.Dictionary")

    Dim lastRow As Long, r As Long
    lastRow = ws.Cells(ws.Rows.Count, COL_KEY).End(xlUp).Row
    For r = FIELD_START_ROW To lastRow
        Dim k As String, t As String, v As String
        k = Trim$(CStr(ws.Cells(r, COL_KEY).Value))
        t = Trim$(CStr(ws.Cells(r, COL_TYPE).Value))
        v = CStr(ws.Cells(r, COL_INPUT).Value)
        If k <> "" Then
            dict(k) = v
            typeDict(k) = t
        End If
    Next r

    ' 必填校验：id
    Dim idVal As String
    idVal = Trim$(CStr(dict("id")))
    If idVal = "" Then
        Notify "请先填写 id 字段（用作文件名）。", vbExclamation
        Exit Sub
    End If
    ' 校验 id 与本 Sheet 前缀匹配
    If LCase(Left$(idVal, Len(prefix))) <> LCase(prefix) Then
        If Confirm("id [" & idVal & "] 与本 Sheet 期望前缀 [" & prefix & "] 不匹配。是否仍然导出？") <> vbYes Then Exit Sub
    End If

    ' 取模板
    Dim tpl As String
    tpl = GetTemplate(cardType)
    If tpl = "" Then
        Notify "未找到 [" & cardType & "] 模板，请检查 _Templates Sheet。", vbCritical
        Exit Sub
    End If

    ' 渲染
    Dim md As String
    md = RenderTemplate(tpl, dict, typeDict)

    ' 文件名 = id + .md
    Dim outPath As String
    outPath = OUTPUT_DIR & "\" & idVal & ".md"

    ' 输出目录确保存在
    Dim fso As Object
    Set fso = CreateObject("Scripting.FileSystemObject")
    If Not fso.FolderExists(OUTPUT_DIR) Then
        Notify "输出目录不存在：" & vbCrLf & OUTPUT_DIR, vbCritical
        Exit Sub
    End If

    ' 同名提示
    If fso.FileExists(outPath) Then
        If Confirm("文件已存在：" & vbCrLf & outPath & vbCrLf & vbCrLf & "是否覆盖？", _
                   vbYesNo + vbQuestion, "覆盖确认") <> vbYes Then Exit Sub
    End If

    ' UTF-8 写入
    WriteUtf8 outPath, md
    Notify "导出成功：" & vbCrLf & outPath
    Exit Sub

ErrHandler:
    Notify "导出失败：" & Err.Number & " - " & Err.Description, vbCritical
End Sub

' ============================================================
' 辅助函数
' ============================================================

Private Function IsInputSheet(ws As Worksheet) As Boolean
    Dim n As String
    n = ws.Name
    IsInputSheet = (InStr(n, "卡片输入") > 0)
End Function

Private Function SheetCardType(ws As Worksheet) As String
    Dim n As String: n = ws.Name
    If InStr(n, "模块") > 0 Then SheetCardType = "module" : Exit Function
    If InStr(n, "平台") > 0 Then SheetCardType = "platform" : Exit Function
    If InStr(n, "方案") > 0 Then SheetCardType = "solution" : Exit Function
    SheetCardType = ""
End Function

Private Function SheetFilenamePrefix(ws As Worksheet) As String
    Select Case SheetCardType(ws)
        Case "module": SheetFilenamePrefix = "MOD-"
        Case "platform": SheetFilenamePrefix = "PLT-"
        Case "solution": SheetFilenamePrefix = "SOL-"
        Case Else: SheetFilenamePrefix = ""
    End Select
End Function

Private Function GetTemplate(cardType As String) As String
    Dim ws As Worksheet
    On Error Resume Next
    Set ws = ThisWorkbook.Worksheets("_Templates")
    On Error GoTo 0
    If ws Is Nothing Then GetTemplate = "" : Exit Function
    Dim r As Long
    For r = 1 To 10
        If LCase(Trim$(CStr(ws.Cells(r, 1).Value))) = LCase(cardType) Then
            GetTemplate = CStr(ws.Cells(r, 2).Value)
            Exit Function
        End If
    Next r
    GetTemplate = ""
End Function

' ------------------------------------------------------------
' 模板渲染：替换 {{key}} 与 {{key|fmt[:indent]}}
Private Function RenderTemplate(tpl As String, dict As Object, typeDict As Object) As String
    Dim s As String: s = tpl
    Dim startPos As Long, endPos As Long, token As String, theKey As String, fmt As String, indent As Long
    Dim theVal As String, replaced As String
    Dim safety As Long: safety = 0

    Do
        startPos = InStr(s, "{{")
        If startPos = 0 Then Exit Do
        endPos = InStr(startPos + 2, s, "}}")
        If endPos = 0 Then Exit Do
        token = Mid$(s, startPos + 2, endPos - startPos - 2)

        ' 解析 key|fmt:indent
        theKey = token : fmt = "" : indent = 0
        Dim pipePos As Long, colonPos As Long
        pipePos = InStr(token, "|")
        If pipePos > 0 Then
            theKey = Left$(token, pipePos - 1)
            fmt = Mid$(token, pipePos + 1)
            colonPos = InStr(fmt, ":")
            If colonPos > 0 Then
                indent = CLng(VBA.Val(Mid$(fmt, colonPos + 1)))
                fmt = Left$(fmt, colonPos - 1)
            End If
        End If

        If dict.Exists(theKey) Then
            theVal = CStr(dict(theKey))
        Else
            theVal = ""
        End If

        Select Case LCase(fmt)
            Case "inline"
                replaced = FmtInline(theVal)
            Case "block"
                replaced = FmtBlock(theVal, indent)
            Case "raw"
                replaced = FmtRaw(theVal, indent)
            Case "modules"
                replaced = FmtModules(theVal, indent)
            Case "workflow"
                replaced = FmtWorkflow(theVal, indent)
            Case "deployments"
                replaced = FmtDeployments(theVal, indent)
            Case "alternatives"
                replaced = FmtAlternatives(theVal, indent)
            Case "upgrades"
                replaced = FmtUpgrades(theVal, indent)
            Case Else
                replaced = FmtScalar(theVal, theKey, typeDict)
        End Select

        s = Left$(s, startPos - 1) & replaced & Mid$(s, endPos + 2)

        safety = safety + 1
        If safety > 5000 Then Exit Do
    Loop

    RenderTemplate = s
End Function

' ------------- formatter helpers -----------------

Private Function FmtScalar(theVal As String, theKey As String, typeDict As Object) As String
    Dim t As String
    If typeDict.Exists(theKey) Then t = LCase(CStr(typeDict(theKey))) Else t = "str"
    If Trim$(theVal) = "" Then
        Select Case t
            Case "int", "float": FmtScalar = "null"
            Case "bool": FmtScalar = "false"
            Case Else: FmtScalar = """"""
        End Select
        ' 字符串/日期空值返回空字符串
        If t = "str" Or t = "date" Then FmtScalar = ""
        Exit Function
    End If
    Select Case t
        Case "int"
            If IsNumeric(theVal) Then FmtScalar = CStr(CLng(VBA.Val(theVal))) Else FmtScalar = theVal
        Case "float"
            If IsNumeric(theVal) Then FmtScalar = CStr(CDbl(VBA.Val(theVal))) Else FmtScalar = theVal
        Case "bool"
            FmtScalar = LCase(Trim$(theVal))
            If FmtScalar <> "true" And FmtScalar <> "false" Then FmtScalar = "false"
        Case Else
            FmtScalar = theVal
    End Select
End Function

Private Function FmtInline(val As String) As String
    ' 逗号分隔 -> 直接清理后输出（模板里已带 [ ]）
    Dim parts() As String, i As Long, out As String
    If Trim$(val) = "" Then FmtInline = "" : Exit Function
    parts = Split(val, ",")
    For i = 0 To UBound(parts)
        If i > 0 Then out = out & ", "
        out = out & Trim$(parts(i))
    Next i
    FmtInline = out
End Function

Private Function FmtBlock(val As String, indent As Long) As String
    Dim lines() As String, i As Long, out As String, ind As String
    ind = String$(indent, " ")
    val = Replace(val, vbCrLf, vbLf)
    val = Replace(val, vbCr, vbLf)
    If Trim$(val) = "" Then
        FmtBlock = ind & "[]"
        Exit Function
    End If
    lines = Split(val, vbLf)
    For i = 0 To UBound(lines)
        Dim ln As String: ln = Trim$(lines(i))
        If ln <> "" Then
            If out <> "" Then out = out & vbCrLf
            out = out & ind & "- " & ln
        End If
    Next i
    If out = "" Then out = ind & "[]"
    FmtBlock = out
End Function

Private Function FmtRaw(val As String, indent As Long) As String
    Dim lines() As String, i As Long, out As String, ind As String
    ind = String$(indent, " ")
    val = Replace(val, vbCrLf, vbLf)
    val = Replace(val, vbCr, vbLf)
    If Trim$(val) = "" Then
        FmtRaw = ind & "[]"
        Exit Function
    End If
    lines = Split(val, vbLf)
    For i = 0 To UBound(lines)
        If Trim$(lines(i)) <> "" Then
            If out <> "" Then out = out & vbCrLf
            out = out & ind & lines(i)
        End If
    Next i
    If out = "" Then out = ind & "[]"
    FmtRaw = out
End Function

' modules: 每行 "MOD-ID | qty | role"
Private Function FmtModules(val As String, indent As Long) As String
    Dim lines() As String, i As Long, out As String, ind As String, ind2 As String
    ind = String$(indent, " ")
    ind2 = String$(indent + 2, " ")
    val = Replace(val, vbCrLf, vbLf) : val = Replace(val, vbCr, vbLf)
    If Trim$(val) = "" Then FmtModules = ind & "[]" : Exit Function
    lines = Split(val, vbLf)
    For i = 0 To UBound(lines)
        Dim ln As String: ln = Trim$(lines(i))
        If ln <> "" Then
            Dim parts() As String
            parts = Split(ln, "|")
            Dim id As String, qty As String, role As String
            id = "" : qty = "1" : role = ""
            If UBound(parts) >= 0 Then id = Trim$(parts(0))
            If UBound(parts) >= 1 Then qty = Trim$(parts(1))
            If UBound(parts) >= 2 Then role = Trim$(parts(2))
            If id <> "" Then
                If out <> "" Then out = out & vbCrLf
                out = out & ind & "- id: " & id & vbCrLf & _
                       ind2 & "quantity: " & qty
                If role <> "" Then out = out & vbCrLf & ind2 & "role: " & role
            End If
        End If
    Next i
    If out = "" Then out = ind & "[]"
    FmtModules = out
End Function

' workflow: "step | name | mod_id | duration_min | params(k=v,k=v) | note"
Private Function FmtWorkflow(val As String, indent As Long) As String
    Dim lines() As String, i As Long, out As String, ind As String, ind2 As String, ind3 As String
    ind = String$(indent, " ")
    ind2 = String$(indent + 2, " ")
    ind3 = String$(indent + 4, " ")
    val = Replace(val, vbCrLf, vbLf) : val = Replace(val, vbCr, vbLf)
    If Trim$(val) = "" Then FmtWorkflow = ind & "[]" : Exit Function
    lines = Split(val, vbLf)
    For i = 0 To UBound(lines)
        Dim ln As String: ln = Trim$(lines(i))
        If ln <> "" Then
            Dim parts() As String
            parts = Split(ln, "|")
            Dim stp As String, nm As String, modId As String, dur As String, params As String, note As String
            stp = "" : nm = "" : modId = "" : dur = "" : params = "" : note = ""
            If UBound(parts) >= 0 Then stp = Trim$(parts(0))
            If UBound(parts) >= 1 Then nm = Trim$(parts(1))
            If UBound(parts) >= 2 Then modId = Trim$(parts(2))
            If UBound(parts) >= 3 Then dur = Trim$(parts(3))
            If UBound(parts) >= 4 Then params = Trim$(parts(4))
            If UBound(parts) >= 5 Then note = Trim$(parts(5))
            If stp <> "" Then
                If out <> "" Then out = out & vbCrLf
                out = out & ind & "- step: " & stp
                If nm <> "" Then out = out & vbCrLf & ind2 & "name: " & nm
                If modId <> "" Then out = out & vbCrLf & ind2 & "module_id: " & modId
                If dur <> "" Then out = out & vbCrLf & ind2 & "duration_min: " & dur
                If params <> "" Then
                    out = out & vbCrLf & ind2 & "parameters:"
                    Dim ps() As String, j As Long
                    ps = Split(params, ",")
                    For j = 0 To UBound(ps)
                        Dim kv As String: kv = Trim$(ps(j))
                        If kv <> "" Then
                            Dim eqp As Long: eqp = InStr(kv, "=")
                            If eqp > 0 Then
                                out = out & vbCrLf & ind3 & Trim$(Left$(kv, eqp - 1)) & ": " & Trim$(Mid$(kv, eqp + 1))
                            End If
                        End If
                    Next j
                End If
                If note <> "" Then out = out & vbCrLf & ind2 & "note: " & note
            End If
        End If
    Next i
    If out = "" Then out = ind & "[]"
    FmtWorkflow = out
End Function

' deployments: "customer | location | date | sample_throughput | feedback | contact_available"
Private Function FmtDeployments(val As String, indent As Long) As String
    Dim lines() As String, i As Long, out As String, ind As String, ind2 As String
    ind = String$(indent, " ")
    ind2 = String$(indent + 2, " ")
    val = Replace(val, vbCrLf, vbLf) : val = Replace(val, vbCr, vbLf)
    If Trim$(val) = "" Then FmtDeployments = ind & "[]" : Exit Function
    lines = Split(val, vbLf)
    For i = 0 To UBound(lines)
        Dim ln As String: ln = Trim$(lines(i))
        If ln <> "" Then
            Dim parts() As String
            parts = Split(ln, "|")
            Dim c As String, loc As String, dt As String, thr As String, fb As String, ca As String
            c = "" : loc = "" : dt = "" : thr = "" : fb = "" : ca = "false"
            If UBound(parts) >= 0 Then c = Trim$(parts(0))
            If UBound(parts) >= 1 Then loc = Trim$(parts(1))
            If UBound(parts) >= 2 Then dt = Trim$(parts(2))
            If UBound(parts) >= 3 Then thr = Trim$(parts(3))
            If UBound(parts) >= 4 Then fb = Trim$(parts(4))
            If UBound(parts) >= 5 Then ca = LCase(Trim$(parts(5)))
            If ca <> "true" And ca <> "false" Then ca = "false"
            If c <> "" Then
                If out <> "" Then out = out & vbCrLf
                out = out & ind & "- customer: " & c
                If loc <> "" Then out = out & vbCrLf & ind2 & "location: " & loc
                If dt <> "" Then out = out & vbCrLf & ind2 & "date: " & dt
                If thr <> "" Then out = out & vbCrLf & ind2 & "sample_throughput: " & thr
                If fb <> "" Then out = out & vbCrLf & ind2 & "feedback: " & fb
                out = out & vbCrLf & ind2 & "contact_available: " & ca
            End If
        End If
    Next i
    If out = "" Then out = ind & "[]"
    FmtDeployments = out
End Function

' alternatives: "SOL-ID | differ_in"
Private Function FmtAlternatives(val As String, indent As Long) As String
    Dim lines() As String, i As Long, out As String, ind As String, ind2 As String
    ind = String$(indent, " ")
    ind2 = String$(indent + 2, " ")
    val = Replace(val, vbCrLf, vbLf) : val = Replace(val, vbCr, vbLf)
    If Trim$(val) = "" Then FmtAlternatives = ind & "[]" : Exit Function
    lines = Split(val, vbLf)
    For i = 0 To UBound(lines)
        Dim ln As String: ln = Trim$(lines(i))
        If ln <> "" Then
            Dim parts() As String
            parts = Split(ln, "|")
            Dim id As String, diff As String
            id = "" : diff = ""
            If UBound(parts) >= 0 Then id = Trim$(parts(0))
            If UBound(parts) >= 1 Then diff = Trim$(parts(1))
            If id <> "" Then
                If out <> "" Then out = out & vbCrLf
                out = out & ind & "- id: " & id
                If diff <> "" Then out = out & vbCrLf & ind2 & "differ_in: " & diff
            End If
        End If
    Next i
    If out = "" Then out = ind & "[]"
    FmtAlternatives = out
End Function

' upgrades: "SOL-ID | upgrade_path"
Private Function FmtUpgrades(val As String, indent As Long) As String
    Dim lines() As String, i As Long, out As String, ind As String, ind2 As String
    ind = String$(indent, " ")
    ind2 = String$(indent + 2, " ")
    val = Replace(val, vbCrLf, vbLf) : val = Replace(val, vbCr, vbLf)
    If Trim$(val) = "" Then FmtUpgrades = ind & "[]" : Exit Function
    lines = Split(val, vbLf)
    For i = 0 To UBound(lines)
        Dim ln As String: ln = Trim$(lines(i))
        If ln <> "" Then
            Dim parts() As String
            parts = Split(ln, "|")
            Dim id As String, up As String
            id = "" : up = ""
            If UBound(parts) >= 0 Then id = Trim$(parts(0))
            If UBound(parts) >= 1 Then up = Trim$(parts(1))
            If id <> "" Then
                If out <> "" Then out = out & vbCrLf
                out = out & ind & "- id: " & id
                If up <> "" Then out = out & vbCrLf & ind2 & "upgrade_path: " & up
            End If
        End If
    Next i
    If out = "" Then out = ind & "[]"
    FmtUpgrades = out
End Function

' UTF-8 写文件（带 BOM 以利 Windows 工具识别中文）
Private Sub WriteUtf8(path As String, content As String)
    Dim stm As Object
    Set stm = CreateObject("ADODB.Stream")
    stm.Type = 2 ' text
    stm.Charset = "utf-8"
    stm.Open
    stm.WriteText content
    stm.SaveToFile path, 2 ' overwrite
    stm.Close
End Sub
