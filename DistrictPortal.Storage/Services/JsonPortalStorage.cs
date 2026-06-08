using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using DistrictPortal.Core.Models;

namespace DistrictPortal.Storage.Services;

public class JsonPortalStorage
{
    private readonly string _filePath;

    public JsonPortalStorage(string filePath)
    {
        _filePath = filePath;
    }

    public List<Post> Load()
    {
        
        if (!File.Exists(_filePath))
            return new List<Post>();

        try
        {
            var json = File.ReadAllText(_filePath);
            var posts = JsonSerializer.Deserialize<List<Post>>(json);
            return posts ?? new List<Post>();
        }
        catch
        {
            
            Console.WriteLine("Предупреждение: файл данных поврежден. Начинаем с пустого списка.");
            return new List<Post>();
        }
    }

    public void Save(List<Post> posts)
    {
        var dir = Path.GetDirectoryName(_filePath);

       
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(posts, new JsonSerializerOptions
        {
            WriteIndented = true 
        });

        File.WriteAllText(_filePath, json);
    }
}