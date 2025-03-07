using Cht.Mappers;

namespace Cht;

public class ChtMappingException : Exception
{
    public IChtMapper? Mapper { get; set; }

    public ChtMappingException(IChtMapper? mapper, Exception innerException)
        : base(innerException.Message, innerException)
    {
    }

    public ChtMappingException(IChtMapper? mapper, string message)
        : base(message)
    {
    }
}
