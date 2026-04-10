
const state = {
  navItems: [],
  recentItems: [],
  quickActions: [],
  navEditing: false,
  keyword: "",
  searchResponse: null,
  searchTimer: null
};

const el = {
  searchInput: document.getElementById("searchInput"),
  clearSearch: document.getElementById("clearSearch"),
  navGrid: document.getElementById("navGrid"),
  editNavBtn: document.getElementById("editNavBtn"),
  editNavText: document.getElementById("editNavText"),
  editTip: document.getElementById("editTip"),
  navPanel: document.getElementById("navPanel"),
  quickGrid: document.getElementById("quickGrid"),
  recentList: document.getElementById("recentList"),
  recentEmpty: document.getElementById("recentEmpty"),
  clearRecentBtn: document.getElementById("clearRecentBtn"),
  bestCard: document.getElementById("bestCard"),
  resultGroups: document.getElementById("resultGroups"),
  searchResults: document.getElementById("searchResults"),
  welcomeDesc: document.getElementById("welcomeDesc")
};

function postHost(action, payload = {}) {
  try {
    const message = JSON.stringify({ action, ...payload });
    if (window.chrome?.webview?.postMessage) {
      window.chrome.webview.postMessage(message);
    }
  } catch (e) {
    console.error("postHost error", e);
  }
}

function iconSvg(name) {
  const map = {
    search: "⌕",
    globe: "🌐",
    app: "▣",
    history: "🕘",
    alert: "!",
    plus: "+"
  };
  const text = map[name] || map.app;
  return `<span class="icon-fallback" aria-hidden="true">${text}</span>`;
}

function escapeHtml(text) {
  return String(text ?? "")
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#39;");
}

function makeShort(title) {
  const value = String(title || "").trim();
  if (!value) return "站";
  if (value.length <= 2) return value.toUpperCase();
  const letters = value.replace(/[^\p{L}\p{N}]/gu, "").slice(0, 3);
  return (letters || value.slice(0, 1)).toUpperCase();
}

function makeColor(key) {
  const colors = ["#0EA5E9", "#6366F1", "#10B981", "#F59E0B", "#F43F5E", "#06B6D4", "#8B5CF6", "#475569"];
  const value = String(key || "");
  let hash = 0;
  for (let i = 0; i < value.length; i += 1) {
    hash = ((hash << 5) - hash) + value.charCodeAt(i);
    hash |= 0;
  }
  return colors[Math.abs(hash) % colors.length];
}

function normalizeNavSort(rawValue) {
  const value = Number(rawValue);
  return Number.isFinite(value) ? value : Number.MAX_SAFE_INTEGER;
}

function normalizePinned(rawValue) {
  if (typeof rawValue === "boolean") return rawValue;
  const normalized = String(rawValue ?? "").trim().toLowerCase();
  return normalized === "true" || normalized === "1" || normalized === "y" || normalized === "yes";
}

function normalizeNavItem(item) {
  const source = item || {};
  return {
    ...source,
    id: String(source.id || "").trim(),
    title: String(source.title || source.name || "").trim(),
    url: String(source.url || source.address || "").trim(),
    group: String(source.group || "").trim(),
    sort: normalizeNavSort(source.sort),
    isPinned: normalizePinned(source.isPinned),
    usedAt: String(source.usedAt || source.lastUsedAt || "").trim()
  };
}

function createNavPresentation(item) {
  const normalized = normalizeNavItem(item);
  return {
    ...normalized,
    shortText: normalized.shortText || makeShort(normalized.title),
    color: normalized.color || makeColor(normalized.title)
  };
}

function getNavAddItem() {
  return {
    id: "__add__",
    title: "添加网站",
    url: "",
    shortText: "+",
    color: "#94A3B8",
    removable: false,
    isAdd: true
  };
}

function getCurrentNavItems() {
  return Array.isArray(state.navItems) ? state.navItems : [];
}

function getNavItemById(id, itemsSource) {
  const items = Array.isArray(itemsSource) ? itemsSource : getCurrentNavItems();
  return items.find((item) => item && item.id === id) || null;
}

function openNavItemHost(item) {
  if (!item || !item.id) return;
  postHost("openNavItem", { id: item.id, title: item.title, url: item.url });
}

