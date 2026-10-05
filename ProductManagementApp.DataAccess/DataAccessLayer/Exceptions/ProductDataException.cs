namespace ProductManagementApp.DataAccessLayer.Exceptions;

public class ProductDataException : Exception
{
    public ProductDataException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
