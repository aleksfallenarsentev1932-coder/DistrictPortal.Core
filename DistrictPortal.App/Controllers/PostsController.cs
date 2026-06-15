using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DistrictPortal.Core.Models;
using DistrictPortal.Storage.Services;

namespace DistrictPortal.App.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class PostsController : ControllerBase
    {
        private readonly FirebasePortalStorage _firebaseStorage;

        public PostsController(FirebasePortalStorage firebaseStorage)
        {
            _firebaseStorage = firebaseStorage;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPosts()
        {
            try
            {
                var posts = await _firebaseStorage.LoadAsync() ?? new List<Post>();
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] PostDetailDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Title))
            {
                return BadRequest(new { error = "Заголовок не может быть пустым." });
            }

            try
            {
                var currentPosts = await _firebaseStorage.LoadAsync() ?? new List<Post>();

                var newPost = new Post
                {
                    Id = Math.Abs(Guid.NewGuid().GetHashCode()),
                    Title = dto.Title
                };

                var textProperty = typeof(Post).GetProperties()
                    .FirstOrDefault(p => p.Name == "Text" || p.Name == "Content" || p.Name == "Description");

                if (textProperty != null)
                {
                    textProperty.SetValue(newPost, dto.Content);
                }

                currentPosts.Add(newPost);
                await _firebaseStorage.SaveAllAsync(currentPosts);

                return Ok(newPost);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Ошибка базы данных: {ex.Message}" });
            }
        }

        [HttpPost("{id:int}/comments")]
        public async Task<IActionResult> AddComment(int id, [FromBody] CommentDto comment)
        {
            if (comment == null || string.IsNullOrEmpty(comment.Text))
                return BadRequest(new { error = "Текст комментария пуст." });

            return Ok(new { author = comment.Author ?? "Сосед", text = comment.Text, date = DateTime.Now.ToString("dd.MM.yyyy HH:mm") });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                var currentPosts = await _firebaseStorage.LoadAsync() ?? new List<Post>();
                var target = currentPosts.FirstOrDefault(p => p.Id == id);

                if (target == null)
                {
                    return NotFound(new { error = "Пост не найден." });
                }

                currentPosts.Remove(target);
                await _firebaseStorage.SaveAllAsync(currentPosts);

                return Ok(new { message = "Успешно удалено" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class PostDetailDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class CommentDto
    {
        public string Author { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}