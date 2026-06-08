using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DistrictPortal.Storage.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Добавляем поддержку контроллеров (это исправит ошибку 405)
builder.Services.AddControllers();

// 2. Регистрируем FirebasePortalStorage как Singleton, чтобы он был доступен контроллерам
builder.Services.AddSingleton<FirebasePortalStorage>(provider =>
{
    // Твой URL из консоли Realtime Database
    var firebaseUri = "https://portal-acfea-default-rtdb.europe-west1.firebasedatabase.app/";
    return new FirebasePortalStorage(firebaseUri);
});

var app = builder.Build();

// 3. Настраиваем раздачу статических файлов (чтобы index.html работал из wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

// 4. Явно маппим маршруты контроллеров (чтобы [HttpPost] в api/posts заработал)
app.MapControllers();

app.Run();