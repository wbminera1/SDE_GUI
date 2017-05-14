using FrontEnd;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class AsynchronousClient
{
    public delegate void DataReceiveCallback(byte[] result, int bytesRead);

    private ManualResetEvent connectDone = new ManualResetEvent(false);
    private ManualResetEvent receiveDone = new ManualResetEvent(true);

    private DataReceiveCallback m_ReceiveCallback;
    private volatile bool m_Stopped;
    private byte[] m_Buffer;
    Socket m_Client;

    public void StartClient(int port, DataReceiveCallback receiveCallback)
    {
        try
        {
            m_ReceiveCallback = receiveCallback;
            m_Buffer = new byte[1024];
            IPHostEntry ipHostInfo = Dns.Resolve("127.0.0.1");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
            m_Stopped = false;

            m_Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            m_Client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), m_Client);
            connectDone.WaitOne();

            while(!m_Stopped && m_Client.Connected)
            {
                if(receiveDone.WaitOne(100))
                {
                    receiveDone.Reset();
                    Receive();
                }
            }

            m_Client.Shutdown(SocketShutdown.Both);
            m_Client.Close();

        }
        catch (Exception)
        {
        }
    }

    public void Stop()
    {
        m_Stopped = true;
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.
            Socket client = (Socket)ar.AsyncState;

            // Complete the connection.
            client.EndConnect(ar);


            // Signal that the connection has been made.
            connectDone.Set();
        }
        catch (Exception)
        {
        }
    }

    private void Receive()
    {
        try
        {
            // Begin receiving the data from the remote device.
            m_Client.BeginReceive(m_Buffer, 0, m_Buffer.GetLength(0), 0, new AsyncCallback(ReceiveCallback), this);
        }
        catch (Exception)
        {
        }
    }

    static private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            AsynchronousClient state = (AsynchronousClient)ar.AsyncState;
            Socket client = state.m_Client;

            int bytesRead = client.EndReceive(ar);
            if (bytesRead > 0)
            {
                state.m_ReceiveCallback(state.m_Buffer, bytesRead);
                state.m_Client.BeginReceive(state.m_Buffer, 0, state.m_Buffer.GetLength(0), 0, new AsyncCallback(ReceiveCallback), state);
            }
            else
            {
                state.receiveDone.Set();
            }
        }
        catch (Exception)
        {
        }
    }

    public void Send(string data)
    {
        byte[] byteData = Encoding.ASCII.GetBytes(data);
        Send(byteData);
    }

    public void Send(byte[] byteData)
    {
        m_Client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), m_Client);
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

