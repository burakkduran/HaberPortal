using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Uyg.API.DTOs;
using Uyg.API.Models;
using Uyg.API.Repositories;

namespace Uyg.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly ArticleRepository _articleRepository;
        private readonly IMapper _mapper;
        ResultDto _result = new ResultDto();
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ArticleController(ArticleRepository articleRepository, IMapper mapper, IWebHostEnvironment webHostEnvironment)
        {
            _articleRepository = articleRepository;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<List<ArticleDto>> List()
        {
            var articles = await _articleRepository.GetAllAsync();
            var articleDtos = _mapper.Map<List<ArticleDto>>(articles);
            return articleDtos;
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ArticleDto> GetById(int id)
        {
            var article = await _articleRepository.GetByIdAsync(id);
            var articleDto = _mapper.Map<ArticleDto>(article);
            return articleDto;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Editor")]
        public async Task<ResultDto> Add(ArticleDto model)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _result.Status = false;
                _result.Message = "Kullanıcı girişi gerekli";
                return _result;
            }

            var article = _mapper.Map<Article>(model);
            article.Created = DateTime.Now;
            article.Updated = DateTime.Now;
            article.UserId = userId;
            article.ImageUrl = "default.jpg";
            article.PublishDate = DateTime.Now;
            article.IsPublished = true;
            article.ViewCount = 0;

            await _articleRepository.AddAsync(article);
            _result.Status = true;
            _result.Message = "Makale başarıyla eklendi";
            return _result;
        }

        [HttpPut]
        [Authorize(Roles = "Admin,Editor")]
        public async Task<ResultDto> Update(Article model)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var article = await _articleRepository.GetByIdAsync(model.Id);
            
            if (article == null)
            {
                _result.Status = false;
                _result.Message = "Makale bulunamadı";
                return _result;
            }

            // Admin her makaleyi düzenleyebilir, Editor sadece kendi makalelerini düzenleyebilir
            if (!User.IsInRole("Admin") && article.UserId != userId)
            {
                _result.Status = false;
                _result.Message = "Bu makaleyi düzenleme yetkiniz yok";
                return _result;
            }

            var updatedArticle = _mapper.Map<Article>(model);
            updatedArticle.Updated = DateTime.Now;
            await _articleRepository.UpdateAsync(updatedArticle);
            _result.Status = true;
            _result.Message = "Makale güncellendi";
            return _result;
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Editor")]
        public async Task<ResultDto> Delete(int id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var article = await _articleRepository.GetByIdAsync(id);
            
            if (article == null)
            {
                _result.Status = false;
                _result.Message = "Makale bulunamadı";
                return _result;
            }

            // Admin her makaleyi silebilir, Editor sadece kendi makalelerini silebilir
            if (!User.IsInRole("Admin") && article.UserId != userId)
            {
                _result.Status = false;
                _result.Message = "Bu makaleyi silme yetkiniz yok";
                return _result;
            }

            await _articleRepository.DeleteAsync(id);
            _result.Status = true;
            _result.Message = "Makale silindi";
            return _result;
        }

        [Route("Upload")]
        [HttpPost]
        [Authorize(Roles = "Admin,Editor")]
        public async Task<ResultDto> Upload(ArticleUploadDto dto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var article = await _articleRepository.GetByIdAsync(dto.ArticleId);
            
            if (article == null)
            {
                _result.Status = false;
                _result.Message = "Makale bulunamadı!";
                return _result;
            }

            // Admin her makalenin görselini değiştirebilir, Editor sadece kendi makalelerinin görselini değiştirebilir
            if (!User.IsInRole("Admin") && article.UserId != userId)
            {
                _result.Status = false;
                _result.Message = "Bu makalenin görselini değiştirme yetkiniz yok";
                return _result;
            }

            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot/Files/ArticleImages");
            string articleImage = article.ImageUrl;

            if (articleImage != "default.jpg")
            {
                var articleImageUrl = Path.Combine(path, articleImage);
                if (System.IO.File.Exists(articleImageUrl))
                {
                    System.IO.File.Delete(articleImageUrl);
                }
            }

            string data = dto.ImageData;
            string base64 = data.Substring(data.IndexOf(',') + 1);
            base64 = base64.Trim('\0');
            byte[] imageBytes = Convert.FromBase64String(base64);
            string filePath = Guid.NewGuid().ToString() + dto.ImageExt;

            var picPath = Path.Combine(path, filePath);
            System.IO.File.WriteAllBytes(picPath, imageBytes);

            article.ImageUrl = filePath;
            await _articleRepository.UpdateAsync(article);

            _result.Status = true;
            _result.Message = "Makale görseli güncellendi";

            return _result;
        }

        [HttpGet("Category/{categoryId}")]
        [AllowAnonymous]
        public async Task<List<ArticleDto>> GetByCategory(int categoryId)
        {
            var articles = await _articleRepository.GetAllAsync();
            var categoryArticles = articles.Where(x => x.CategoryId == categoryId).ToList();
            var articleDtos = _mapper.Map<List<ArticleDto>>(categoryArticles);
            return articleDtos;
        }

        [HttpGet("User/{userId}")]
        [AllowAnonymous]
        public async Task<List<ArticleDto>> GetByUser(string userId)
        {
            var articles = await _articleRepository.GetAllAsync();
            var userArticles = articles.Where(x => x.UserId == userId).ToList();
            var articleDtos = _mapper.Map<List<ArticleDto>>(userArticles);
            return articleDtos;
        }
    }
}
