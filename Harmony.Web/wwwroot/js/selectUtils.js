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
  clearSelection: function (id) {
    const select = document.getElementById(id);
    if (select) {
      // This works for multi-select to clear all selections.
      select.selectedIndex = -1;
    }
  },
  registerDblClick: function (id, dotNetRef, methodName) {
    const el = document.getElementById(id);
    if (!el) return;
    
    // Always unregister any existing handler first to prevent duplicates
    if (el._harmonyDblHandler) {
      el.removeEventListener('dblclick', el._harmonyDblHandler);
      el._harmonyDblHandler = null;
    }
    if (el._harmonyDotNetRef) {
      try { el._harmonyDotNetRef.dispose(); } catch {}
      el._harmonyDotNetRef = null;
    }
    
    // Register the new handler
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

window.downloadFile = function (fileName, base64Data, contentType) {
  const byteCharacters = atob(base64Data);
  const byteNumbers = new Array(byteCharacters.length);
  for (let i = 0; i < byteCharacters.length; i++) {
    byteNumbers[i] = byteCharacters.charCodeAt(i);
  }
  const byteArray = new Uint8Array(byteNumbers);
  const blob = new Blob([byteArray], { type: contentType });
  
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = fileName;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  window.URL.revokeObjectURL(url);
};


