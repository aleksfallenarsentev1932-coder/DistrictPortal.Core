using System;
using Xunit;
using DistrictPortal.Core.Services;

namespace DistrictPortal.Tests
{
    public class TaskValidatorTests
    {
        

        [Fact]
        public void Validate_CorrectData_ShouldNotThrow()
        {
            string title = "Сдам паркинг";
            string description = "Место на минус первом этаже, сухое, охрана есть.";

            var exception = Record.Exception(() => TaskValidator.Validate(title, description));
            Assert.Null(exception);
        }

        [Fact]
        public void Validate_BoundaryValidValues_ShouldNotThrow()
        {
            string title = new string('A', 50);
            string description = new string('B', 300);

            var exception = Record.Exception(() => TaskValidator.Validate(title, description));
            Assert.Null(exception);
        }

        [Fact]
        public void Validate_DescriptionIsNull_ShouldNotThrow()
        {
            string title = "Нормальный заголовок";
            string description = null;

            var exception = Record.Exception(() => TaskValidator.Validate(title, description));
            Assert.Null(exception);
        }


        [Fact]
        public void Validate_EmptyTitle_ShouldThrowException()
        {
            string title = "";
            string description = "Какое-то описание.";

            var exception = Assert.Throws<Exception>(() => TaskValidator.Validate(title, description));
            Assert.Equal("Ошибка: Заголовок не может быть пустым.", exception.Message);
        }

        [Fact]
        public void Validate_WhitespaceTitle_ShouldThrowException()
        {
            string title = "    ";
            string description = "Какое-то описание.";

            var exception = Assert.Throws<Exception>(() => TaskValidator.Validate(title, description));
            Assert.Equal("Ошибка: Заголовок не может быть пустым.", exception.Message);
        }

        [Fact]
        public void Validate_NullTitle_ShouldThrowException()
        {
            string title = null;
            string description = "Какое-то описание.";

            var exception = Assert.Throws<Exception>(() => TaskValidator.Validate(title, description));
            Assert.Equal("Ошибка: Заголовок не может быть пустым.", exception.Message);
        }

        [Fact]
        public void Validate_TooLongTitle_ShouldThrowException()
        {
            string title = new string('A', 51);
            string description = "Какое-то описание.";

            var exception = Assert.Throws<Exception>(() => TaskValidator.Validate(title, description));
            Assert.Equal("Ошибка: Заголовок должен быть не более 50 символов.", exception.Message);
        }

        [Fact]
        public void Validate_TooLongDescription_ShouldThrowException()
        {
            string title = "Нормальный заголовок";
            string description = new string('B', 301);

            var exception = Assert.Throws<Exception>(() => TaskValidator.Validate(title, description));
            Assert.Equal("Ошибка: Описание должно быть не более 300 символов.", exception.Message);
        }
    }
}