public interface ILanguageProvider
{
    string CurrentLang { get; }
}

public class LanguageProvider : ILanguageProvider
{
    private readonly IHttpContextAccessor _accessor;
    public LanguageProvider(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public string CurrentLang =>
        _accessor.HttpContext?.Items["Lang"]?.ToString() ?? "en";
}