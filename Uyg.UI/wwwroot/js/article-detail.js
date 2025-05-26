$(document).ready(function () {
  const articleId = new URLSearchParams(window.location.search).get("id");
  if (articleId) {
    loadArticleDetail(articleId);
  }
});

function loadArticleDetail(articleId) {
  $.ajax({
    url: `${API_URL}/Article/${articleId}`,
    type: "GET",
    success: function (response) {
      displayArticleDetail(response);
    },
    error: function (error) {
      console.error("Haber detayı yüklenirken hata oluştu:", error);
      $("#article-detail").html(
        '<div class="alert alert-danger">Haber detayı yüklenirken bir hata oluştu.</div>'
      );
    },
  });
}

function displayArticleDetail(article) {
  const articleHtml = `
        <div class="card">
            <img src="${API_URL}/Files/ArticleImages/${
    article.imageUrl
  }" class="card-img-top" alt="${article.title}">
            <div class="card-body">
                <h1 class="card-title">${article.title}</h1>
                <p class="text-muted">
                    <small>
                        Yazar: ${article.user?.fullName || "Anonim"} | 
                        Kategori: ${article.category?.name || "Genel"} | 
                        Tarih: ${new Date(
                          article.publishDate
                        ).toLocaleDateString("tr-TR")}
                    </small>
                </p>
                <div class="card-text">
                    ${article.content}
                </div>
            </div>
        </div>
    `;
  $("#article-detail").html(articleHtml);
}
