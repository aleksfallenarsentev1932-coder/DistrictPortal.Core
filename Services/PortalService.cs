using System;
using System.Collections.Generic;
using System.Linq;
using DistrictPortal.Core.Models;
using DistrictPortal.Core.Services;
using DistrictPortal.Core.Reports;

namespace DistrictPortal.Core.Services;

public class PortalService
{
    private readonly List<Post> _posts;
    private int _nextId;

    public PortalService(List<Post>? initialPosts = null)
    {
        _posts = initialPosts ?? new List<Post>();
        _nextId = _posts.Count == 0 ? 1 : _posts.Max(p => p.Id) + 1;
    }


    public Post AddPost(string title, string description, PostCategory category)
    {
        TaskValidator.Validate(title, description);

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

    public void UpdatePost(int id, string title, string description)
    {
        TaskValidator.Validate(title, description); 

        var post = GetExisting(id);
        post.Title = title.Trim();
        post.Description = description.Trim();
    }

    public List<Post> GetAllPosts()
    {
        return _posts.ToList();
    }

    private Post GetExisting(int id)
    {
        var post = _posts.FirstOrDefault(p => p.Id == id);
        if (post is null)
            throw new ArgumentException($"Объявление с Id={id} не найдено.");
        return post;
    }

    public Post ChangeCategory(int id, PostCategory newCategory)
    {
        var post = GetExisting(id);
        post.Category = newCategory;
        return post;
    }

    public void Delete(int id)
    {
        var post = GetExisting(id);
        _posts.Remove(post);
    }

    public List<Post> SearchByTitle(string query)
    {
        query ??= "";
        query = query.Trim();
        if (query.Length == 0) return GetAllPosts();

        return _posts
            .Where(p => (p.Title ?? "").Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<Post> FilterByCategory(PostCategory? category)
    {
        if (category is null) return GetAllPosts();
        return _posts.Where(p => p.Category == category).ToList();
    }

    public List<Post> SortById(bool ascending = true)
    {
        return ascending
            ? _posts.OrderBy(p => p.Id).ToList()
            : _posts.OrderByDescending(p => p.Id).ToList();
    }

    public void ReplaceAll(List<Post> newPosts)
    {
        newPosts ??= new List<Post>();
        _posts.Clear();
        _posts.AddRange(newPosts);
        _nextId = _posts.Count == 0 ? 1 : _posts.Max(p => p.Id) + 1;
    }
    public TaskStats GetStats()
    {
        var stats = new TaskStats();
        stats.Total = _posts.Count;
        foreach (var p in _posts)
        {
            if (p.Category == PostCategory.News) stats.NewCount++;
            else if (p.Category == PostCategory.Services) stats.InProgressCount++;
            else stats.DoneCount++;
        }
        return stats;
    }
}