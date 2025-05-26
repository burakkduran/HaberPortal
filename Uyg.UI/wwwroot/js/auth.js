// API URL'si
const API_URL = "https://localhost:7218/api";

// Token yönetimi için fonksiyonlar
window.tokenManager = {
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
  getUserInfo: function () {
    return new Promise((resolve, reject) => {
      $.ajax({
        url: `${API_URL}/Account/GetUserInfo`,
        type: "GET",
        success: function (response) {
          resolve(response);
        },
        error: function (error) {
          reject(error);
        },
      });
    });
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
  // Login sayfasında isek kontrol yapma
  if (
    window.location.pathname === "/Account/Login" ||
    window.location.pathname === "/Account/Register"
  ) {
    return true;
  }

  if (!tokenManager.isAuthenticated()) {
    window.location.href = "/Account/Login";
    return false;
  }
  return true;
}
