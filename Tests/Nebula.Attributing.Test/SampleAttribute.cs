using System;
using System.Reflection;

namespace Nebula.Attributing.Test;

public class SampleAttribute : ScriptAttribute
{
    public override void Apply(object holderObject, MemberInfo holderMember)
    {
        if (holderObject is SampleHolder holder)
        {
            holder.AppliedMemberAttributes.Add(holderMember.Name);
        }
    }

    public override void Apply(object holderObject, Type holderType)
    {
        if (holderObject is SampleHolder holder)
        {
            holder.AppliedTypeAttributes.Add(holderType.Name);
        }
    }
}