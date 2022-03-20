using Nebula.Presetting;
using Nebula.Presetting.Features;
using Nebula.Resource;

namespace Nebula.Framework.Presets;

/// <summary>
/// Location an object from the current domain.
/// </summary>
public class ObjectLocationPreset : IItem<object?>
{
    [Property("Class")]
    public IItem<Type> Class;
    
    [Property("Scopes")]
    public IItem<Scopes> Scopes;
    
    [Content]
    public IItem<IIdentifier> Identifier;

    public ObjectLocationPreset(IItem<Type> type, IItem<IIdentifier> identifier, IItem<Scopes> scopes)
    {
        Class = type;
        Scopes = scopes;
        Identifier = identifier;
    }

    public object? Translate() => 
        Domain.Current.Acquire(Class.Translate(), Identifier.Translate(), Scopes.Translate());
}