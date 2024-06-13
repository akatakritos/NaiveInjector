using FluentAssertions;

namespace NaiveInjector.Tests;

public class ResolveInterfaceTests
{
    interface IHasInterface
    {
    }

    class ImplementsInterface : IHasInterface
    {
    }
    
    [Fact(DisplayName = "It can resolve an interface")]
    public void InterfaceTest()
    {
        var registry = new NaiveRegistry();
        registry.Register<IHasInterface, ImplementsInterface>();

        var injector = registry.Build();
        var instance = injector.Resolve<IHasInterface>();
        
        instance.Should().BeOfType<ImplementsInterface>();
    }
}