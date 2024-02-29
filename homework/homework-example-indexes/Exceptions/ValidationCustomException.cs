using Microsoft.IdentityModel.Tokens;

namespace OtusSocialNetwork.Exceptions;

public class ValidationCustomException : Exception
{
    public ValidationCustomException() : base("ValidationException")
    {
        Errors = new List<string>();
    }
    public List<string> Errors { get; }
    public ValidationCustomException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        foreach (var failure in failures)
        {
            Errors.Add(failure.ToString());
        }
    }

    public ValidationCustomException(IEnumerable<string> failures)
        : this()
    {
        foreach (var failure in failures.Distinct().ToList())
        {
            Errors.Add(failure);
        }
    }

}
