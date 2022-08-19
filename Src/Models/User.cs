using System.Text.Json.Serialization;
using auth_sevice.src.Types;
using Microsoft.EntityFrameworkCore;

namespace auth_sevice.src.Models
{
  [Index(nameof(User.Username), IsUnique = true)]
  public class User : BaseModel
  {
    public string Username { get; set; } = string.Empty;
    [JsonIgnore]
    public string Password { get; set; } = string.Empty;
  }
}