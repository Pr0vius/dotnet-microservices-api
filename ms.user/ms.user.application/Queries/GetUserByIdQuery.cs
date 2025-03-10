using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ms.user.application.Queries
{
    public record GetUserByIdQuery(Guid id): IRequest<User>;
}
