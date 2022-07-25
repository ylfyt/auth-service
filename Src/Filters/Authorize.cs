using auth_sevice.Src.Models;
using auth_sevice.Src.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace auth_sevice.Src.Filters
{
  public class Authorize : Attribute, IAsyncAuthorizationFilter
  {
    private int[] _filteredUserLevel = { };
    public Authorize()
    {
    }

    public Authorize(int l1 = -1, int l2 = -1, int l3 = -1, int l4 = -1)
    {
      List<int> temp = new List<int>();
      if (l1 != -1) temp.Add(l1);
      if (l2 != -1) temp.Add(l2);
      if (l3 != -1) temp.Add(l3);
      if (l4 != -1) temp.Add(l4);
      _filteredUserLevel = temp.ToArray();
    }
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
      try
      {
        var authHeader = context.HttpContext.Request.Headers.FirstOrDefault(x => x.Key == "Authorization").Value.ToString().Split(" ");
        if (authHeader == null || authHeader.Length != 2)
          return SendUnauthorized(context);

        var _tm = context.HttpContext.RequestServices.GetService(typeof(ITokenManager)) as ITokenManager;
        if (_tm == null)
          return SendUnauthorized(context);

        var token = authHeader[1];
        var valid = _tm.VerifyAccessToken(token);
        if (!valid)
          return SendUnauthorized(context);

        return Task.CompletedTask;

        // if (!IsQualified(user))
        // {
        //   SendUnauthorized(context);
        //   return;
        // }
        // context.HttpContext.Items["user"] = user;
      }
      catch (System.Exception)
      {
        return SendUnauthorized(context);
      }
    }

    // private bool IsQualified(User user)
    // {
    //   if (_filteredUserLevel.Length == 0)
    //   {
    //     return true;
    //   }

    //   return _filteredUserLevel.Contains(user.Level) ? true : false;
    // }

    public Task SendUnauthorized(AuthorizationFilterContext context, string? msg = null)
    {
      context.HttpContext.Response.StatusCode = 401;
      object? data = null;
      context.Result = new JsonResult(new
      {
        success = false,
        message = msg ?? "Unauthorized",
        data
      });

      return Task.CompletedTask;
    }
  }
}