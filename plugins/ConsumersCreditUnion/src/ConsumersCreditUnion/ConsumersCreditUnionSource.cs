using Microsoft.Extensions.DependencyInjection;

namespace Banking.Plugin.ConsumersCreditUnion;

public class ConsumersCreditUnionSource : ISource
{
    public ConsumersCreditUnionSource(IServiceProvider serviceProvider)
    {
        Creator = ActivatorUtilities.CreateInstance<ConsumersCreditUnionCreator>(serviceProvider);
    }

    public string Id => "consumers-credit-union";
    public string Name => "Consumers Credit Union";

    public ICreator Creator { get; }
}