function deleteNavItemHost(item) {
  if (!item || !item.id) return;
  if (!window.confirm(`确认删除“${item.title}”吗？`)) return;
  postHost("deleteNavItem", { id: item.id });
}

function saveNavItemHost(payload) {
  if (!payload) return;
  postHost("saveNavItem", payload);
}

function normalizeNavSavePayload(payload, seed) {
  const source = payload || {};
  const fallback = seed || {};
  return {
    id: String(source.id ?? fallback.id ?? "").trim(),
    title: String(source.title ?? "").trim(),
    url: String(source.url ?? "").trim(),
    group: String(source.group ?? fallback.group ?? "").trim(),
    isPinned: normalizePinned(source.isPinned ?? fallback.isPinned)
  };
}

function renderNavTile(item) {
  const removable = state.navEditing && !item.isAdd;
  const title = escapeHtml(item.title);
  const shortText = escapeHtml(item.shortText || makeShort(item.title));
  const color = escapeHtml(item.color || makeColor(item.title));
  const dataId = escapeHtml(item.id);

  return `
    <div class="nav-tile${item.isAdd ? " is-nav-add-tile" : ""}">
      ${removable ? `<button type="button" class="nav-delete is-removable" data-id="${dataId}" aria-label="删除">×</button>` : ""}
      <button type="button" class="nav-tile-btn" data-id="${dataId}">
        <span class="nav-avatar" style="background:${color}">${shortText}</span>
        <span class="nav-title">${title}</span>
      </button>
    </div>
  `;
}

function sortNavItemsForBrowsing(items) {
  return [...items].sort((a, b) => {
    const sortDelta = normalizeNavSort(a?.sort) - normalizeNavSort(b?.sort);
    if (sortDelta !== 0) return sortDelta;
    const titleA = String(a?.title || "");
    const titleB = String(b?.title || "");
    return titleA.localeCompare(titleB, "zh-Hans-CN-u-co-pinyin");
  });
}

function renderNavEmptyState() {
  if (!el.navGrid) return;
  el.navGrid.classList.remove("is-grouped");
  el.navGrid.innerHTML = `
    <div class="state-card">
      <div class="state-card__title">暂无网页导航</div>
      <div class="state-card__desc">点击右上角“编辑”后可直接新增。</div>
    </div>
  `;
}

function renderNavBrowsing(baseItems) {
  if (!el.navGrid) return;
  el.navGrid.classList.remove("is-grouped");

  const items = sortNavItemsForBrowsing(Array.isArray(baseItems) ? baseItems : []);
  el.navGrid.innerHTML = items.map(renderNavTile).join("");

  bindNavTileEvents(items);
}

function renderNavEditing(baseItems) {
  if (!el.navGrid) return;
  el.navGrid.classList.remove("is-grouped");

  const items = [...baseItems, getNavAddItem()];
  el.navGrid.innerHTML = items.map(renderNavTile).join("");

  bindNavTileEvents(items);
}

let draggingId = "";

function persistNavOrderFromDom() {
  if (!el.navGrid) return;
  const ids = Array.from(el.navGrid.querySelectorAll(".nav-tile-btn[data-id]"))
    .map((btn) => btn.getAttribute("data-id") || "")
    .filter((id) => id && id !== "__add__");

  if (!ids.length) return;

  const map = new Map(getCurrentNavItems().map((item) => [item.id, item]));
  state.navItems = ids.map((id) => map.get(id)).filter(Boolean);
  postHost("saveNavOrder", { ids });
}

