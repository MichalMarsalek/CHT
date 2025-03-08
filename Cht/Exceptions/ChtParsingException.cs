namespace Cht;

/// <summary>
/// Exception signaling an error while parsing a CHT document.
/// </summary>
public class ChtParsingException : ChtException
{
    /// <summary>
    /// 1-based number of the line the error occurred on.
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// 1-based number of the column the error occurred on.
    /// </summary>
    public int ColumnNumber { get; set; }

    public ChtParsingException(int lineNumber, int columnNumber, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        LineNumber = lineNumber;
        ColumnNumber = columnNumber;
    }
}
