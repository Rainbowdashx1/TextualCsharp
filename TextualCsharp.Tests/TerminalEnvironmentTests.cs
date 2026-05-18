using System;
using System.Threading.Tasks;
using TextualCsharp.Input;
using Xunit;

namespace TextualCsharp.Tests;

public class TerminalEnvironmentTests
{
    private static IDisposable ScopedEnv(string name, string? value)
    {
        var previous = Environment.GetEnvironmentVariable(name);
        Environment.SetEnvironmentVariable(name, value);
        return new Disposable(() => Environment.SetEnvironmentVariable(name, previous));
    }

    private static IDisposable ScopedForce(TerminalEnvironmentKind? kind)
    {
        var previous = TerminalEnvironment.ForceKind;
        TerminalEnvironment.ForceKind = kind;
        return new Disposable(() => TerminalEnvironment.ForceKind = previous);
    }

    [Fact]
    public void Detect_HonorsForceKind()
    {
        using var _ = ScopedForce(TerminalEnvironmentKind.RemoteSsh);
        Assert.Equal(TerminalEnvironmentKind.RemoteSsh, TerminalEnvironment.Detect());
        Assert.True(TerminalEnvironment.IsSsh);
    }

    [Fact]
    public void Detect_SshFromEnvironmentVariable()
    {
        using var _f = ScopedForce(null);
        using var _v = ScopedEnv("SSH_CONNECTION", "10.0.0.1 22 10.0.0.2 51000");
        using var _t = ScopedEnv("TEXTUALCSHARP_FORCE_SSH", null);
        Assert.Equal(TerminalEnvironmentKind.RemoteSsh, TerminalEnvironment.Detect());
    }

    [Fact]
    public void Detect_ForceSshViaSpecialVariable()
    {
        using var _f = ScopedForce(null);
        using var _c = ScopedEnv("SSH_CONNECTION", null);
        using var _l = ScopedEnv("SSH_CLIENT", null);
        using var _y = ScopedEnv("SSH_TTY", null);
        using var _v = ScopedEnv("TEXTUALCSHARP_FORCE_SSH", "1");
        Assert.Equal(TerminalEnvironmentKind.RemoteSsh, TerminalEnvironment.Detect());
    }

    [Fact]
    public async Task Factory_SshAlwaysReturnsPosixDriver()
    {
        await using var d = TerminalDriverFactory.Create(TerminalEnvironmentKind.RemoteSsh);
        Assert.IsType<PosixTerminalDriver>(d);
    }

    [Fact]
    public async Task Factory_HeadlessReturnsHeadlessDriver()
    {
        await using var d = TerminalDriverFactory.Create(TerminalEnvironmentKind.Headless);
        Assert.IsType<HeadlessTerminalDriver>(d);
    }

    [Fact]
    public void Describe_IncludesKindAndOs()
    {
        using var _ = ScopedForce(TerminalEnvironmentKind.Local);
        var s = TerminalEnvironment.Describe();
        Assert.Contains("kind=Local", s);
    }

    private sealed class Disposable : IDisposable
    {
        private readonly Action _onDispose;
        public Disposable(Action onDispose) { _onDispose = onDispose; }
        public void Dispose() => _onDispose();
    }
}
