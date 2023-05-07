namespace Banking.Plugin.ConsumersCreditUnion;

public class ConsumersCreditUnionOptions
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string DeviceId { get; init; }
    public required string SecretKey { get; init; }
}