function installGridDrag() {
  if (!el.navGrid || el.navGrid.dataset.dragInstalled === "1") return;
  el.navGrid.dataset.dragInstalled = "1";

  el.navGrid.addEventListener("dragover", (e) => {
    if (!state.navEditing || !draggingId) return;

    const dragging = el.navGrid.querySelector(`.nav-tile-btn[data-id="${draggingId}"]`)?.closest(".nav-tile");
    const target = e.target.closest(".nav-tile");
    if (!dragging || !target || target === dragging) return;

    const targetBtn = target.querySelector(".nav-tile-btn[data-id]");
    const targetId = targetBtn ? (targetBtn.getAttribute("data-id") || "") : "";
    if (!targetId || targetId === "__add__") return;

    e.preventDefault();

    const rect = target.getBoundingClientRect();
    const insertAfter = e.clientX > rect.left + rect.width / 2;
    if (insertAfter) {
      if (target.nextSibling !== dragging) {
        el.navGrid.insertBefore(dragging, target.nextSibling);
      }
    } else {
      el.navGrid.insertBefore(dragging, target);
    }
  });

  el.navGrid.addEventListener("drop", (e) => {
    if (!state.navEditing || !draggingId) return;
    e.preventDefault();
    persistNavOrderFromDom();
    draggingId = "";
  });
}

function bindNavTileEvents(itemsSource) {
  const items = Array.isArray(itemsSource) ? itemsSource : getCurrentNavItems();
  installGridDrag();

  el.navGrid?.querySelectorAll(".nav-tile-btn").forEach((mainBtn) => {
    const id = mainBtn.getAttribute("data-id") || "";
    const item = getNavItemById(id, items) || (id === "__add__" ? getNavAddItem() : null);
    if (!item) return;

    mainBtn.addEventListener("click", () => {
      if (item.isAdd) {
        createNavItem();
        return;
      }

      if (state.navEditing) {
        editNavItem(item);
        return;
      }

      openNavItemHost(item);
    });

    mainBtn.draggable = state.navEditing && !item.isAdd;
    mainBtn.addEventListener("dragstart", (e) => {
      if (!state.navEditing || item.isAdd) return;
      draggingId = item.id;
      const tile = mainBtn.closest(".nav-tile");
      tile?.classList.add("is-dragging");
      try {
        e.dataTransfer.effectAllowed = "move";
        e.dataTransfer.setData("text/plain", item.id);
      } catch {}
    });

    mainBtn.addEventListener("dragend", () => {
      const tile = mainBtn.closest(".nav-tile");
      tile?.classList.remove("is-dragging");
      if (draggingId) {
        persistNavOrderFromDom();
      }
      draggingId = "";
    });
  });

  el.navGrid?.querySelectorAll(".nav-delete.is-removable").forEach((btn) => {
    btn.addEventListener("click", (e) => {
      e.stopPropagation();
      const id = btn.getAttribute("data-id") || "";
      const target = getNavItemById(id, items);
      if (!target) return;
      deleteNavItemHost(target);
    });
  });
}

function renderNav() {
  if (!el.navGrid) return;

  el.navGrid.innerHTML = "";
  el.navPanel?.classList.toggle("is-nav-editing", state.navEditing);
  if (el.editTip) el.editTip.classList.toggle("is-hidden", !state.navEditing);
  if (el.editNavText) el.editNavText.textContent = state.navEditing ? "完成" : "编辑";
  if (el.editTip) {
    el.editTip.textContent = state.navEditing
      ? "点击站点可编辑，点右上角 × 删除；支持拖拽排序；加号在最后面。"
      : "首页网页导航当前为单层图标模式，点击图标即可打开。";
  }

  const baseItems = [...getCurrentNavItems()];
  if (!baseItems.length && !state.navEditing) {
    renderNavEmptyState();
    return;
  }

  if (state.navEditing) {
    renderNavEditing(baseItems);
  } else {
    renderNavBrowsing(baseItems);
  }
}

let navEditorRefs = null;
let navEditorSeed = null;

function normalizePreviewUrl(rawValue) {
  const raw = String(rawValue || "").trim();
  if (!raw) return "";
  return /^https?:\/\//i.test(raw) ? raw : `https://${raw}`;
}

