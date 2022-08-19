namespace auth_sevice.src.Dtos
{
  public class ResponseDto<T>
  {
    public bool success { get; set; } = false;
    public string message { get; set; } = string.Empty;
    public int status { get; set; } = 400;
    public T? data { get; set; }
  }
}