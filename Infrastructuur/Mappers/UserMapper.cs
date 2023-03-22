using Infrastructuur.Dtos;
using Infrastructuur.Entities;
using Infrastructuur.Extensions;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructuur.Mappers
{
    public static class UserMapper
    {
        public static UserEntity Map(UserDto userDto, ObjectId id, int userId)
        {
            return new()
            {
                Id = id,
                UserId = userId,
                UserName= userDto.UserName,
                Email= userDto.Email,
                Password= userDto.Password.HashToPassword(),
                Address= userDto.Address,
                Name= userDto.Name,
                Role= userDto.Role,
            };
        }
        // on create
        public static UserEntity Map(UserDto userDto, int userId)
        {
            return new()
            {
                UserId = userId,
                UserName = userDto.UserName,
                Email = userDto.Email,
                Password = userDto.Password.HashToPassword(),
                Address = userDto.Address,
                Name = userDto.Name,
                Role = userDto.Role,
            };
        }
    }
}
