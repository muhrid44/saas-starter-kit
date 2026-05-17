using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

public record User(string Email, string FullName) : IRequest<Guid>;

namespace SaasStarterKit.Application.Users.Commands.CreateUser
{
    public class CreateUserHandler : IRequestHandler<User, Guid>
    {
        public Task<Guid> Handle(User request, CancellationToken cancellationToken)
        {
            // this is only for dummy, will connect to database later on
            var newGuid = Guid.NewGuid();
            return Task.FromResult(newGuid);
        }
    }
}
