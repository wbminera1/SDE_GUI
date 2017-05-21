using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SDE_GUI
{
    public class AsynchronousServer
    {
        // State object for reading client data asynchronously  
        public class State
        {
            public Socket               m_Socket = null;
            public const int            cBufferSize = 1024;
            public byte[]               m_Buffer = new byte[cBufferSize];
            public DataReceiveCallback  m_ReceiveCallback = null;
            public ManualResetEvent    m_ReceiveDone = new ManualResetEvent(true);
        }

        public delegate void DataReceiveCallback(State state, int bytesRead);
        public delegate DataReceiveCallback AcceptCallbackDelegate(State state);

        private Socket m_Listener;
        private AcceptCallbackDelegate m_AcceptCallback;
        private ManualResetEvent m_AcceptDone = new ManualResetEvent(false);
        private volatile bool m_Stopped = false;

        public void Start(string hostName, int port, AcceptCallbackDelegate acceptCallback)
        {
            Console.WriteLine("AsynchronousServer.Started");
            IPHostEntry ipHostInfo = Dns.Resolve(hostName);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            m_AcceptCallback = acceptCallback;

            m_Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                m_Listener.Bind(localEndPoint);
                m_Listener.Listen(100);

                while (!m_Stopped)
                {
                    m_AcceptDone.Reset();
                    m_Listener.BeginAccept( new AsyncCallback(AcceptCallback), this);
                    m_AcceptDone.WaitOne();
                }
            }
            catch (Exception)
            {
            }
            Console.WriteLine("AsynchronousServer.Stopped");
        }
        public void Stop()
        {
            Console.WriteLine("AsynchronousServer.Stop");
            m_Stopped = true;
            m_Listener.Close();
            m_AcceptDone.Set();
        }
        private static void AcceptCallback(IAsyncResult ar)
        {
            Console.WriteLine("AsynchronousServer.AcceptCallback.Started");
            AsynchronousServer self = (AsynchronousServer)ar.AsyncState;
            if(!self.m_Stopped)
            {
                self.m_AcceptDone.Set();

                Socket handler = self.m_Listener.EndAccept(ar);

                State state = new State();
                state.m_Socket = handler;
                state.m_ReceiveCallback = self.m_AcceptCallback(state);
                if (state.m_ReceiveCallback == null)
                {
                    state.m_Socket.Close();
                    state.m_ReceiveDone.Set();
                }
                else
                {
                    handler.BeginReceive(state.m_Buffer, 0, State.cBufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
            }
            Console.WriteLine("AsynchronousServer.AcceptCallback.Stopped");
        }
        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                State state = (State)ar.AsyncState;

                int bytesRead = state.m_Socket.EndReceive(ar);
                if (bytesRead > 0)
                {
                    state.m_ReceiveCallback(state, bytesRead);
                    state.m_Socket.BeginReceive(state.m_Buffer, 0, State.cBufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    state.m_ReceiveDone.Set();
                }
            }
            catch (Exception)
            {
            }
        }
        public void Send(State state, byte[] dataBuffer, int dataSize)
        {
            state.m_Socket.BeginSend(dataBuffer, 0, dataSize, 0, new AsyncCallback(SendCallback), state);
        }
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                State state = (State)ar.AsyncState;
                state.m_Socket.EndSend(ar);
            }
            catch (Exception)
            {
            }
        }

    }
}
