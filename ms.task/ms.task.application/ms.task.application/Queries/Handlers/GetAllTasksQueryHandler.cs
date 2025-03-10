using MediatR;
using Microsoft.AspNetCore.Razor.TagHelpers;
using ms.task.domain.Entities;
using ms.task.domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ms.task.application.Queries.Handlers
{
    public class GetAllTasksQueryHandler: IRequestHandler<GetAllTasksQuery, List<UserTask>>
    {
        private readonly ITaskRepository _taskRepository;
        public GetAllTasksQueryHandler(ITaskRepository taskRepository) {
            _taskRepository = taskRepository;
        }

        public async Task<List<UserTask>> Handle(GetAllTasksQuery query, CancellationToken cancellationToken)
        {
            return await _taskRepository.GetAllByUserId(query.userId);
        }

    }
}
