using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{

    public abstract class Session
    {
        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        Socket _socket;
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();

        public void Start(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _recvArgs.SetBuffer(new byte[1024], 0, 1024);
            RegisterRecv();

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
        }

        #region Receive 네트워크 통신
        void RegisterRecv()
        {
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false)
                OnRecvCompleted(null, _recvArgs);
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            // args.BytesTransferred : 클라이언트로부터 몇 바이트를 받았냐?
            if ( args.BytesTransferred > 0 && args.SocketError == SocketError.Success )
            {
                try
                {
                    // 성공적으로 데이터를 받아왔다.
                    OnRecv(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));
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

        Queue<byte[]> _sendQueue = new Queue<byte[]>(); // 데이터를 한번에 보내기위한 큐
        object _lock = new object();

        public void Send(byte[] sendBuff)
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
            // Queue에 쌓여있는 모든 Buffer를 한번에 모아 보낸다.
            while (_sendQueue.Count > 0) 
            {
                byte[] buff = _sendQueue.Dequeue();
                _pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
            }

            // Queue에 쌓여있는 모든 Buffer를 한번에 모아 보낸다.
            _sendArgs.BufferList = _pendingList;

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
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
            
        }
        #endregion
    }
}
