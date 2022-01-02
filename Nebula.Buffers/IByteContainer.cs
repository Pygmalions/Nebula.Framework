namespace Nebula.Buffers;

public interface IByteContainer
{
    /// <summary>
    /// Current position index for reader methods.
    /// All reading methods will start at this index.
    /// </summary>
    int ReaderPosition { get; set; }
    
    /// <summary>
    /// Current position index for writer methods.
    /// All writing methods will start at this index.
    /// </summary>
    int WriterPosition { get; set; }
    
    /// <summary>
    /// Capacity of the bytes array.
    /// </summary>
    int Capacity { get; }

    /// <summary>
    /// Write a byte into this array.
    /// </summary>
    /// <param name="data">Byte to write.</param>
    void WriteByte(byte data);
    /// <summary>
    /// Write bytes into this array.
    /// </summary>
    /// <param name="data">Bytes to write.</param>
    void WriteBytes(byte[] data);
    /// <summary>
    /// Write bytes into this array with a span of bytes.
    /// </summary>
    /// <param name="data">Bytes to write.</param>
    void WriteBytes(Span<byte> data);
    
    /// <summary>
    /// Read a byte from this array.
    /// </summary>
    /// <param name="data">Result of reading.</param>
    /// <returns>If true, data is the result of reading, otherwise no byte is read.</returns>
    bool ReadByte(out byte data);
    /// <summary>
    /// Read bytes from this array to the a new array.
    /// </summary>
    /// <param name="readingCount">Max count of bytes to read.</param>
    /// <returns>
    /// New array to store the result of reading.
    /// Its length is the actual count of read bytes.
    /// </returns>
    byte[]? ReadBytes(int readingCount = -1);
    /// <summary>
    /// Read bytes from this array to the given span of array.
    /// This method will create a slice of the content, which means the bytes in data may change after writing methods
    /// are invoked.
    /// </summary>
    /// <param name="buffer">Memory of read bytes.</param>
    /// <returns>Actual count of read bytes.</returns>
    int ReadBytes(Span<byte> buffer);

    byte this[int index] { get; set; }
    
    byte[] this[Range range] { get; }
}