﻿using MediatR;
using Mediaverse.Application.Authentication.Common.Dtos;

namespace Mediaverse.Application.Authentication.Commands.SignUp
{
    public class SignUpCommand : IRequest<UserDto>
    {
        public string Nickname { get; set; }
        public string Password { get; set; }
        public string PasswordConfirmation { get; set; }
        public string Email { get; set; }
    }
}