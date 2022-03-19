namespace Nebula.Resource;

public static class ScopesHelper
{
    /// <summary>
    /// Split a scopes enumeration into an list of contained scope..
    /// </summary>
    /// <param name="scopes">Scopes value to split.</param>
    /// <returns>A list of Scope contained in the given scopes.</returns>
    public static IReadOnlyList<Scope> Split(this Scopes scopes)
    {
        return Enum.GetValues<Scope>().Where(flag => ((byte)flag & (byte)scopes) == (byte)flag).ToList();
    }
}