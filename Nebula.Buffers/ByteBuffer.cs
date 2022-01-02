namespace Nebula.Buffers;

/// <summary>
/// A ByteBuffer is a chain of ByteArray, provides reading and writing methods based on the buffer chain.
/// </summary>
public class ByteBuffer : IByteContainer
{
    /// <summary>
    /// Buffer chain.
    /// </summary>
    private readonly LinkedList<ByteArray> _content = new();
    
    /// <inheritdoc />
    public int ReaderPosition
    {
        get => CurrentReadingBuffer.ReaderPosition;
        set
        {
            if (value < 0)
                throw new ArgumentException("The given reader position is negative.");
            SeekBuffer(value, out var node, out var offset);
            _currentReadingNode = node;
            CurrentReadingBuffer.ReaderPosition = offset;
        }
    }
    
    /// <inheritdoc />
    public int WriterPosition 
    { 
        get => _currentWritingNode.Value.WriterPosition;
        set
        {
            if (value < 0)
                throw new ArgumentException("The given writer position is negative.");
            SeekBuffer(value, out var node, out var offset);
            _currentWritingNode = node;
            CurrentWritingBuffer.WriterPosition = offset;
        }
    }

    /// <inheritdoc />
    public int Capacity { get; private set; }

    /// <summary>
    /// Byte array currently in use for writing.
    /// </summary>
    private LinkedListNode<ByteArray> _currentReadingNode;

    /// <summary>
    /// Get the ByteArray through CurrentReadingNode.
    /// </summary>
    private ByteArray CurrentReadingBuffer => _currentReadingNode.ValueRef;
    
    /// <summary>
    /// Byte array currently in use for reading.
    /// </summary>
    private LinkedListNode<ByteArray> _currentWritingNode;

    /// <summary>
    /// Get the ByteArray through CurrentWritingNode.
    /// </summary>
    private ByteArray CurrentWritingBuffer => _currentReadingNode.ValueRef;

    /// <summary>
    /// Construct a byte buffer with a initial buffer.
    /// </summary>
    public ByteBuffer()
    {
        var initialBuffer = AllocateSpace();
        _currentReadingNode = initialBuffer;
        _currentWritingNode = initialBuffer;
    }
    
    /// <summary>
    /// Seek the buffer which the byte of the given position belongs to.
    /// </summary>
    /// <param name="position">Position of the byte to seek for.</param>
    /// <param name="node">Found buffer node which owns the given position.</param>
    /// <param name="offset">Offset of the found buffer for the given position.</param>
    /// <exception cref="ArgumentException">Throw when position is negative or bigger than capacity.</exception>
    private void SeekBuffer(int position, out LinkedListNode<ByteArray> node, out int offset)
    {
        if (position < 0)
            throw new ArgumentException("The given position to seek for is negative.");
        for (var arrayNode = _content.First; arrayNode is not null; arrayNode = arrayNode.Next)
        {
            if (position < arrayNode.ValueRef.Capacity)
            {
                node = arrayNode;
                offset = position;
                return;
            }
            position -= arrayNode.ValueRef.Capacity;
        }
        throw new ArgumentException("The given position to seek for is bigger than capacity.");
    }

    /// <summary>
    /// Allocate new buffer and shrink the last buffer to the writer position,
    /// then added the new allocated buffer to the end of the buffer chain.
    /// </summary>
    /// <param name="size">Space of bytes to alloc.</param>
    /// <returns>New allocated space.</returns>
    private LinkedListNode<ByteArray> AllocateSpace(int size = 256)
    {
        size = size switch
        {
            < 256 => 256,
            < 512 => 512,
            < 1024 => 1024,
            < 2048 => 2048,
            < 4096 => 4096,
            _ => size
        };
        var buffer = new ByteArray(size);
        return AppendSpace(buffer);
    }
    
    /// <inheritdoc />
    public void WriteByte(byte data)
    {
        if (CurrentWritingBuffer.AvailableBytesToWrite() > 0)
        {
            CurrentWritingBuffer.WriteByte(data);
            return;
        }
        AllocateSpace().ValueRef.WriteByte(data);
    }

