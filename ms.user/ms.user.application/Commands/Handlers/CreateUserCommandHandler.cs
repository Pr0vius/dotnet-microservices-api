
using MediatR;
using ms.user.domain.Interfaces;

namespace ms.user.application.Commands.Handlers
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, User>
    {
        private readonly IUserRepository _userRepository;
        public CreateUserCommandHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var res = await _userRepository.CreateUser(request.username, request.accountId, request.email);

            return res;
        }
    }
}
