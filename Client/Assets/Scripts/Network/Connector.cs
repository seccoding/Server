using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace ServerCore
{
    /*
     * 서버끼리 통신하기 위해 필요.
     *  -> Clustered Server 간 통신.
     */
    public class Connector
    {
        Func<Session> _sessionFactory;
        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory, int count = 1)
        {

            for (int i = 0; i < count; i++)
            {
                // 연결 Pipe 생성
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _sessionFactory = sessionFactory;

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnectedComplete);
                // 상대방의 주소?
                args.RemoteEndPoint = endPoint;
                args.UserToken = socket;

                Debug.Log($"연결 시도");

                RegisterConnect(args);
            }
        }

        void RegisterConnect(SocketAsyncEventArgs args)
        {
            Socket socket = (Socket) args.UserToken;
            if (socket == null)
                return;

            bool pending = socket.ConnectAsync(args);

            if (pending == false)
                OnConnectedComplete(null, args);
        }

        void OnConnectedComplete(object sender, SocketAsyncEventArgs args)
        {
            
            if (args.SocketError == SocketError.Success)
            {
                Debug.Log($"연결 성공");

                Session session = _sessionFactory.Invoke();
                session.Start(args.ConnectSocket);
                session.OnConnected(args.RemoteEndPoint);

            }
            else
            {
                Debug.Log($"OnConnectedComplete Failed : {args.SocketError}");
            }
        }
    }
}
