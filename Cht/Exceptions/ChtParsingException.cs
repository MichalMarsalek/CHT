namespace Cht;

public class ChtParsingException : ChtException
{
    public int LineNumber { get; set; }
    public int ColumnNumber { get; set; }

    public ChtParsingException(int lineNumber, int columnNumber, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        LineNumber = lineNumber;
        ColumnNumber = columnNumber;
    }
}
