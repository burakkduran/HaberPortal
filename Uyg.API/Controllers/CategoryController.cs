using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uyg.API.DTOs;
using Uyg.API.Models;
using Uyg.API.Repositories;

namespace Uyg.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly ArticleRepository _articleRepository;
        private readonly IMapper _mapper;
        ResultDto _result = new ResultDto();

        public CategoryController(IMapper mapper, CategoryRepository categoryRepository, ArticleRepository articleRepository)
        {
            _mapper = mapper;
            _categoryRepository = categoryRepository;
            _articleRepository = articleRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<List<CategoryDto>> List()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var categoryDtos = _mapper.Map<List<CategoryDto>>(categories);
            return categoryDtos;
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<CategoryDto> GetById(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            var categoryDto = _mapper.Map<CategoryDto>(category);
            return categoryDto;
        }

        [HttpGet("{id}/Articles")]
        [AllowAnonymous]
        public async Task<List<ArticleDto>> ArticleList(int id)
        {
            var articles = await _articleRepository.Where(s => s.CategoryId == id)
                .Include(i => i.Category)
                .Include(i => i.User)
                .ToListAsync();
            var articleDtos = _mapper.Map<List<ArticleDto>>(articles);
            return articleDtos;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ResultDto> Add(CategoryDto model)
        {
            var list = _categoryRepository.Where(s => s.Name == model.Name).ToList();
            if (list.Count() > 0)
            {
                _result.Status = false;
                _result.Message = "Bu kategori adı zaten kullanılıyor!";
                return _result;
            }

            var category = _mapper.Map<Category>(model);
            category.Created = DateTime.Now;
            category.Updated = DateTime.Now;
            await _categoryRepository.AddAsync(category);
            _result.Status = true;
            _result.Message = "Kategori başarıyla eklendi";
            return _result;
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ResultDto> Update(Category model)
        {
            var category = await _categoryRepository.GetByIdAsync(model.Id);
            if (category == null)
            {
                _result.Status = false;
                _result.Message = "Kategori bulunamadı";
                return _result;
            }

            category.Name = model.Name;
            category.IsActive = model.IsActive;
            category.Updated = DateTime.Now;
            await _categoryRepository.UpdateAsync(category);
            _result.Status = true;
            _result.Message = "Kategori güncellendi";
            return _result;
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ResultDto> Delete(int id)
        {
            var list = await _articleRepository.Where(s => s.CategoryId == id).ToListAsync();
            if (list.Count() > 0)
            {
                _result.Status = false;
                _result.Message = "Bu kategoriye ait makaleler olduğu için silinemez!";
                return _result;
            }

            await _categoryRepository.DeleteAsync(id);
            _result.Status = true;
            _result.Message = "Kategori silindi";
            return _result;
        }
    }
}
