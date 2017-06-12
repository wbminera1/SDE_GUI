using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class AsynchronousClient
{
    public delegate void DataReceiveCallback(byte[] result, int bytesRead);
    public delegate void ConnectResultCallback(bool succes);

    private ManualResetEvent m_ConnectDone = new ManualResetEvent(false);
    private ManualResetEvent m_ReceiveDone = new ManualResetEvent(true);

    private DataReceiveCallback m_ReceiveCallback;
    private volatile bool m_Stopped = true;
    private byte[] m_Buffer;
    private Socket m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    public void Start(string hostName, int port, DataReceiveCallback receiveCallback, ConnectResultCallback connectCallback)
    {
        try
        {
            m_ReceiveCallback = receiveCallback;
            m_Buffer = new byte[1024];
            IPHostEntry ipHostInfo = Dns.Resolve(hostName);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            lock(m_Socket)
            {
                m_Socket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), m_Socket);
                m_Stopped = false;
            }
            m_ConnectDone.WaitOne();

            connectCallback(m_Socket.Connected);

            while (!m_Stopped && m_Socket.Connected)
            {
                if(m_ReceiveDone.WaitOne(100))
                {
                    m_ReceiveDone.Reset();
                    Receive();
                }
            }

            m_Socket.Shutdown(SocketShutdown.Both);
            m_Socket.Close();

        }
        catch (Exception)
        {
        }
    }

    public void Stop()
    {
        lock (m_Socket)
        {
            if(!m_Stopped)
            {
                m_Stopped = true;
                try
                {
                    m_Socket.Shutdown(SocketShutdown.Both);
                }
                catch(Exception)
                {

                }
            }
        }
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        lock (m_Socket)
        {
            if(!m_Stopped)
            {
                try
                {
                    Socket client = (Socket)ar.AsyncState;
                    client.EndConnect(ar);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                m_ConnectDone.Set();
            }
        }
    }

    private void Receive()
    {
        lock(m_Socket)
        {
            try
            {
                // Begin receiving the data from the remote device.
                m_Socket.BeginReceive(m_Buffer, 0, m_Buffer.GetLength(0), 0, new AsyncCallback(ReceiveCallback), this);
            }
            catch (Exception)
            {
            }
        }
    }

    static private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            AsynchronousClient state = (AsynchronousClient)ar.AsyncState;
            lock(state.m_Socket)
            {
                int bytesRead = state.m_Socket.EndReceive(ar);
                if (!state.m_Stopped && bytesRead > 0)
                {
                    state.m_ReceiveCallback(state.m_Buffer, bytesRead);
                    state.m_Socket.BeginReceive(state.m_Buffer, 0, state.m_Buffer.GetLength(0), 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    state.m_ReceiveDone.Set();
                }
            }
        }
        catch (Exception)
        {
        }
    }

    public void Send(string data)
    {
        byte[] byteData = Encoding.ASCII.GetBytes(data);
        Send(byteData, byteData.Length);
    }

    public void Send(byte[] byteData, int dataSize)
    {
        lock(m_Socket)
        {
            if(!m_Stopped)
            {
                m_Socket.BeginSend(byteData, 0, dataSize, 0, new AsyncCallback(SendCallback), m_Socket);
            }
        }
    }

    static private void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket client = (Socket)ar.AsyncState;
            client.EndSend(ar);
        }
        catch (Exception)
        {
        }
    }
}

