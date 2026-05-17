using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

public record UserDto(Guid Id, string Email, string FullName);

public record UsersDto : IRequest<List<UserDto>>;

namespace SaasStarterKit.Application.Users.Queries.GetUsers
{
    public class GetUsersHandler : IRequestHandler<UsersDto, List<UserDto>>
    {
        public Task<List<UserDto>> Handle(UsersDto request, CancellationToken cancellationToken)
        {
            // Dummy data for now — will connect to DB later on
            var users = new List<UserDto>
        {
            new(Guid.NewGuid(), "admin@test.com", "Admin User"),
            new(Guid.NewGuid(), "user@test.com", "Regular User"),
        };

            return Task.FromResult(users);
        }
    }
}
