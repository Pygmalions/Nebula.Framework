using NUnit.Framework;

namespace Nebula.Attributing.Test;

public class ScriptAttributeTest
{
    [Test]
    public void ApplyOnType()
    {
        var holder = new SampleHolder();
        holder.ApplyClassScriptAttribute();
        Assert.Contains("SampleHolder", holder.AppliedTypeAttributes);
    }

    [Test]
    public void ApplyOnMember()
    {
        var holder = new SampleHolder();
        holder.ApplyMemberScriptAttribute();
        Assert.Contains("PublicMember", holder.AppliedMemberAttributes);
        Assert.Contains("PublicMethod", holder.AppliedMemberAttributes);
    }
    
    [Test]
    public void ApplyOnPrivateMember()
    {
        var holder = new SampleHolder();
        holder.ApplyMemberScriptAttribute(true);
        Assert.Contains("PublicMember", holder.AppliedMemberAttributes);
        Assert.Contains("PublicMethod", holder.AppliedMemberAttributes);
        Assert.Contains("PrivateMember", holder.AppliedMemberAttributes);
        Assert.Contains("PrivateMethod", holder.AppliedMemberAttributes);
    }
}