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
                return Ok(MapPostsToDto(posts));
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
                return BadRequest(new { error = "Заголовок пуст" });

            try
            {
                var currentPosts = await _firebaseStorage.LoadAsync() ?? new List<Post>();

                if (!Enum.TryParse(dto.Category, true, out PostCategory parsedCat))
                {
                    parsedCat = PostCategory.News;
                }

                var newPost = new Post
                {
                    Id = Math.Abs(Guid.NewGuid().GetHashCode()),
                    Title = dto.Title,
                    Description = dto.Content,
                    Category = parsedCat,
                    CommentsList = new List<Comment>(),
                    ComplaintsCount = 0
                };

                currentPosts.Add(newPost);
                await _firebaseStorage.SaveAllAsync(currentPosts);

                return Ok(MapPostsToDto(currentPosts));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id:int}/report")]
        public async Task<IActionResult> ReportPost(int id)
        {
            try
            {
                var posts = await _firebaseStorage.LoadAsync() ?? new List<Post>();
                var post = posts.FirstOrDefault(p => p.Id == id);

                if (post == null)
                    return NotFound(new { error = "Пост не найден" });

                post.ComplaintsCount++;

                await _firebaseStorage.SaveAllAsync(posts);
                return Ok(MapPostsToDto(posts));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id:int}/unlock")]
        public async Task<IActionResult> UnlockPost(int id)
        {
            try
            {
                var posts = await _firebaseStorage.LoadAsync() ?? new List<Post>();
                var post = posts.FirstOrDefault(p => p.Id == id);

                if (post == null)
                    return NotFound(new { error = "Объявление не найдено" });

                post.ComplaintsCount = 0;

                await _firebaseStorage.SaveAllAsync(posts);
                return Ok(MapPostsToDto(posts));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id:int}/comments")]
        public async Task<IActionResult> AddComment(int id, [FromBody] CommentDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Text))
                return BadRequest(new { error = "Текст комментария пуст." });

            try
            {
                var currentPosts = await _firebaseStorage.LoadAsync() ?? new List<Post>();
                var post = currentPosts.FirstOrDefault(p => p.Id == id);

                if (post == null)
                    return NotFound(new { error = "Пост не найден." });

                if (post.CommentsList == null)
                    post.CommentsList = new List<Comment>();

                var newComment = new Comment
                {
                    Id = Guid.NewGuid().ToString(),
                    Author = string.IsNullOrEmpty(dto.Author) ? "Сосед" : dto.Author,
                    Text = dto.Text,
                    CreatedAt = DateTime.Now,
                    Replies = new List<Comment>()
                };

                if (!string.IsNullOrEmpty(dto.ParentCommentId))
                {
                    var rootComment = post.CommentsList.FirstOrDefault(c =>
                        c.Id == dto.ParentCommentId ||
                        (c.Replies != null && c.Replies.Any(r => r.Id == dto.ParentCommentId))
                    );

                    if (rootComment == null)
                        return NotFound(new { error = "Тред для комментария не найден." });

                    if (rootComment.Replies == null)
                        rootComment.Replies = new List<Comment>();

                    rootComment.Replies.Add(newComment);
                }
                else
                {
                    post.CommentsList.Add(newComment);
                }

                await _firebaseStorage.SaveAllAsync(currentPosts);
                return Ok(MapPostsToDto(currentPosts));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                var currentPosts = await _firebaseStorage.LoadAsync() ?? new List<Post>();
                var target = currentPosts.FirstOrDefault(p => p.Id == id);
                if (target == null) return NotFound();

                currentPosts.Remove(target);
                await _firebaseStorage.SaveAllAsync(currentPosts);

                return Ok(MapPostsToDto(currentPosts));
            }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        private List<object> MapPostsToDto(List<Post> posts)
        {
            return posts.Select(p => new {
                id = p.Id,
                title = p.Title,
                description = p.Description,
                category = p.Category.ToString(),
                complaintsCount = p.ComplaintsCount,
                commentsList = p.CommentsList ?? new List<Comment>()
            }).ToList<object>();
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
        public string ParentCommentId { get; set; } = string.Empty;
    }
}