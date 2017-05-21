using FrontEnd;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace SDE_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int m_Port = -1;
        private ProcessAsync m_ProcessAsync = new ProcessAsync();
        private AsynchronousClient m_Client = new AsynchronousClient();
        private Settings    m_Settings = new Settings();
        private GDBRemoteSerialProtocol m_GDBRSP = new GDBRemoteSerialProtocol();
        private DebugConsole m_DebugConsole;
        private Thread m_ProcessThread;
        private Proxy m_Proxy;

        public MainWindow()
        {
            InitializeComponent();

            m_DebugConsole = new DebugConsole(DebugConsoleText);
            m_DebugConsole.WriteLine("Started");

            if (!Settings.Load(out m_Settings))
            {
                GUI.Settings win2 = new GUI.Settings();
                win2.ShowDialog();
                m_Settings.Save();
            }

            Proxy.OnSwitched = OnProxySwitched;
            //m_ProcessThread = new Thread(RunProcess);
            //m_ProcessThread.Start();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            m_ProcessAsync.Kill();
        }
        private void RunProcess()
        {
            string fullPath = "";
            string fullArgs = "";
            if (m_Settings.Selection == 0)
            {
                fullPath = m_Settings.SDEPath;
                fullArgs = "-debug -- " + m_Settings.CMDPath + " " + m_Settings.Args;
            } else if (m_Settings.Selection == 1)
            {
                fullPath = m_Settings.GDBPath;
                fullArgs = "127.0.0.1:10000 " + m_Settings.CMDPath + " " + m_Settings.Args;
            }
            m_ProcessAsync.Run(fullPath, fullArgs, new DataReceivedEventHandler(ProcessOutputHandler));
        }
        private void ProcessOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                m_DebugConsole.WriteLine(outLine.Data);
                if(outLine.Data.Contains("on port")) {
                    int portIdx = outLine.Data.IndexOfAny("0123456789".ToCharArray());
                    if (portIdx >= 0)
                    {
                        string portStr = outLine.Data.Substring(portIdx);
                        m_Port = Int32.Parse(portStr);
                        m_Client.Start("127.0.0.1",m_Port, DataReceiveCallback, ConnectCallback);
                    }
                }
            }
        }
        private void ConnectCallback(bool success)
        {

        }
        private void DataReceiveCallback(byte[] result, int bytesRead)
        {
            m_DebugConsole.WriteLineAsString(result, bytesRead);
            byte[] response = m_GDBRSP.DataReceiveCallback(result, bytesRead);
            if(response != null)
            {
                m_DebugConsole.WriteLineAsString(response);
                m_Client.Send(response, bytesRead);
            }
        }
        private void MenuItem_Click_Open(object sender, RoutedEventArgs e)
        {

        }
        private void MenuItem_Click_Close(object sender, RoutedEventArgs e)
        {

        }
        private void MenuItem_Click_Save(object sender, RoutedEventArgs e)
        {

        }
        private void MenuItem_Click_Settings(object sender, RoutedEventArgs e)
        {
            GUI.Settings win2 = new GUI.Settings();
            win2.ShowDialog();
        }
        private void MenuItem_Click_Quit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void qSupported_Click(object sender, RoutedEventArgs e)
        {
            byte[] cmd = m_GDBRSP.qSupported();
            m_Client.Send(cmd, cmd.Length);
        }
        private void eRegisters_Click(object sender, RoutedEventArgs e)
        {
            byte[] cmd = m_GDBRSP.eRegisters();
            m_Client.Send(cmd, cmd.Length);
        }
        private void eRegister_Click(object sender, RoutedEventArgs e)
        {
            byte[] cmd = m_GDBRSP.eRegister("eax");
            m_Client.Send(cmd, cmd.Length);
        }
        private void eStepSingle_Click(object sender, RoutedEventArgs e)
        {
            byte[] cmd = m_GDBRSP.eStepSingle();
            m_Client.Send(cmd, cmd.Length);
        }
        private void eReason_Click(object sender, RoutedEventArgs e)
        {
            byte[] cmd = m_GDBRSP.eReason();
            m_Client.Send(cmd, cmd.Length);
        }
        private void eCmd_Click(object sender, RoutedEventArgs e)
        {
            string cmdTxt = CommandText.Text;
            byte[] cmd = GDBRemoteSerialProtocol.MakeCommand(cmdTxt);
            m_Client.Send(cmd, cmd.Length);
        }
        bool OnProxySwitched(bool newState)
        {
            if (newState && m_Proxy == null)
            {
                m_Proxy = new Proxy("127.0.0.1", Int32.Parse(ProxyFrom.Text), "127.0.0.1", Int32.Parse(ProxyTo.Text));
                m_Proxy.SetDebugOutput(m_DebugConsole);
                m_Proxy.Start();
                return true;
            }
            if (!newState && m_Proxy != null)
            {
                m_Proxy.Stop();
                //m_Proxy = null;
                return true;
            }
            return false;
        }
    }
}
