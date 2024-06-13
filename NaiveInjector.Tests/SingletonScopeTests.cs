using FluentAssertions;

namespace NaiveInjector.Tests;

public class SingletonScopeTests
{
    class Singleton {}

    class RequiresSingleton(Singleton singleton)
    {
        public Singleton Singleton { get; } = singleton;
    }

    [Fact(DisplayName = "A singleton resolves the same instance every time")]
    public void SingletonTest()
    {
        var registry = new NaiveRegistry();
        registry.RegisterSingleton<Singleton>();
        registry.Register<RequiresSingleton>();

        var injector = registry.Build();
        var instance1 = injector.Resolve<RequiresSingleton>();
        var instance2 = injector.Resolve<RequiresSingleton>();
        
        instance1.Should().NotBeSameAs(instance2);
        instance1.Singleton.Should().BeSameAs(instance2.Singleton);

    }
    
}