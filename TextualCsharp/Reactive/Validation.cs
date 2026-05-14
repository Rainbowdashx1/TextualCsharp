namespace TextualCsharp.Reactive;

/// <summary>Resultado de una validación.</summary>
public readonly record struct ValidationResult(bool IsValid, string? Error = null)
{
    public static ValidationResult Success => new(true, null);
    public static ValidationResult Failure(string error) => new(false, error);
}

/// <summary>Validador genérico para reactivas o inputs.</summary>
public abstract class Validator<T>
{
    public abstract ValidationResult Validate(T value);

    public static Validator<T> From(Func<T, bool> predicate, string error)
        => new DelegateValidator<T>(predicate, error);

    public static Validator<T> Combine(params Validator<T>[] validators)
        => new CompositeValidator<T>(validators);
}

internal sealed class DelegateValidator<T> : Validator<T>
{
    private readonly Func<T, bool> _predicate;
    private readonly string _error;
    public DelegateValidator(Func<T, bool> predicate, string error)
    { _predicate = predicate; _error = error; }
    public override ValidationResult Validate(T value)
        => _predicate(value) ? ValidationResult.Success : ValidationResult.Failure(_error);
}

internal sealed class CompositeValidator<T> : Validator<T>
{
    private readonly Validator<T>[] _validators;
    public CompositeValidator(Validator<T>[] validators) { _validators = validators; }
    public override ValidationResult Validate(T value)
    {
        foreach (var v in _validators)
        {
            var r = v.Validate(value);
            if (!r.IsValid) return r;
        }
        return ValidationResult.Success;
    }
}

/// <summary>Validadores comunes para strings.</summary>
public static class StringValidators
{
    public static Validator<string> NotEmpty(string error = "Required")
        => Validator<string>.From(s => !string.IsNullOrWhiteSpace(s), error);

    public static Validator<string> MaxLength(int max, string? error = null)
        => Validator<string>.From(s => (s ?? "").Length <= max, error ?? $"Max {max} characters");

    public static Validator<string> MinLength(int min, string? error = null)
        => Validator<string>.From(s => (s ?? "").Length >= min, error ?? $"Min {min} characters");
}
