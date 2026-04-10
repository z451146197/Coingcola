(function () {
  if (window.__coingcolaHomeFrontFixV12Applied) return;
  window.__coingcolaHomeFrontFixV12Applied = true;

  function injectSearchFixStyle() {
    if (document.getElementById('coingcola-home-search-fix-style')) return;
    const style = document.createElement('style');
    style.id = 'coingcola-home-search-fix-style';
    style.textContent = `
      .cc-search-top-grid{
        display:grid;
        grid-template-columns:minmax(0,1fr) minmax(220px,280px);
        gap:14px;
        align-items:stretch;
      }
      .cc-search-top-grid.is-only-web{
        grid-template-columns:minmax(220px,280px);
        justify-content:start;
      }
      .cc-search-top-card{
        width:100%;
        min-height:112px;
        border:1px solid var(--line, #dbe3ef);
        border-radius:20px;
        background:var(--bg-card, #ffffff);
        display:flex;
        flex-direction:column;
        align-items:flex-start;
        justify-content:center;
        gap:8px;
        padding:18px 20px;
        text-align:left;
        cursor:pointer;
      }
      .cc-search-top-card[disabled]{
        cursor:default;
      }
      .cc-search-top-card-title{
        font-size:14px;
        line-height:20px;
        color:var(--text-subtle, #6b7a90);
      }
      .cc-search-top-card-main{
        font-size:18px;
        line-height:28px;
        font-weight:700;
        color:var(--text-strong, #14213d);
        word-break:break-word;
      }
      .cc-search-top-card-sub{
        font-size:13px;
        line-height:20px;
        color:var(--text-muted, #71839b);
        word-break:break-word;
      }
      .cc-search-top-card-arrow{
        margin-left:auto;
        font-size:18px;
        line-height:18px;
        color:var(--text-subtle, #6b7a90);
      }
      .cc-search-top-card-head{
        width:100%;
        display:flex;
        align-items:center;
        gap:8px;
      }
      .cc-search-top-card-icon{
        width:18px;
        height:18px;
        display:inline-flex;
        align-items:center;
        justify-content:center;
        color:var(--text-subtle, #6b7a90);
        flex:0 0 auto;
      }
      .cc-result-group{
        margin-top:16px;
      }
      .cc-result-group-title{
        margin:0 0 10px 0;
        font-size:14px;
        line-height:20px;
        color:var(--text-subtle, #6b7a90);
      }
      .cc-result-item{
        width:100%;
        margin:0 0 10px 0;
        border:1px solid var(--line, #dbe3ef);
        border-radius:18px;
        background:var(--bg-card, #ffffff);
        display:flex;
        align-items:center;
        justify-content:space-between;
        gap:12px;
        padding:14px 16px;
        text-align:left;
        cursor:pointer;
      }
      .cc-result-item-main{
        min-width:0;
        display:flex;
        flex-direction:column;
        gap:4px;
      }
      .cc-result-item-title{
        font-size:16px;
        line-height:24px;
        color:var(--text-strong, #14213d);
        word-break:break-word;
      }
      .cc-result-item-sub{
        font-size:13px;
        line-height:20px;
        color:var(--text-muted, #71839b);
        word-break:break-word;
      }
      .cc-result-item-arrow{
        flex:0 0 auto;
        font-size:18px;
        line-height:18px;
        color:var(--text-subtle, #6b7a90);
      }
      @media (max-width: 1100px){
        .cc-search-top-grid,
        .cc-search-top-grid.is-only-web{
          grid-template-columns:1fr;
        }
      }
    `;
    document.head.appendChild(style);
  }

  function svg(name) {
    const icons = {
      search: '<svg viewBox="0 0 24 24" width="18" height="18" aria-hidden="true"><circle cx="11" cy="11" r="6.5" fill="none" stroke="currentColor" stroke-width="1.8"></circle><path d="M16 16L21 21" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"></path></svg>',
      globe: '<svg viewBox="0 0 24 24" width="18" height="18" aria-hidden="true"><circle cx="12" cy="12" r="8" fill="none" stroke="currentColor" stroke-width="1.8"></circle><path d="M4 12H20" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round"></path><path d="M12 4C14.8 6.8 14.8 17.2 12 20" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round"></path><path d="M12 4C9.2 6.8 9.2 17.2 12 20" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round"></path></svg>',
      app: '<svg viewBox="0 0 24 24" width="18" height="18" aria-hidden="true"><rect x="5" y="5" width="14" height="14" rx="2.2" fill="none" stroke="currentColor" stroke-width="1.8"></rect><path d="M12 5V19M5 12H19" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round"></path></svg>',
      history: '<svg viewBox="0 0 24 24" width="18" height="18" aria-hidden="true"><path d="M4.5 12A7.5 7.5 0 1 0 7 6.4" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"></path><path d="M4 4V9H9" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"></path><path d="M12 8V12L15 14" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"></path></svg>',
      alert: '<svg viewBox="0 0 24 24" width="18" height="18" aria-hidden="true"><path d="M12 4L20 19H4L12 4Z" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round"></path><path d="M12 9V13" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"></path><circle cx="12" cy="16.4" r="1" fill="currentColor"></circle></svg>',
      plus: '<svg viewBox="0 0 24 24" width="18" height="18" aria-hidden="true"><path d="M12 5V19M5 12H19" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round"></path></svg>',
      zap: '<svg viewBox="0 0 24 24" width="18" height="18" aria-hidden="true"><path d="M13 3L6 13H11L10 21L18 10H13L13 3Z" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round"></path></svg>',
      settings: '<svg viewBox="0 0 24 24" width="18" height="18" aria-hidden="true"><circle cx="12" cy="12" r="3.2" fill="none" stroke="currentColor" stroke-width="1.8"></circle><path d="M12 4.5V2.8M12 21.2V19.5M19.5 12H21.2M2.8 12H4.5M17.3 6.7L18.5 5.5M5.5 18.5L6.7 17.3M17.3 17.3L18.5 18.5M5.5 5.5L6.7 6.7" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round"></path></svg>',
      grid: '<svg viewBox="0 0 24 24" width="18" height="18" aria-hidden="true"><rect x="4.5" y="4.5" width="5.5" height="5.5" rx="1.2" fill="none" stroke="currentColor" stroke-width="1.8"></rect><rect x="14" y="4.5" width="5.5" height="5.5" rx="1.2" fill="none" stroke="currentColor" stroke-width="1.8"></rect><rect x="4.5" y="14" width="5.5" height="5.5" rx="1.2" fill="none" stroke="currentColor" stroke-width="1.8"></rect><rect x="14" y="14" width="5.5" height="5.5" rx="1.2" fill="none" stroke="currentColor" stroke-width="1.8"></rect></svg>',
      device: '<svg viewBox="0 0 24 24" width="18" height="18" aria-hidden="true"><rect x="4" y="5" width="16" height="10" rx="2" fill="none" stroke="currentColor" stroke-width="1.8"></rect><path d="M8 19H16M12 15V19" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"></path></svg>',
      repair: '<svg viewBox="0 0 24 24" width="18" height="18" aria-hidden="true"><path d="M14.5 6.2A3.7 3.7 0 0 0 10 10.8L5.2 15.6A1.8 1.8 0 0 0 7.8 18.2L12.6 13.4A3.7 3.7 0 0 0 17.2 8.9L15.1 11L13 8.9L15.1 6.8L14.5 6.2Z" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linejoin="round"></path></svg>'
    };
    return icons[String(name || '').trim()] || icons.app;
  }

  function normalizeQuickActions(items) {
    const list = Array.isArray(items) ? items.slice() : [];
    const hasRepair = list.some((x) => String(x?.id || '').trim() === 'quick::repair' || String(x?.title || '').trim() === '常见修复');
    if (!hasRepair) {
      list.push({
        id: 'quick::repair',
        title: '常见修复',
        iconName: 'repair',
        level1: '电脑优化',
        level2: '常见修复'
      });
    }
    return list;
  }

  function applyKeywordVisibility() {
    try {
      const hasKeyword = String(state?.keyword || '').trim().length > 0;
      if (el?.navPanel) {
        el.navPanel.classList.toggle('is-hidden', hasKeyword);
      }
    } catch (e) {
      console.error('applyKeywordVisibility error', e);
    }
  }

  function patchIconSvg() {
    try {
      iconSvg = function (name) {
        return svg(name);
      };
    } catch (e) {
      console.error('patchIconSvg error', e);
    }
  }

  function patchRenderQuick() {
    if (typeof renderQuick !== 'function' || renderQuick.__frontFixedV12) return;
    const original = renderQuick;
    renderQuick = function () {
      state.quickActions = normalizeQuickActions(state.quickActions);
      return original.apply(this, arguments);
    };
    renderQuick.__frontFixedV12 = true;
  }

  function isPlaceholderItem(item) {
    if (!item || typeof item !== 'object') return false;
    const title = String(item.title || '').trim();
    const subtitle = String(item.subtitle || '').trim();
    const target = String(item.target || '').trim();
    const text = [title, subtitle, target].join(' | ');
    return (
      title.startsWith('用 Everything 搜文件：') ||
      title.includes('无命中') ||
      text.includes('Everything 已接入，但当前未直接返回结果') ||
      text.includes('可继续唤起 Everything 搜索')
    );
  }

  function buildIdentityKey(item) {
    if (!item || typeof item !== 'object') return '';
    const target = String(item.target || '').trim().toLowerCase();
    if (target) return 'target:' + target;

    const id = String(item.id || '').trim().toLowerCase();
    if (id) return 'id:' + id;

    const title = String(item.title || '').trim().toLowerCase();
    const subtitle = String(item.subtitle || '').trim().toLowerCase();
    if (title || subtitle) return 'text:' + title + '|' + subtitle;

    return '';
  }

  function sanitizeResponse(response) {
    if (!response || typeof response !== 'object') return response;

    const sanitized = Object.assign({}, response);
    sanitized.bestMatch = isPlaceholderItem(response.bestMatch) ? null : response.bestMatch;

    const bestKey = buildIdentityKey(sanitized.bestMatch);
    const rawGroups = Array.isArray(response.groups) ? response.groups : [];
    sanitized.groups = rawGroups
      .map(function (group) {
        const rawItems = Array.isArray(group?.items) ? group.items : [];
        const items = rawItems.filter(function (item) {
          if (isPlaceholderItem(item)) return false;
          const itemKey = buildIdentityKey(item);
          if (bestKey && itemKey && bestKey === itemKey) return false;
          return true;
        });
        return Object.assign({}, group, { items: items });
      })
      .filter(function (group) {
        return Array.isArray(group.items) && group.items.length > 0;
      });

    return sanitized;
  }

  function makeTopCard(options) {
    const btn = document.createElement('button');
    btn.type = 'button';
    btn.className = 'cc-search-top-card';
    if (options.disabled) btn.disabled = true;

    const head = document.createElement('div');
    head.className = 'cc-search-top-card-head';

    const icon = document.createElement('span');
    icon.className = 'cc-search-top-card-icon';
    icon.innerHTML = svg(options.iconName || 'app');

    const title = document.createElement('span');
    title.className = 'cc-search-top-card-title';
    title.textContent = options.title || '';

    const arrow = document.createElement('span');
    arrow.className = 'cc-search-top-card-arrow';
    arrow.textContent = '›';

    head.appendChild(icon);
    head.appendChild(title);
    head.appendChild(arrow);

    const main = document.createElement('div');
    main.className = 'cc-search-top-card-main';
    main.textContent = options.main || '';

    const sub = document.createElement('div');
    sub.className = 'cc-search-top-card-sub';
    sub.textContent = options.sub || '';

    btn.appendChild(head);
    btn.appendChild(main);
    btn.appendChild(sub);

    if (typeof options.onClick === 'function' && !options.disabled) {
      btn.addEventListener('click', options.onClick);
    }

    return btn;
  }

  function renderBestCardPatched(keyword, response) {
    if (!el?.bestCard) return;
    el.bestCard.innerHTML = '';

    const safeResponse = sanitizeResponse(response);
    const grid = document.createElement('div');
    grid.className = 'cc-search-top-grid';

    if (!safeResponse) {
      const loading = makeTopCard({
        iconName: 'search',
        title: '正在搜索',
        main: keyword || '请稍候',
        sub: '正在等待搜索结果返回',
        disabled: true
      });
      grid.classList.add('is-only-web');
      grid.appendChild(loading);
      el.bestCard.appendChild(grid);
      return;
    }

    const webCard = makeTopCard({
      iconName: 'globe',
      title: '网络搜索',
      main: `搜索“${keyword}”`,
      sub: '直接打开浏览器查看网络结果',
      onClick: function () {
        postHost('searchWeb', { query: keyword });
      }
    });

    if (safeResponse.error) {
      grid.classList.add('is-only-web');
      grid.appendChild(webCard);
      el.bestCard.appendChild(grid);
      return;
    }

    if (safeResponse.bestMatch) {
      const item = safeResponse.bestMatch;
      const best = makeTopCard({
        iconName: item.iconName || 'app',
        title: '最佳匹配',
        main: String(item.title || ''),
        sub: String(item.subtitle || item.target || ''),
        onClick: function () {
          postHost('executeSearchItem', { query: keyword, item: item });
        }
      });
      grid.appendChild(best);
      grid.appendChild(webCard);
    } else {
      grid.classList.add('is-only-web');
      grid.appendChild(webCard);
    }

    el.bestCard.appendChild(grid);
  }

  function renderResultGroupsPatched(keyword, response) {
    if (!el?.resultGroups) return;
    el.resultGroups.innerHTML = '';

    const safeResponse = sanitizeResponse(response);
    if (!safeResponse || safeResponse.error || !Array.isArray(safeResponse.groups) || !safeResponse.groups.length) {
      return;
    }

    safeResponse.groups.forEach(function (group) {
      const items = Array.isArray(group?.items) ? group.items : [];
      if (!items.length) return;

      const wrapper = document.createElement('section');
      wrapper.className = 'cc-result-group';

      const title = document.createElement('h4');
      title.className = 'cc-result-group-title';
      title.textContent = String(group?.title || '');
      wrapper.appendChild(title);

      items.forEach(function (item) {
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'cc-result-item';

        const main = document.createElement('span');
        main.className = 'cc-result-item-main';

        const itemTitle = document.createElement('span');
        itemTitle.className = 'cc-result-item-title';
        itemTitle.textContent = String(item?.title || '');

        const itemSub = document.createElement('span');
        itemSub.className = 'cc-result-item-sub';
        itemSub.textContent = String(item?.subtitle || item?.target || '');

        const arrow = document.createElement('span');
        arrow.className = 'cc-result-item-arrow';
        arrow.textContent = '›';

        main.appendChild(itemTitle);
        main.appendChild(itemSub);
        btn.appendChild(main);
        btn.appendChild(arrow);

        btn.addEventListener('click', function () {
          postHost('executeSearchItem', { query: keyword, item: item });
        });

        wrapper.appendChild(btn);
      });

      el.resultGroups.appendChild(wrapper);
    });
  }

  function patchRenderSearch() {
    if (typeof renderSearch !== 'function' || renderSearch.__frontFixedV12) return;

    renderSearch = function () {
      const keyword = String(state?.keyword || '').trim();

      if (!keyword) {
        state.searchResponse = null;
        if (el?.searchResults) el.searchResults.classList.add('is-hidden');
        if (el?.bestCard) el.bestCard.innerHTML = '';
        if (el?.resultGroups) el.resultGroups.innerHTML = '';
        if (el?.clearSearch) el.clearSearch.classList.add('is-hidden');
        if (el?.welcomeDesc) {
          el.welcomeDesc.textContent = '从搜索、网站导航或常用操作开始';
        }
        applyKeywordVisibility();
        return;
      }

      if (el?.searchResults) el.searchResults.classList.remove('is-hidden');
      if (el?.clearSearch) el.clearSearch.classList.remove('is-hidden');
      if (el?.welcomeDesc) {
        el.welcomeDesc.textContent = '已在首页展开搜索结果，右侧内容保持稳定';
      }

      renderBestCardPatched(keyword, state.searchResponse);
      renderResultGroupsPatched(keyword, state.searchResponse);
      applyKeywordVisibility();
    };

    renderSearch.__frontFixedV12 = true;
  }

  function patchReceiveHomeData() {
    if (!window.coingcolaHome || typeof window.coingcolaHome.receiveHomeData !== 'function' || window.coingcolaHome.receiveHomeData.__frontFixedV12) return;
    const original = window.coingcolaHome.receiveHomeData;
    window.coingcolaHome.receiveHomeData = function (payload) {
      const safePayload = payload || {};
      safePayload.quickActions = normalizeQuickActions(safePayload.quickActions);
      const result = original.call(this, safePayload);
      applyKeywordVisibility();
      return result;
    };
    window.coingcolaHome.receiveHomeData.__frontFixedV12 = true;
  }

  function refreshNow() {
    injectSearchFixStyle();
    patchIconSvg();
    patchRenderQuick();
    patchRenderSearch();
    patchReceiveHomeData();

    try { state.quickActions = normalizeQuickActions(state.quickActions); } catch (e) {}
    try { if (typeof renderQuick === 'function') renderQuick(); } catch (e) {}
    try { if (typeof renderSearch === 'function') renderSearch(); } catch (e) {}
    try { if (typeof syncSideHeight === 'function') syncSideHeight(); } catch (e) {}
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', refreshNow);
  } else {
    refreshNow();
  }

  window.setTimeout(refreshNow, 0);
  window.setTimeout(refreshNow, 220);
  window.setTimeout(refreshNow, 480);
})();

