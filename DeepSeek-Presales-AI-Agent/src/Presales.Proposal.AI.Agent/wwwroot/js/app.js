/* SmartLabOS 主数据维护 — 纯原生 JS 单页应用，由 /api/meta 元数据驱动 */
(() => {
  "use strict";

  const API = "";                 // 同源；开发跨域时改为 "http://localhost:5080"
  const COLPREF_KEY = "slos_colprefs";
  const state = {
    meta: null,
    entity: null,                 // 当前 EntityDef
    page: 1,
    pageSize: 20,
    total: 0,
    search: "",
    editingId: null,              // null = 新增
    lastItems: [],                // 当前页数据(用于切换列时本地重绘)
    colPrefs: {},                 // { entityKey: [列名...] } 每实体自定义可见列
    selectedIds: new Set(),       // 跨页保留的选中记录(供导出"指定记录")
    importPreview: null,          // { filePath, rows } 最近一次导入预览
  };
  let jsonEditors = {};           // 当前表单中的 JSON 折叠编辑器 (按字段名)

  const $ = (id) => document.getElementById(id);

  // ---------- 网络 ----------
  async function api(path, options) {
    const res = await fetch(API + path, {
      headers: { "Content-Type": "application/json" },
      ...options,
    });
    if (res.status === 204) return null;
    const text = await res.text();
    const data = text ? JSON.parse(text) : null;
    if (!res.ok) throw new Error((data && data.message) || `HTTP ${res.status}`);
    return data;
  }

  // ---------- 启动 ----------
  async function init() {
    try { state.colPrefs = JSON.parse(localStorage.getItem(COLPREF_KEY) || "{}"); }
    catch { state.colPrefs = {}; }
    checkHealth();
    try {
      state.meta = await api("/api/meta");
    } catch (e) {
      toast("无法加载元数据：" + e.message, "bad");
      return;
    }
    renderNav();
    bindEvents();
    if (state.meta.entities.length) selectEntity(state.meta.entities[0].key);
  }

  async function checkHealth() {
    const el = $("health");
    try {
      const h = await api("/api/meta/health");
      if (h.db === "connected") { el.textContent = "数据库：已连接"; el.className = "health ok"; }
      else { el.textContent = "数据库：状态未知"; el.className = "health"; }
    } catch (e) {
      el.textContent = "数据库：连接失败"; el.className = "health bad";
    }
  }

  // ---------- 导航 ----------
  function renderNav() {
    const nav = $("entityNav");
    nav.innerHTML = "";
    state.meta.entities.forEach((e) => {
      const b = document.createElement("button");
      b.className = "nav-item";
      b.dataset.key = e.key;
      b.textContent = e.label;
      b.addEventListener("click", () => selectEntity(e.key));
      nav.appendChild(b);
    });
  }

  function selectEntity(key) {
    state.entity = state.meta.entities.find((e) => e.key === key);
    state.page = 1;
    state.search = "";
    state.selectedIds = new Set();        // 切换实体清空选中
    $("searchBox").value = "";
    $("colMenu").hidden = true;
    document.querySelectorAll(".nav-item").forEach((n) =>
      n.classList.toggle("active", n.dataset.key === key));
    $("entityTitle").textContent = state.entity.label;
    buildColMenu();
    loadList();
  }

  // ---------- 可见列选择 ----------
  function defaultColNames(entity) {
    return entity.columns.filter((c) => c.inList).map((c) => c.name);
  }
  // 返回按定义顺序排列、当前应显示的列定义
  function chosenColumns() {
    const saved = state.colPrefs[state.entity.key];
    const names = (saved && saved.length) ? saved : defaultColNames(state.entity);
    const set = new Set(names);
    return state.entity.columns.filter((c) => set.has(c.name));
  }

  function buildColMenu() {
    const menu = $("colMenu");
    menu.innerHTML = "";
    const reset = document.createElement("button");
    reset.type = "button";
    reset.className = "col-reset";
    reset.textContent = "恢复默认列";
    reset.addEventListener("click", () => {
      delete state.colPrefs[state.entity.key];
      localStorage.setItem(COLPREF_KEY, JSON.stringify(state.colPrefs));
      buildColMenu();
      renderTable(state.lastItems);
    });
    menu.appendChild(reset);

    const chosen = new Set(chosenColumns().map((c) => c.name));
    state.entity.columns.forEach((c) => {
      const opt = document.createElement("label");
      opt.className = "col-opt";
      const cb = document.createElement("input");
      cb.type = "checkbox";
      cb.checked = chosen.has(c.name);
      cb.addEventListener("change", () => {
        const cur = new Set(chosenColumns().map((x) => x.name));
        cb.checked ? cur.add(c.name) : cur.delete(c.name);
        if (cur.size === 0) { cb.checked = true; return; }   // 至少保留一列
        state.colPrefs[state.entity.key] =
          state.entity.columns.filter((x) => cur.has(x.name)).map((x) => x.name);
        localStorage.setItem(COLPREF_KEY, JSON.stringify(state.colPrefs));
        renderTable(state.lastItems);
      });
      const span = document.createElement("span");
      span.textContent = " " + c.label + (c.inList ? "" : "（扩展）");
      opt.appendChild(cb);
      opt.appendChild(span);
      menu.appendChild(opt);
    });
  }

  // ---------- 列表 ----------
  async function loadList() {
    const e = state.entity;
    const q = new URLSearchParams({ page: state.page, pageSize: state.pageSize });
    if (state.search) q.set("search", state.search);
    let result;
    try {
      result = await api(`/api/${e.key}?${q}`);
    } catch (err) {
      toast("加载失败：" + err.message, "bad");
      return;
    }
    state.total = result.total;
    state.lastItems = result.items;
    renderTable(result.items);
    const pages = Math.max(1, Math.ceil(result.total / state.pageSize));
    $("pageInfo").textContent = `第 ${result.page} / ${pages} 页 · 共 ${result.total} 条`;
    $("prevPage").disabled = state.page <= 1;
    $("nextPage").disabled = state.page >= pages;
  }

  function renderTable(items) {
    items = items || [];
    const cols = chosenColumns();
    $("tableHead").innerHTML =
      `<th class="col-check"><input type="checkbox" id="checkAllPage" title="选择本页" /></th>` +
      cols.map((c) => `<th>${esc(c.label)}</th>`).join("") + "<th>操作</th>";

    const body = $("tableBody");
    body.innerHTML = "";
    $("emptyHint").hidden = items.length > 0;

    items.forEach((row) => {
      const id = row[idField()];
      const tr = document.createElement("tr");

      const checkTd = document.createElement("td");
      checkTd.className = "col-check";
      const cb = document.createElement("input");
      cb.type = "checkbox";
      cb.checked = state.selectedIds.has(String(id));
      cb.addEventListener("change", () => {
        if (cb.checked) state.selectedIds.add(String(id));
        else state.selectedIds.delete(String(id));
        updateSelCount();
        syncCheckAll();
      });
      checkTd.appendChild(cb);
      tr.appendChild(checkTd);

      cols.forEach((c) => {
        const t = cellText(row[c.name]);
        const td = document.createElement("td");
        td.title = t;
        td.textContent = t;
        tr.appendChild(td);
      });

      const td = document.createElement("td");
      td.className = "actions";
      td.innerHTML =
        `<button class="btn" data-act="edit">编辑</button>` +
        `<button class="btn btn-danger" data-act="del">删除</button>`;
      td.querySelector('[data-act="edit"]').addEventListener("click", () => openEdit(id));
      td.querySelector('[data-act="del"]').addEventListener("click", () => doDelete(id));
      tr.appendChild(td);
      body.appendChild(tr);
    });

    const checkAll = $("checkAllPage");
    if (checkAll) {
      checkAll.addEventListener("change", () => {
        pageIds().forEach((id) => {
          if (checkAll.checked) state.selectedIds.add(id);
          else state.selectedIds.delete(id);
        });
        renderTable(state.lastItems);
        updateSelCount();
      });
      syncCheckAll();
    }
    updateSelCount();
  }

  // 选中相关辅助
  function pageIds() {
    return (state.lastItems || []).map((r) => String(r[idField()]));
  }
  function updateSelCount() {
    const el = $("exportSelCount");
    if (el) el.textContent = state.selectedIds.size;
  }
  function syncCheckAll() {
    const ca = $("checkAllPage");
    if (!ca) return;
    const ids = pageIds();
    ca.checked = ids.length > 0 && ids.every((id) => state.selectedIds.has(id));
  }

  function cellText(v) {
    const s = v === null || v === undefined ? "" : String(v);
    return s.length > 160 ? s.slice(0, 160) + "…" : s;
  }

  function idField() {
    return state.entity.columns.find((c) => c.kind === "key").name;
  }

  // ---------- 表单 ----------
  function buildForm(record) {
    jsonEditors = {};
    const form = $("recordForm");
    form.innerHTML = "";
    state.entity.columns.forEach((c) => {
      if (c.kind === "readonly" && !record) return;          // 时间戳仅编辑时显示
      const val = record ? (record[c.name] ?? "") : "";
      const isKeyOnEdit = c.kind === "key" && record;         // 主键编辑时只读
      const readOnly = !c.editable || isKeyOnEdit;
      const reqMark = c.required ? ' <span class="req">*</span>' : "";

      const field = document.createElement("div");
      field.className = "field";

      // JSON 字段 → 折叠式树形编辑器
      if (c.kind === "json") {
        const label = document.createElement("label");
        label.innerHTML = `${esc(c.label)}（${esc(c.name)}）${reqMark}`;
        const mount = document.createElement("div");
        field.appendChild(label);
        field.appendChild(mount);
        form.appendChild(field);
        jsonEditors[c.name] = createJsonEditor(mount, c.name, String(val), readOnly);
        return;
      }

      let control;
      if (c.kind === "longtext") {
        control = `<textarea name="${c.name}" rows="6" class="code" ${readOnly ? "readonly" : ""}>${esc(val)}</textarea>`;
      } else {
        control = `<input name="${c.name}" value="${esc(val)}" ${readOnly ? "readonly" : ""} />`;
      }
      field.innerHTML = `<label>${esc(c.label)}（${esc(c.name)}）${reqMark}</label>${control}`;
      form.appendChild(field);
    });
  }

  function collectForm() {
    Object.values(jsonEditors).forEach((ed) => ed.sync());   // 树形 → 文本回写
    const form = $("recordForm");
    const out = {};
    state.entity.columns.forEach((c) => {
      if (c.kind === "readonly") return;
      const el = form.elements[c.name];
      if (!el) return;
      out[c.name] = el.value;
    });
    return out;
  }

  function openNew() {
    state.editingId = null;
    $("modalTitle").textContent = `新增 — ${state.entity.label}`;
    $("formError").textContent = "";
    buildForm(null);
    $("modalMask").hidden = false;
  }

  async function openEdit(id) {
    state.editingId = id;
    $("formError").textContent = "";
    let record;
    try { record = await api(`/api/${state.entity.key}/${encodeURIComponent(id)}`); }
    catch (e) { toast("读取失败：" + e.message, "bad"); return; }
    $("modalTitle").textContent = `编辑 — ${id}`;
    buildForm(record);
    $("modalMask").hidden = false;
  }

  function closeModal() { $("modalMask").hidden = true; }

  async function save() {
    const payload = collectForm();
    const e = state.entity;
    $("formError").textContent = "";
    try {
      if (state.editingId === null) {
        await api(`/api/${e.key}`, { method: "POST", body: JSON.stringify(payload) });
        toast("新增成功", "ok");
      } else {
        await api(`/api/${e.key}/${encodeURIComponent(state.editingId)}`,
          { method: "PUT", body: JSON.stringify(payload) });
        toast("保存成功", "ok");
      }
      closeModal();
      loadList();
    } catch (err) {
      $("formError").textContent = err.message;
    }
  }

  async function doDelete(id) {
    if (!confirm(`确认删除「${id}」？此操作不可恢复。`)) return;
    try {
      await api(`/api/${state.entity.key}/${encodeURIComponent(id)}`, { method: "DELETE" });
      toast("已删除", "ok");
      loadList();
    } catch (e) { toast("删除失败：" + e.message, "bad"); }
  }

  // =====================================================================
  //  JSON 折叠式树形编辑器
  //  - 树形视图：对象/数组节点可折叠展开，叶子值可编辑，可增删字段/项、重命名键
  //  - 源码视图：原始 JSON 文本编辑，带「美化」与实时校验
  //  - 名为 name 的隐藏 textarea 是表单取值来源；sync() 在保存前回写
  // =====================================================================
  function createJsonEditor(mount, name, initialText, readOnly) {
    mount.classList.add("json-editor");
    mount.innerHTML =
      `<div class="je-toolbar">
         <button type="button" class="je-tab" data-v="tree">树形</button>
         <button type="button" class="je-tab" data-v="raw">源码</button>
         <button type="button" class="je-btn" data-act="pretty">美化</button>
         <span class="je-status"></span>
       </div>
       <div class="je-tree"></div>
       <textarea class="code je-raw" name="${name}" spellcheck="false" ${readOnly ? "readonly" : ""}></textarea>`;
    const treeEl = mount.querySelector(".je-tree");
    const rawEl = mount.querySelector(".je-raw");
    const statusEl = mount.querySelector(".je-status");
    const tabTree = mount.querySelector('[data-v="tree"]');
    const tabRaw = mount.querySelector('[data-v="raw"]');
    let model = {};
    let mode = "tree";

    const setStatus = (ok, msg) => { statusEl.textContent = msg; statusEl.className = "je-status " + (ok ? "ok" : "bad"); };
    const serialize = () => JSON.stringify(model, null, 2);
    const tryParse = (txt) => {
      txt = (txt || "").trim();
      if (txt === "") return { ok: true, value: {} };
      try { return { ok: true, value: JSON.parse(txt) }; }
      catch (e) { return { ok: false, error: e.message }; }
    };

    function renderTree() {
      treeEl.innerHTML = "";
      treeEl.appendChild(buildNode(model, null, (nv) => { model = nv; }, 0, readOnly, renderTree));
    }
    function showTree() {
      renderTree();
      rawEl.value = serialize();
      treeEl.hidden = false; rawEl.hidden = true; mode = "tree";
      tabTree.classList.add("active"); tabRaw.classList.remove("active");
      setStatus(true, "✓ 合法 JSON");
    }
    function showRaw() {
      rawEl.value = serialize();
      treeEl.hidden = true; rawEl.hidden = false; mode = "raw";
      tabRaw.classList.add("active"); tabTree.classList.remove("active");
    }

    // 初始化
    const init0 = tryParse(initialText);
    if (init0.ok) { model = init0.value; showTree(); }
    else {                                   // 非法 JSON：进源码模式保留原文
      rawEl.value = initialText || "";
      treeEl.hidden = true; rawEl.hidden = false; mode = "raw";
      tabRaw.classList.add("active");
      setStatus(false, "✗ " + init0.error);
    }

    rawEl.addEventListener("input", () => {
      const r = tryParse(rawEl.value);
      r.ok ? (model = r.value, setStatus(true, "✓ 合法 JSON")) : setStatus(false, "✗ " + r.error);
    });
    tabTree.addEventListener("click", () => {
      if (mode === "tree") return;
      const r = tryParse(rawEl.value);
      if (!r.ok) { setStatus(false, "✗ 无法切换树形：" + r.error); return; }
      model = r.value; showTree();
    });
    tabRaw.addEventListener("click", () => { if (mode !== "raw") showRaw(); });
    mount.querySelector('[data-act="pretty"]').addEventListener("click", () => {
      const r = tryParse(rawEl.value);
      if (!r.ok) { setStatus(false, "✗ " + r.error); return; }
      model = r.value; rawEl.value = serialize(); setStatus(true, "✓ 已格式化");
      if (mode === "tree") renderTree();
    });

    return { sync() { if (mode === "tree") rawEl.value = serialize(); } };
  }

  // 递归构建一个 JSON 节点的 DOM
  function buildNode(value, key, setSelf, depth, readOnly, rerenderRoot) {
    const wrap = document.createElement("div");
    wrap.className = "jn";
    const isArr = Array.isArray(value);
    const isObj = value && typeof value === "object" && !isArr;
    const container = isArr || isObj;

    const row = document.createElement("div");
    row.className = "jn-row";

    const addKey = (k) => {
      if (k === null) return;
      const ks = document.createElement("span");
      ks.className = "jn-key";
      ks.textContent = k;
      row.appendChild(ks);
      row.appendChild(Object.assign(document.createElement("span"), { className: "jn-colon", textContent: "：" }));
      return ks;
    };

    if (container) {
      const toggle = document.createElement("span");
      toggle.className = "jn-toggle";
      const children = document.createElement("div");
      children.className = "jn-children";
      const collapsed = depth >= 1;                       // 折叠默认：深层收起
      toggle.textContent = collapsed ? "▸" : "▾";
      children.hidden = collapsed;
      toggle.addEventListener("click", () => {
        children.hidden = !children.hidden;
        toggle.textContent = children.hidden ? "▸" : "▾";
      });
      row.appendChild(toggle);
      addKey(key);

      const meta = document.createElement("span");
      meta.className = "jn-meta";
      const setMeta = () => { meta.textContent = isArr ? `[ ${value.length} 项 ]` : `{ ${Object.keys(value).length} 字段 }`; };
      setMeta();
      row.appendChild(meta);

      if (!readOnly) {
        const add = document.createElement("button");
        add.type = "button"; add.className = "jn-add";
        add.textContent = isArr ? "+项" : "+字段";
        add.addEventListener("click", () => {
          if (isArr) value.push("");
          else { let i = 1, nk = "新字段"; while (nk in value) nk = "新字段" + (i++); value[nk] = ""; }
          renderChildren(); setMeta();
        });
        row.appendChild(add);
      }
      wrap.appendChild(row);

      function renderChildren() {
        children.innerHTML = "";
        const keys = isArr ? value.map((_, i) => i) : Object.keys(value);
        keys.forEach((k) => {
          const childEl = buildNode(value[k], isArr ? `[${k}]` : k,
            (nv) => { value[k] = nv; }, depth + 1, readOnly, rerenderRoot);
          if (!readOnly) {
            const childRow = childEl.querySelector(".jn-row");
            const del = document.createElement("button");
            del.type = "button"; del.className = "jn-del"; del.textContent = "✕"; del.title = "删除";
            del.addEventListener("click", () => {
              if (isArr) value.splice(k, 1); else delete value[k];
              renderChildren(); setMeta();
            });
            childRow.appendChild(del);
            if (!isArr) {                                   // 对象键可重命名
              const ks = childRow.querySelector(".jn-key");
              if (ks) {
                ks.contentEditable = "true";
                ks.classList.add("editable");
                ks.addEventListener("keydown", (e) => { if (e.key === "Enter") { e.preventDefault(); ks.blur(); } });
                ks.addEventListener("blur", () => {
                  const nk = ks.textContent.trim();
                  if (nk && nk !== k && !(nk in value)) {
                    const ordered = {};
                    Object.keys(value).forEach((kk) => { ordered[kk === k ? nk : kk] = value[kk]; });
                    Object.keys(value).forEach((kk) => delete value[kk]);
                    Object.assign(value, ordered);
                    renderChildren();
                  } else { ks.textContent = k; }
                });
              }
            }
          }
          children.appendChild(childEl);
        });
      }
      renderChildren();
      wrap.appendChild(children);
    } else {
      addKey(key);
      row.appendChild(leafControl(value, setSelf, readOnly));
      wrap.appendChild(row);
    }
    return wrap;
  }

  function leafControl(value, setSelf, readOnly) {
    const t = value === null ? "null" : typeof value;
    if (t === "boolean") {
      const sel = document.createElement("select");
      sel.className = "jn-val jn-bool";
      sel.innerHTML = '<option value="true">true</option><option value="false">false</option>';
      sel.value = String(value);
      sel.disabled = readOnly;
      sel.addEventListener("change", () => setSelf(sel.value === "true"));
      return sel;
    }
    const input = document.createElement("input");
    input.className = "jn-val jn-" + t;
    input.value = value === null ? "" : String(value);
    if (t === "null") input.placeholder = "null";
    input.readOnly = readOnly;
    input.addEventListener("input", () => {
      const v = input.value;
      if (t === "number") {
        const n = Number(v);
        setSelf(v.trim() === "" ? null : (isNaN(n) ? v : n));
      } else if (t === "null") {
        setSelf(v === "" ? null : v);
      } else {
        setSelf(v);
      }
    });
    return input;
  }

  // =====================================================================
  //  导出 YAML-MD（服务器端目录；全部 / 仅选中）
  // =====================================================================
  function openExport() {
    $("exportError").textContent = "";
    $("exportResult").innerHTML = "";
    updateSelCount();
    const hasSel = state.selectedIds.size > 0;
    document.querySelector('input[name="exportScope"][value="selected"]').checked = hasSel;
    document.querySelector('input[name="exportScope"][value="all"]').checked = !hasSel;
    $("exportMask").hidden = false;
  }

  async function runExport() {
    const dir = $("exportDir").value.trim();
    const scope = document.querySelector('input[name="exportScope"]:checked').value;
    const overwrite = $("exportOverwrite").checked;
    $("exportError").textContent = "";
    if (!dir) { $("exportError").textContent = "请填写导出目录"; return; }
    const ids = Array.from(state.selectedIds);
    if (scope === "selected" && ids.length === 0) {
      $("exportError").textContent = "未选中任何记录"; return;
    }
    try {
      const r = await api(`/api/${state.entity.key}/export`, {
        method: "POST",
        body: JSON.stringify({ directory: dir, scope, ids, overwrite }),
      });
      const errs = r.files.filter((f) => f.status === "error").length;
      $("exportResult").innerHTML =
        `<div class="ok-line">已导出 ${r.written} 个文件` +
        (r.skipped ? `，跳过 ${r.skipped} 个同名` : "") +
        (errs ? `，失败 ${errs} 个` : "") + `</div>` +
        `<div class="path-line">目录：${esc(r.directory)}</div>`;
      toast("导出完成", "ok");
    } catch (e) { $("exportError").textContent = e.message; }
  }

  // =====================================================================
  //  导入 Excel（预览 → 选择覆盖 → 确认导入）
  // =====================================================================
  function openImport() {
    $("importError").textContent = "";
    $("importSummary").innerHTML = "";
    $("importPreviewWrap").hidden = true;
    $("importPreviewBody").innerHTML = "";
    $("importOverwriteAllWrap").hidden = true;
    $("importCommitBtn").disabled = true;
    state.importPreview = null;
    $("importTitle").textContent = `导入 Excel — ${state.entity.label}`;
    $("importMask").hidden = false;
  }

  async function runImportPreview() {
    const path = $("importPath").value.trim();
    $("importError").textContent = "";
    if (!path) { $("importError").textContent = "请填写 Excel 文件路径"; return; }
    let r;
    try {
      r = await api(`/api/${state.entity.key}/import/preview`, {
        method: "POST", body: JSON.stringify({ filePath: path }),
      });
    } catch (e) { $("importError").textContent = e.message; return; }
    state.importPreview = { filePath: path };
    renderImportPreview(r);
  }

  function renderImportPreview(r) {
    $("importSummary").innerHTML =
      `<div>共 ${r.sheetCount} 张数据工作表 · 新增 ${r.newCount} · 冲突 ${r.conflictCount} · 非法 ${r.invalidCount}</div>`;

    const body = $("importPreviewBody");
    body.innerHTML = "";
    r.rows.forEach((row) => {
      const tr = document.createElement("tr");
      tr.className = "imp-" + row.status;
      const statusText = row.status === "new" ? "新增"
        : row.status === "conflict" ? "已存在(冲突)" : "非法";
      const ow = row.status === "conflict"
        ? `<input type="checkbox" class="imp-ow" data-id="${esc(row.id || "")}" />` : "";
      tr.innerHTML =
        `<td title="${esc(row.sheet || "")}">${esc(row.sheet || "")}</td><td>${esc(row.id || "")}</td>` +
        `<td>${statusText}</td><td>${ow}</td><td>${esc(row.message || "")}</td>`;
      body.appendChild(tr);
    });

    $("importPreviewWrap").hidden = false;
    $("importOverwriteAllWrap").hidden = r.conflictCount === 0;
    $("importOverwriteAll").checked = false;
    $("importCommitBtn").disabled = (r.newCount + r.conflictCount) === 0;
  }

  async function runImportCommit() {
    const pv = state.importPreview;
    if (!pv) return;
    const overwriteIds = Array.from(document.querySelectorAll(".imp-ow"))
      .filter((c) => c.checked).map((c) => c.dataset.id);
    $("importError").textContent = "";
    let r;
    try {
      r = await api(`/api/${state.entity.key}/import/commit`, {
        method: "POST", body: JSON.stringify({ filePath: pv.filePath, overwriteIds }),
      });
    } catch (e) { $("importError").textContent = e.message; return; }
    const errLines = (r.errors && r.errors.length)
      ? `<div class="path-line">${r.errors.slice(0, 8).map((x) =>
          esc(`第${x.row}行 ${x.id || ""}：${x.message}`)).join("<br>")}</div>` : "";
    $("importSummary").innerHTML =
      `<div class="ok-line">导入完成：新增 ${r.created} · 覆盖 ${r.updated} · 跳过 ${r.skipped} · 失败 ${r.invalid}</div>` +
      errLines;
    $("importPreviewWrap").hidden = true;
    $("importCommitBtn").disabled = true;
    state.selectedIds = new Set();
    toast("导入完成", "ok");
    loadList();
  }

  // 本机原生选择对话框（由后端在服务器/本机桌面弹出）
  async function pickPath(kind) {
    try {
      const r = await api(`/api/picker/${kind}`, { method: "POST", body: "{}" });
      return r && r.path ? r.path : null;
    } catch (e) {
      toast("无法打开选择对话框：" + e.message, "bad");
      return null;
    }
  }

  async function browseExportDir() {
    const p = await pickPath("folder");
    if (p) { $("exportDir").value = p; $("exportError").textContent = ""; }
  }

  async function browseImportFile() {
    const p = await pickPath("file");
    if (p) {
      $("importPath").value = p;
      $("importError").textContent = "";
      runImportPreview();             // 选定文件后自动预览
    }
  }

  // ---------- 事件绑定 ----------
  function bindEvents() {
    $("newBtn").addEventListener("click", openNew);
    $("searchBtn").addEventListener("click", () => { state.search = $("searchBox").value.trim(); state.page = 1; state.selectedIds = new Set(); loadList(); });
    $("searchBox").addEventListener("keydown", (e) => { if (e.key === "Enter") $("searchBtn").click(); });
    $("prevPage").addEventListener("click", () => { if (state.page > 1) { state.page--; loadList(); } });
    $("nextPage").addEventListener("click", () => { state.page++; loadList(); });
    $("modalClose").addEventListener("click", closeModal);
    $("cancelBtn").addEventListener("click", closeModal);
    $("saveBtn").addEventListener("click", save);
    $("modalMask").addEventListener("click", (e) => { if (e.target === $("modalMask")) closeModal(); });

    // 导出
    $("exportBtn").addEventListener("click", openExport);
    $("exportClose").addEventListener("click", () => { $("exportMask").hidden = true; });
    $("exportCancel").addEventListener("click", () => { $("exportMask").hidden = true; });
    $("exportRun").addEventListener("click", runExport);
    $("exportBrowse").addEventListener("click", browseExportDir);
    $("exportMask").addEventListener("click", (e) => { if (e.target === $("exportMask")) $("exportMask").hidden = true; });

    // 导入
    $("importBtn").addEventListener("click", openImport);
    $("importClose").addEventListener("click", () => { $("importMask").hidden = true; });
    $("importCancel").addEventListener("click", () => { $("importMask").hidden = true; });
    $("importBrowse").addEventListener("click", browseImportFile);
    $("importPreviewBtn").addEventListener("click", runImportPreview);
    $("importCommitBtn").addEventListener("click", runImportCommit);
    $("importOverwriteAll").addEventListener("change", () => {
      document.querySelectorAll(".imp-ow").forEach((c) => { c.checked = $("importOverwriteAll").checked; });
    });
    $("importMask").addEventListener("click", (e) => { if (e.target === $("importMask")) $("importMask").hidden = true; });
    // 列选择下拉：点按钮切换；点选项区域内不关闭；点外部关闭
    $("colBtn").addEventListener("click", () => {
      $("colMenu").hidden = !$("colMenu").hidden;
    });
    document.addEventListener("click", (e) => {
      if (e.target.closest(".col-chooser")) return;   // 在选择器内(按钮或菜单)：保持
      $("colMenu").hidden = true;                       // 其它任意处：关闭
    });
  }

  // ---------- 工具 ----------
  function esc(v) {
    if (v === null || v === undefined) return "";
    return String(v)
      .replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  let toastTimer;
  function toast(msg, kind) {
    const t = $("toast");
    t.textContent = msg;
    t.className = "toast" + (kind ? " " + kind : "");
    t.hidden = false;
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => (t.hidden = true), 3000);
  }

  document.addEventListener("DOMContentLoaded", init);
})();
