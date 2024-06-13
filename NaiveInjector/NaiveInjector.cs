using System.Diagnostics;

namespace NaiveInjector;

public class NaiveRegistry
{
    private readonly List<Type> _registeredTypes = new();
    public void Register<T>()
    {
        _registeredTypes.Add(typeof(T));
    }
    
    public NaiveInjector Build()
    {
        return new NaiveInjector(_registeredTypes);
    }
}

public class NaiveInjector
{
    private readonly List<Type> _registeredTypes;

    public NaiveInjector(List<Type> registeredTypes)
    {
        _registeredTypes = registeredTypes;
    }
    public T Resolve<T>()
    {
        return (T)Resolve(typeof(T));
    }
    
    public object Resolve(Type type)
    {
        if (!_registeredTypes.Contains(type))
        {
            throw new UnregistedTypeException(type);
        }
        
        var ctor = type.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .First();
        
        var parameters = ctor.GetParameters()
            .Select(p => Resolve(p.ParameterType))
            .ToArray();
        var instance = Activator.CreateInstance(type, parameters);
        Debug.Assert(instance != null);
        return instance;
    }
}

public class UnregistedTypeException : Exception
{
    public UnregistedTypeException(Type type)
        : base($"Type {type} is not registered")
    {
    }
}