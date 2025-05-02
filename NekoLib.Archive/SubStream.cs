namespace NekoLib.Archive;

internal class SubStream : Stream {
    public Stream InnerStream;
    public long Offset { get; }
    public override long Length { get; }
    public sealed override long Position { get; set; }

    public SubStream(Stream stream, long offset, long length) {
#if !NETSTANDARD2_1
        ArgumentNullException.ThrowIfNull(stream);
#else
        if (stream is null) throw new ArgumentNullException(nameof(stream));
#endif
        
        if (!stream.CanSeek)
            throw new NotSupportedException();

        InnerStream = stream;

        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset is lesser than zero");

        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length is lesser than zero");

        Offset = offset;
        Length = length;
        Position = 0;
    }

    public override void Flush() {
        throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count) {
        if (!CanRead)
            throw new NotSupportedException("Stream does not support reading");

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count cant be less than zero");
        
        count = (int)Math.Min(count, Length - Position);
        
        InnerStream.Seek(Offset + Position, SeekOrigin.Begin);
#if NET7_0_OR_GREATER
        InnerStream.ReadExactly(buffer, offset, count);
#else
        var read = InnerStream.Read(buffer, offset, count);
        if (read < count) throw new EndOfStreamException();
#endif

        Position += count;

        return count;
    }

    public override long Seek(long offset, SeekOrigin origin) {
        switch (origin) {
            case SeekOrigin.Begin:
                if (offset < 0)
                    throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be less than zero when seeking from the beginning");
                if (offset > Length)
                    throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be greater than the length of the substream");
                InnerStream.Seek(Offset + (Position = offset), SeekOrigin.Begin);
                break;
            case SeekOrigin.End:
                if (offset > 0)
                    throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be greater than zero when seeking from the end");

                if (offset < -Length)
                    throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be less than the length of the substream");

                InnerStream.Seek(Position = (Length + offset), SeekOrigin.End);
                break;
            case SeekOrigin.Current:
                if (Position + offset < 0)
                    throw new NotSupportedException("Attempted to seek before the start of the substream");

                if (Position + offset > Length)
                    throw new NotSupportedException("Attempted to seek beyond the end of the substream");

                InnerStream.Seek(Position += offset, SeekOrigin.Current);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
        }
        return Position;
    }
    public override void SetLength(long value) {
        throw new NotSupportedException();
    }
    
    public override void Write(byte[] buffer, int offset, int count) {
        if (!CanWrite)
            throw new NotSupportedException("Stream does not support writing");

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count cant be less than zero");
        
        count = (int)Math.Min(count, Length - Position);
        
        InnerStream.Seek(Offset + Position, SeekOrigin.Begin);
        InnerStream.Write(buffer, offset, count);

        Position += count;
    }

    public override bool CanRead => InnerStream.CanRead;
    public override bool CanSeek => InnerStream.CanWrite;
    public override bool CanWrite => InnerStream.CanWrite;
    public override bool CanTimeout => InnerStream.CanTimeout;
}