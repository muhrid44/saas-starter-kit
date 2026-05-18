using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

public record CreateUserModel(string email, string fullName, string password) : IRequest<Guid>;

namespace SaasStarterKit.Application.Users.Commands.CreateUser
{
    public class CreateUserHandler : IRequestHandler<CreateUserModel, Guid>
    {
        public Task<Guid> Handle(CreateUserModel request, CancellationToken cancellationToken)
        {
            // this is only for dummy, will connect to database later on
            var newGuid = Guid.NewGuid();
            return Task.FromResult(newGuid);
        }
    }
}
