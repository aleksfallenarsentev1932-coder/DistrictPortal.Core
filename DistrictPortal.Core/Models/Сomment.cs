using System;
using System.Collections.Generic;

namespace DistrictPortal.Core.Models
{
    public class Comment
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Author { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Список для ответов внутри этого комментария
        public List<Comment> Replies { get; set; } = new List<Comment>();
    }
}