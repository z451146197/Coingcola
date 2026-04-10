(function (window) {
  function escapeHtml(value) {
    return String(value ?? "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  function render(options) {
    const navGrid = options?.navGrid;
    const editTip = options?.editTip;
    const editNavText = options?.editNavText;
    const navItems = Array.isArray(options?.navItems) ? options.navItems : [];
    const navEditing = !!options?.navEditing;
    const emptyText = options?.emptyText || "暂无常用网站。";
    const onOpen = typeof options?.onOpen === "function" ? options.onOpen : null;
    const onDelete = typeof options?.onDelete === "function" ? options.onDelete : null;

    if (!navGrid) {
      return;
    }

    if (editNavText) {
      editNavText.textContent = navEditing ? "完成" : "编辑";
    }

    if (editTip && editTip.classList) {
      editTip.classList.toggle("is-hidden", !navEditing);
    }

    if (!navItems.length) {
      navGrid.innerHTML = `<div class="state-card"><div class="state-desc">${escapeHtml(emptyText)}</div></div>`;
      return;
    }

    navGrid.innerHTML = navItems.map((item) => `
      <div class="nav-tile${item.removable ? " is-removable" : ""}">
        <button class="nav-delete ${navEditing && item.removable ? "is-removable" : ""}" type="button" data-site-id="${escapeHtml(item.id)}" aria-label="删除 ${escapeHtml(item.title)}">×</button>
        <button class="nav-tile-btn" type="button" data-open-site="1" data-site-id="${escapeHtml(item.id)}">
          <span class="nav-icon">${escapeHtml(item.shortName || "站")}</span>
          <span class="nav-label">${escapeHtml(item.title)}</span>
        </button>
      </div>
    `).join("");

    const itemsById = new Map(navItems.map((item) => [String(item.id), item]));

    navGrid.querySelectorAll("[data-open-site]").forEach((btn) => {
      btn.addEventListener("click", () => {
        if (!onOpen) return;
        const siteId = btn.getAttribute("data-site-id") || "";
        const item = itemsById.get(siteId);
        if (!item) return;
        onOpen(item);
      });
    });

    navGrid.querySelectorAll(".nav-delete.is-removable").forEach((btn) => {
      btn.addEventListener("click", (event) => {
        event.stopPropagation();
        if (!onDelete) return;
        const siteId = btn.getAttribute("data-site-id") || "";
        const item = itemsById.get(siteId);
        if (!item) return;
        onDelete(item);
      });
    });
  }

  window.CoingcolaWebNavigation = {
    render
  };
})(window);
