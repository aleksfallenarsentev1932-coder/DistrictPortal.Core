using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DistrictPortal.Core.Models;

namespace DistrictPortal.Core.Services;

public class PortalService
{
    private readonly List<Post> _posts = new();
    private int _nextId = 1;


    public Post AddPost(string title, string description, PostCategory category)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Заголовок объявления не может быть пустым.");

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Описание объявления не может быть пустым.");

        var post = new Post
        {
            Id = _nextId++,
            Title = title.Trim(),
            Description = description.Trim(),
            Category = category
        };

        _posts.Add(post);
        return post;
    }

    public List<Post> GetAllPosts()
    {
        return _posts.ToList(); 
    }
}
