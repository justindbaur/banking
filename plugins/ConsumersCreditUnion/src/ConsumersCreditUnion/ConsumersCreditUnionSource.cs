using Microsoft.Extensions.DependencyInjection;

namespace Banking.Plugin.ConsumersCreditUnion;

public class ConsumersCreditUnionSource : ISourceService
{
    public ConsumersCreditUnionSource(IServiceProvider serviceProvider)
    {
        Creator = ActivatorUtilities.CreateInstance<ConsumersCreditUnionCreator>(serviceProvider);
        TransactionSource = ActivatorUtilities.CreateInstance<ConsumersTransactionSource>(serviceProvider);
    }

    public string Id => "consumers-credit-union";
    public string Name => "Consumers Credit Union";

    public ICreator Creator { get; }

    public ITransactionSource TransactionSource { get; }
}
