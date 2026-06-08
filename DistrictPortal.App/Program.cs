using System;
using System.IO;
using System.Collections.Generic;
using DistrictPortal.Core.Models;
using DistrictPortal.Core.Services;
using DistrictPortal.Storage.Services; 

var dataFilePath = Path.Combine(AppContext.BaseDirectory, "data", "posts.json");
var storage = new JsonPortalStorage(dataFilePath);
var loadedPosts = storage.Load();
var service = new PortalService(loadedPosts);
var backupsFolder = Path.Combine(AppContext.BaseDirectory, "backups");
var exportsFolder = Path.Combine(AppContext.BaseDirectory, "exports");


var logsFolder = Path.Combine(AppContext.BaseDirectory, "logs");
var logger = new AppLogger(logsFolder);
logger.Info("DistrictPortal started");

Console.WriteLine($"=== СИСТЕМА ХРАНЕНИЯ ДАННЫХ ИНИЦИАЛИЗИРОВАНА ===");
Console.WriteLine($"Путь к файлу данных: {dataFilePath}");
Console.WriteLine($"Успешно загружено объявлений из базы: {loadedPosts.Count}");
Console.WriteLine("================================================\n");

static void PrintPosts(List<Post> posts)
{
    if (posts.Count == 0) { Console.WriteLine("\nНичего не найдено."); return; }
    foreach (var p in posts)
    {
        Console.WriteLine($"\n[#{p.Id}] {p.Title} | Категория: {p.Category}");
        if (!string.IsNullOrWhiteSpace(p.Description))
            Console.WriteLine($" Описание: {p.Description}");
        Console.WriteLine("--------------------------------------------");
    }
}

while (true)
{
    Console.WriteLine();
    Console.WriteLine("=== Районный портал 'DistrictPortal' v0.5 ===");
    Console.WriteLine("1) Добавить, 2) Лента, 5) Редактировать");
    Console.WriteLine("6) Поиск, 7) Фильтр, 8) Сортировка");
    Console.WriteLine("9) Backup, 10) Экспорт, 11) Импорт");
    Console.WriteLine("12) Статистика, 13) Экспорт отчёта, 14) Логи, 0) Выход");
    Console.Write("Выберите пункт меню: ");

    var input = Console.ReadLine();
    if (input == "0") break;

    if (input == "1")
    {
        Console.Write("Введите заголовок: ");
        var title = Console.ReadLine() ?? "";
        Console.Write("Введите описание: ");
        var description = Console.ReadLine() ?? "";
        Console.WriteLine("Категория: 0-Новости, 1-Услуги, 2-Находки, 3-Мероприятия");
        if (int.TryParse(Console.ReadLine(), out int catId) && catId >= 0 && catId <= 3)
        {
            try
            {
                TaskValidator.Validate(title, description);
                var newPost = service.AddPost(title, description, (PostCategory)catId);
                storage.Save(service.GetAllPosts());
                logger.Info($"ADD id={newPost.Id} title=\"{newPost.Title}\"");
                Console.WriteLine($"[Успех] Объявление #{newPost.Id} добавлено.");
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
    }
    else if (input == "2") PrintPosts(service.GetAllPosts());
    else if (input == "5")
    {
        Console.Write("Введите ID для редактирования: ");
        if (int.TryParse(Console.ReadLine(), out int id))
        {
            Console.Write("Новый заголовок: "); var title = Console.ReadLine() ?? "";
            Console.Write("Новое описание: "); var description = Console.ReadLine() ?? "";
            try
            {
                TaskValidator.Validate(title, description);
                service.UpdatePost(id, title, description);
                storage.Save(service.GetAllPosts());
                logger.Info($"UPDATE id={id} title=\"{title}\""); 
                Console.WriteLine("Обновлено.");
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
    }
    else if (input == "6") { Console.Write("Поиск: "); PrintPosts(service.SearchByTitle(Console.ReadLine() ?? "")); }
    else if (input == "7") { Console.WriteLine("Категория (0-3): "); if (int.TryParse(Console.ReadLine(), out int c)) PrintPosts(service.FilterByCategory((PostCategory)c)); }
    else if (input == "8") { Console.WriteLine("1-Id (возр), 2-Id (убыв)"); PrintPosts(Console.ReadLine() == "2" ? service.SortById(false) : service.SortById(true)); }
    else if (input == "9") { try { BackupService.CreateBackup(dataFilePath, backupsFolder); logger.Info($"BACKUP created"); Console.WriteLine("Бэкап создан."); } catch (Exception ex) { Console.WriteLine(ex.Message); } }
    else if (input == "10") { var exportFile = Path.Combine(exportsFolder, $"posts_{DateTime.Now:yyyyMMdd}.json"); new JsonPortalStorage(exportFile).Save(service.GetAllPosts()); logger.Info($"EXPORT file={exportFile}"); Console.WriteLine("Экспорт JSON."); }
    else if (input == "11")
    {
        Console.Write("Путь к файлу: "); var path = Console.ReadLine() ?? "";
        try
        {
            var data = new JsonPortalStorage(path).Load();
            foreach (var p in data) TaskValidator.Validate(p.Title, p.Description);
            service.ReplaceAll(data);
            storage.Save(service.GetAllPosts());
            logger.Info($"IMPORT success from {path}");
            Console.WriteLine("Импорт успешен.");
        }
        catch (Exception ex) { logger.Error($"IMPORT failed: {ex.Message}"); Console.WriteLine($"Ошибка: {ex.Message}"); }
    }
    else if (input == "12")
    {
        var stats = service.GetStats();
        Console.WriteLine($"\nСтатистика:\nВсего: {stats.Total}\nNew: {stats.NewCount}\nInProgress: {stats.InProgressCount}\nDone: {stats.DoneCount}");
    }
    else if (input == "13")
    {
        try
        {
            var rFolder = Path.Combine(AppContext.BaseDirectory, "reports");
            Directory.CreateDirectory(rFolder);
            var stats = service.GetStats();
            var path = Path.Combine(rFolder, $"report_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");
            File.WriteAllLines(path, new[] { "Report", $"Total: {stats.Total}", $"New: {stats.NewCount}", $"InProgress: {stats.InProgressCount}", $"Done: {stats.DoneCount}" });
            logger.Info($"EXPORT report to {path}");
            Console.WriteLine($"Отчёт сохранён: {path}");
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); }
    }
    else if (input == "14") 
    {
        try
        {
            var logFile = Path.Combine(logsFolder, $"app_{DateTime.Now:yyyy-MM-dd}.log");
            if (!File.Exists(logFile)) { Console.WriteLine("Лог за сегодня не найден."); continue; }
            var lines = File.ReadAllLines(logFile);
            Console.WriteLine("Последние 20 строк лога:");
            int start = Math.Max(0, lines.Length - 20);
            for (int i = start; i < lines.Length; i++) Console.WriteLine(lines[i]);
        }
        catch (Exception ex) { Console.WriteLine($"Ошибка чтения лога: {ex.Message}"); }
    }
    else Console.WriteLine("Неизвестная команда.");
}