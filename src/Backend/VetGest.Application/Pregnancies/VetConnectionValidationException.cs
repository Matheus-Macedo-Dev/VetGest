namespace VetGest.Application.Pregnancies;

public sealed class VetConnectionValidationException : Exception
{
    public VetConnectionValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Vet connection validation failed.") => Errors = errors;

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
