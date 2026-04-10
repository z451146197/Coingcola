(function () {
  if (window.__coingcolaHomeSearchFixV8Applied) return;
  window.__coingcolaHomeSearchFixV8Applied = true;

  function esc(v) {
    if (typeof escapeHtml === "function") return escapeHtml(v);
    return String(v ?? "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  function icon(name) {
    if (typeof iconSvg === "function") return iconSvg(name);
    return name === "globe" ? "🌐" : "⌕";
  }

  function ensureSearchStateVisibility() {
    try {
      const hasKeyword = String(state?.keyword || "").trim().length > 0;
      if (el?.navPanel) el.navPanel.classList.toggle("is-hidden", hasKeyword);
      if (el?.searchResults) el.searchResults.classList.toggle("is-hidden", !hasKeyword);
    } catch (e) {
      console.error("ensureSearchStateVisibility error", e);
    }
  }

  function buildSearchTopHtml(keyword, response) {
    const query = String(keyword || "").trim();
    const safeQuery = esc(query);
    const hasResponse = !!response;
    const hasError = !!response?.error;
    const hasBest = !!response?.bestMatch && !hasError;

    let leftTitle = "正在搜索";
    let leftName = "请稍候…";
    let leftDesc = "Everything 与本地回退搜索正在执行";
    let leftClickable = false;

    if (hasError) {
      leftTitle = "搜索异常";
      leftName = esc(response.error);
      leftDesc = "可直接改用网络搜索继续";
    } else if (hasBest) {
      const item = response.bestMatch || {};
      leftTitle = "最佳匹配";
      leftName = esc(item.title || "");
      leftDesc = esc(item.subtitle || item.target || "");
      leftClickable = true;
    } else if (hasResponse) {
      leftTitle = "未找到直接匹配";
      leftName = "本地结果未直接命中";
      leftDesc = "可继续查看下方结果或直接网络搜索";
    }

    return `
      <div class="search-top-grid">
        <button type="button" class="search-split-card search-split-card--best${leftClickable ? "" : " is-static"}" ${leftClickable ? 'data-role="best-match"' : 'disabled'}>
          <span class="search-split-card__label">${leftTitle}</span>
          <span class="search-split-card__name">${leftName}</span>
          <span class="search-split-card__desc">${leftDesc}</span>
          ${leftClickable ? '<span class="search-split-card__arrow">›</span>' : ""}
        </button>
        <button type="button" class="search-split-card search-split-card--web" data-role="search-web">
          <span class="search-split-card__label">网络搜索</span>
          <span class="search-split-card__name"><span class="search-web-inline-icon">' + icon("globe") + '</span><span>搜索“' + safeQuery + '”</span></span>
          <span class="search-split-card__desc">直接打开浏览器查看网络结果</span>
          <span class="search-split-card__arrow">›</span>
        </button>
      </div>
    `;
  }

  function patchRenderBestCard() {
    if (typeof renderBestCard !== "function" || renderBestCard.__searchFixedV8) return;
    renderBestCard = function (keyword, response) {
      if (!el?.bestCard) return;
      el.bestCard.innerHTML = buildSearchTopHtml(keyword, response);

      el.bestCard.querySelector('[data-role="search-web"]')?.addEventListener("click", function () {
        postHost("searchWeb", { query: keyword });
      });

      el.bestCard.querySelector('[data-role="best-match"]')?.addEventListener("click", function () {
        const item = response?.bestMatch;
        if (!item) return;
        postHost("executeSearchItem", { query: keyword, fromBest: true, item: item });
      });
    };
    renderBestCard.__searchFixedV8 = true;
  }

  function patchRenderSearch() {
    if (typeof renderSearch !== "function" || renderSearch.__searchFixedV8) return;
    const original = renderSearch;
    renderSearch = function () {
      const result = original.apply(this, arguments);
      ensureSearchStateVisibility();
      return result;
    };
    renderSearch.__searchFixedV8 = true;
  }

  function refreshNow() {
    patchRenderBestCard();
    patchRenderSearch();
    ensureSearchStateVisibility();
    try { if (typeof renderSearch === "function") renderSearch(); } catch (e) {}
    try { if (typeof syncSideHeight === "function") syncSideHeight(); } catch (e) {}
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", refreshNow);
  } else {
    refreshNow();
  }

  window.setTimeout(refreshNow, 0);
  window.setTimeout(refreshNow, 240);
})();