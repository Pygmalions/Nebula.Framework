using System;
using NUnit.Framework;

namespace Nebula.Buffers.Test;

[TestFixture]
public class ByteArrayTest
{
    [Test]
    public void ReadSingleByte()
    {
        var array = new ByteArray(0x00, 0x01, 0x02, 0x03, 0x04)
        {
            WriterPosition = 5
        };
        Assert.True(array.ReadByte(out var singleByte));
        Assert.AreEqual(0x00, singleByte);
    }

    [Test]
    public void ReadBytesViaArray()
    {
        var array = new ByteArray(0x00, 0x01, 0x02, 0x03, 0x04)
        {
            WriterPosition = 5
        };
        Assert.AreEqual(new byte[]{0x00, 0x01}, array.ReadBytes(2));
        Assert.AreEqual(new byte[]{0x02, 0x03, 0x04}, array.ReadBytes(3));
    }

    [Test]
    public void WriteSingleByte()
    {
        var array = new ByteArray(5);
        array.WriteByte(0x01);
        Assert.AreEqual(0x01, array.Content[0]);
        array.WriteByte(0x02);
        Assert.AreEqual(0x02, array.Content[1]);
    }

    [Test]
    public void ReadBytesViaSpan()
    {
        var array = new ByteArray(0x00, 0x01, 0x02, 0x03, 0x04)
        {
            WriterPosition = 5
        };
        var result1 = new byte[3];
        Assert.AreEqual(3, array.ReadBytes(result1));
        Assert.AreEqual(new byte[]{0x00, 0x01, 0x02}, result1);
        var result2 = new byte[3];
        Assert.AreEqual(2, array.ReadBytes(result2));
        Assert.AreEqual(new byte[]{0x03, 0x04, 0x00}, result2);
    }
    
    [Test]
    public void WriteBytesViaArray()
    {
        var array = new ByteArray(5);
        array.WriteBytes(new byte[]{0x01, 0x02});
        Assert.AreEqual(new byte[]{0x01, 0x02, 0x00, 0x00, 0x00}, array.Content);
        array.WriteBytes(new byte[]{0x03, 0x04, 0x05});
        Assert.AreEqual(new byte[]{0x01, 0x02, 0x03, 0x04, 0x05}, array.Content);
    }

    [Test]
    public void WriteBytesViaSpan()
    {
        var data = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
        var array = new ByteArray(5);
        array.WriteBytes(data.AsSpan()[..2]);
        Assert.AreEqual(new byte[]{0x01, 0x02, 0x00, 0x00, 0x00}, array.Content);
        array.WriteBytes(data.AsSpan()[2..]);
        Assert.AreEqual(new byte[]{0x01, 0x02, 0x03, 0x04, 0x05}, array.Content);
    }

    [Test]
    public void Shrink()
    {
        var array = new ByteArray(0x00, 0x01, 0x02, 0x03, 0x04)
        {
            WriterPosition = 5
        };
        var result1 = new byte[3];
        Assert.AreEqual(3, array.ReadBytes(result1));
        Assert.AreEqual(new byte[]{0x00, 0x01, 0x02}, result1);
        Assert.DoesNotThrow(()=> array.Shrink(4));
        var result2 = new byte[3];
        Assert.AreEqual(1, array.ReadBytes(result2));
        Assert.AreEqual(new byte[]{0x03, 0x00, 0x00}, result2);
        Assert.DoesNotThrow(()=> array.Shrink(5));
    }
    
    [Test]
    public void Slice()
    {
        var testingArray = new ByteArray(0x1, 0x2, 0x3, 0x4, 0x5);
        Assert.AreEqual(new byte[] { 0x02, 0x03 }, testingArray.Slice(1, 2).ToArray());
    }

    [Test]
    public void IndexAccess()
    {
        var testingArray = new ByteArray(0x1, 0x2, 0x3, 0x4, 0x5);
        Assert.AreEqual(0x03, testingArray[2]);
        testingArray[3] = 0x09;
        Assert.AreEqual(new byte[]{0x1, 0x2, 0x3, 0x9, 0x5}, testingArray.Content);
    }

    [Test]
    public void RangeAccess()
    {
        var testingArray = new ByteArray(0x1, 0x2, 0x3, 0x4, 0x5);
        Assert.AreEqual(new byte[]{0x03, 0x04}, testingArray[2..4]);
    }

    [Test]
    public void CastToBytes()
    {
        var testingArray = new ByteArray(0x1, 0x2, 0x3, 0x4, 0x5);
        Assert.AreSame(testingArray.Content, (byte[])testingArray);
    }
}