/* SmartLabOS 售前方案自动生成 — 原生 JS，配合 /api/presales/* 接口 */
(() => {
  "use strict";

  const API = "";
  const $ = (id) => document.getElementById(id);

  const state = {
    initialized: false,
    options: null,        // { protocols, processScope, loadingMethods, softwareTypes, limits }
    projects: [],
    current: null,        // 当前选中项目(完整 DTO)
    pollTimer: null,
  };

  // ---------- 视图切换 ----------
  function bindTabs() {
    document.querySelectorAll(".mode-tab").forEach((b) => {
      b.addEventListener("click", () => switchView(b.dataset.view));
    });
  }

  function switchView(view) {
    document.querySelectorAll(".mode-tab").forEach((b) =>
      b.classList.toggle("active", b.dataset.view === view));
    $("viewData").hidden = view !== "data";
    $("viewPresales").hidden = view !== "presales";
    if (view === "presales") ensureInit();
  }

  async function ensureInit() {
    if (state.initialized) return;
    state.initialized = true;
    bindPresalesEvents();
    try {
      state.options = await api("/api/presales/options");
    } catch (e) {
      toast("无法加载选项：" + e.message, "bad");
      state.initialized = false;
      return;
    }
    await loadProjects();
  }

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

  // ---------- 项目列表 ----------
  async function loadProjects() {
    let r;
    try { r = await api("/api/presales/projects"); }
    catch (e) { toast("加载项目失败：" + e.message, "bad"); return; }
    state.projects = r.items || [];
    renderProjectList();
  }

  function renderProjectList() {
    const box = $("psProjectList");
    box.innerHTML = "";
    if (state.projects.length === 0) {
      box.innerHTML = `<p class="hint" style="padding:10px">暂无项目，点击「+ 新建项目」。</p>`;
      return;
    }
    state.projects.forEach((p) => {
      const b = document.createElement("button");
      b.className = "ps-project-item" + (state.current && state.current.id === p.id ? " active" : "");
      b.innerHTML =
        `<span class="ps-pi-name">${esc(p.projectName)}</span>` +
        `<span class="ps-pi-status ps-st-${esc(p.genStatus)}">${statusText(p.genStatus)}</span>`;
      b.addEventListener("click", () => selectProject(p.id));
      box.appendChild(b);
    });
  }

  // ---------- 选中并渲染表单 ----------
  async function selectProject(id) {
    stopPoll();
    let p;
    try { p = await api(`/api/presales/projects/${id}`); }
    catch (e) { toast("读取项目失败：" + e.message, "bad"); return; }
    state.current = p;
    renderProjectList();
    renderForm(p);
    refreshStatus(true);
  }

  function renderForm(p) {
    $("psEmpty").hidden = true;
    $("psForm").hidden = false;
    $("psTitle").textContent = p.projectName;
    $("psDirLine").textContent = "输出目录：" + (p.projectDir || "");
    setStatusBadge($("psStatusBadge"), p.genStatus);

    // 现状
    const ta = $("psCurrentStatus");
    ta.value = p.currentStatus || "";
    $("psStatusCount").textContent = ta.value.length;

    // 流程标准
    renderProtocols(p.protocols || []);

    // 挑战 / 期望
    renderItemList("psChallenges", p.challenges || [], "挑战");
    renderItemList("psExpectations", p.expectations || [], "期望");

    // 流程范围 / 上下料 / 软件
    renderChecks("psProcessScope", state.options.processScope, p.processScope || []);
    renderRadios("psLoading", "psLoading", state.options.loadingMethods, p.loadingMethod);
    renderRadios("psSoftware", "psSoftware", state.options.softwareTypes, p.softwareType);

    $("psGenPanel").hidden = false;
  }

  function renderProtocols(selected) {
    const box = $("psProtocols");
    box.innerHTML = "";
    const set = new Set(selected);
    (state.options.protocols || []).forEach((name) => {
      const lab = document.createElement("label");
      lab.className = "ps-check";
      const cb = document.createElement("input");
      cb.type = "checkbox"; cb.value = name; cb.checked = set.has(name);
      cb.addEventListener("change", updateProtoCount);
      lab.appendChild(cb);
      lab.appendChild(Object.assign(document.createElement("span"), { textContent: " " + name }));
      box.appendChild(lab);
    });
    updateProtoCount();
  }

  function updateProtoCount() {
    $("psProtoCount").textContent =
      document.querySelectorAll("#psProtocols input:checked").length;
  }

  function renderChecks(containerId, options, selected) {
    const box = $(containerId);
    box.innerHTML = "";
    const set = new Set(selected);
    options.forEach((opt) => {
      const lab = document.createElement("label");
      lab.className = "ps-check";
      const cb = document.createElement("input");
      cb.type = "checkbox"; cb.value = opt; cb.checked = set.has(opt);
      lab.appendChild(cb);
      lab.appendChild(Object.assign(document.createElement("span"), { textContent: " " + opt }));
      box.appendChild(lab);
    });
  }

  function renderRadios(containerId, groupName, options, selected) {
    const box = $(containerId);
    box.innerHTML = "";
    options.forEach((opt) => {
      const lab = document.createElement("label");
      lab.className = "ps-radio";
      const rb = document.createElement("input");
      rb.type = "radio"; rb.name = groupName; rb.value = opt; rb.checked = opt === selected;
      lab.appendChild(rb);
      lab.appendChild(Object.assign(document.createElement("span"), { textContent: " " + opt }));
      box.appendChild(lab);
    });
  }

  // 动态条目列表（挑战/期望）
  function renderItemList(containerId, values, label) {
    const box = $(containerId);
    box.innerHTML = "";
    const vals = values.length ? values : [""];
    vals.forEach((v) => addItemRow(box, v, label));
  }

  function addItemRow(box, value, label) {
    const MAX = (state.options.limits && state.options.limits.maxItems) || 20;
    if (box.children.length >= MAX) {
      toast(`${label}最多 ${MAX} 条`, "bad");
      return;
    }
    const row = document.createElement("div");
    row.className = "ps-item-row";
    const idx = box.children.length + 1;
    const ta = document.createElement("textarea");
    ta.className = "ps-item-input";
    ta.rows = 2; ta.maxLength = 200;
    ta.placeholder = `${label}-${idx}（≤200字）`;
    ta.value = value || "";
    const del = document.createElement("button");
    del.type = "button"; del.className = "btn btn-sm btn-danger"; del.textContent = "✕";
    del.addEventListener("click", () => { row.remove(); renumber(box, label); });
    row.appendChild(ta);
    row.appendChild(del);
    box.appendChild(row);
  }

  function renumber(box, label) {
    Array.from(box.querySelectorAll(".ps-item-input")).forEach((ta, i) => {
      ta.placeholder = `${label}-${i + 1}（≤200字）`;
    });
  }

  function collectItems(containerId) {
    return Array.from(document.querySelectorAll(`#${containerId} .ps-item-input`))
      .map((ta) => ta.value.trim()).filter((s) => s.length > 0);
  }

  function collectChecks(containerId) {
    return Array.from(document.querySelectorAll(`#${containerId} input:checked`)).map((c) => c.value);
  }

  function collectRadio(groupName) {
    const r = document.querySelector(`input[name="${groupName}"]:checked`);
    return r ? r.value : null;
  }

  // ---------- 保存 ----------
  async function save() {
    if (!state.current) return;
    const payload = {
      currentStatus: $("psCurrentStatus").value.trim(),
      protocols: collectChecks("psProtocols"),
      challenges: collectItems("psChallenges"),
      expectations: collectItems("psExpectations"),
      processScope: collectChecks("psProcessScope"),
      loadingMethod: collectRadio("psLoading"),
      softwareType: collectRadio("psSoftware"),
    };
    try {
      const updated = await api(`/api/presales/projects/${state.current.id}`,
        { method: "PUT", body: JSON.stringify(payload) });
      state.current = updated;
      toast("已保存需求信息", "ok");
      return true;
    } catch (e) {
      toast("保存失败：" + e.message, "bad");
      return false;
    }
  }

  // ---------- 新建项目 ----------
  function openNew() {
    $("psNewName").value = "";
    $("psNewError").textContent = "";
    $("psNewMask").hidden = false;
    $("psNewName").focus();
  }
  async function createProject() {
    const name = $("psNewName").value.trim();
    $("psNewError").textContent = "";
    if (!name) { $("psNewError").textContent = "请填写项目名称"; return; }
    let created;
    try {
      created = await api("/api/presales/projects",
        { method: "POST", body: JSON.stringify({ projectName: name }) });
    } catch (e) { $("psNewError").textContent = e.message; return; }
    $("psNewMask").hidden = true;
    toast("项目已创建", "ok");
    await loadProjects();
    selectProject(created.id);
  }

  // ---------- 删除 ----------
  async function deleteProject() {
    if (!state.current) return;
    if (!confirm(`确认删除项目「${state.current.projectName}」的需求记录？\n（projects/ 下已生成的提案文件不会被删除）`)) return;
    try {
      await api(`/api/presales/projects/${state.current.id}`, { method: "DELETE" });
      toast("已删除", "ok");
      state.current = null;
      $("psForm").hidden = true;
      $("psEmpty").hidden = false;
      await loadProjects();
    } catch (e) { toast("删除失败：" + e.message, "bad"); }
  }

  // ---------- 指令预览 ----------
  async function preview() {
    if (!state.current) return;
    if (!(await save())) return;            // 先保存，预览反映最新需求
    try {
      const r = await api(`/api/presales/projects/${state.current.id}/command-preview`);
      $("psPreviewText").textContent = r.command || "";
      $("psPreviewMask").hidden = false;
    } catch (e) { toast("预览失败：" + e.message, "bad"); }
  }

  // ---------- 方案生成 ----------
  async function generate() {
    if (!state.current) return;
    if (!(await save())) return;            // 生成前先落库
    if (collectChecks("psProtocols").length === 0) {
      toast("请至少选择一个流程标准", "bad"); return;
    }
    if (!confirm("将调用本机 Claude Code 在项目目录下自动生成提案文件：\n先生成解决方案 HTML 与 URS 文档，再据此生成 WORD(.docx) 提案。\n两阶段过程可能需要较长时间。是否开始？")) return;
    try {
      await api(`/api/presales/projects/${state.current.id}/generate`, { method: "POST", body: "{}" });
      toast("已开始生成方案", "ok");
      setStatusBadge($("psGenStatus"), "running");
      startPoll();
    } catch (e) { toast("启动生成失败：" + e.message, "bad"); }
  }

  function startPoll() {
    stopPoll();
    state.pollTimer = setInterval(() => refreshStatus(false), 3000);
  }
  function stopPoll() {
    if (state.pollTimer) { clearInterval(state.pollTimer); state.pollTimer = null; }
  }

  async function refreshStatus(silent) {
    if (!state.current) return;
    let s;
    try { s = await api(`/api/presales/projects/${state.current.id}/generate/status`); }
    catch (e) { if (!silent) toast("状态获取失败：" + e.message, "bad"); return; }

    setStatusBadge($("psGenStatus"), s.status);
    setStatusBadge($("psStatusBadge"), s.status);
    renderFiles(s.outputFiles || []);
    if (s.log != null) $("psLog").textContent = s.log;

    if (s.running) {
      if (!state.pollTimer) startPoll();
    } else {
      stopPoll();
      // 同步列表中的状态徽标
      const item = state.projects.find((x) => x.id === state.current.id);
      if (item) { item.genStatus = s.status; renderProjectList(); }
      if (s.status === "succeeded" && !silent) toast("方案生成完成", "ok");
      if (s.status === "failed" && !silent) toast("方案生成失败：" + (s.error || "见日志"), "bad");
    }
  }

  function renderFiles(files) {
    const box = $("psFiles");
    box.innerHTML = "";
    if (!files.length) { box.innerHTML = `<p class="hint">尚无生成文件。</p>`; return; }
    files.forEach((f) => {
      const name = typeof f === "string" ? f : f.name;
      const isDocx = /\.docx$/i.test(name);
      const isUrs = /URS/i.test(name);
      const a = document.createElement("a");
      a.className = "ps-file" + (isDocx ? " ps-file-docx" : isUrs ? " ps-file-urs" : "");
      a.href = `/api/presales/projects/${state.current.id}/file?name=${encodeURIComponent(name)}`;
      if (isDocx) {
        a.download = name;            // WORD 文档下载而非内联打开
      } else {
        a.target = "_blank";
      }
      a.textContent = (isDocx ? "📝 " : isUrs ? "📋 " : "📄 ") + name;
      box.appendChild(a);
    });
  }

  // ---------- 状态显示 ----------
  function statusText(s) {
    return { draft: "草稿", queued: "排队", running: "生成中", succeeded: "已生成", failed: "失败" }[s] || s || "—";
  }
  function setStatusBadge(el, s) {
    if (!el) return;
    el.textContent = statusText(s);
    el.className = "ps-badge ps-st-" + (s || "draft");
  }

  // ---------- 事件绑定 ----------
  function bindPresalesEvents() {
    $("psNewBtn").addEventListener("click", openNew);
    $("psNewClose").addEventListener("click", () => ($("psNewMask").hidden = true));
    $("psNewCancel").addEventListener("click", () => ($("psNewMask").hidden = true));
    $("psNewCreate").addEventListener("click", createProject);
    $("psNewName").addEventListener("keydown", (e) => { if (e.key === "Enter") createProject(); });
    $("psNewMask").addEventListener("click", (e) => { if (e.target === $("psNewMask")) $("psNewMask").hidden = true; });

    $("psSaveBtn").addEventListener("click", save);
    $("psPreviewBtn").addEventListener("click", preview);
    $("psGenerateBtn").addEventListener("click", generate);
    $("psDeleteBtn").addEventListener("click", deleteProject);

    $("psPreviewClose").addEventListener("click", () => ($("psPreviewMask").hidden = true));
    $("psPreviewCancel").addEventListener("click", () => ($("psPreviewMask").hidden = true));
    $("psPreviewMask").addEventListener("click", (e) => { if (e.target === $("psPreviewMask")) $("psPreviewMask").hidden = true; });

    $("psCurrentStatus").addEventListener("input", (e) => {
      $("psStatusCount").textContent = e.target.value.length;
    });
    $("psAddChallenge").addEventListener("click", () =>
      addItemRow($("psChallenges"), "", "挑战"));
    $("psAddExpectation").addEventListener("click", () =>
      addItemRow($("psExpectations"), "", "期望"));
  }

  // ---------- 工具（与 app.js 隔离，避免污染）----------
  function esc(v) {
    if (v === null || v === undefined) return "";
    return String(v).replace(/&/g, "&amp;").replace(/</g, "&lt;")
      .replace(/>/g, "&gt;").replace(/"/g, "&quot;");
  }
  // 复用 app.js 的 toast（同一 DOM 元素）
  let toastTimer;
  function toast(msg, kind) {
    const t = $("toast");
    if (!t) return;
    t.textContent = msg;
    t.className = "toast" + (kind ? " " + kind : "");
    t.hidden = false;
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => (t.hidden = true), 3000);
  }

  document.addEventListener("DOMContentLoaded", bindTabs);
})();
