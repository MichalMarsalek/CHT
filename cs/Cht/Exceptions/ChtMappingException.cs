using Cht.Mappers;

namespace Cht;

/// <summary>
/// Exception signaling an error while mapping to/from CHT Nodes.
/// </summary>
public class ChtMappingException : ChtException
{
    public IChtMapper? Mapper { get; set; }

    public ChtNode? Node { get; set; }

    public ChtMappingException(string message, Exception? innerException, IChtMapper? mapper = null, ChtNode? node = null)
        : base(message, innerException)
    {
        Mapper = mapper;
        Node = node;
    }

    public ChtMappingException(string message, IChtMapper? mapper = null, ChtNode? node = null)
        : base(message)
    {
        Mapper = mapper;
        Node = node;
    }
}
