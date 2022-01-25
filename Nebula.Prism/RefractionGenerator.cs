using System.Reflection;

namespace Nebula.Prism;

public class RefractionGenerator
{
    private static readonly DynamicGenerator Generator = new(
        Assembly.GetExecutingAssembly().GetName().Name + ".Prism", "Refractions");
    
    public static Type Get(Type baseType)
    {
        return Generator.GetRefraction(baseType);
    }

    public static Type Get<TBase>()
    {
        return Get(typeof(TBase));
    }
}