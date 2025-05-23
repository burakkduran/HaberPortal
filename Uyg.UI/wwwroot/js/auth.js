// Token yönetimi için fonksiyonlar
const tokenManager = {
  setToken: function (token) {
    localStorage.setItem("jwt_token", token);
  },
  getToken: function () {
    return localStorage.getItem("jwt_token");
  },
  removeToken: function () {
    localStorage.removeItem("jwt_token");
  },
  isAuthenticated: function () {
    return !!this.getToken();
  },
};

// AJAX istekleri için default ayarlar
$.ajaxSetup({
  beforeSend: function (xhr) {
    const token = tokenManager.getToken();
    if (token) {
      xhr.setRequestHeader("Authorization", "Bearer " + token);
    }
  },
});

// Yetkilendirme kontrolü
function checkAuth() {
  if (!tokenManager.isAuthenticated()) {
    window.location.href = "/Account/Login";
    return false;
  }
  return true;
}
