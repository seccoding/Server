using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ServerCore
{
    public class SendBufferHelper
    {
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => null);

        public static int ChunkSize { get; set; } = short.MaxValue * 100;

        public static ArraySegment<byte> Open(int reserveSize)
        {
            // 데이터가 없다면 새롭게 생성
            if (CurrentBuffer.Value == null)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            // 남아있는 공간보다 예약 공간이 크다면 새롭게 생성
            if (CurrentBuffer.Value.FreeSize < reserveSize)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);
            
            return CurrentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }
    }

    public class SendBuffer
    {
        byte[] _buffer;
        int _usedSize;

        public SendBuffer(int chunkSize)
        {
            _buffer = new byte[chunkSize];
        }

        public int FreeSize { get { return _buffer.Length - _usedSize; } }

        // 버퍼 예약
        public ArraySegment<byte> Open(int reserveSize)
        {
            if (FreeSize < reserveSize)
                return null;

            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }

        // 버퍼 확정
        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            _usedSize += usedSize;
            return segment;
        }
    }
}
