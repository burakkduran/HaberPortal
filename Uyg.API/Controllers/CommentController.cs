using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uyg.API.DTOs;
using Uyg.API.Models;
using Uyg.API.Repositories;

namespace Uyg.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly CommentRepository _commentRepository;
        private readonly IMapper _mapper;
        private readonly ResultDto _result = new ResultDto();

        public CommentController(CommentRepository commentRepository, IMapper mapper)
        {
            _commentRepository = commentRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<List<CommentDto>> List()
        {
            var comments = await _commentRepository.GetAllAsync();
            var commentDtos = _mapper.Map<List<CommentDto>>(comments);
            return commentDtos;
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<CommentDto> GetById(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            var commentDto = _mapper.Map<CommentDto>(comment);
            return commentDto;
        }

        [HttpGet("Article/{articleId}")]
        [AllowAnonymous]
        public async Task<List<CommentDto>> GetByArticle(int articleId)
        {
            var comments = await _commentRepository.GetByArticleIdAsync(articleId);
            var commentDtos = _mapper.Map<List<CommentDto>>(comments);
            return commentDtos;
        }

        [HttpPost]
        [Authorize]
        public async Task<ResultDto> Add(CreateCommentDto model)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _result.Status = false;
                _result.Message = "User not found";
                return _result;
            }

            var comment = new Comment
            {
                Content = model.Content,
                ArticleId = model.ArticleId,
                UserId = userId,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                IsActive = true
            };

            await _commentRepository.AddAsync(comment);
            _result.Status = true;
            _result.Message = "Comment added successfully";
            return _result;
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ResultDto> Update(int id, CreateCommentDto model)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var comment = await _commentRepository.GetByIdAsync(id);

            if (comment == null)
            {
                _result.Status = false;
                _result.Message = "Comment not found";
                return _result;
            }

            if (comment.UserId != userId)
            {
                _result.Status = false;
                _result.Message = "You are not authorized to update this comment";
                return _result;
            }

            comment.Content = model.Content;
            comment.Updated = DateTime.Now;

            await _commentRepository.UpdateAsync(comment);
            _result.Status = true;
            _result.Message = "Comment updated successfully";
            return _result;
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ResultDto> Delete(int id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var comment = await _commentRepository.GetByIdAsync(id);

            if (comment == null)
            {
                _result.Status = false;
                _result.Message = "Comment not found";
                return _result;
            }

            if (comment.UserId != userId)
            {
                _result.Status = false;
                _result.Message = "You are not authorized to delete this comment";
                return _result;
            }

            await _commentRepository.DeleteAsync(id);
            _result.Status = true;
            _result.Message = "Comment deleted successfully";
            return _result;
        }
    }
} 