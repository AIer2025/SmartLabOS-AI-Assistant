# SmartLabOS 主数据维护 — 缺陷修复与功能增强总结

> 生成时间：2026-06-11
> 项目目录：`C:\TestClaude\SmartLabOS-AI-Assistant\DataMaintenance\`

两个功能都已加上并在真实浏览器里验证通过。更重要的是——在测试中发现了最初那个"空白对话框"的真正根因，之前所说的"浏览器缓存"是错误的，特此更正。

## 一、真正的根因（已修复）

`.modal-mask { display: flex }` 这条样式会**覆盖 `hidden` 属性的 `display:none`**（作者样式优先级高于浏览器默认样式）。结果是那个半透明遮罩层 + 对话框**一进页面就永远盖在最上层**：标题是静态默认的"编辑"、表单是空的（因为 `buildForm` 还没被触发）、还挡住了整个列表的点击。点"保存"就触发新增 → 后端返回"缺少必填主键字段: id"。这完全对上了"为什么一开始是这个界面"的疑问。

修复只需一行 CSS（`src/.../wwwroot/css/style.css`）：

```css
.modal-mask[hidden] { display: none; }
```

用真实鼠标点击 + 计算后可见性（不再依赖会误导人的 `hidden` 属性）复测：**首屏不再有遮罩**、编辑弹窗正常带出 15 个字段、新增带出 13 个字段，全程 0 JS 错误。（之前加的 no-cache 头和 favicon 是无害的小改进，保留。）

## 二、新增功能 1：列表自定义列

工具栏新增 **「列 ▾」** 按钮 → 勾选任意字段加入列表（标"扩展"的是默认隐藏列，如 `物料号` / `版本` / `data_json` 等）。选择**按实体分别记忆在浏览器 localStorage**，刷新后保留，可一键「恢复默认列」。实测：勾选 `版本` 后表头从 8 列变 9 列，点列表外自动收起菜单。

## 三、新增功能 2：data_json 折叠式树形编辑器

`结构化数据(JSON)` 字段从纯文本框升级为编辑器（界面参见 `docs/screenshot-json-tree-editor.png`）：

- **树形 / 源码** 双视图切换，**美化** 一键格式化，右上角**实时 JSON 校验**（✓ 合法 / ✗ 错误并提示原因）；
- 树形视图：对象/数组节点**可折叠展开**（深层默认收起），**叶子值可直接编辑**（数字/布尔/字符串/null 类型感知），每个节点可**删除(✕)**、容器可**新增字段/项**、对象**键名可双击重命名**；
- 保存时自动把树形改动回写、再做合法性校验，非法 JSON 拒绝保存。实测：在树里把某个叶子改成 `AUTOTEST`，切到源码视图能看到该改动已同步。

## 四、真实浏览器验证结果（真实鼠标点击 + 计算可见性）

| 检查项 | 结果 |
|--------|------|
| 首屏对话框隐藏（原始缺陷） | ✅ 不可见 |
| 「列」按钮打开列菜单 | ✅ 可见 |
| 勾选"版本"新增一列（8→9） | ✅ |
| 点击外部关闭菜单 | ✅ |
| 编辑弹窗带出全部 15 字段 | ✅ |
| data_json 渲染为 77 节点折叠树 | ✅ |
| 树形叶子编辑同步到源码 JSON | ✅ |
| 新增表单显示 13 字段 | ✅ |
| JS 错误 | ✅ 无 |

## 五、涉及改动的文件

- `src/SmartLabOS.DataMaintenance.Api/wwwroot/css/style.css` — `.modal-mask[hidden]` 修复 + 列菜单/JSON 树形编辑器样式
- `src/SmartLabOS.DataMaintenance.Api/wwwroot/js/app.js` — 列选择逻辑 + JSON 折叠编辑器 + 健壮的下拉开关
- `src/SmartLabOS.DataMaintenance.Api/wwwroot/index.html` — 「列 ▾」按钮 + favicon 占位
- `src/SmartLabOS.DataMaintenance.Api/Program.cs` — 静态资源 no-cache 头（预防旧缓存）
- `docs/screenshot-json-tree-editor.png` — 折叠式 JSON 编辑器界面截图

## 六、生效步骤

这些都是前端(JS/CSS/HTML) + 一处 `Program.cs` 改动。当前正在跑的服务器是旧的：

1. 终端按 **Ctrl+C** 停掉当前 `dotnet run`；
2. `dotnet run` 重新启动（C# 改动已 `dotnet build` 通过，0 错误）；
3. 浏览器 **Ctrl+Shift+R** 强制刷新。

刷新后：首屏直接是干净的模块列表（无空白弹窗），右上有「列 ▾」，点任意行"编辑"即可看到带折叠 JSON 编辑器的完整表单。
