namespace ProductManagementApp.Business.Exceptions;

public class ProductValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ProductValidationException(IEnumerable<string> errors)
        : base("Product validation failed.")
    {
        Errors = errors.ToList();
    }
}
