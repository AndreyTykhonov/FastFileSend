using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FastFileSend.Main.RemoteFile
{
    class SegmentedStream : Stream
    {
        Stream Stream { get; set; }
        long StreamStart { get; set; }
        long StreamLength { get; set; }

        public SegmentedStream(Stream stream, long position, long size)
        {
            Stream = stream;
            StreamStart = position;
            StreamLength = size;
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => Math.Min((Stream.Length - StreamStart), StreamLength);

        public override long Position { get => Stream.Position - StreamStart; set => Stream.Position = StreamStart + value; }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return Stream.Seek(StreamStart + offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Stream.Write(buffer, offset, count);
        }
    }
}
