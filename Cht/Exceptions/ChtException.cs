namespace Cht;

/// <summary>
/// Base CHT exception.
/// </summary>
public class ChtException : Exception
{
    public ChtException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
