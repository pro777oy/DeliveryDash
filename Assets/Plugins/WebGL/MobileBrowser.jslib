mergeInto(LibraryManager.library, {
  IsMobileBrowser: function () {
    var mobileUserAgent = /Android|iPhone|iPad|iPod|IEMobile|Opera Mini/i.test(navigator.userAgent);
    var touchCapableIPad = navigator.platform === "MacIntel" && navigator.maxTouchPoints > 1;
    return mobileUserAgent || touchCapableIPad ? 1 : 0;
  }
});
