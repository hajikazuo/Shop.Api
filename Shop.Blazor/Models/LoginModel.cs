﻿using System.ComponentModel.DataAnnotations;

namespace Shop.Blazor.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Informe o email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Informe a senha")]
        public string? Password { get; set; }
    }
}
