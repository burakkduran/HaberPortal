// Haberleri listele
function loadArticles() {
  if (!checkAuth()) return;

  $.ajax({
    url: "https://localhost:7218/api/Article",
    method: "GET",
    success: function (response) {
      const articlesContainer = $("#articles-container");
      articlesContainer.empty();

      response.forEach((article) => {
        const articleHtml = `
                    <div class="col-md-4 mb-4">
                        <div class="card h-100">
                            <div class="card-body">
                                <h5 class="card-title">${article.title}</h5>
                                <p class="card-text">${article.content.substring(
                                  0,
                                  150
                                )}...</p>
                                <div class="d-flex justify-content-between align-items-center">
                                    <small class="text-muted">${new Date(
                                      article.publishDate
                                    ).toLocaleDateString("tr-TR")}</small>
                                    <button class="btn btn-primary btn-sm" onclick="viewArticle(${
                                      article.id
                                    })">Devamını Oku</button>
                                </div>
                            </div>
                        </div>
                    </div>
                `;
        articlesContainer.append(articleHtml);
      });
    },
    error: function (xhr) {
      if (xhr.status === 401) {
        alert("Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.");
        window.location.href = "/Account/Login";
      } else {
        alert("Haberler yüklenirken bir hata oluştu.");
        console.error(xhr);
      }
    },
  });
}

// Sayfa yüklendiğinde haberleri getir
$(document).ready(function () {
  loadArticles();
});
