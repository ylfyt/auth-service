namespace auth_sevice.Src.Types
{
  public class Pagination
  {
    public int Page { get; set; } = 0;
    public int Limit { get; set; } = 10;
    public int Total { get; set; } = 1;
  }
  public class BaseData
  {
    public Pagination Pagination { get; set; } = new Pagination();
  }
}