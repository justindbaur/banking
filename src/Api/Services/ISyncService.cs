namespace Banking.Api.Services;

public interface ISyncService
{
    Task FullSyncAsync(CancellationToken cancellationToken);
}
