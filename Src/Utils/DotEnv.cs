namespace auth_sevice.src.Utils
{
  public static class DotEnv
  {
    private const string ENV_FILENAME = ".env";
    public static void Init()
    {
      string filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), ENV_FILENAME);
      if (!File.Exists(filePath))
        return;

      foreach (var line in File.ReadAllLines(filePath))
      {
        var parts = line.Split(new[] { '=' }, 2);

        if (parts.Length != 2)
          continue;

        Environment.SetEnvironmentVariable(parts[0], parts[1]);
      }
    }
  }
}