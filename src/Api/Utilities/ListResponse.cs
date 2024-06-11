namespace Banking.Api.Utilities;

public record ListResponse<T>
{
    public ListResponse(IReadOnlyCollection<T> data)
    {
        Data = data;
    }

    public int Count => Data.Count;
    public IReadOnlyCollection<T> Data { get; }
}
static class ListResponse
{
    public static ListResponse<T> Create<T>(IReadOnlyCollection<T> data) => new(data);
}