function looksLikeUrl(rawValue) {
  const raw = String(rawValue || "").trim();
  if (!raw) return false;
  if (/\s/.test(raw)) return false;
  const preview = normalizePreviewUrl(raw);
  return /^https?:\/\/[^\s/$.?#].[^\s]*$/i.test(preview);
}

function ensureNavEditor() {
  if (navEditorRefs) return navEditorRefs;

  const backdrop = document.createElement("div");
  backdrop.className = "nav-editor-backdrop is-hidden";
  backdrop.innerHTML = `
    <div class="nav-editor-panel" role="dialog" aria-modal="true" aria-labelledby="navEditorTitle">
      <div class="nav-editor-header">
        <div>
          <div class="nav-editor-title" id="navEditorTitle">新增网站</div>
          <div class="nav-editor-desc">在页面内完成网站信息维护。</div>
        </div>
        <button type="button" class="nav-editor-close" data-role="close" aria-label="关闭">×</button>
      </div>

      <div class="nav-editor-form">
        <label class="nav-editor-field">
          <span class="nav-editor-label">网站名称</span>
          <input class="nav-editor-input" data-role="title" type="text" maxlength="60" placeholder="请输入网站名称" />
          <div class="nav-editor-helper nav-editor-helper--title">建议使用简洁明确的名称，便于首页快速识别。</div>
        </label>

        <label class="nav-editor-field">
          <span class="nav-editor-label">网站地址</span>
          <input class="nav-editor-input" data-role="url" type="text" maxlength="500" placeholder="https://example.com" />
          <div class="nav-editor-helper nav-editor-helper--url">支持直接输入域名，保存时会自动补全 https://</div>
        </label>

        <label class="nav-editor-field" style="display:none;">
          <span class="nav-editor-label">所属分组</span>
          <input class="nav-editor-input" data-role="group" type="text" maxlength="30" placeholder="默认分组" />
        </label>

        <label class="nav-editor-check" style="display:none;">
          <input data-role="pinned" type="checkbox" />
          <span>固定到前排</span>
        </label>
      </div>

      <div class="nav-editor-validation is-hidden">请输入网站名称和有效的网址地址。</div>

      <div class="nav-editor-actions">
        <button type="button" class="nav-editor-btn nav-editor-btn--ghost" data-role="cancel">取消</button>
        <button type="button" class="nav-editor-btn nav-editor-btn--danger is-hidden" data-role="delete">删除</button>
        <button type="button" class="nav-editor-btn nav-editor-btn--primary" data-role="save">保存</button>
      </div>
    </div>
  `;

  document.body.appendChild(backdrop);

  const refs = {
    backdrop,
    titleText: backdrop.querySelector('[data-role="title"]'),
    urlText: backdrop.querySelector('[data-role="url"]'),
    groupText: backdrop.querySelector('[data-role="group"]'),
    pinnedCheck: backdrop.querySelector('[data-role="pinned"]'),
    saveBtn: backdrop.querySelector('[data-role="save"]'),
    cancelBtn: backdrop.querySelector('[data-role="cancel"]'),
    closeBtn: backdrop.querySelector('[data-role="close"]'),
    deleteBtn: backdrop.querySelector('[data-role="delete"]'),
    panelTitle: backdrop.querySelector('#navEditorTitle'),
    validationText: backdrop.querySelector('.nav-editor-validation'),
    urlHelper: backdrop.querySelector('.nav-editor-helper--url')
  };

  function close() {
    refs.backdrop.classList.add("is-hidden");
    navEditorSeed = null;
  }

  function updateValidationState() {
    const title = String(refs.titleText?.value || "").trim();
    const url = String(refs.urlText?.value || "").trim();
    const titleValid = title.length > 0;
    const urlValid = looksLikeUrl(url);
    const valid = titleValid && urlValid;

    if (refs.saveBtn) refs.saveBtn.disabled = !valid;
    if (refs.titleText) refs.titleText.classList.toggle("is-invalid", !titleValid && document.activeElement === refs.titleText);
    if (refs.urlText) refs.urlText.classList.toggle("is-invalid", url.length > 0 && !urlValid);
    if (refs.validationText) refs.validationText.classList.toggle("is-hidden", valid);
    if (refs.urlHelper) {
      refs.urlHelper.textContent = url
        ? `将保存为：${normalizePreviewUrl(url)}`
        : "支持直接输入域名，保存时会自动补全 https://";
    }
  }

  refs.cancelBtn.addEventListener("click", close);
  refs.closeBtn.addEventListener("click", close);
  refs.backdrop.addEventListener("click", (e) => {
    if (e.target === refs.backdrop) close();
  });
  refs.deleteBtn.addEventListener("click", () => {
    if (!navEditorSeed?.id) return;
    deleteNavItemHost(navEditorSeed);
    close();
  });
  refs.saveBtn.addEventListener("click", () => {
    const title = String(refs.titleText.value || "").trim();
    if (!title) {
      refs.titleText.focus();
      updateValidationState();
      return;
    }
    let url = String(refs.urlText.value || "").trim();
    if (!url || !looksLikeUrl(url)) {
      refs.urlText.focus();
      updateValidationState();
      return;
    }
    url = normalizePreviewUrl(url);
    const payload = normalizeNavSavePayload({
      id: navEditorSeed?.id || "",
      title,
      url,
      group: "",
      isPinned: false
    }, navEditorSeed || {});
    saveNavItemHost(payload);
    close();
  });

  [refs.titleText, refs.urlText].forEach((input) => {
    if (!input) return;
    input.addEventListener("input", updateValidationState);
    input.addEventListener("keydown", (e) => {
      if (e.key === "Enter" && !e.shiftKey) {
        e.preventDefault();
        if (refs.saveBtn && !refs.saveBtn.disabled) refs.saveBtn.click();
      }
    });
  });

  window.addEventListener("keydown", (e) => {
    if (e.key === "Escape" && navEditorRefs && !navEditorRefs.backdrop.classList.contains("is-hidden")) {
      close();
    }
  });

  refs.updateValidationState = updateValidationState;
  navEditorRefs = refs;
  return refs;
}

function openNavEditor(seed, mode) {
  const refs = ensureNavEditor();
  navEditorSeed = seed || {};
  refs.panelTitle.textContent = mode === "edit" ? "编辑网站" : "新增网站";
  refs.titleText.value = seed?.title || "";
  refs.urlText.value = seed?.url || "https://";
  refs.deleteBtn.classList.toggle("is-hidden", !(seed && seed.id));
  refs.backdrop.classList.remove("is-hidden");
  refs.updateValidationState();
  setTimeout(() => {
    refs.titleText.focus();
    refs.titleText.select();
  }, 0);
}

function createNavItem() {
  openNavEditor({ title: "", url: "https://", group: "", isPinned: false }, "create");
}

function editNavItem(item) {
  openNavEditor(item || {}, "edit");
}

function renderQuick() {
  if (!el.quickGrid) return;

  if (!state.quickActions.length) {
    el.quickGrid.innerHTML = `<div class="state-card__desc">暂无常用入口</div>`;
    return;
  }

  el.quickGrid.innerHTML = state.quickActions.map((item) => `
    <button type="button" class="quick-action" data-id="${escapeHtml(item.id)}">
      <span class="quick-action__icon">${iconSvg(item.iconName || "app")}</span>
      <span class="quick-action__text">${escapeHtml(item.title)}</span>
      <span class="quick-action__arrow">›</span>
    </button>
  `).join("");

  el.quickGrid.querySelectorAll(".quick-action").forEach((btn) => {
    btn.addEventListener("click", () => {
      const id = btn.getAttribute("data-id");
      const item = state.quickActions.find((x) => String(x.id) === String(id));
      if (!item) return;
      postHost("openQuickAction", { query: state.keyword, item });
    });
  });
}

function renderRecent() {
  if (!el.recentList || !el.recentEmpty || !el.clearRecentBtn) return;

  el.recentList.innerHTML = "";
  const empty = state.recentItems.length === 0;
  el.recentEmpty.classList.toggle("is-hidden", !empty);
  el.clearRecentBtn.classList.toggle("is-hidden", empty);

  if (empty) return;

  state.recentItems.forEach((item) => {
    const node = document.createElement("button");
    node.type = "button";
    node.className = "recent-item";
    node.innerHTML = `
      <span class="recent-item__title">${escapeHtml(item.title)}</span>
      <span class="recent-item__arrow">›</span>
      <span class="recent-remove" data-id="${escapeHtml(item.id || "")}">×</span>
    `;
    node.addEventListener("click", () => {
      postHost("openRecent", { query: state.keyword, item });
    });
    el.recentList.appendChild(node);
  });

  el.recentList.querySelectorAll(".recent-remove").forEach((btn) => {
    btn.addEventListener("click", (e) => {
      e.stopPropagation();
      const id = btn.getAttribute("data-id");
      postHost("deleteRecent", { id });
    });
  });
}

function renderBestCard(keyword, response) {
  if (!el.bestCard) return;

  if (!response) {
    el.bestCard.innerHTML = `
      <div class="search-card__title">正在搜索</div>
      <div class="search-card__desc">请稍候…</div>
    `;
    return;
  }

  if (response.error) {
    el.bestCard.innerHTML = `
      <div class="search-card__title">搜索异常</div>
      <div class="search-card__desc">${escapeHtml(response.error)}</div>
    `;
    return;
  }

  if (!response.bestMatch) {
    el.bestCard.innerHTML = `
      <div class="search-card__title">未找到直接匹配</div>
      <button type="button" class="search-web-btn" data-role="search-web">
        <span class="search-web-btn__icon">${iconSvg("globe")}</span>
        <span class="search-web-btn__text">搜索“${escapeHtml(keyword)}”</span>
      </button>
    `;
    el.bestCard.querySelector('[data-role="search-web"]')?.addEventListener("click", () => {
      postHost("searchWeb", { query: keyword });
    });
    return;
  }

  const item = response.bestMatch;
  el.bestCard.innerHTML = `
    <div class="search-card__title">最佳匹配</div>
    <button type="button" class="search-best-btn" data-role="best-match">
      <span class="search-best-btn__main">
        <span class="search-best-btn__name">${escapeHtml(item.title || "")}</span>
        <span class="search-best-btn__desc">${escapeHtml(item.subtitle || item.target || "")}</span>
      </span>
      <span class="search-best-btn__arrow">›</span>
    </button>
  `;
  el.bestCard.querySelector('[data-role="best-match"]')?.addEventListener("click", () => {
    postHost("executeSearchItem", { query: keyword, item });
  });
}

function renderResultGroups(keyword, response) {
  if (!el.resultGroups) return;
  el.resultGroups.innerHTML = "";

  if (!response || response.error || !Array.isArray(response.groups) || !response.groups.length) {
    return;
  }

  el.resultGroups.innerHTML = response.groups.map((group, groupIndex) => `
    <section class="result-group">
      <div class="result-group__title">${escapeHtml(group.title || "")}</div>
      <div class="result-group__list">
        ${(group.items || []).map((item, itemIndex) => `
          <button type="button" class="result-item" data-group-index="${groupIndex}" data-item-index="${itemIndex}">
            <span class="result-item__main">
              <span class="result-item__name">${escapeHtml(item.title || "")}</span>
              <span class="result-item__desc">${escapeHtml(item.subtitle || item.target || "")}</span>
            </span>
            <span class="result-item__arrow">›</span>
          </button>
        `).join("")}
      </div>
    </section>
  `).join("");

  el.resultGroups.querySelectorAll(".result-item").forEach((btn) => {
    btn.addEventListener("click", () => {
      const groupIndex = Number(btn.getAttribute("data-group-index"));
      const itemIndex = Number(btn.getAttribute("data-item-index"));
      const item = response.groups?.[groupIndex]?.items?.[itemIndex];
      if (!item) return;
      postHost("executeSearchItem", { query: keyword, item });
    });
  });
}

function renderSearch() {
  const keyword = String(state.keyword || "").trim();

  if (!keyword) {
    state.searchResponse = null;
    if (el.searchResults) el.searchResults.classList.add("is-hidden");
    if (el.bestCard) el.bestCard.innerHTML = "";
    if (el.resultGroups) el.resultGroups.innerHTML = "";
    if (el.clearSearch) el.clearSearch.classList.add("is-hidden");
    if (el.welcomeDesc) {
      el.welcomeDesc.textContent = "从搜索、网站导航或常用操作开始";
    }
    return;
  }

  if (el.searchResults) el.searchResults.classList.remove("is-hidden");
  if (el.clearSearch) el.clearSearch.classList.remove("is-hidden");
  if (el.welcomeDesc) {
    el.welcomeDesc.textContent = "已在首页展开搜索结果，右侧内容保持稳定";
  }

  renderBestCard(keyword, state.searchResponse);
  renderResultGroups(keyword, state.searchResponse);
}

function syncSideHeight() {
  try {
    const homeMain = document.querySelector(".home-main");
    const homeSide = document.querySelector(".home-side");
    if (!homeMain || !homeSide) return;
    homeSide.style.minHeight = `${homeMain.offsetHeight}px`;
  } catch {}
}

function requestSearch(keyword) {
  state.keyword = String(keyword || "").trim();

  if (state.searchTimer) {
    window.clearTimeout(state.searchTimer);
    state.searchTimer = null;
  }

  if (!state.keyword) {
    state.searchResponse = null;
    renderSearch();
    syncSideHeight();
    return;
  }

  state.searchResponse = null;
  renderSearch();
  syncSideHeight();

  state.searchTimer = window.setTimeout(() => {
    postHost("search", { keyword: state.keyword });
  }, 220);
}

function receiveHomeData(payload) {
  state.navItems = Array.isArray(payload?.navItems)
    ? payload.navItems.map(createNavPresentation).filter((item) => item.id && item.title)
    : [];
  state.recentItems = Array.isArray(payload?.recentItems) ? payload.recentItems : [];
  state.quickActions = Array.isArray(payload?.quickActions) ? payload.quickActions : [];

  renderNav();
  renderQuick();
  renderRecent();
  renderSearch();
  syncSideHeight();
}

function receiveSearchResult(payload) {
  state.searchResponse = payload || null;
  renderSearch();
  syncSideHeight();
}

window.coingcolaHome = window.coingcolaHome || {};
window.coingcolaHome.receiveHomeData = receiveHomeData;
window.coingcolaHome.receiveSearchResult = receiveSearchResult;

function initHomeEvents() {
  el.searchInput?.addEventListener("input", (e) => {
    requestSearch(e.target.value || "");
  });

  el.clearSearch?.addEventListener("click", () => {
    if (el.searchInput) el.searchInput.value = "";
    requestSearch("");
    el.searchInput?.focus();
  });

  el.editNavBtn?.addEventListener("click", () => {
    state.navEditing = !state.navEditing;
    renderNav();
    syncSideHeight();
  });

  el.clearRecentBtn?.addEventListener("click", () => {
    postHost("clearRecent");
  });
}

function boot() {
  initHomeEvents();
  renderNav();
  renderQuick();
  renderRecent();
  renderSearch();
  syncSideHeight();
  postHost("homeReady");
}

if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", boot);
} else {
  boot();
}


