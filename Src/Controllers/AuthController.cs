using auth_sevice.Src.Data;
using auth_sevice.Src.Dtos;
using auth_sevice.Src.Models;
using auth_sevice.Src.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace auth_sevice.Src.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class AuthController : ControllerBase
  {
    private readonly DataContext context;
    private readonly ITokenManager tm;
    private readonly IBlacklistTokenManager btm;

    public AuthController(DataContext context, ITokenManager tm, IBlacklistTokenManager btm)
    {
      this.context = context;
      this.tm = tm;
      this.btm = btm;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ResponseDto<DataUser>>> Register(RegisterInputDto data)
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
        return new ResponseDto<DataUser>
        {
          success = true,
          status = 200,
          data = new DataUser
          {
            User = newUser,
          }
        };
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

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ResponseDto<TokenDto>>> RefreshToken(RefreshTokenDto data)
    {
      try
      {
        var error = tm.Verify(data.token);
        RefreshToken? oldRefreshToken = null;
        if (error == "TOKEN_EXPIRED" || error == null)
        {
          oldRefreshToken = await context.RefreshTokens.Where(t => t.Token == data.token).FirstOrDefaultAsync();
          if (oldRefreshToken != null)
          {
            context.RefreshTokens.Remove(oldRefreshToken);
            await context.SaveChangesAsync();
          }

        }

        if (error != null || oldRefreshToken == null)
          return BadRequest("Token is invalid");


        var user = await context.Users.FindAsync(oldRefreshToken.UserId);
        if (user == null) return BadRequest("User not found");

        var tokenData = await tm.CreateRefreshToken(user);
        var refreshToken = tokenData.Item1;
        var refreshTokenId = tokenData.Item2;
        if (refreshToken == null)
          return BadRequest("Already logged in");

        var accessToken = tm.CreateAccessToken(user, refreshTokenId);

        new TokenDto
        {
          Access = accessToken,
          Refresh = refreshToken
        };

        return new ResponseDto<TokenDto>
        {
          success = true,
          status = 200,
          data = new TokenDto
          {
            Access = accessToken,
            Refresh = refreshToken
          }
        };
      }
      catch (System.Exception)
      {
        return BadRequest();
      }
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ResponseDto<bool>>> Logout(RefreshTokenDto data)
    {
      var error = tm.Verify(data.token);

      if (error == "NOT_VALID")
        return BadRequest("Token is invalid");

      var oldRefreshToken = await context.RefreshTokens.Where(t => t.Token == data.token).FirstOrDefaultAsync();
      if (oldRefreshToken == null) return BadRequest("Token is invalid");

      context.RefreshTokens.Remove(oldRefreshToken);
      await context.SaveChangesAsync();
      btm.CreateToken(oldRefreshToken.Id);

      if (error == "TOKEN_EXPIRED")
        return BadRequest("Token is invalid");

      return new ResponseDto<bool>
      {
        success = true,
        status = 200,
        data = true
      };
    }
    [HttpPost("logout-all")]
    public async Task<ActionResult<ResponseDto<bool>>> LogoutAll(RegisterInputDto data)
    {
      var user = await context.Users.Where(u => u.Username == data.Username).FirstOrDefaultAsync();

      if (user == null) return BadRequest("Username or password is wrong");

      var valid = BCrypt.Net.BCrypt.Verify(data.Password, user.Password);
      if (!valid) return BadRequest("Username or password is wrong");

      var refreshTokens = await context.RefreshTokens.Where(t => t.UserId == user.Id).ToListAsync();
      for (int i = 0; i < refreshTokens.Count; i++)
      {
        var refreshToken = refreshTokens[i];
        context.RefreshTokens.Remove(refreshToken);
        await context.SaveChangesAsync();
        btm.CreateToken(refreshToken.Id);
      }

      return new ResponseDto<bool>
      {
        success = true,
        status = 200,
        data = true
      };
    }

    [HttpPost("verify-access-token")]
    public ActionResult<ResponseDto<VerifyResponse>> VerifyAccessToken(AccessTokenDto data)
    {
      var payload = tm.VerifyAccessToken(data.token);

      return new ResponseDto<VerifyResponse>
      {
        success = true,
        status = 200,
        data = new VerifyResponse
        {
          Payload = payload ?? null,
          Valid = payload != null
        }
      };
    }
  }
}