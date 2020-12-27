﻿using DataAccessLibrary.DB;
using DataAccessLibrary.DB.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using WEBApi.Authentication;
using WEBApi.DTOs;
using WEBApi.Validators;
using System.Threading;

namespace WEBApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AutherizationController : ControllerBase
    {
        private readonly IJWTokenManager _manager;
        private readonly IUserAddRepository _writerepo;
        private readonly IModelConverter _converter;
        private readonly RegistrationValidator _validator;

        public AutherizationController(IJWTokenManager manager, IUserAddRepository writerepo,
            IModelConverter converter, RegistrationValidator validator)
        {
            this._manager = manager;
            this._writerepo = writerepo;
            this._converter = converter;
            this._validator = validator;
        }

        [HttpPost("register")]
        public async Task<ActionResult> RegisterUser([FromBody] UserRegistrationModel userDto, CancellationToken ct)
        {
            try
            {
                var results = await _validator.ValidateAsync(userDto, ct);

                if (!results.IsValid)
                {
                    List<string> ErrorMessages = new List<string>();
                    foreach (var Error in results.Errors)
                    {
                        ErrorMessages.Add(Error.ErrorMessage);
                    }
                    return BadRequest(ErrorMessages);
                }

                ct.ThrowIfCancellationRequested();

                User user = _converter.ConvertUserFromDTO(userDto);

                await _writerepo.InsertUserIntoTheDb(user);

                var token = await _manager.Authorize(user.Email, user.Password);

                if (token is not null)
                {
                    return Ok(token);
                }

                return Unauthorized();
            }
            catch (TaskCanceledException)
            {
                return BadRequest("Canceled");
            }
            catch (Exception)
            {
                return BadRequest("Something went wrong");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult> LoginUser([FromBody] UserLoginModel user, CancellationToken ct)
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                if (user is not null)
                {
                    var token = await _manager.Authorize(user.Email, user.Password);
                    if (token is not null)
                    {
                        return Ok(token);
                    }
                    return Unauthorized();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (TaskCanceledException)
            {
                return BadRequest("Canceled");
            }
            catch (Exception)
            {
                return BadRequest("Something went wrong");
            }
        }
        //Authorization check
        /*
        [Authorize]
        [HttpGet]
        public ActionResult<string> GetUSCapital()
        {
            return Ok("Washington DC");
        }*/
    }
}
