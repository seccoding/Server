using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    public class Listener
    {
        Socket _listenSocket;
        Func<Session> _sessionFactory;

        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory, int register = 10, int backlog = 100)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // Pipe Endpoint 설정
            _listenSocket.Bind(endPoint);
            // 최대 대기 수
            // 이 이상으로 접속시 다 튕겨낸다.
            _listenSocket.Listen(backlog);

            // Inject Session
            _sessionFactory = sessionFactory;

            // User가 많이 모여 Event 처리가 오래 걸리거나 StackOverFlow 가 발생한다면
            // 아래코드처럼 EventHandler를 여러개 만들어 두면 된다.
            for (int i = 0; i < register; i++)
            {
                // 한번 생성하면 재사용이 가능한 어마어마한 장점이 있다.
                // 모든 소켓 비동기 작업은 'SocketAsyncEventArgs'를 통해 이벤트 단위로 처리되기 때문에 코드 추적이 간단하다.
                // 1. Client 연결 정보가 들어있다.
                // 2. 필요할 때마다 메시지를 전달해준다.
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();

                // 접속 성공시 OnAcceptCompleted를 Callback으로 처리할 수 있도록 해준다.
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);

                // 최조 한번 Accept 시도를 하도록 한다.
                // 클라이언트에서 접속시 OnAcceptCompleted 가 실행된다.
                RegisterAccept(args);
            }
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            /*
             * 클라이언트를 초기화 시켜준다.
             * 이 구문이 없다면, Error 가 발생한다.
             * 이미 연결이 완료되었다가 종료된 클라이언트 정보가 들어있기 때문.
             * ----------------------------------------------------------------------------------
             * Unhandled exception. System.ObjectDisposedException: Safe handle has been closed.
             * Object name: 'SafeHandle'. ...
             * ----------------------------------------------------------------------------------
             */
            args.AcceptSocket = null;

            // Non-Blocking (비동기)
            // 연결 성공여부에 관계없이 Tast<Socket>을 우선 반환 --> 접속에 대한 처리를 ThreadPool 에서 처리한다.
            // Client에 대한 후처리 작업은 Callback으로 한다.
            // 연결 가능한지 아닌지 판단
            bool pending = _listenSocket.AcceptAsync(args);

            // 서버에 연결 대기중인 건이 없어 즉시 연결 가능한 상태
            // pending == true : 현재 서버에서 처리중인 건이 존재한다.
            if (pending == false)
                OnAcceptCompleted(null, args);
        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            // 소켓 통신에 성공했다면 처리
            if (args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
                Console.WriteLine(args.SocketError.ToString());

            // 접속에 성공하고 모든 통신이 성공했다면
            // 다시 한번 연결을 시도하도록 한다.
            // Init -> RegisterAccept -> OnAcceptCompleted -> RegisterAccept -> OnAcceptCompleted -> ... 계속 반복
            // StackOverFlow 발생하지 않나?
            // _listenSocket.Listen(10); 최대 대기수가 10개라 if (pending == false) 가 계속 발생할 수 없으므로 자연스럽게 Stack이 초기화 된다.
            RegisterAccept(args);
        }
    }
}
