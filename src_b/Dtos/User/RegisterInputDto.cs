using System.ComponentModel.DataAnnotations;

namespace auth_sevice.src.Dtos
{
  public class RegisterInputDto
  {
    [MinLength(4)]
    public string Username { get; set; } = string.Empty;
    [MinLength(4)]
    public string Password { get; set; } = string.Empty;
  }
}