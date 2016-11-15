﻿using Coolector.Dto.Users;

namespace Coolector.Services.Storage.Mappers
{
    public class UserMapper : IMapper<UserDto>
    {
        public UserDto Map(dynamic source)
        {
            return new UserDto
            {
                Id = source.id,
                UserId = source.userId,
                Email = source.email,
                Provider = source.provider,
                Name = source.name,
                Role = source.role,
                State = source.state,
                PictureUrl = source.pictureUrl,
                CreatedAt = source.createdAt
            };
        }
    }
}