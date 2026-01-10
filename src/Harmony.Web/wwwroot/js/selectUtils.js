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
  // Decode base64 string to binary
  const binaryString = window.atob(base64Data);
  const bytes = new Uint8Array(binaryString.length);
  for (let i = 0; i < binaryString.length; i++) {
    bytes[i] = binaryString.charCodeAt(i);
  }
  
  const blob = new Blob([bytes], { type: contentType });
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = fileName;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  window.URL.revokeObjectURL(url);
};

window.searchUtils = {
  registerF3Focus: function (elementId) {
    // Remove any existing handler first
    if (window._harmonyF3Handler) {
      document.removeEventListener('keydown', window._harmonyF3Handler);
      window._harmonyF3Handler = null;
    }
    
    // Create new handler
    const handler = function (e) {
      // Check if F3 is pressed
      if (e.key === 'F3') {
        const activeElement = document.activeElement;
        const searchInput = document.getElementById(elementId);
        
        // If search input doesn't exist, do nothing
        if (!searchInput) return;
        
        // If search input is already focused, do nothing (let user continue typing)
        if (activeElement === searchInput) return;
        
        // Prevent default F3 behavior (browser search)
        e.preventDefault();
        
        // Focus and select the search input
        searchInput.focus();
        searchInput.select(); // Select existing text for easy replacement
      }
    };
    
    document.addEventListener('keydown', handler);
    window._harmonyF3Handler = handler;
  },
  
  unregisterF3Focus: function () {
    if (window._harmonyF3Handler) {
      document.removeEventListener('keydown', window._harmonyF3Handler);
      window._harmonyF3Handler = null;
    }
  }
};


