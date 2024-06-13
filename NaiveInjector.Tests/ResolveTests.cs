using FluentAssertions;

namespace NaiveInjector.Tests;

public class ResolveTests
{
    public class SimpleClass
    {
    }
    

    [Fact(DisplayName = "It can resolve a simple class")]
    public void Basic()
    {
        var registry = new NaiveRegistry();
        registry.Register<SimpleClass>();

        var injector = registry.Build();
        var instance = injector.Resolve<SimpleClass>();
        
        instance.Should().BeOfType<SimpleClass>();

    }
}