(function () {
  if (window.__coingcolaHomeNavDenseFixV13Applied) return;
  window.__coingcolaHomeNavDenseFixV13Applied = true;

  function injectNavDenseStyle() {
    if (document.getElementById('coingcola-home-nav-dense-style')) return;

    const style = document.createElement('style');
    style.id = 'coingcola-home-nav-dense-style';
    style.textContent = `
      /* 仅作用于网页导航容器，不影响其他容器 */
      #navPanel .nav-grid,
      #navPanel #navGrid{
        display:grid !important;
        grid-template-columns:repeat(6, minmax(0, 1fr)) !important;
        gap:10px !important;
        align-items:start !important;
      }

      #navPanel .nav-tile{
        min-width:0 !important;
      }

      #navPanel .nav-tile-btn{
        min-height:84px !important;
        padding:10px 6px !important;
        border-radius:16px !important;
        display:flex !important;
        flex-direction:column !important;
        align-items:center !important;
        justify-content:center !important;
        gap:8px !important;
      }

      #navPanel .nav-tile-btn .nav-tile-avatar,
      #navPanel .nav-tile-btn .nav-avatar,
      #navPanel .nav-tile-btn .nav-icon,
      #navPanel .nav-tile-btn .site-avatar{
        width:34px !important;
        height:34px !important;
        min-width:34px !important;
        min-height:34px !important;
        border-radius:10px !important;
        font-size:14px !important;
        line-height:34px !important;
        display:flex !important;
        align-items:center !important;
        justify-content:center !important;
      }

      #navPanel .nav-tile-btn .nav-tile-title,
      #navPanel .nav-tile-btn .nav-title,
      #navPanel .nav-tile-btn .site-title,
      #navPanel .nav-tile-btn .tile-title{
        width:100% !important;
        max-width:100% !important;
        font-size:12px !important;
        line-height:16px !important;
        font-weight:500 !important;
        text-align:center !important;
        white-space:nowrap !important;
        overflow:hidden !important;
        text-overflow:ellipsis !important;
      }

      #navPanel .nav-delete{
        width:18px !important;
        height:18px !important;
        top:6px !important;
        right:6px !important;
        font-size:12px !important;
        line-height:18px !important;
      }

      #navPanel.is-nav-editing .nav-tile-btn{
        min-height:88px !important;
      }

      @media (max-width: 1360px){
        #navPanel .nav-grid,
        #navPanel #navGrid{
          grid-template-columns:repeat(5, minmax(0, 1fr)) !important;
        }
      }

      @media (max-width: 1180px){
        #navPanel .nav-grid,
        #navPanel #navGrid{
          grid-template-columns:repeat(4, minmax(0, 1fr)) !important;
        }
      }
    `;
    document.head.appendChild(style);
  }

  function applyNavDenseFix() {
    injectNavDenseStyle();
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', applyNavDenseFix);
  } else {
    applyNavDenseFix();
  }

  window.setTimeout(applyNavDenseFix, 0);
  window.setTimeout(applyNavDenseFix, 220);
  window.setTimeout(applyNavDenseFix, 480);
})();

