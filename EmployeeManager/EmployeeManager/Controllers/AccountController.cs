﻿using System.ComponentModel.DataAnnotations;
using EmployeeManager.DTOs;
using EmployeeManager.Models;
using EmployeeManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EmployeeManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TokenService _tokenService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger, UserManager<ApplicationUser> userManager, TokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// Login User
        /// </summary>
        /// <param name="loginDto">User Credentials</param>
        /// <returns>Session Token</returns>
        [HttpPost("login")]
        public async Task<ActionResult<UserTokenDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                if (user != null)
                {
                    _logger.LogInformation("Failed log in the user with email {Email}. [ Wrong password ]", credentialDto.Email);
                }
                return Unauthorized();
            }

            return new UserTokenDto 
            { 
                Email = user.Email,
                Token = await _tokenService.GenerateToken(user)
            };
        }

        /// <summary>
        /// Register new User
        /// </summary>
        /// <param name="registerDto">User Credentials</param>
        /// <returns>Status Message</returns>
        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto registerDto)
        {
            var user = new ApplicationUser 
            { 
                UserName = registerDto.Email, 
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                CNP = registerDto.LastName,
                Adress = registerDto.Adress,
                PhoneNumber = registerDto.PhoneNumber,
                PhotoUrl = registerDto.PhotoUrl
            };
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }

                return ValidationProblem();
            }

            await _userManager.AddToRoleAsync(user, "Employee");
            _logger.LogInformation("New account created with email {Email}", credentialDto.Email);
            return StatusCode(201);
        }

        /// <summary>
        /// Get Current User Session Token
        /// </summary>
        /// <returns>Session Token</returns>
        [Authorize]
        [HttpGet("currentUser")]
        public async Task<ActionResult<UserTokenDto>> GetCurrentUser()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            return new UserTokenDto
            {
                Email = user.Email,
                Token = await _tokenService.GenerateToken(user),
                Name = user.FirstName + " " + user.LastName
            };
        }
    }
}
