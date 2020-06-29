using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    // TCP의 특성상 한번의 통신으로 모든 데이터가 도착하지 않는 경우가 발생한다.
    // 여러번의 TCP 통신으로 하나의 전문이 도달할 수 있도록 해주는 처리가 필요하다.
    public class RecvBuffer
    {
        // Case 1
        // [rw][][][][w][][][][][] => 8byte가 와야하는데 5바이트만 왔을 경우
        // [r][][][][][w][][][][] => 5byte만 보냄. 나중에는 w부터 읽어야 함.
        // [r][][][][][][][][w][] => 3byte를 추가로 더 받는다.
        // 이 패킷이 아래처럼 변해야 한다.
        // [][][][][][][][][rw][] => 패킷 수신 완료
        // [rw][][][][][][][][][] => 다음 패킷을 받을 준비를 한다.
        ArraySegment<byte> _buffer;
        int _readPos; // 읽고 있는 Cursor
        int _writePos; // 쓰고 있는 Cursor (작성하는 커서)

        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        // 현재까지 받은 데이터의 크기
        // [0][1][2][3][4][5][ ][ ][ ][ ]
        // [r][ ][ ][ ][ ][w][ ][ ][ ][ ]
        // DataSize : 5 - 0 = 5;
        public int DataSize { get { return _writePos - _readPos; } }

        // 버퍼에 남은 공간의 크기
        // [0][1][2][3][4][5][6][7][8][9]
        // [r][ ][ ][ ][ ][w][ ][ ][ ][ ]
        // DataSize : (9 + 1) - 5 = 5;
        public int FreeSize { get { return _buffer.Count - _writePos; } }

        // 데이터의 휴효범위가 어디부터 어디까지냐
        // [r][ ][ ][ ][ ][w][ ][ ][ ][ ]
        // [●][●][●][●][●][ ][ ][ ][ ][ ]
        public ArraySegment<byte> ReadSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
        }

        // 다음에 받아야할 유효 범위가 어디부터 어디까지냐
        // [r][ ][ ][ ][ ][w][ ][ ][ ][ ]
        // [ ][ ][ ][ ][ ][●][●][●][●][●]
        public ArraySegment<byte> WriteSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }

        // 데이터를 처음부터 받아오기 위해 Position 조정
        // [ ][ ][r][ ][ ][w][ ][ ][ ][ ]
        // [r][ ][ ][w][ ][ ][ ][ ][ ][ ] 로 조정
        // 혹은
        // [ ][ ][ ][rw][ ][ ][ ][ ][ ][ ]
        // [rw][ ][ ][ ][ ][ ][ ][ ][ ][ ] 로 조정
        public void Clean()
        {
            int dataSize = DataSize;
            if (dataSize == 0) // [ ][ ][ ][rw][ ][ ][ ][ ][ ][ ] 데이터 처리를 완료한 상태임.
            {
                // 남은 데이터가 없으면 복사하지 않고 커서 위치만 리셋
                _readPos = _writePos = 0;
            }
            else
            {
                // 남은 데이터가 있으면 시작 위치로 복사
                // [ ][ ][r][ ][ ][w][ ][ ][ ][ ]
                // [r][ ][ ][w][ ][ ][ ][ ][ ][ ] 로 조정
                // _buffer의 ReadPosition까지의 데이터를 0번 인덱스로 복사한다.
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }

        // 데이터 읽기를 성공적으로 처리 했을 경우 readPos 커서 위치를 옮겨줌.
        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > DataSize)
                return false;

            _readPos += numOfBytes;
            return true;
        }

        // Server가 Receive를 했을때 writePos 커서 위치를 옮겨줌.
        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize)
                return false;

            _writePos += numOfBytes;
            return true;
        }
    }
}