/* COINGCOLA_NAV_SINGLE_LAYER_FIX_V15_START */
(function () {
  if (window.__coingcolaNavSingleLayerFixV15Applied) return;
  window.__coingcolaNavSingleLayerFixV15Applied = true;

  function hideNavGroupFieldForV15() {
    const groupInput = document.querySelector('.nav-editor-input[data-role="group"]');
    const field = groupInput ? groupInput.closest('.nav-editor-field') : null;
    if (field) {
      field.style.display = 'none';
    }
    document.querySelectorAll('.nav-editor-helper--group').forEach((node) => {
      node.style.display = 'none';
    });
  }

  if (typeof renderNav === 'function' && !window.__coingcolaNavSingleLayerFixV15Wrapped) {
    const originalRenderNav = renderNav;
    renderNav = function () {
      const result = originalRenderNav.apply(this, arguments);
      try {
        hideNavGroupFieldForV15();
      } catch (e) {}
      return result;
    };
    window.__coingcolaNavSingleLayerFixV15Wrapped = true;
  }

  if (typeof openNavEditor === 'function' && !window.__coingcolaNavSingleLayerFixV15OpenWrapped) {
    const originalOpenNavEditor = openNavEditor;
    openNavEditor = function () {
      const result = originalOpenNavEditor.apply(this, arguments);
      try {
        hideNavGroupFieldForV15();
      } catch (e) {}
      return result;
    };
    window.__coingcolaNavSingleLayerFixV15OpenWrapped = true;
  }

  function ready() {
    try {
      hideNavGroupFieldForV15();
    } catch (e) {}
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', ready);
  } else {
    ready();
  }
})();
/* COINGCOLA_NAV_SINGLE_LAYER_FIX_V15_END */