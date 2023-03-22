﻿using DnsClient;
using Infrastructuur.Dtos;
using Infrastructuur.Entities;
using Infrastructuur.Extensions;
using Infrastructuur.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using System.Security.Claims;

namespace InventoryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IRepository<UserEntity> _userRepository;

        public UserController(IRepository<UserEntity> userRepository)
        {
            _userRepository = userRepository;
        }
        [HttpGet("GetAllUsers")]
        public async Task<ActionResult<List<UserEntity>>> GetAllUsers()
        {
            var users = await _userRepository.GetAllAsync();
            return Ok(users);
        }
        [HttpGet("GetUserById/{id}")]
        public async Task<IActionResult> GetUserById([FromRoute] int id)
        {
            var user = await _userRepository.GetByIdAsync(x => x.UserId == id);
            if (user is null) return NotFound();
            return Ok(user);
        }
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] UserDto user)
        {
          
            var users = await _userRepository.GetAllAsync();
            if (users.Any(x => x.UserName == user.UserName || x.Email == user.Email)) return BadRequest();
            // TODO : map this
            var userToAdd = new UserEntity();
            userToAdd.UserName = user.UserName;
            userToAdd.Password = user.Password;
            userToAdd.Email = user.Email;
            userToAdd.Address = user.Address;
            userToAdd.Name = user.Name;
            userToAdd.Role = user.Role;
            userToAdd.UserId = 1;
            if (users.Any(x => x.UserId > 0))
            {
                userToAdd.UserId = users.Max(x => x.UserId) + 1;
            }
            userToAdd.Password = user.Password.HashToPassword();
            await _userRepository.AddAsync(userToAdd);
            return CreatedAtAction(nameof(GetUserById), new { id = userToAdd.UserName }, userToAdd);
        }
        [HttpPut("UpdateUserById/{id}")]
        public async Task<IActionResult> UpdateUserById(int id, UserDto user)
        {
            // TODO : map this
            var existingUser = await _userRepository.GetByIdAsync(x => x.UserId == id);
            if (existingUser is null) return NotFound();
            existingUser.UserName = user.UserName;
            existingUser.Password = user.Password;
            existingUser.Email = user.Email;
            existingUser.Address = user.Address;
            existingUser.Name = user.Name;
            existingUser.Role = user.Role;
            existingUser.Password = existingUser.Password.HashToPassword();
            await _userRepository.UpdateAsync(x => x.UserId == id, existingUser);
            return NoContent();
        }
        [HttpDelete("DeleteUserById/{id}")]
        public async Task<IActionResult> DeleteCategoryById(int id)
        {
            var existingUser = await _userRepository.GetByIdAsync(x => x.UserId == id);
            if (existingUser == null) return NotFound();
            await _userRepository.DeleteAsync(x => x.UserId == id);
            return NoContent();
        }
        [HttpPost("GetUserByUserNameAndPassWord")]
        public async Task<IActionResult> GetUserByUserNameAndPassWord([FromBody] LoginRequestEnity userEntity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = (await _userRepository.GetAllAsync())
                .FirstOrDefault(x => x.UserName == userEntity.UserName);
            if (user == null) return NotFound();
         
            if (!HashPassword.VerifyPassword(userEntity.Password, user.Password)) return BadRequest();
            
            return Ok(user);
        }
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromQuery] LoginRequestEnity userE, [FromBody] UserDto userDto)
        {

            var existingUser = (await _userRepository.GetAllAsync())
                .FirstOrDefault(x => x.UserName == userDto.UserName);
            if (existingUser == null) return NotFound();

            if (!HashPassword.VerifyPassword(userE.Password, existingUser.Password)) return BadRequest();
            existingUser.UserName = userDto.UserName;
            existingUser.Password = userDto.Password;
            existingUser.Email = userDto.Email;
            existingUser.Address = userDto.Address;
            existingUser.Name = userDto.Name;
            existingUser.Role = userDto.Role;
            existingUser.Password = existingUser.Password.HashToPassword();
            await _userRepository.UpdateAsync(x => x.UserId == existingUser.UserId, existingUser);
            return NoContent();
        }
    }
}
