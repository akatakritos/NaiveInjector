using FluentAssertions;

namespace NaiveInjector.Tests;

public class ResolveTests
{
    public class UnregisteredClass
    {
    }

    public class SimpleClass
    {
    }
    
    public class WithDependency(SimpleClass simpleClass)
    {
        public SimpleClass SimpleClass { get; } = simpleClass;
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
    
    [Fact(DisplayName = "It throws an exception when trying to resolve an unregistered class")]
    public void UnregisteredTest()
    {
        var registry = new NaiveRegistry();
        var injector = registry.Build();

        injector.Invoking(i => i.Resolve<UnregisteredClass>())
            .Should().Throw<UnregistedTypeException>();
    }
    
    [Fact(DisplayName = "It can resolve a class with a dependency")]
    public void WithDependencyTest()
    {
        var registry = new NaiveRegistry();
        registry.Register<SimpleClass>();
        registry.Register<WithDependency>();

        var injector = registry.Build();
        var instance = injector.Resolve<WithDependency>();
        
        instance.Should().BeOfType<WithDependency>();
        instance.SimpleClass.Should().BeOfType<SimpleClass>();
    }
}