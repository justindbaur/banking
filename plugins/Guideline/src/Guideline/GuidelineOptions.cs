namespace Banking.Plugin.Guideline;

public class GuidelineOptions
{
    public virtual required string Email { get; init; }
    public virtual required string Password { get; init; }
    public virtual required string SecretKey { get; init; }
    public virtual required string Uuid { get; init; }
}
