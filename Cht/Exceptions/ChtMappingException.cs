using Cht.Mappers;

namespace Cht;

/// <summary>
/// Exception signaling an error while mapping to/from CHT Nodes.
/// </summary>
public class ChtMappingException : Exception
{
    public IChtMapper? Mapper { get; set; }

    public ChtMappingException(IChtMapper? mapper, Exception innerException)
        : base(innerException.Message, innerException)
    {
    }
    public ChtMappingException(IChtMapper? mapper, string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    public ChtMappingException(IChtMapper? mapper, string message)
        : base(message)
    {
    }
}
