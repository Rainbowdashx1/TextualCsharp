namespace TextualCsharp.Input;

/// <summary>
/// Asociación declarativa entre una pulsación de tecla y una acción.
/// Equivalente a <c>textual.binding.Binding</c>.
/// </summary>
public sealed record Binding(string KeyName, string Action, string? Description = null)
{
    public static string NormalizeKeyName(string keyName)
    {
        ArgumentNullException.ThrowIfNull(keyName);
        return keyName.Trim().ToLowerInvariant().Replace(" ", "");
    }
}

/// <summary>Colección de bindings indexada por nombre canónico de tecla.</summary>
public sealed class BindingMap
{
    private readonly Dictionary<string, Binding> _bindings = new(StringComparer.Ordinal);

    public IReadOnlyCollection<Binding> All => _bindings.Values;

    public void Add(Binding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);
        _bindings[Binding.NormalizeKeyName(binding.KeyName)] = binding;
    }

    public void Add(string keyName, string action, string? description = null)
        => Add(new Binding(keyName, action, description));

    public bool TryResolve(KeyEvent ev, out Binding binding)
        => _bindings.TryGetValue(ev.Name, out binding!);

    public void Clear() => _bindings.Clear();
}
