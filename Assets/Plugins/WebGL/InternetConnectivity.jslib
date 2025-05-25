mergeInto(LibraryManager.library, {
  IsOnline: function () {
    return navigator.onLine ? 1 : 0;
  },

  SetConnectivityCallback: function (callback) {
    window.addEventListener('online', function () {
      wasmTable.get(callback)(1);
    });
    window.addEventListener('offline', function () {
      wasmTable.get(callback)(0);
    });
  }
});
