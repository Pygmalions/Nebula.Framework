using System.Collections.Generic;

namespace Nebula.Attributing.Test;

[SampleAttribute]
public class SampleHolder
{
    [SampleAttribute]
    public int PublicMember;

    [SampleAttribute]
    private float PrivateMember;

    [SampleAttribute]
    public void PublicMethod()
    {}
    
    [SampleAttribute]
    private void PrivateMethod()
    {}

    public readonly List<string> AppliedTypeAttributes = new();

    public readonly List<string> AppliedMemberAttributes = new();
}