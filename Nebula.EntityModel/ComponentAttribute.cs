using System;

namespace Nebula.EntityModel;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ComponentAttribute : Attribute
{
    public string Description = "";
}