﻿using Microsoft.AspNetCore.Mvc;
using ms.user.api.Responses;
using System.Net;
using MediatR;
using ms.user.application.Queries;
using ms.user.application.Commands;
using ms.user.application.Requests;

namespace ms.user.api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        public UsersController(IMediator mediator) => _mediator = mediator;


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var res = await _mediator.Send(new GetAllUsersQuery());

                return ApiResponse<List<User>>.Success(res, "User List", HttpStatusCode.OK);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("/{id}")]
        public async Task<IActionResult> SelectById(Guid id)
        {
            try
            {
                var res = await _mediator.Send(new GetUserByIdQuery(id));
                return ApiResponse<User>.Success(res, $"User with id: {id}", HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest user)
        {
            try
            {
                var res = await _mediator.Send(new CreateUserCommand(user.AccountId, user.Username, user.Email));

                return ApiResponse<User>.Success(res, "User Created", HttpStatusCode.Created);
            }
            catch (Exception ex) {
                return ApiResponse<object?>.Error(ex.Message, HttpStatusCode.InternalServerError);
            }
        }
    }
}
