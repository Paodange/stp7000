using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using Mgi.STP7000.Infrastructure.ApiProtocol;
using Mgi.STP7000.Model.Entity;
using Mgi.STP7000.Model.Request;
using Mgi.STP7000.Model.Response;
using Mgi.STP7000.Service;
using MGI.STP7000.WebHost.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MGI.STP7000.WebHost.Controllers
{
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Route("user")]
    public class UserController : ControllerBase
    {
        readonly IUserService _userService;
        readonly JwtSetting _jwtSetting;
        public UserController(IUserService userService, IOptions<JwtSetting> jwtSettingOption)
        {
            _userService = userService;
            _jwtSetting = jwtSettingOption.Value;
        }

        [HttpPost]
        [Route("login")]
        public ApiResponse<UserLoginResponse> Login(UserLoginRequest request)
        {
            var user = _userService.UserLogin(request);

            return new ApiResponse<UserLoginResponse>(ResponseCode.Ok,
                new UserLoginResponse()
                {
                    Avatar = user.Avatar,
                    Name = user.Name,
                    Token = GenerateToken(user)
                });
        }


        [HttpGet]
        [Route("test1")]
        [Authorize(Policy = "authenticatedUser")]
        public ApiResponse Test1()
        {
            return ResponseCode.Ok;
        }

        [HttpGet]
        [Route("test2")]
        [Authorize(Policy = "adminOnly")]
        public ApiResponse Test2()
        {
            return ResponseCode.Ok;
        }

        private string GenerateToken(UserInfo userInfo)
        {
            var key = Encoding.ASCII.GetBytes(_jwtSetting.Secret);
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityTokenDescriptor = new SecurityTokenDescriptor()
            {
                Audience = _jwtSetting.Audience,
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userInfo.UserName),
                    new Claim(ClaimTypes.Role, userInfo.Role),
                    new Claim(ClaimTypes.Name, userInfo.Name),
                }),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
                Issuer = _jwtSetting.Issuer,
                IssuedAt = DateTime.Now
            };
            return tokenHandler.WriteToken(tokenHandler.CreateJwtSecurityToken(securityTokenDescriptor));
        }
    }
}
