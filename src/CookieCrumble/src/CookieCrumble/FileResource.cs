namespace CookieCrumble;

public static class FileResource
{
    public static string? Open(string name)
    {
        var path = Path.Combine("__resources__", name);
        return File.Exists(path) ? File.ReadAllText(path) : null;
    }
}
