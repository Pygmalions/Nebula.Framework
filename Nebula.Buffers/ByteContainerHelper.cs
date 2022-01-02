using System.Text;

namespace Nebula.Buffers;

/// <summary>
/// This class provides read and write methods with type for IByteContainer.
/// </summary>
public static class ByteContainerHelper
{
    public static void Write(this IByteContainer container, short value)
    {
        container.WriteBytes(BitConverter.GetBytes(value));
    }
    
    public static void Write(this IByteContainer container, ushort value)
    {
        container.WriteBytes(BitConverter.GetBytes(value));
    }
    
    public static void Write(this IByteContainer container, int value)
    {
        container.WriteBytes(BitConverter.GetBytes(value));
    }
    
    public static void Write(this IByteContainer container, uint value)
    {
        container.WriteBytes(BitConverter.GetBytes(value));
    }
    
    public static void Write(this IByteContainer container, long value)
    {
        container.WriteBytes(BitConverter.GetBytes(value));
    }
    
    public static void Write(this IByteContainer container, ulong value)
    {
        container.WriteBytes(BitConverter.GetBytes(value));
    }

    public static void Write(this IByteContainer container, string text, Encoding? encoding = null)
    {
        encoding ??= Encoding.Default;
        container.WriteBytes(encoding.GetBytes(text).AsSpan());
    }
    
    public static void Read(this IByteContainer container, out short value)
    {
        var buffer = container.ReadBytes(sizeof(short));
        value = BitConverter.ToInt16(buffer.AsSpan());
    }
    
    public static void Read(this IByteContainer container, out ushort value)
    {
        var buffer = container.ReadBytes(sizeof(ushort));
        value = BitConverter.ToUInt16(buffer.AsSpan());
    }
    
    public static void Read(this IByteContainer container, out int value)
    {
        var buffer = container.ReadBytes(sizeof(int));
        value = BitConverter.ToInt32(buffer.AsSpan());
    }
    
    public static void Read(this IByteContainer container, out uint value)
    {
        var buffer = container.ReadBytes(sizeof(uint));
        value = BitConverter.ToUInt32(buffer.AsSpan());
    }
    
    public static void Read(this IByteContainer container, out long value)
    {
        var buffer = container.ReadBytes(sizeof(long));
        value = BitConverter.ToInt64(buffer.AsSpan());
    }
    
    public static void Read(this IByteContainer container, out ulong value)
    {
        var buffer = container.ReadBytes(sizeof(ulong));
        value = BitConverter.ToUInt64(buffer.AsSpan());
    }
    
    /// <summary>
    /// Read a string for a byte container.
    /// </summary>
    /// <param name="container">Container to read from.</param>
    /// <param name="count">Count of byte to form a text.</param>
    /// <param name="text">Formed string.</param>
    /// <param name="encoding">Encoding to use for decoding bytes into a string.</param>
    public static void Read(this IByteContainer container, out string text, int count = -1, Encoding? encoding = null)
    {
        encoding ??= Encoding.Default;
        var buffer = container.ReadBytes(count);
        text = encoding.GetString(buffer.AsSpan());
    }

    /// <summary>
    /// Calculate the available bytes to read in this container.
    /// </summary>
    /// <param name="container">Container to calculate.</param>
    /// <returns>Count of available bytes to write.</returns>
    public static int AvailableBytesToRead(this IByteContainer container)
        => container.WriterPosition - container.ReaderPosition;

    /// <summary>
    /// Calculate the available free bytes space for writing in this container.
    /// </summary>
    /// <param name="container">Container to calculate.</param>
    /// <returns>Count of free space for writing.</returns>
    public static int AvailableBytesToWrite(this IByteContainer container)
        => container.Capacity - container.WriterPosition;
}