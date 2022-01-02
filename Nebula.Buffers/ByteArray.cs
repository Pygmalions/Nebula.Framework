namespace Nebula.Buffers;

/// <summary>
/// A byte array is a wrapper of byte array with separate reader and writer indexes.
/// </summary>
public class ByteArray : IByteContainer
{
    public readonly byte[] Content;

    private int _readerPosition;
    /// <inheritdoc />
    public int ReaderPosition
    {
        get => _readerPosition;
        set
        {
            if (value < 0)
                throw new ArgumentException("The given reader position is negative.");
            if (value > Capacity)
                throw new ArgumentException("The given reader position is bigger than capacity.");
            _readerPosition = value;
        }
    }

    private int _writerPosition;
    /// <inheritdoc />
    public int WriterPosition 
    { 
        get => _writerPosition;
        set
        {
            if (value < 0)
                throw new ArgumentException("The given writer position is negative.");
            if (value > Capacity)
                throw new ArgumentException("The given writer position is bigger than capacity.");
            _writerPosition = value;
        }
    }

    /// <inheritdoc />
    public int Capacity { get; private set; }

    /// <summary>
    /// Allocate a new byte array of the given capacity,
    /// and construct a new ByteArray on it.
    /// </summary>
    /// <param name="capacity">Capacity of the array to allocate.</param>
    /// <exception cref="ArgumentException">Throw if the given capacity is not positive.</exception>
    public ByteArray(int capacity = 128)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity of ByteArray must be positive.");    
        Content = new byte[capacity];
        Capacity = capacity;
    }
    
    /// <summary>
    /// Construct a ByteArray on the existing byte array.
    /// This ByteArray will use the given array directly.
    /// This constructor will not change the writer position,
    /// which means the initial writer position is still 0.
    /// </summary>
    /// <param name="array">Existing array to base on.</param>
    public ByteArray(params byte[] array)
    {
        Content = array;
        Capacity = array.Length;
    }

    /// <inheritdoc />
    public void WriteByte(byte data)
    {
        if (this.AvailableBytesToWrite() <= 0) throw new IndexOutOfRangeException(
            "ByteArray has no space for new bytes to write in.");
        Content[WriterPosition++] = data;
    }
    /// <inheritdoc />
    public void WriteBytes(byte[] data)
    {
        if (WriterPosition + data.Length > Capacity) throw new IndexOutOfRangeException(
            "ByteArray dose not have enough space for new bytes to write in.");
        data.CopyTo(Content, WriterPosition);
        WriterPosition += data.Length;
    }
    /// <inheritdoc />
    public void WriteBytes(Span<byte> data)
    {
        if (WriterPosition + data.Length > Capacity) throw new IndexOutOfRangeException(
            "ByteArray dose not have enough space for new bytes to write in.");
        data.CopyTo(new Span<byte>(Content, WriterPosition, Capacity - WriterPosition));
        WriterPosition += data.Length;
    }
    /// <inheritdoc />
    public bool ReadByte(out byte data)
    {
        data = 0;
        if (this.AvailableBytesToRead() <= 0) return false;
        data = Content[ReaderPosition++];
        return true;
    }
    /// <inheritdoc />
    public byte[]? ReadBytes(int readingCount = -1)
    {
        if (readingCount < 0) readingCount = Capacity - ReaderPosition;
        if (this.AvailableBytesToRead() <= 0 || readingCount == 0)
            return null;
        var buffer = new byte[readingCount];
        new Span<byte>(Content, ReaderPosition, readingCount).CopyTo(buffer);
        ReaderPosition += readingCount;
        return buffer; 
    }
    /// <inheritdoc />
    public int ReadBytes(Span<byte> buffer)
    {
        var readingCount = buffer.Length;
        if (readingCount < 0) readingCount = Capacity - ReaderPosition;
        if (readingCount > buffer.Length)
            throw new ArgumentException(
                "The given array does not have the enough capacity to store the result.");
        var availableBytes = this.AvailableBytesToRead();
        if (availableBytes <= 0 || readingCount == 0) return 0;
        readingCount = readingCount < availableBytes ? readingCount : availableBytes;
        new Span<byte>(Content, ReaderPosition, readingCount).CopyTo(buffer);
        ReaderPosition += readingCount;
        return readingCount; 
    }
    /// <summary>
    /// Get the memory span of a slice of this array.
    /// </summary>
    /// <param name="startingIndex">Starting index to slice from.</param>
    /// <param name="count">Count of bytes to slice.</param>
    /// <returns>A slice of this array.</returns>
    /// <exception cref="ArgumentException">Throw when starting index is invalid.</exception>
    public Memory<byte> Slice(int startingIndex, int count)
    {
        if (startingIndex < 0)
            throw new ArgumentException("Starting index to slice must not be negative.");
        if (startingIndex >= Capacity)
            throw new ArgumentException(
                "Starting index is bigger than the capacity of ByteArray.");
        if (count < 0 || startingIndex + count > Capacity) count = Capacity - ReaderPosition;
        return new Memory<byte>(Content, startingIndex, count);
    }

    /// <summary>
    /// Shrink this array to a capacity which is smaller than the real capacity of the content array.
    /// This method will adjust the writer position and reader position to suite the new capacity.
    /// </summary>
    /// <param name="size">New capacity to shrink to.</param>
    /// <returns>Reduced capacity, which is the content capacity minus logic capacity.</returns>
    /// <exception cref="ArgumentException">
    /// Throw when new size of capacity is negative or bigger than the real capacity of the content array.
    /// </exception>
    public int Shrink(int size)
    {
        if (size < 0)
            throw new ArgumentException("Capacity to shrink can not be negative.");
        if (size > Content.Length)
            throw new ArgumentException("Capacity to shrink can not be larger than the real capacity.");
        Capacity = size;
        if (WriterPosition > Capacity)
            WriterPosition = Capacity;
        if (ReaderPosition > Capacity)
            ReaderPosition = Capacity;
        return Content.Length - Capacity;
    }
    
    /// <inheritdoc />
    public byte this[int index]
    {
        get => Content[index];
        set => Content[index] = value;
    }
    /// <inheritdoc />
    public byte[] this[Range range] => Content[range];
    
    /// <summary>
    /// Implicit cast operator to byte[].
    /// </summary>
    /// <param name="array">ByteArray to cast to byte[].</param>
    /// <returns>The reference to the Content of the given ByteArray.</returns>
    public static implicit operator byte[](ByteArray array)
    {
        return array.Content;
    }
}