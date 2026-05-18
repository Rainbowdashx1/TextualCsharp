using System.Runtime.InteropServices;

namespace TextualCsharp.Input;

/// <summary>
/// Tipo de entorno de terminal detectado en tiempo de ejecución.
/// </summary>
public enum TerminalEnvironmentKind
{
    /// <summary>Terminal local (Windows Console, conhost, Windows Terminal, xterm, gnome-terminal…).</summary>
    Local,
    /// <summary>Sesión remota establecida vía SSH.</summary>
    RemoteSsh,
    /// <summary>Entorno sin TTY (CI, redirección a fichero, contenedor sin terminal).</summary>
    Headless,
}

/// <summary>
/// Detección automática del entorno de terminal en el que corre la aplicación.
/// Permite distinguir terminal local de sesión SSH y elegir el driver adecuado.
///
/// <para>
/// <b>Método de detección:</b>
/// </para>
/// <list type="bullet">
///   <item>Si <c>Console.IsInputRedirected</c> y <c>Console.IsOutputRedirected</c> son <c>true</c>
///         y no hay variables SSH, se considera <see cref="TerminalEnvironmentKind.Headless"/>.</item>
///   <item>Si está definida cualquiera de <c>SSH_CONNECTION</c>, <c>SSH_CLIENT</c> o
///         <c>SSH_TTY</c>, se considera <see cref="TerminalEnvironmentKind.RemoteSsh"/>.</item>
///   <item>Como heurística adicional, se inspecciona <c>TERM</c> en busca de valores
///         típicos de cliente SSH (<c>xterm-256color</c> en combinación con
///         <c>SSH_AUTH_SOCK</c>, etc.).</item>
///   <item>En cualquier otro caso se asume <see cref="TerminalEnvironmentKind.Local"/>.</item>
/// </list>
///
/// <para>
/// <b>Limitaciones:</b> los procesos lanzados por usuarios remotos a través de
/// herramientas que no exportan las variables <c>SSH_*</c> (por ejemplo, <c>sudo</c>
/// sin <c>-E</c>, ciertos shells de contenedor o sesiones <c>tmux</c> detached que
/// pierden el entorno original) pueden quedar clasificadas como locales. Para
/// estos casos se puede forzar el modo SSH con la variable de entorno
/// <c>TEXTUALCSHARP_FORCE_SSH=1</c> o con <see cref="ForceKind"/>.
/// </para>
/// </summary>
public static class TerminalEnvironment
{
    /// <summary>Override manual para tests o casos avanzados (p.e. <c>--ssh</c>).</summary>
    public static TerminalEnvironmentKind? ForceKind { get; set; }

    /// <summary>Variables de entorno estándar que delatan una sesión SSH.</summary>
    public static readonly string[] SshIndicatorVariables =
    {
        "SSH_CONNECTION", "SSH_CLIENT", "SSH_TTY",
    };

    /// <summary>Devuelve el entorno detectado. No cachea para soportar tests.</summary>
    public static TerminalEnvironmentKind Detect()
    {
        if (ForceKind is { } forced) return forced;

        if (string.Equals(
                Environment.GetEnvironmentVariable("TEXTUALCSHARP_FORCE_SSH"),
                "1", StringComparison.Ordinal))
        {
            return TerminalEnvironmentKind.RemoteSsh;
        }

        bool ssh = false;
        foreach (var name in SshIndicatorVariables)
        {
            var v = Environment.GetEnvironmentVariable(name);
            if (!string.IsNullOrEmpty(v)) { ssh = true; break; }
        }

        bool headless;
        try
        {
            headless = Console.IsInputRedirected && Console.IsOutputRedirected;
        }
        catch
        {
            headless = false;
        }

        if (ssh) return TerminalEnvironmentKind.RemoteSsh;
        if (headless) return TerminalEnvironmentKind.Headless;
        return TerminalEnvironmentKind.Local;
    }

    /// <summary>Atajo: <c>true</c> si el entorno es una sesión SSH.</summary>
    public static bool IsSsh => Detect() == TerminalEnvironmentKind.RemoteSsh;

    /// <summary>Atajo: <c>true</c> si estamos en Linux o macOS (sistemas POSIX).</summary>
    public static bool IsPosix =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
        RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD);

    /// <summary>Atajo: <c>true</c> si estamos en Windows.</summary>
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    /// <summary>Resumen legible (útil para logs).</summary>
    public static string Describe()
    {
        var kind = Detect();
        var os = IsWindows ? "Windows" : (IsPosix ? "POSIX" : "Other");
        var term = Environment.GetEnvironmentVariable("TERM") ?? "(unset)";
        return $"TerminalEnvironment: kind={kind}, os={os}, TERM={term}";
    }
}
