﻿using Cht.Mappers;

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
    public static ChtSerializer AddIntMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new IntMapper());

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
    /// Adds an object mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <param name="types">Types supported for deserialization. Serialization doesn't require the types to be registered.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddObjectMapper(this ChtSerializer serializer, IEnumerable<Type> types)
        => serializer.AddMapper(new ObjectMapper(types));

    /// <summary>
    /// Adds an IEnumerable mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddIEnumerableMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new IEnumerableMapper());

    /// <summary>
    /// Adds an IDictionary mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddIDictionaryMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new IDictionaryMapper());

    /// <summary>
    /// Adds a GUID mapper to the serializer.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddGuidMapper(this ChtSerializer serializer)
        => serializer.AddMapper(new GuidMapper());

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
    /// To be precies, it adds the following mappers: Object, Enum, Null, IEnumerable, IDictionary, Bool, Int, String, DateOnly, TimeOnly, Guid.
    /// </summary>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <param name="types">Types to register for the ObjectMapper & EnumMapper.</param>
    /// <param name="enumStyle">Enum serialization style.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddCommonMappers(this ChtSerializer serializer, IEnumerable<Type> types, EnumMappingStyle enumStyle = EnumMappingStyle.TypedRawName)
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

    /// <summary>
    /// Adds common mappers to the serializer.
    /// To be precies, it adds the following mappers: Object, Enum, Null, IEnumerable, IDictionary, Bool, Int, String, DateOnly, TimeOnly, Guid.
    /// </summary>
    /// <typeparam name="T">All types from the assembly containing T are registered.</typeparam>
    /// <param name="serializer">Serializer to add the mapper to.</param>
    /// <param name="enumStyle">Enum serialization style.</param>
    /// <returns>The same serializer reference.</returns>
    public static ChtSerializer AddCommonMappers<T>(this ChtSerializer serializer, EnumMappingStyle enumStyle = EnumMappingStyle.TypedRawName)
        => AddCommonMappers(serializer, typeof(T).Assembly.GetTypes(), enumStyle);
}