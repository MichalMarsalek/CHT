using Cht.Mappers;

namespace Cht;

public static class ChtSerializerExtensions
{
    /// <summary>
    /// Adds a mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <param name="mapper">Mapper to add.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddMapper(this ChtSerializer serializer, IChtMapper mapper)
    {
        serializer.Mappers.Add(mapper);
        return serializer;
    }

    /// <summary>
    /// Adds a null mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddNullMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new NullMapper());

    /// <summary>
    /// Adds a string mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddStringMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new StringMapper());

    /// <summary>
    /// Adds a bool mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddBoolMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new BoolMapper());

    /// <summary>
    /// Adds an int mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddIntMapper(this ChtSerializer serializer, string suffix = "")
        => serializer.AddMapper(new IntMapper(suffix));

    /// <summary>
    /// Adds a long mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddLongMapper(this ChtSerializer serializer, string suffix = "")
        => serializer.AddMapper(new LongMapper(suffix));

    /// <summary>
    /// Adds a BigInteger mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddBigIntegerMapper(this ChtSerializer serializer, string suffix = "")
        => serializer.AddMapper(new BigIntegerMapper(suffix));

    /// <summary>
    /// Adds a float mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddFloatMapper(this ChtSerializer serializer, string suffix = "")
        => serializer.AddMapper(new FloatMapper(suffix));

    /// <summary>
    /// Adds a double mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddDoubleMapper(this ChtSerializer serializer, string suffix = "")
        => serializer.AddMapper(new DoubleMapper(suffix));

    /// <summary>
    /// Adds a decimal mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddDecimalMapper(this ChtSerializer serializer, string suffix = "")
        => serializer.AddMapper(new DecimalMapper(suffix));

    /// <summary>
    /// Adds a DateOnly mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddDateOnlyMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new DateOnlyMapper());

    /// <summary>
    /// Adds a TimeOnly mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddTimeOnlyMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new TimeOnlyMapper());

    /// <summary>
    /// Adds a DateTimeOffset mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddDateTimeOffsetMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new DateTimeOffsetMapper());

    /// <summary>
    /// Adds an object mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <param name="types">Types supported for deserialization. Serialization doesn't require the types to be registered.</param>
    /// <param name="skipTrailingNulls">Whether to skip trailing nulls when converting to ChtNode.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddObjectMapper(this ChtSerializer serializer, IEnumerable<Type> types, bool skipTrailingNulls = true)
        => serializer.AddMapper(new ObjectMapper(types, skipTrailingNulls));

    /// <summary>
    /// Adds a GenericList mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddGenericListMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new GenericListMapper());

    /// <summary>
    /// Adds a GenericDictionary mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddGenericDictionaryMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new GenericDictionaryMapper());

    /// <summary>
    /// Adds a GUID mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddGuidMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new GuidMapper());

    /// <summary>
    /// Adds a Uri mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <param name="uriType">Type name of the Uri nonterminal node, null means terminal node.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddUriMapper(this ChtSerializer serializer, string? uriType)
        => serializer.AddMapper(new UriMapper(uriType));

    /// <summary>
    /// Adds a Color mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddColorMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new ColorMapper());

    /// <summary>
    /// Adds an Enum mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <param name="style">Serialization style.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddEnumMapper(this ChtSerializer serializer, IEnumerable<Type> enumTypes, EnumMappingStyle style = EnumMappingStyle.TypedRawName)
        => serializer.AddMapper(new EnumMapper(enumTypes, style));

    /// <summary>
    /// Adds an Enum mapper to the serializer.
    /// </summary>
    /// <typeparam name="T">All enums from the assembly containing T are registered.</typeparam>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <param name="style">Serialization style.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddEnumMapper<T>(this ChtSerializer serializer, EnumMappingStyle style = EnumMappingStyle.TypedRawName)
        => serializer.AddEnumMapper(typeof(T).Assembly.GetTypes(), style);

    /// <summary>
    /// Adds common mappers to the serializer.
    /// To be precies, it adds the following mappers: Object, Enum, Null, IEnumerable, IDictionary, Bool, Int, Long, BigInteger, Float, Double, Decimal, String, DateOnly, TimeOnly, DateTimeOffset, Guid, Uri, Color.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <param name="types">Types to register for the ObjectMapper & EnumMapper.</param>
    /// <param name="skipTrailingNulls">Whether to skip trailing nulls when converting to ChtNode.</param>
    /// <param name="enumStyle">Enum serialization style.</param>
    /// <param name="uriType">Type name of the URI nonterminal node, null means terminal node.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddCommonMappers(this ChtSerializer serializer, IEnumerable<Type> types, bool skipTrailingNulls = true, EnumMappingStyle enumStyle = EnumMappingStyle.TypedRawName, string? uriType = null, bool useNumberSuffixes = false)
        => serializer
        .AddObjectMapper(types, skipTrailingNulls)
        .AddEnumMapper(types)
        .AddNullMapper()
        .AddGenericListMapper()
        .AddGenericDictionaryMapper()
        .AddBoolMapper()
        .AddDecimalMapper(useNumberSuffixes ? "m" : "")
        .AddDoubleMapper(useNumberSuffixes ? "d" : "")
        .AddFloatMapper(useNumberSuffixes ? "f" : "")
        .AddBigIntegerMapper(useNumberSuffixes ? "n" : "")
        .AddLongMapper(useNumberSuffixes ? "L" : "")
        .AddIntMapper()
        .AddStringMapper()
        .AddDateOnlyMapper()
        .AddTimeOnlyMapper()
        .AddDateTimeOffsetMapper()
        .AddGuidMapper()
        .AddUriMapper(uriType)
        .AddColorMapper();

    /// <summary>
    /// Adds common mappers to the serializer.
    /// To be precies, it adds the following mappers: Object, Enum, Null, IEnumerable, IDictionary, Bool, Int, Long, BigInteger, Float, Double, Decimal, String, DateOnly, TimeOnly, Guid, Color.
    /// </summary>
    /// <typeparam name="T">All types from the assembly containing T are registered.</typeparam>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <param name="autoFlatten">Autoflattens all nonstring IEnumerable props.</param>
    /// <param name="enumStyle">Enum serialization style.</param>
    /// <param name="uriType">Type name of the URI nonterminal node, null means terminal node.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddCommonMappers<T>(this ChtSerializer serializer, bool autoFlatten = false, EnumMappingStyle enumStyle = EnumMappingStyle.TypedRawName, string? uriType = null, bool useNumberSuffixes = false)
        => AddCommonMappers(serializer, typeof(T).Assembly.GetTypes(), autoFlatten, enumStyle, uriType, useNumberSuffixes);
}