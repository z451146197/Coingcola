(function () {
  function normalizeGroupName(value) {
    const text = String(value ?? '').trim();
    return text || '未分组';
  }

  function ensureGroupingStyle() {
    if (document.getElementById('home-nav-grouping-style')) return;
    const style = document.createElement('style');
    style.id = 'home-nav-grouping-style';
    style.textContent = `
      .nav-group-section {
        display: flex;
        flex-direction: column;
        gap: 10px;
        margin-bottom: 14px;
      }
      .nav-group-section:last-child {
        margin-bottom: 0;
      }
      .nav-group-title {
        display: flex;
        align-items: center;
        min-height: 20px;
        padding: 0 2px;
        font-size: 12px;
        line-height: 20px;
        font-weight: 600;
        color: #64748b;
      }
      .nav-group-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(92px, 1fr));
        gap: 12px;
      }
    `;
    document.head.appendChild(style);
  }

  function findTileId(tile) {
    const mainButton = tile.querySelector('.nav-tile-btn');
    const deleteButton = tile.querySelector('.nav-delete');
    return mainButton?.getAttribute('data-id')
      || mainButton?.getAttribute('data-site-id')
      || deleteButton?.getAttribute('data-id')
      || deleteButton?.getAttribute('data-site-id')
      || '';
  }

  function regroupNavTiles() {
    if (typeof state === 'undefined' || typeof el === 'undefined' || !el.navGrid) return;
    if (!Array.isArray(state.navItems) || !state.navItems.length) return;
    if (state.navEditing) return;

    ensureGroupingStyle();

    const tileNodes = Array.from(el.navGrid.children).filter((node) => node.classList?.contains('nav-tile'));
    if (!tileNodes.length) return;

    const tileMap = new Map();
    tileNodes.forEach((tile) => {
      const id = findTileId(tile);
      if (id) tileMap.set(id, tile);
    });

    const orderedGroups = [];
    const groupedItems = new Map();
    state.navItems.forEach((item) => {
      const groupName = normalizeGroupName(item.group);
      if (!groupedItems.has(groupName)) {
        groupedItems.set(groupName, []);
        orderedGroups.push(groupName);
      }
      groupedItems.get(groupName).push(item);
    });

    if (orderedGroups.length <= 1) return;

    const fragment = document.createDocumentFragment();

    orderedGroups.forEach((groupName) => {
      const section = document.createElement('section');
      section.className = 'nav-group-section';
      section.setAttribute('data-nav-group', groupName);

      const title = document.createElement('div');
      title.className = 'nav-group-title';
      title.textContent = groupName;
      section.appendChild(title);

      const grid = document.createElement('div');
      grid.className = 'nav-group-grid';

      groupedItems.get(groupName).forEach((item) => {
        const tile = tileMap.get(String(item.id || ''));
        if (tile) {
          grid.appendChild(tile);
        }
      });

      section.appendChild(grid);
      fragment.appendChild(section);
    });

    if (!fragment.childNodes.length) return;

    while (el.navGrid.firstChild) {
      el.navGrid.removeChild(el.navGrid.firstChild);
    }
    el.navGrid.appendChild(fragment);
  }

  const originalRenderNav = typeof renderNav === 'function' ? renderNav : null;
  if (!originalRenderNav) return;

  renderNav = function () {
    originalRenderNav();
    try {
      regroupNavTiles();
    } catch (error) {
      console.warn('nav grouping patch failed', error);
    }
  };
})();