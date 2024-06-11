using Banking.Abstractions;
using Banking.Api.Repositories;
using Banking.Api.Services;
using Banking.Api.Utilities;
using static Microsoft.AspNetCore.Http.Results;

namespace Banking.Api.Endpoints;

public static class SourcesEndpoints
{
    public static RouteGroupBuilder MapSourcesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/sources");

        group.MapGet("", async (ISourceRepository sourceRepository) =>
        {
            var sources = await sourceRepository.GetAllAsync();

            return ListResponse.Create(sources.Select(s => new
            {
                s.Id,
                s.DisplayName,
                s.Enabled,
                s.SourceId,
            }).ToArray());
        });

        group.MapGet("/{sourceId}", async (Guid sourceId, ISourceRepository sourceRepository, ISourceProvider sourceProvider) =>
        {
            var source = await sourceRepository.GetByIdAsync(sourceId);

            if (source == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                source.Id,
                source.SourceId,
                source.DisplayName,
                source.Enabled,
            });
        });

        group.MapGet("/templates", (IEnumerable<ISource> sources) =>
        {
            return ListResponse.Create(
                sources.Select(s => new { s.Id, s.Name }).ToArray()
            );
        });

        group.MapGet("/{sourceId}/start", async Task<IResult> (ISourceProvider sourceProvider, string sourceId, CancellationToken cancellationToken) =>
        {
            if (!sourceProvider.TryGetSource(sourceId, out var source))
            {
                return ValidationProblem(
                    new Dictionary<string, string[]>(),
                    "Invalid Source"
                );
            }

            var creator = source.Creator;

            var startToken = await creator.StartAsync(cancellationToken);

            return Ok(startToken);
        });

        group.MapPost("/{sourceId}/resume", async Task<IResult> (ISourceProvider sourceProvider, string sourceId, StepResponse stepResponse, ISourceRepository sourceRepository, CancellationToken cancellationToken) =>
        {
            if (!sourceProvider.TryGetSource(sourceId, out var source))
            {
                return ValidationProblem(
                    new Dictionary<string, string[]>(),
                    "Invalid Source"
                );
            }

            var creator = source.Creator;

            var resumeToken = await creator.ResumeAsync(stepResponse, cancellationToken);

            if (resumeToken.IsComplete)
            {
                var newSourceId = await sourceRepository.CreateAsync(new CreateSource(source.Id, source.Name, resumeToken.FinalConfiguration));
                return Ok(new
                {
                    IsComplete = true,
                    SourceId = newSourceId,
                });
            }

            return Ok(resumeToken);
        });

        group.MapGet("{sourceId}/accounts", async Task<IResult> (ISourceProvider sourceProvider, string sourceId, CancellationToken cancellationToken) =>
        {
            if (!sourceProvider.TryGetSource(sourceId, out var source))
            {
                return ValidationProblem(
                    new Dictionary<string, string[]>(),
                    "Invalid Source"
                );
            }


            var transactionSource = source.TransactionSource;

            if (transactionSource == null)
            {
                return NoContent();
            }

            // TODO: Get configuration
            var accounts = await transactionSource.GetAccountsAsync(null!, cancellationToken);
            return Ok(ListResponse.Create(accounts));
        });

        return group;
    }
}