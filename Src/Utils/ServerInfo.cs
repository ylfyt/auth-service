namespace auth_sevice.src.Utils
{
  public static class ServerInfo
  {
    public static string DB_CONNECT { get; set; } = string.Empty;
    public static string JWT_ACCESS_TOKEN_SECRET_KEY { get; set; } = string.Empty;
    public static string JWT_REFRESH_TOKEN_SECRET_KEY { get; set; } = string.Empty;
    public static double JWT_ACCESS_TOKEN_EXPIRY_TIME { get; set; }
    public static double JWT_REFRESH_TOKEN_EXPIRY_TIME { get; set; }
    public static void Init()
    {
      var dbConnect = Environment.GetEnvironmentVariable("DB_CONNECT");
      var accessTokenSecretKey = Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_SECRET_KEY");
      var refreshTokenSecretKey = Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_SECRET_KEY");
      var accessTokenExpiryTime = Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRY_TIME");
      var refreshTokenExpiryTime = Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRY_TIME");

      if (
        dbConnect == null ||
        accessTokenSecretKey == null ||
        refreshTokenSecretKey == null ||
        accessTokenExpiryTime == null ||
        refreshTokenExpiryTime == null
      )
        throw new Exception("======= SOME ENV VARIABLE IS NULL =======");

      ServerInfo.DB_CONNECT = dbConnect;
      ServerInfo.JWT_ACCESS_TOKEN_SECRET_KEY = accessTokenSecretKey;
      ServerInfo.JWT_REFRESH_TOKEN_SECRET_KEY = refreshTokenSecretKey;
      ServerInfo.JWT_ACCESS_TOKEN_EXPIRY_TIME = Double.Parse(accessTokenExpiryTime);
      ServerInfo.JWT_REFRESH_TOKEN_EXPIRY_TIME = Double.Parse(refreshTokenExpiryTime);
    }
  }
}