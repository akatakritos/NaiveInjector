using System.Diagnostics;

namespace NaiveInjector;

internal enum Lifetime
{
    Transient = 0,
    Singleton = 1,
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
    
    public NaiveInjector Build()
    {
        return new NaiveInjector(_registrations);
    }
}

public class NaiveInjector
{
    private readonly List<Registration> _registeredTypes;
    private readonly Dictionary<Registration, object> _singletons = new();

    internal NaiveInjector(List<Registration> registeredTypes)
    {
        _registeredTypes = registeredTypes;
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