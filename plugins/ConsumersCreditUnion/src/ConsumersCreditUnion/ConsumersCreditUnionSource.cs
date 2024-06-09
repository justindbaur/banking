namespace Banking.Plugin.ConsumersCreditUnion;

public class ConsumersCreditUnionSource : ISource
{
    public string Id => "consumers-credit-union";
    public string Name => "Consumers Credit Union";

    public ICreator Creator { get; } = new ConsumersCreditUnionCreator();
}
