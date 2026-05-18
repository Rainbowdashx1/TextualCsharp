namespace TextualCsharp.Input;

/// <summary>
/// Selecciona el <see cref="ITerminalDriver"/> adecuado según el sistema operativo
/// y el entorno de ejecución (terminal local vs SSH vs headless).
///
/// <para>
/// <b>Reglas de selección:</b>
/// </para>
/// <list type="number">
///   <item><see cref="TerminalEnvironmentKind.Headless"/> ⇒ <see cref="HeadlessTerminalDriver"/>.</item>
///   <item>POSIX (Linux/macOS/BSD) ⇒ <see cref="PosixTerminalDriver"/> tanto en local como en SSH.</item>
///   <item>Windows en sesión SSH ⇒ <see cref="PosixTerminalDriver"/> (los servidores SSH de
///         Windows como OpenSSH exponen un PTY estilo VT, no la consola Win32, así que
///         el driver POSIX/ANSI funciona mejor). Si <c>termios</c> no está disponible,
///         el driver hace fallback a <c>Console.ReadKey</c> automáticamente.</item>
///   <item>Windows local ⇒ <see cref="WindowsTerminalDriver"/> (usa Win32 ReadConsoleInput
///         para ratón nativo y mejor latencia).</item>
/// </list>
/// </summary>
public static class TerminalDriverFactory
{
    /// <summary>
    /// Crea el driver más apropiado para el entorno actual. Si <paramref name="forceKind"/>
    /// se especifica, fuerza ese entorno (útil para tests o flags <c>--ssh</c> de la app).
    /// </summary>
    public static ITerminalDriver Create(TerminalEnvironmentKind? forceKind = null)
    {
        var env = forceKind ?? TerminalEnvironment.Detect();

        if (env == TerminalEnvironmentKind.Headless)
            return new HeadlessTerminalDriver();

        if (TerminalEnvironment.IsPosix)
            return new PosixTerminalDriver();

        // Windows
        if (env == TerminalEnvironmentKind.RemoteSsh)
        {
            // OpenSSH Server (sshd) en Windows usa un PTY VT-style; el driver POSIX
            // de ANSI/XTermParser le sienta mejor que Win32 ReadConsoleInput.
            return new PosixTerminalDriver();
        }

        return new WindowsTerminalDriver();
    }
}
