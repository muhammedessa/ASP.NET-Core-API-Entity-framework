using EssaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System;   
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;


using BC = BCrypt.Net.BCrypt;

namespace EssaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        public IConfiguration _configuration;
        private readonly muhammedContext _context;

        public TokenController(IConfiguration config , muhammedContext context)
        {
            _configuration = config;
            _context = context;
        }


        [HttpPost]
        public async Task<IActionResult> Post(UserInfo _userData)
        {
           
            if (_userData != null && _userData.Email != null && _userData.Password != null)
            {
                // var user = await GetUser(_userData.Email, _userData.Password);

                var user = await CheckUserEmail(_userData.Email);
               // var user =  _context.UserInfo.SingleOrDefault(x => x.Email == _userData.Email);

                // Console.WriteLine( BC.Verify(_userData.Password , user.Password)    );

                if (user != null ||  BC.Verify(_userData.Password, user.Password))
                {
                    var claims = new[]
                    {
                      new Claim(JwtRegisteredClaimNames.Sub,_configuration["Jwt:Subject"])  ,
                      new Claim (JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                      new Claim  (JwtRegisteredClaimNames.Iat,DateTime.UtcNow.ToString()),
                      new Claim  ("id", user.UserId.ToString()),
                      new Claim  ("FirstName", user.FirstName),
                      new Claim  ("LastName", user.LastName),
                      new Claim  ("UserName", user.UserName),
                      new Claim  ("Email", user.Email)
                    };
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"],claims,expires:DateTime.UtcNow.AddDays(1),signingCredentials:signIn);

                    return Ok(new JwtSecurityTokenHandler().WriteToken(token));
                }
                else
                {
                    return BadRequest("Invalid email and password");

                }
            }
            else
            {
                return BadRequest();
            }
           
        }

        private async Task<UserInfo> GetUser(string email , string password)
        {
            return await _context.UserInfo.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
        }

        private async Task<UserInfo> CheckUserEmail(string email )
        {
            return await _context.UserInfo.FirstOrDefaultAsync(u => u.Email == email );
        }


    }
}
