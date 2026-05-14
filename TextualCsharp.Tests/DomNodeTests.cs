using TextualCsharp.Dom;
using TextualCsharp.Messaging;
using Xunit;

namespace TextualCsharp.Tests;

public class DomNodeTests
{
    private sealed class TestNode : DomNode
    {
        public int MountCalls;
        public int UnmountCalls;
        protected override void OnMount() => MountCalls++;
        protected override void OnUnmount() => UnmountCalls++;
    }

    [Fact]
    public void Children_Add_sets_parent_and_fires_events()
    {
        var parent = new TestNode();
        var child = new TestNode();
        DomNode? added = null;
        parent.Children.Added += n => added = n;
        parent.Children.Add(child);
        Assert.Same(child, added);
        Assert.Same(parent, child.Parent);
        Assert.Equal(1, parent.Children.Count);
    }

    [Fact]
    public void Children_Remove_clears_parent()
    {
        var parent = new TestNode();
        var child = new TestNode();
        parent.Children.Add(child);
        Assert.True(parent.Children.Remove(child));
        Assert.Null(child.Parent);
    }

    [Fact]
    public async Task MountAsync_mounts_recursively_and_calls_OnMount()
    {
        var root = new TestNode();
        var child = new TestNode();
        root.Children.Add(child);
        await root.MountAsync();
        try
        {
            Assert.True(root.IsMounted);
            Assert.True(child.IsMounted);
            Assert.Equal(1, root.MountCalls);
            Assert.Equal(1, child.MountCalls);
        }
        finally
        {
            await root.UnmountAsync();
        }
        Assert.False(root.IsMounted);
        Assert.False(child.IsMounted);
        Assert.Equal(1, root.UnmountCalls);
        Assert.Equal(1, child.UnmountCalls);
    }

    [Fact]
    public void Walk_DepthFirst_visits_preorder()
    {
        var a = new TestNode { Id = "a" };
        var b = new TestNode { Id = "b" };
        var c = new TestNode { Id = "c" };
        var d = new TestNode { Id = "d" };
        a.Children.Add(b);
        a.Children.Add(c);
        b.Children.Add(d);
        var ids = Walk.DepthFirst(a).Select(n => ((TestNode)n).Id).ToArray();
        Assert.Equal(new[] { "a", "b", "d", "c" }, ids);
    }

    [Fact]
    public void Walk_BreadthFirst_visits_levels()
    {
        var a = new TestNode { Id = "a" };
        var b = new TestNode { Id = "b" };
        var c = new TestNode { Id = "c" };
        var d = new TestNode { Id = "d" };
        a.Children.Add(b);
        a.Children.Add(c);
        b.Children.Add(d);
        var ids = Walk.BreadthFirst(a).Select(n => ((TestNode)n).Id).ToArray();
        Assert.Equal(new[] { "a", "b", "c", "d" }, ids);
    }

    [Fact]
    public void Root_returns_topmost_ancestor()
    {
        var a = new TestNode();
        var b = new TestNode();
        var c = new TestNode();
        a.Children.Add(b);
        b.Children.Add(c);
        Assert.Same(a, c.Root);
    }
}
