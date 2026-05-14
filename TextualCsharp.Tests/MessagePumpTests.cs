using TextualCsharp.Messaging;
using Xunit;

namespace TextualCsharp.Tests;

public class MessagePumpTests
{
    private sealed record Ping(int N) : Message;

    private sealed class CountingPump : MessagePump
    {
        public int Count;
        public int Last;
        protected override ValueTask OnMessageAsync(Message message)
        {
            if (message is Ping p) { Count++; Last = p.N; }
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task Processes_messages_in_order()
    {
        await using var pump = new CountingPump();
        await pump.StartAsync();
        for (int i = 1; i <= 5; i++) await pump.PostAsync(new Ping(i));
        await pump.StopAsync();
        Assert.Equal(5, pump.Count);
        Assert.Equal(5, pump.Last);
    }

    [Fact]
    public async Task Stop_drains_pending_messages()
    {
        await using var pump = new CountingPump();
        await pump.StartAsync();
        await pump.PostAsync(new Ping(1));
        await pump.PostAsync(new Ping(2));
        await pump.StopAsync();
        Assert.Equal(2, pump.Count);
    }

    [Fact]
    public async Task TryPost_succeeds_when_running()
    {
        await using var pump = new CountingPump();
        await pump.StartAsync();
        Assert.True(pump.TryPost(new Ping(42)));
        await pump.StopAsync();
        Assert.Equal(42, pump.Last);
    }
}
