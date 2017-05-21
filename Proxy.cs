using FrontEnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SDE_GUI
{
    class Proxy
    {
        private AsynchronousServer m_Server;
        private AsynchronousClient m_Client;
        private AsynchronousServer.State m_State = null;
        private DebugConsole m_DebugConsole = null;
        private Thread m_ServerThread;
        private Thread m_ClientThread;
        private ManualResetEvent m_ConnectToDone = new ManualResetEvent(false);

        private string m_AddrFrom;
        private int m_PortFrom;
        private string m_AddrTo;
        private int m_PortTo;

        private volatile bool m_Stopped = false;

        public Proxy(string addrFrom, int portFrom, string addrTo, int portTo = -1)
        {
            m_AddrFrom = addrFrom;
            m_PortFrom = portFrom;
            m_AddrTo = addrTo;
            m_PortTo = portTo;

            m_Server = new AsynchronousServer();
            m_Client = new AsynchronousClient();

            m_ServerThread = new Thread(RunServer);
            m_ClientThread = new Thread(RunClient);

        }
        public void Start()
        {
            m_ServerThread.Start();
        }
        public void Stop()
        {
            m_Stopped = true;
            m_Client.Stop();
            m_Server.Stop();
            if(m_ClientThread.IsAlive)
            {
                m_ClientThread.Join();
            }
            if (m_ServerThread.IsAlive)
            {
                m_ServerThread.Join();
            }
        }
        public void SetDebugOutput(DebugConsole debugConsole)
        {
            m_DebugConsole = debugConsole;
        }
        private void RunServer()
        {
            m_Server.Start(m_AddrFrom, m_PortFrom, AcceptCallback);
        }
        private void RunClient()
        {
            m_Client.Start(m_AddrTo, m_PortTo, DataReceiveClientCallback, ConnectCallback);
        }
        private void ConnectCallback(bool success)
        {
            if(!success)
            {
                m_State.m_Socket.Close();
            }
            m_ConnectToDone.Set();
        }
        private AsynchronousServer.DataReceiveCallback AcceptCallback(AsynchronousServer.State state)
        {
            m_DebugConsole?.WriteLine("Connection accepted");
            if (m_State != null && !m_Stopped) {
                return null;
            }
            m_State = state;
            if(m_AddrTo.Length > 0) {
                m_AddrTo = m_AddrFrom;
            }
            if (m_PortTo <= 0)
            {
                m_PortTo = m_PortFrom;
            }
            m_ConnectToDone.Reset();
            m_ClientThread.Start();
            m_ConnectToDone.WaitOne();
            if (m_State.m_Socket.Connected)
            {
                return DataReceiveServerCallback;
            }
            m_State = null;
            return null;
        }
        private void DataReceiveServerCallback(AsynchronousServer.State state, int bytesRead)
        {
            if(!m_Stopped)
            {
                m_DebugConsole?.WriteLineAsString(state.m_Buffer, bytesRead, Brushes.Green);
                m_Client.Send(state.m_Buffer, bytesRead);
            }
        }
        private void DataReceiveClientCallback(byte[] result, int bytesRead)
        {
            if (!m_Stopped)
            {
                m_DebugConsole?.WriteLineAsString(result, bytesRead, Brushes.Blue);
                m_Server.Send(m_State, result, bytesRead);
            }
        }

    }
}