(function () {
  if (window.__coingcolaHomeNavRenderFixV19Applied) return;
  window.__coingcolaHomeNavRenderFixV19Applied = true;

  function getNavPanel() {
    if (window.el && el.navPanel) return el.navPanel;
    return document.getElementById('navPanel');
  }

  function isElement(node) {
    return node && node.nodeType === 1;
  }

  function textOf(node) {
    return String((node && node.textContent) || '').replace(/\s+/g, ' ').trim();
  }

  function getCandidateContainers(panel) {
    const nodes = Array.from(panel.querySelectorAll('div, section, ul'));
    return nodes.filter(function (node) {
      if (!isElement(node)) return false;
      const children = Array.from(node.children || []).filter(isElement);
      if (children.length < 4) return false;
      let score = 0;
      children.forEach(function (child) {
        if (child.querySelector('button, a')) score += 2;
        const t = ((child.className || '') + ' ' + (child.id || '')).toLowerCase();
        if (/(nav|site|tile|item|link|card)/.test(t)) score += 2;
      });
      return score >= 8;
    });
  }

  function pickBestContainer(panel) {
    const list = getCandidateContainers(panel);
    if (!list.length) return null;

    list.sort(function (a, b) {
      const aChildren = a.children ? a.children.length : 0;
      const bChildren = b.children ? b.children.length : 0;
      const aText = ((a.className || '') + ' ' + (a.id || '')).toLowerCase();
      const bText = ((b.className || '') + ' ' + (b.id || '')).toLowerCase();
      const aScore = (/(nav|site|grid|list)/.test(aText) ? 100 : 0) + aChildren;
      const bScore = (/(nav|site|grid|list)/.test(bText) ? 100 : 0) + bChildren;
      return bScore - aScore;
    });

    return list[0];
  }

  function findAction(card) {
    return card.querySelector('button, a') || card;
  }

  function findIconNode(action) {
    const preferred = action.querySelector('img, svg, canvas, [class*="avatar"], [class*="Avatar"], [class*="icon"], [class*="Icon"], [class*="logo"], [class*="Logo"], [class*="short"]');
    if (preferred) return preferred;

    const elements = Array.from(action.children || []).filter(isElement);
    return elements[0] || null;
  }

  function findTitleNode(action) {
    const preferred = action.querySelector('[class*="title"], [class*="Title"], [class*="label"], [class*="Label"], [class*="name"], [class*="Name"]');
    if (preferred && textOf(preferred)) return preferred;

    const list = Array.from(action.querySelectorAll('span, div, p')).filter(function (node) {
      const text = textOf(node);
      return text && text.length <= 24;
    });

    if (!list.length) return null;

    list.sort(function (a, b) {
      const la = textOf(a).length;
      const lb = textOf(b).length;
      return lb - la;
    });

    return list[0];
  }

  function applyStylesToContainer(container) {
    if (!container) return;

    container.style.display = 'grid';
    container.style.gridTemplateColumns = 'repeat(6, minmax(0, 1fr))';
    container.style.gap = '10px 12px';
    container.style.alignItems = 'start';

    const cards = Array.from(container.children || []).filter(isElement);
    cards.forEach(function (card) {
      card.style.minWidth = '0';
      card.style.position = 'relative';

      const action = findAction(card);
      if (action) {
        action.style.width = '100%';
        action.style.minHeight = '84px';
        action.style.padding = '8px 4px';
        action.style.borderRadius = '14px';
        action.style.display = 'flex';
        action.style.flexDirection = 'column';
        action.style.alignItems = 'center';
        action.style.justifyContent = 'flex-start';
        action.style.gap = '8px';
        action.style.textAlign = 'center';
        action.style.boxSizing = 'border-box';
      }

      const icon = action ? findIconNode(action) : null;
      if (icon) {
        icon.style.width = '34px';
        icon.style.height = '34px';
        icon.style.minWidth = '34px';
        icon.style.minHeight = '34px';
        icon.style.flex = '0 0 34px';
        icon.style.borderRadius = '10px';
      }

      const title = action ? findTitleNode(action) : null;
      if (title) {
        title.style.width = '100%';
        title.style.maxWidth = '100%';
        title.style.fontSize = '12px';
        title.style.lineHeight = '16px';
        title.style.fontWeight = '500';
        title.style.textAlign = 'center';
        title.style.whiteSpace = 'nowrap';
        title.style.overflow = 'hidden';
        title.style.textOverflow = 'ellipsis';
      }

      const deleteBtn = card.querySelector('[class*="delete"], [class*="Delete"], [class*="remove"], [class*="Remove"], [class*="close"], [class*="Close"]');
      if (deleteBtn) {
        deleteBtn.style.width = '18px';
        deleteBtn.style.height = '18px';
        deleteBtn.style.minWidth = '18px';
        deleteBtn.style.minHeight = '18px';
        deleteBtn.style.top = '4px';
        deleteBtn.style.right = '4px';
        deleteBtn.style.borderRadius = '999px';
        deleteBtn.style.fontSize = '11px';
        deleteBtn.style.lineHeight = '18px';
      }
    });
  }

  function applyNavSixColumnsOnce() {
    const panel = getNavPanel();
    if (!panel) return;

    const container = pickBestContainer(panel);
    if (!container) return;

    applyStylesToContainer(container);
  }

  function patchRenderNav() {
    if (typeof renderNav !== 'function' || renderNav.__v19Patched) return;

    const original = renderNav;
    renderNav = function () {
      const result = original.apply(this, arguments);
      window.setTimeout(applyNavSixColumnsOnce, 0);
      window.setTimeout(applyNavSixColumnsOnce, 120);
      return result;
    };
    renderNav.__v19Patched = true;
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function () {
      patchRenderNav();
      applyNavSixColumnsOnce();
    });
  } else {
    patchRenderNav();
    applyNavSixColumnsOnce();
  }

  window.setTimeout(function () {
    patchRenderNav();
    applyNavSixColumnsOnce();
  }, 260);
})();
