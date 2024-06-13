using System.Diagnostics;

namespace NaiveInjector;

internal record Registration(Type InterfaceType, Type ImplementationType);
public class NaiveRegistry
{
    private readonly List<Registration> _registrations = [];
    public void Register<T>()
    {
        _registrations.Add(new Registration(typeof(T), typeof(T)));
    }

    public void Register<TInterface, TImplementation>() where TImplementation : TInterface
    {
        _registrations.Add(new Registration(typeof(TInterface), typeof(TImplementation)));
    }
    
    public NaiveInjector Build()
    {
        return new NaiveInjector(_registrations);
    }
}

public class NaiveInjector
{
    private readonly List<Registration> _registeredTypes;

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