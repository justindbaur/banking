using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Banking.Api.Repositories.Yaml;

public class DateTimeOffsetYamlConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(DateTimeOffset);
    }

    public object? ReadYaml(IParser parser, Type type)
    {
        var value = parser.Consume<Scalar>().Value;
        return DateTimeOffset.ParseExact(value, "G", null);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        var dto = (DateTimeOffset)value!;
        emitter.Emit(new Scalar(
            AnchorName.Empty,
            TagName.Empty,
            dto.ToString("G"),
            ScalarStyle.SingleQuoted,
            true,
            false));
    }
}