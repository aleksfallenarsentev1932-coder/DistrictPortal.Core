using System;

namespace DistrictPortal.Core.Services;

public static class TaskValidator
{
    public static void Validate(string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new Exception("Ошибка: Заголовок не может быть пустым.");

        if (title.Length > 50)
            throw new Exception("Ошибка: Заголовок должен быть не более 50 символов.");

        if (description != null && description.Length > 300)
            throw new Exception("Ошибка: Описание должно быть не более 300 символов.");
    }
}