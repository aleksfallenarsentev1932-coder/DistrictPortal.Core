using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace DistrictPortal.App.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private static readonly Dictionary<string, string> SmsCodes = new Dictionary<string, string>();

        [HttpPost("send-code")]
        public IActionResult SendCode([FromBody] PhoneDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.PhoneNumber))
                return BadRequest(new { error = "Неверный номер телефона" });

            var rnd = new Random();
            var code = rnd.Next(1000, 9999).ToString();
            SmsCodes[dto.PhoneNumber] = code;

            
            System.Diagnostics.Debug.WriteLine($"\n======================================\n[SMS AUTH] Код для {dto.PhoneNumber}: {code}\n======================================\n");
            Console.WriteLine($"[SMS AUTH] Код для {dto.PhoneNumber}: {code}");

            return Ok(new { message = "Код отправлен! Загляни в консоль бэкенда в VS." });
        }

        [HttpPost("verify-code")]
        public IActionResult VerifyCode([FromBody] VerifyDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.PhoneNumber) || string.IsNullOrEmpty(dto.Code))
                return BadRequest(new { error = "Заполните все поля" });

            if (SmsCodes.TryGetValue(dto.PhoneNumber, out var savedCode) && savedCode == dto.Code)
            {
                SmsCodes.Remove(dto.PhoneNumber);
                return Ok(new { success = true, phoneNumber = dto.PhoneNumber });
            }

            return BadRequest(new { error = "Неверный SMS-код" });
        }
    }

    public class PhoneDto { public string PhoneNumber { get; set; } = string.Empty; }
    public class VerifyDto { public string PhoneNumber { get; set; } = string.Empty; public string Code { get; set; } = string.Empty; }
}