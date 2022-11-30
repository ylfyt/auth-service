using auth_sevice.src.Data;
using auth_sevice.src.Dtos;
using auth_sevice.src.Models;
using auth_sevice.src.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace auth_sevice.src.Controllers
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

        if (user == null) return BadRequest(new ResponseDto<DataUser>
        {
          message = "Username or password is wrong",
          data = null,
          status = 400,
          success = false
        });

        var valid = BCrypt.Net.BCrypt.Verify(data.Password, user.Password);
        if (!valid) return BadRequest(new ResponseDto<DataUser>
        {
          message = "Username or password is wrong",
          data = null,
          status = 400,
          success = false
        });

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
              AccessToken = accessToken,
              RefreshToken = refreshToken,
              ExpiredIn = (long)ServerInfo.JWT_ACCESS_TOKEN_EXPIRY_TIME
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
        var isValid = tm.ValidateRefreshToken(data.RefreshToken, out Guid refreshTokenId);

        RefreshToken? oldRefreshToken = null;
        if (refreshTokenId != Guid.Empty)
        {
          oldRefreshToken = await context.RefreshTokens.Where(t => t.Id == refreshTokenId).FirstOrDefaultAsync();
          if (oldRefreshToken != null)
          {
            context.RefreshTokens.Remove(oldRefreshToken);
            await context.SaveChangesAsync();
          }
        }

        if (!isValid || oldRefreshToken == null)
          return BadRequest("Token is not valid");

        var user = await context.Users.FindAsync(oldRefreshToken.UserId);
        if (user == null) return BadRequest("User not found");

        var tokenData = await tm.CreateRefreshToken(user);
        var refreshToken = tokenData.Item1;
        var newRefreshTokenId = tokenData.Item2;
        if (refreshToken == null)
          return BadRequest("Already logged in");

        var accessToken = tm.CreateAccessToken(user, newRefreshTokenId);

        return new ResponseDto<TokenDto>
        {
          success = true,
          status = 200,
          data = new TokenDto
          {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiredIn = (long)ServerInfo.JWT_ACCESS_TOKEN_EXPIRY_TIME
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
      var isValid = tm.ValidateRefreshToken(data.RefreshToken, out Guid refreshTokenId);

      if (!isValid && refreshTokenId == Guid.Empty)
        return BadRequest("Token is invalid");

      var oldRefreshToken = await context.RefreshTokens.FirstOrDefaultAsync(token => token.Id == refreshTokenId);
      if (oldRefreshToken == null) return BadRequest("Token is invalid");

      context.RefreshTokens.Remove(oldRefreshToken);
      await context.SaveChangesAsync();
      btm.CreateToken(oldRefreshToken.Id);

      if (!isValid)
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
      var payload = tm.ValidateAccessToken(data.AccessToken);

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