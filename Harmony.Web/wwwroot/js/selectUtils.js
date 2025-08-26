window.selectUtils = {
  getSelectedValues: function (id) {
    const el = document.getElementById(id);
    if (!el) return [];
    // Works for both <select multiple> and standard selects
    const options = el.selectedOptions ? el.selectedOptions : el.options;
    return Array.from(options)
      .filter(o => o.selected)
      .map(o => o.value);
  },
  registerDblClick: function (id, dotNetRef, methodName) {
    const el = document.getElementById(id);
    if (!el) return;
    const handler = function (e) {
      const target = e.target;
      if (target && target.tagName === 'OPTION') {
        dotNetRef.invokeMethodAsync(methodName, target.value);
      }
    };
    el.addEventListener('dblclick', handler);
    el._harmonyDblHandler = handler;
    el._harmonyDotNetRef = dotNetRef;
  },
  unregisterDblClick: function (id) {
    const el = document.getElementById(id);
    if (!el) return;
    if (el._harmonyDblHandler) {
      el.removeEventListener('dblclick', el._harmonyDblHandler);
      el._harmonyDblHandler = null;
    }
    if (el._harmonyDotNetRef) {
      try { el._harmonyDotNetRef.dispose(); } catch {}
      el._harmonyDotNetRef = null;
    }
  }
};


