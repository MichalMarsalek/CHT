namespace Cht;

public class ChtException : Exception
{
    public ChtException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
