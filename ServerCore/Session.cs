using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{

    public abstract class PacketSession : Session
    {

        public static readonly int HeaderSize = 2;

        //[size(2)][packetId(2)][...][size(2)][packetId(2)][...]...
        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            // 전체 패킷 크기
            int processLen = 0;
            int packetCount = 0;

            while (true)
            {
                // 최소 헤더는 파싱 할 수 있는지 확인
                if (buffer.Count < HeaderSize)
                    break;

                // 모든 패킷이 완전하게 도착했는지 확인
                // ToUInt16 : ushort 만큼 읽어 온다.
                // ToInt16  : short 만큼 읽어 온다.
                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);

                // 버퍼의 크기와 데이터 사이즈가 다르면 Break
                if (buffer.Count < dataSize)
                    break;
                
                // 지금부터 패킷 조립 가능
                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));
                packetCount++;

                // 다음 패킷을 분리한다.
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);

                processLen += dataSize;
            }

            if (packetCount > 1)
                Console.WriteLine($"패킷 모아보내기 : {packetCount}");
            return processLen;
        }

        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    }

    public abstract class Session
    {
        public int SessionId { get; set; }

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        RecvBuffer _recvBuffer = new RecvBuffer(short.MaxValue);

        Socket _socket;
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();

        void Clear()
        {
            lock (_lock)
            {
                _sendQueue.Clear();
                _pendingList.Clear();
            }
        }

        public void Start(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
            
            RegisterRecv();
        }

        #region Receive 네트워크 통신
        void RegisterRecv()
        {
            if (_disconnect == 1)
                return;

            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try 
            {
                bool pending = _socket.ReceiveAsync(_recvArgs);
                if (pending == false)
                    OnRecvCompleted(null, _recvArgs);
            }
            catch(Exception e)
            {
                Console.WriteLine($"RegisterRecv Failed. {e}");
            }
            
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            // args.BytesTransferred : 클라이언트로부터 몇 바이트를 받았냐?
            if ( args.BytesTransferred > 0 && args.SocketError == SocketError.Success )
            {
                try
                {
                    // Write 커서를 이동한다.
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    // 컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다.
                    // 데이터 범위 만큼을 넘겨준다.
                    int processLen = OnRecv(_recvBuffer.ReadSegment);
                    // 처리를 못했거나 버퍼의 크기보다 크게 처리했을 경우
                    if (processLen < 0 || processLen > _recvBuffer.DataSize)
                    {
                        Disconnect();
                        return;
                    }

                    // Read 커서를 이동한다.
                    if (_recvBuffer.OnRead(processLen) == false)
                    {
                        Disconnect();
                        return;
                    }

                    RegisterRecv();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e.ToString()}");
                }
            }
            else
                Disconnect();
        }
        #endregion

        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>(); // 데이터를 한번에 보내기위한 큐
        object _lock = new object();

        public void Send(List<ArraySegment<byte>> sendBuffList)
        {
            if (sendBuffList.Count == 0)
                return;

            lock (_lock)
            {
                foreach (ArraySegment<byte> sendBuff in sendBuffList)
                    _sendQueue.Enqueue(sendBuff);

                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        public void Send(ArraySegment<byte> sendBuff)
        {
            lock (_lock) 
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        #region Send 네트워크 통신
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        void RegisterSend()
        {
            if (_disconnect == 1)
                return;

            // Queue에 쌓여있는 모든 Buffer를 한번에 모아 보낸다.
            while (_sendQueue.Count > 0) 
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                _pendingList.Add(buff);
            }

            // Queue에 쌓여있는 모든 Buffer를 한번에 모아 보낸다.
            _sendArgs.BufferList = _pendingList;

            try
            {
                bool pending = _socket.SendAsync(_sendArgs);
                if (pending == false)
                    OnSendCompleted(null, _sendArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterSend Failed. {e}"); ;
            }
        }

        /**
         * 클라이언트에게 데이터를 모두 보낸 후 실행됨.
         */
        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                // args.BytesTransferred : Client 에게 byte를 얼마나 보냈나?
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        // 모두 전송했으므로 BufferList와 PendingList를 모두 비워준다.
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                        // 큐에 남아있는 SendData를 소진시킨다.
                        if (_sendQueue.Count > 0)
                            RegisterSend();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e.ToString()}");
                    }
                }
                else
                    Disconnect();
            }
        }
        #endregion

        #region Disconnect
        int _disconnect = 0;

        public void Disconnect()
        {
            // 이미 Disconnect되었다면 return
            if (Interlocked.Exchange(ref _disconnect, 1) == 1)
                return;

            OnDisconnected(_socket.RemoteEndPoint);

            // 최초로 Disconnect가 되었다.
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();

            Clear();
        }
        #endregion
    }
}
