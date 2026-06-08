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

        // GET: api/posts — Получение всех постов
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

        // POST: api/posts — Создание нового поста
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] Post newPost)
        {
            if (newPost == null)
            {
                return BadRequest(new { error = "Не удалось десериализовать данные формы." });
            }

            try
            {
                var currentPosts = await _firebaseStorage.LoadAsync() ?? new List<Post>();
                newPost.Id = Math.Abs(Guid.NewGuid().GetHashCode());

                currentPosts.Add(newPost);
                await _firebaseStorage.SaveAllAsync(currentPosts);

                return Ok(newPost);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Ошибка базы данных: {ex.Message}" });
            }
        }

        // PUT: api/posts/{id} — Редактирование существующего поста
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] Post updatedPost)
        {
            if (updatedPost == null) return BadRequest(new { error = "Данные для обновления не указаны." });

            try
            {
                var currentPosts = await _firebaseStorage.LoadAsync() ?? new List<Post>();
                var existingPost = currentPosts.FirstOrDefault(p => p.Id == id);

                if (existingPost == null)
                {
                    return NotFound(new { error = $"Пост с ID {id} не найден." });
                }

                // Обновляем поля (на всякий случай проверяем и Title, и текстовые поля)
                existingPost.Title = updatedPost.Title;

                // Динамически ищем, какое поле используется в твоей C# модели
                var textProperty = typeof(Post).GetProperties().FirstOrDefault(p => p.Name == "Text" || p.Name == "Content" || p.Name == "Description");
                if (textProperty != null)
                {
                    var newValue = typeof(Post).GetProperties()
                        .Where(p => p.Name == "Text" || p.Name == "Content" || p.Name == "Description")
                        .Select(p => p.GetValue(updatedPost))
                        .FirstOrDefault(v => v != null && !string.IsNullOrEmpty(v.ToString()));

                    if (newValue != null) textProperty.SetValue(existingPost, newValue.ToString());
                }

                await _firebaseStorage.SaveAllAsync(currentPosts);
                return Ok(existingPost);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Ошибка при изменении: {ex.Message}" });
            }
        }

        // DELETE: api/posts/{id} — Удаление поста
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                var currentPosts = await _firebaseStorage.LoadAsync() ?? new List<Post>();
                var postToDelete = currentPosts.FirstOrDefault(p => p.Id == id);

                if (postToDelete == null)
                {
                    return NotFound(new { error = $"Пост с ID {id} не найден." });
                }

                currentPosts.Remove(postToDelete);
                await _firebaseStorage.SaveAllAsync(currentPosts);

                return Ok(new { message = "Пост успешно удален." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Ошибка при удалении: {ex.Message}" });
            }
        }
    }
}