    /// <inheritdoc />
    public void WriteBytes(byte[] data)
    {
        var remainingCount = data.Length - CurrentWritingBuffer.AvailableBytesToWrite();
        if (remainingCount <= 0)
        {
            CurrentWritingBuffer.WriteBytes(data.AsSpan());
        }
        else
        {
            CurrentWritingBuffer.WriteBytes(data.AsSpan(0, CurrentWritingBuffer.AvailableBytesToWrite()));
            AllocateSpace(remainingCount).ValueRef.WriteBytes(data.AsSpan(remainingCount));
        }
    }

    /// <inheritdoc />
    public void WriteBytes(Span<byte> data)
    {
        var remainingCount = data.Length - CurrentWritingBuffer.AvailableBytesToWrite();
        if (remainingCount <= 0)
        {
            CurrentWritingBuffer.WriteBytes(data);
        }
        else
        {
            CurrentWritingBuffer.WriteBytes(data[.. CurrentWritingBuffer.AvailableBytesToWrite()]);
            AllocateSpace(remainingCount).ValueRef.WriteBytes(data[remainingCount ..]);
        }
    }

    /// <inheritdoc />
    public bool ReadByte(out byte data)
    {
        data = 0;
        while (CurrentReadingBuffer.AvailableBytesToRead() <= 0)
        {
            if (_currentReadingNode.Next != null)
                _currentReadingNode = _currentReadingNode.Next;
            else return false;
        }

        return CurrentReadingBuffer.ReadByte(out data);
    }

    /// <inheritdoc />
    public byte[]? ReadBytes(int readingCount = -1)
    {
        var totalCount = 0;
        switch (readingCount)
        {
            case < 0:
            {
                // readingCount is negative, then calculate the count of remaining readable bytes. 
                for (var node = _currentReadingNode; node != null; node = node.Next)
                {
                    totalCount += node.ValueRef.AvailableBytesToRead();
                }

                break;
            }
            case 0:
                totalCount = 0;
                break;
            default:
            {
                // readingCount is positive, then verify the real size to read.
                // The real size is the smaller one between readingCount and count of remaining readable bytes.
                for (var node = _currentReadingNode; node != null; node = node.Next)
                {
                    var currentNodeAvailableToRead = node.ValueRef.AvailableBytesToRead();
                    if (totalCount + currentNodeAvailableToRead < readingCount)
                    {
                        totalCount += currentNodeAvailableToRead;
                        continue;
                    }

                    totalCount = readingCount;
                }

                break;
            }
        }

        if (totalCount == 0) return null;
        
        var buffer = new byte[totalCount];
        var offset = 0;
        /*
         * Do not iterate CurrentReadingNode directly, because exceptions may occurs here.
         * To keep reading position even when exceptions occur, 
         * iterate another reference, and then apply the change after all the reading finish.
         */
        var readingNode = _currentReadingNode;
        while (true)
        {
            var availableReadingLength = readingNode.ValueRef.AvailableBytesToRead();
            var remainingReadingLength = totalCount - offset;
            var realReadingLength = availableReadingLength < remainingReadingLength
                ? availableReadingLength
                : remainingReadingLength;
            
            readingNode.ValueRef.ReadBytes(buffer.AsSpan()[offset .. (offset + realReadingLength)]);
            
            if (remainingReadingLength < availableReadingLength)
                break;

            offset += realReadingLength;
            readingNode = readingNode.Next ?? throw new Exception(
                                       "Reaches the end of the buffer chain earlier than expected.");
        }

        _currentReadingNode = readingNode;
        return buffer;
    }
    
