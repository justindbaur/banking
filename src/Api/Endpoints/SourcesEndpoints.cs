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

        group.MapGet("", async (ISourceRepository sourceRepository, CancellationToken cancellationToken) =>
        {
            var sources = await sourceRepository.GetAllAsync(cancellationToken);

            return ListResponse.Create(sources.Select(s => new
            {
                s.Id,
                s.DisplayName,
                s.Enabled,
                s.SourceTemplateId,
            }).ToArray());
        });

        group.MapGet("/{sourceId}", async (Guid sourceId, ISourceRepository sourceRepository, ISourceTemplateProvider sourceProvider, CancellationToken cancellationToken) =>
        {
            var source = await sourceRepository.GetByIdAsync(sourceId, cancellationToken);

            if (source == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                source.Id,
                source.SourceTemplateId,
                source.DisplayName,
                source.Enabled,
            });
        });

        group.MapGet("/templates", (IEnumerable<ISourceService> sources) =>
        {
            return ListResponse.Create(
                sources.Select(s => new { s.Id, s.Name }).ToArray()
            );
        });

        group.MapGet("/{sourceTemplateId}/start", async Task<IResult> (ISourceTemplateProvider sourceTemplateProvider, string sourceTemplateId, CancellationToken cancellationToken) =>
        {
            if (!sourceTemplateProvider.TryGetSource(sourceTemplateId, out var source))
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

        group.MapPost("/{sourceTemplateId}/resume", async Task<IResult> (ISourceTemplateProvider sourceProvider, string sourceTemplateId, StepResponse stepResponse, ISourceRepository sourceRepository, CancellationToken cancellationToken) =>
        {
            if (!sourceProvider.TryGetSource(sourceTemplateId, out var source))
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

        group.MapGet("{sourceId}/accounts", async Task<IResult> (ISourceTemplateProvider sourceProvider, ISourceRepository sourceRepository, Guid sourceId, CancellationToken cancellationToken) =>
        {
            var source = await sourceRepository.GetByIdAsync(sourceId, cancellationToken);

            if (source == null)
            {
                return NotFound();
            }

            if (!sourceProvider.TryGetSource(source.SourceTemplateId, out var sourceTemplate))
            {
                return ValidationProblem(
                    new Dictionary<string, string[]>(),
                    "Invalid Source"
                );
            }

            var transactionSource = sourceTemplate.TransactionSource;

            if (transactionSource == null)
            {
                return NoContent();
            }

            var accounts = await transactionSource.GetAccountsAsync(source.Config, cancellationToken);
            return Ok(ListResponse.Create(accounts));
        });

        return group;
    }
}