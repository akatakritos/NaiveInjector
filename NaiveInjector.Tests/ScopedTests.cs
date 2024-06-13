using FluentAssertions;

namespace NaiveInjector.Tests;

public class ScopedTests
{
    class Scoped
    {
    }

    class Singleton
    {
    }

    [Fact(DisplayName = "It can reuse a scoped instance")]
    public void ScopedTest()
    {
        var registry = new NaiveRegistry();
        registry.RegisterScoped<Scoped>();

        var injector = registry.Build();
        var scope = injector.BeginScope();
        
        var instance1 = scope.Resolve<Scoped>();
        var instance2 = scope.Resolve<Scoped>();

        instance1.Should().BeSameAs(instance2);
    }
    
    [Fact(DisplayName = "Different scoped get different scoped instances but same singleton")]
    public void ScopedSingletonTest()
    {
        var registry = new NaiveRegistry();
        registry.RegisterScoped<Scoped>();
        registry.RegisterSingleton<Singleton>();
         var injector = registry.Build();
        
        var scope1 = injector.BeginScope();
        var scope2 = injector.BeginScope();

        scope1.Resolve<Singleton>().Should().BeSameAs(scope2.Resolve<Singleton>());
        scope1.Resolve<Scoped>().Should().NotBeSameAs(scope2.Resolve<Scoped>());
    }
}