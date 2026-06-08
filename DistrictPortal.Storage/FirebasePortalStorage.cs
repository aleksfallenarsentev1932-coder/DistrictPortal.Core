using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using DistrictPortal.Core.Models;

namespace DistrictPortal.Storage.Services
{
    public class FirebasePortalStorage
    {
        private readonly FirebaseClient _firebase;
        private const string ChildName = "posts";

        // Конструктор принимает URL твоей базы данных Firebase
        public FirebasePortalStorage(string firebaseEndpoint, string authSecret = null)
        {
            _firebase = new FirebaseClient(
                firebaseEndpoint,
                new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(authSecret) }
            );
        }

        // Асинхронная загрузка всех постов из Firebase при старте сайта
        public async Task<List<Post>> LoadAsync()
        {
            try
            {
                var firebaseObjects = await _firebase
                    .Child(ChildName)
                    .OnceAsync<Post>();

                return firebaseObjects.Select(x => x.Object).ToList();
            }
            catch (Exception)
            {
                // Если база пустая или возникла ошибка, возвращаем пустой список
                return new List<Post>();
            }
        }

        // Асинхронное сохранение (перезапись) всех постов при добавлении или редактировании
        public async Task SaveAllAsync(List<Post> posts)
        {
            // Полностью очищаем старый узел в Firebase, чтобы данные не дублировались
            await _firebase.Child(ChildName).DeleteAsync();

            // Записываем каждый пост под своим уникальным ID
            foreach (var post in posts)
            {
                await _firebase
                    .Child(ChildName)
                    .Child(post.Id.ToString())
                    .PutAsync(post);
            }
        }
    }
}