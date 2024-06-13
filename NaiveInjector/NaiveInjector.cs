namespace NaiveInjector;

public class NaiveRegistry
{
    public void Register<T>()
    {
    }
    
    public NaiveInjector Build()
    {
        return new NaiveInjector();
    }
}

public class NaiveInjector
{
    public T Resolve<T>()
    {
        return Activator.CreateInstance<T>();
    }
}