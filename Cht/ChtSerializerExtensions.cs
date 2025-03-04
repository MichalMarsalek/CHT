using Cht.Mappers;

namespace Cht;

public static class ChtSerializerExtensions
{
    public static ChtSerializer AddMapper(this ChtSerializer serializer, IChtMapper mapper)
    {
        serializer.Mappers.Add(mapper);
        return serializer;
    }

    public static ChtSerializer AddNullMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new NullMapper());

    public static ChtSerializer AddStringMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new StringMapper());

    public static ChtSerializer AddBoolMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new BoolMapper());

    public static ChtSerializer AddIntMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new IntMapper());

    public static ChtSerializer AddDateOnlyMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new DateOnlyMapper());

    public static ChtSerializer AddTimeOnlyMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new TimeOnlyMapper());

    public static ChtSerializer AddObjectMapper(this ChtSerializer serializer, IEnumerable<Type> types)
        => serializer.AddMapper(new ObjectMapper(types));

    public static ChtSerializer AddIEnumerableMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new IEnumerableMapper());

    public static ChtSerializer AddIDictionaryMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new IDictionaryMapper());

    public static ChtSerializer AddGuidMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new GuidMapper());

    public static ChtSerializer AddEnumMapper(this ChtSerializer serializer, IEnumerable<Type> enumTypes)
    => serializer.AddMapper(new EnumMapper(enumTypes));

    public static ChtSerializer AddCommonMappers(this ChtSerializer serializer, IEnumerable<Type> types)
        => serializer
        .AddObjectMapper(types)
        .AddEnumMapper(types)
        .AddNullMapper()
        .AddIEnumerableMapper()
        .AddIDictionaryMapper()
        .AddBoolMapper()
        .AddIntMapper()
        .AddStringMapper()
        .AddDateOnlyMapper()
        .AddTimeOnlyMapper()
        .AddGuidMapper();
}