    /// <inheritdoc />
    public int ReadBytes(Span<byte> buffer)
    {
        var readingCount = buffer.Length;
        var totalCount = 0;
        switch (readingCount)
        {
            case < 0:
            {
                // readingCount is negative, then calculate the count of remaining readable bytes. 
                for (var node = _currentReadingNode; node != null; node = node.Next)
                {
                    totalCount += node.ValueRef.AvailableBytesToRead();
                }

                break;
            }
            case 0:
                totalCount = 0;
                break;
            default:
            {
                // readingCount is positive, then verify the real size to read.
                // The real size is the smaller one between readingCount and count of remaining readable bytes.
                for (var node = _currentReadingNode; node != null; node = node.Next)
                {
                    var currentNodeAvailableToRead = node.ValueRef.AvailableBytesToRead();
                    if (totalCount + currentNodeAvailableToRead < readingCount)
                    {
                        totalCount += currentNodeAvailableToRead;
                        continue;
                    }

                    totalCount = readingCount;
                }

                break;
            }
        }

        if (totalCount == 0) return 0;
        
        var offset = 0;
        /*
         * Do not iterate CurrentReadingNode directly, because exceptions may occurs here.
         * To keep reading position even when exceptions occur, 
         * iterate another reference, and then apply the change after all the reading finish.
         */
        var readingNode = _currentReadingNode;
        while (true)
        {
            var availableReadingLength = readingNode.ValueRef.AvailableBytesToRead();
            var remainingReadingLength = totalCount - offset;
            var realReadingLength = availableReadingLength < remainingReadingLength
                ? availableReadingLength
                : remainingReadingLength;
            
            readingNode.ValueRef.ReadBytes(buffer[offset .. (offset + realReadingLength)]);
            
            if (remainingReadingLength < availableReadingLength)
                break;

            offset += realReadingLength;
            readingNode = readingNode.Next ?? throw new Exception(
                                       "Reaches the end of the buffer chain earlier than expected.");
        }

        _currentReadingNode = readingNode;
        return totalCount;
    }

    /// <summary>
    /// Append a buffer space for writing.
    /// This operation will not change the current writing position.
    /// </summary>
    /// <param name="buffer">Buffer which can be considered as empty.</param>
    public LinkedListNode<ByteArray> AppendSpace(ByteArray buffer)
    {
        Capacity += buffer.Capacity;
        return _content.AddLast(buffer);
    }

    /// <summary>
    /// Append a ByteArray of content into this buffer.
    /// This operation will append the given buffer into the buffer node chain and take over it,
    /// thus any following manual operation on the given buffer is not recommended.
    /// This operation will move the writer position to the end of this buffer.
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public LinkedListNode<ByteArray> AppendContent(ByteArray buffer)
    {
        if (_content.Last is not null)
        {
            var tailArray = _content.Last.Value;
            Capacity -= tailArray.Shrink(tailArray.WriterPosition);
        }
        Capacity += buffer.Capacity;
        var node = _content.AddLast(buffer);
        _currentWritingNode = node;
        return node;
    }
    
    public byte this[int index]
    {
        get
        {
            SeekBuffer(index, out var buffer, out var offset);
            return buffer.ValueRef[offset];
        }
        set
        { 
            SeekBuffer(index, out var buffer, out var offset);
            buffer.ValueRef[offset] = value;
        }
    }

    public byte[] this[Range range]
    {
        get
        {
            var rangeStarting = range.Start.GetOffset(Capacity);
            var rangeEnding = range.End.GetOffset(Capacity);
            var rangeLength = rangeEnding - rangeStarting;
            var resultBuffer = new byte[rangeLength];
            var resultOffset = 0;
            SeekBuffer(range.Start.Value, out var iteratingNode, out var startingOffset);
            while (true)
            {
                var copyingLength = iteratingNode.ValueRef.Content.Length - startingOffset;
                if (copyingLength > rangeLength - resultOffset) 
                    copyingLength = rangeLength - resultOffset;
                iteratingNode.ValueRef.Content.AsSpan()[startingOffset .. (startingOffset + copyingLength)].
                    CopyTo(resultBuffer.AsSpan()[resultOffset ..]);
                resultOffset += copyingLength;
                if (resultOffset >= rangeLength || iteratingNode.Next == null)
                    break;
                iteratingNode = iteratingNode.Next;
                startingOffset = 0;
            }

            return resultBuffer;
        }
        set
        {
            var rangeStarting = range.Start.GetOffset(Capacity);
            var rangeEnding = range.End.GetOffset(Capacity);
            var rangeLength = rangeEnding - rangeStarting;
            var valueOffset = 0;
            SeekBuffer(range.Start.Value, out var iteratingNode, out var startingOffset);
            while (true)
            {
                var copyingLength = iteratingNode.ValueRef.Content.Length - startingOffset;
                if (copyingLength > rangeLength - valueOffset) 
                    copyingLength = rangeLength - valueOffset;
                value.AsSpan()[valueOffset .. (valueOffset + copyingLength)].
                    CopyTo(iteratingNode.ValueRef.Content.AsSpan()[startingOffset ..]);
                valueOffset += copyingLength;
                if (valueOffset >= rangeLength || iteratingNode.Next == null)
                    break;
                iteratingNode = iteratingNode.Next;
                startingOffset = 0;
            }
        }
    }
}