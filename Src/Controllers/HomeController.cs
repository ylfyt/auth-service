using Microsoft.AspNetCore.Mvc;
namespace auth_sevice.Src.Controllers
{
  [ApiController, Route("")]
  public class HomeController : ControllerBase
  {
    public HomeController()
    {
    }

    [HttpGet]
    public string get()
    {
      return "ok";
    }

    [HttpGet("ping")]
    public string ping()
    {
      var dt = DateTime.UtcNow.AddHours(7);
      var formatDt = dt.ToString("dd-MM-yyyy HH:mm:ss");
      Console.WriteLine($"[{formatDt}] PING ME");
      return formatDt;
    }
  }
}