﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shop.Api.Services.Interface;
using Shop.Common.Models.DTO.Auth;
using Shop.Common.Models.Entities.Users;

namespace Shop.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenRepository;

        public AuthController(UserManager<User> userManager, ITokenService tokenRepository)
        {
            _userManager = userManager;
            _tokenRepository = tokenRepository;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var identityUser = await _userManager.FindByEmailAsync(request.Email);

            if (identityUser is not null)
            {
                var checkPasswordResult = await _userManager.CheckPasswordAsync(identityUser, request.Password);

                if (checkPasswordResult)
                {
                    var roles = await _userManager.GetRolesAsync(identityUser);

                    var jwtToken = _tokenRepository.CreateJwtToken(identityUser, roles.ToList());
                    var response = new LoginResponseDto()
                    {
                        Email = identityUser.Email,
                        Roles = roles.ToList(),
                        Token = jwtToken.Token,
                        Expiration = jwtToken.Expiration    
                    };

                    return Ok(response);
                }
            }
            ModelState.AddModelError("", "Invalid email or password");

            return ValidationProblem(ModelState);
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var user = new User
            {
                UserName = request.Email?.Trim(),
                Email = request.Email?.Trim()
            };

            var identityResult = await _userManager.CreateAsync(user, request.Password);

            if (identityResult.Succeeded)
            {
                identityResult = await _userManager.AddToRoleAsync(user, "Client");

                if (identityResult.Succeeded)
                {
                    return Ok();
                }
            }
            else
            {
                if (identityResult.Errors.Any())
                {
                    foreach (var error in identityResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return ValidationProblem(ModelState);
        }
    }
}
