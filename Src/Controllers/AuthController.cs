using auth_sevice.Src.Data;
using auth_sevice.Src.Dtos;
using auth_sevice.Src.Models;
using auth_sevice.Src.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace auth_sevice.Src.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
  private readonly DataContext context;
  private readonly ITokenManager tm;

  public AuthController(DataContext context, ITokenManager tm)
  {
    this.context = context;
    this.tm = tm;
  }

  [HttpPost("register")]
  public async Task<ActionResult<User>> Register(RegisterInputDto data)
  {
    try
    {
      var pw = BCrypt.Net.BCrypt.HashPassword(data.Password);
      var newUser = new User
      {
        Password = pw,
        Username = data.Username
      };

      await context.AddAsync(newUser);
      await context.SaveChangesAsync();
      return newUser;
    }
    catch (System.Exception)
    {
      return BadRequest();
    }
  }

  [HttpPost("login")]
  public async Task<ActionResult<ResponseDto<DataUser>>> Login(RegisterInputDto data)
  {
    try
    {
      var user = await context.Users.Where(u => u.Username == data.Username).FirstOrDefaultAsync();

      if (user == null) return BadRequest("Username or password is wrong");

      var valid = BCrypt.Net.BCrypt.Verify(data.Password, user.Password);
      if (!valid) return BadRequest("Username or password is wrong");

      // CREATE TOKEN
      var tokenData = await tm.CreateRefreshToken(user);
      var refreshToken = tokenData.Item1;
      var refreshTokenId = tokenData.Item2;
      if (refreshToken == null) return BadRequest("Already logged in with other devices");

      var accessToken = tm.CreateAccessToken(user, refreshTokenId);

      return new ResponseDto<DataUser>
      {
        success = true,
        status = 200,
        data = new DataUser
        {
          User = user,
          Token = new TokenDto
          {
            Access = accessToken,
            Refresh = refreshToken
          }
        }
      };
    }
    catch (System.Exception)
    {
      return BadRequest();
    }
  }
}
