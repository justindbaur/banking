using Banking.Api.Utilities;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Banking.Api.Repositories.Yaml;

public interface IYamlRepository<T>
{
    void Mutate(Action<List<T>> mutateAction);
    void Mutate<TState>(Action<List<T>, TState> mutateAction, TState state);

    IReadOnlyCollection<T> Get();
}

public interface IYamlRepositoryProvider
{
    IYamlRepository<T> Get<T>(string repositoryName);
}

public class YamlRepositoryProvider : IYamlRepositoryProvider
{
    private readonly BankingOptions _bankingOptions;

    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    public YamlRepositoryProvider(IOptions<BankingOptions> bankingOptions)
    {
        _bankingOptions = bankingOptions.Value;

        _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeConverter(new JsonYamlConverter())
            .Build();

        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeConverter(new JsonYamlConverter())
            .Build();
    }

    public IYamlRepository<T> Get<T>(string repositoryName)
    {
        // TODO: Cache?
        if (!Directory.Exists(_bankingOptions.RootConfigDirectory))
        {
            Directory.CreateDirectory(_bankingOptions.RootConfigDirectory);
        }

        var filePath = Path.Combine(_bankingOptions.RootConfigDirectory, repositoryName + ".yml");

        

        return new DefaultYamlRepository<T>(
            _serializer, _deserializer, filePath);
    }


    private class DefaultYamlRepository<T> : IYamlRepository<T>
    {
        private readonly ISerializer _serializer;
        private readonly IDeserializer _deserializer;
        private readonly string _filePath;


        public DefaultYamlRepository(ISerializer serializer, IDeserializer deserializer, string filePath)
        {
            _serializer = serializer;
            _deserializer = deserializer;
            _filePath = filePath;
        }

        public IReadOnlyCollection<T> Get()
        {
            if (!File.Exists(_filePath))
            {
                return [];
            }

            using var fs = File.OpenRead(_filePath);
            using var sr = new StreamReader(fs);
            return _deserializer.Deserialize<List<T>>(sr) ?? [];
        }

        public void Mutate(Action<List<T>> mutateAction)
        {
            Mutate<object?>((items, state) => mutateAction(items), null);
        }

        public void Mutate<TState>(Action<List<T>, TState> mutateAction, TState state)
        {
            using var fs = File.Open(_filePath, FileMode.OpenOrCreate);
            using var sr = new StreamReader(fs);
            var items = _deserializer.Deserialize<List<T>>(sr) ?? [];
            mutateAction(items, state);
            using var sw = new StreamWriter(fs);
            _serializer.Serialize(sw, items);
        }
    }
}