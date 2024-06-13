using System.Diagnostics;

namespace NaiveInjector;

internal enum Lifetime
{
    Transient = 0,
    Singleton = 1,
    Scoped = 2
}

internal record Registration(Type InterfaceType, Type ImplementationType, Lifetime Lifetime);
public class NaiveRegistry
{
    private readonly List<Registration> _registrations = [];
    public void Register<T>()
    {
        _registrations.Add(new Registration(typeof(T), typeof(T), Lifetime.Transient));
    }

    public void Register<TInterface, TImplementation>() where TImplementation : TInterface
    {
        _registrations.Add(new Registration(typeof(TInterface), typeof(TImplementation), Lifetime.Transient));
    }
    
    public void RegisterSingleton<T>()
    {
        _registrations.Add(new Registration(typeof(T), typeof(T), Lifetime.Singleton));
    }
    
    public void RegisterSingleton<TInterface, TImplementation>() where TImplementation : TInterface
    {
        _registrations.Add(new Registration(typeof(TInterface), typeof(TImplementation), Lifetime.Singleton));
    }
    
    public void RegisterScoped<T>()
    {
        _registrations.Add(new Registration(typeof(T), typeof(T), Lifetime.Scoped));
    }
    
    public void RegisterScoped<TInterface, TImplementation>() where TImplementation : TInterface
    {
        _registrations.Add(new Registration(typeof(TInterface), typeof(TImplementation), Lifetime.Scoped));
    }
    
    public NaiveInjector Build()
    {
        return new NaiveInjector(_registrations);
    }
}

public class NaiveInjector: IScope
{
    private readonly List<Registration> _registeredTypes;
    private readonly Dictionary<Registration, object> _singletons = new();
    private readonly IScope _rootScope;

    internal NaiveInjector(List<Registration> registeredTypes)
    {
        _registeredTypes = registeredTypes;
        _rootScope = new Scope(registeredTypes, _singletons);
    }
    
    
    public IScope BeginScope()
    {
        return new Scope(_registeredTypes, _singletons);
    }

    public T Resolve<T>() => _rootScope.Resolve<T>();

    public object Resolve(Type type) => _rootScope.Resolve(type);
}

public interface IScope
{
    T Resolve<T>();
    object Resolve(Type type);
}

internal class Scope: IScope
{
    private readonly List<Registration> _registeredTypes;
    private readonly Dictionary<Registration, object> _singletons;
    private readonly Dictionary<Registration, object> _scopedInstances = new();

    public Scope(List<Registration> registeredTypes, Dictionary<Registration, object> singletons)
    {
        _registeredTypes = registeredTypes;
        _singletons = singletons;
    }
    
    public T Resolve<T>()
    {
        return (T)Resolve(typeof(T));
    }
    
    public object Resolve(Type type)
    {
        var registration = _registeredTypes
            .FirstOrDefault(r => r.InterfaceType == type);
        if (registration == null)
        {
            throw new UnregisteredTypeException(type);
        }

        if (registration.Lifetime == Lifetime.Singleton)
        {
            if (!_singletons.TryGetValue(registration, out var instance))
            {
                instance = CreateInstance(registration);
                _singletons[registration] = instance;
            }

            return instance;
        }

        if (registration.Lifetime == Lifetime.Scoped)
        {
            if (!_scopedInstances.TryGetValue(registration, out var instance))
            {
                instance = CreateInstance(registration);
                _scopedInstances[registration] = instance;
            }

            return instance;

        }
        
        // Transient
        return CreateInstance(registration);

    }

    private object CreateInstance(Registration registration)
    {
        var ctor = registration.ImplementationType.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .First();
        
        var parameters = ctor.GetParameters()
            .Select(p => Resolve(p.ParameterType))
            .ToArray();
        var instance = Activator.CreateInstance(registration.ImplementationType, parameters);
        Debug.Assert(instance != null);
        
        return instance;
    }
}


public class UnregisteredTypeException : Exception
{
    public UnregisteredTypeException(Type type)
        : base($"Type {type} is not registered")
    {
    }
}