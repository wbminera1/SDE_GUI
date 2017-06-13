using FrontEnd;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace SDE_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AsynchronousClient m_Client = new AsynchronousClient();
        private GDBRemoteSerialProtocol m_GDBRSP = new GDBRemoteSerialProtocol();
        private DebugConsole m_DebugConsole;

        public MainWindow()
        {
            InitializeComponent();

            m_DebugConsole = new DebugConsole(DebugConsoleText);
            m_DebugConsole.WriteLine("Started");

            AppState.Instance.GDB.SetDebugConsole(m_DebugConsole);
            Proxy.OnSwitched = AppState.Instance.GDB.OnProxySwitched;
            GDBServer.OnSwitched = AppState.Instance.GDB.OnGDBServerSwitched;
            SDEServer.OnSwitched = AppState.Instance.GDB.OnSDEServerSwitched;
            GDB.OnSwitched = AppState.Instance.GDB.OnGDBSwitched;

        }
        protected override void OnClosing(CancelEventArgs e)
        {
            //m_ProcessAsync.Kill();
        }
        private void RunProcess()
        {
            string fullPath = "";
            string fullArgs = "";
            Settings settings = AppState.Instance.GetSettings();
            if (settings.Selection == 0)
            {
                fullPath = settings.SDEPath;
                fullArgs = "-debug -- " + settings.CMDPath + " " + settings.Args;
            }
            else if (settings.Selection == 1)
            {
                fullPath = settings.GDBServerPath;
                fullArgs = "127.0.0.1:10000 " + settings.CMDPath + " " + settings.Args;
            }
            //m_ProcessAsync.Run(fullPath, fullArgs, new DataReceivedEventHandler(ProcessOutputHandler));
        }
        private void ConnectCallback(bool success)
        {

        }
        private void DataReceiveCallback(byte[] result, int bytesRead)
        {
            m_DebugConsole.WriteLineAsString(result, bytesRead);
            byte[] response = m_GDBRSP.DataReceiveCallback(result, bytesRead);
            if (response != null)
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

        private void SDEServerPort_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void GDBServerPort_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            AppState.Instance.GDB.GDBServerPort = Int32.Parse(GDBServerPort.Text);
        }

        private void ProxyFrom_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            AppState.Instance.GDB.ProxyPortFrom = Int32.Parse(ProxyFrom.Text);
        }

        private void ProxyTo_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            AppState.Instance.GDB.ProxyPortTo = Int32.Parse(ProxyTo.Text);
        }

        private void GDBPort_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            AppState.Instance.GDB.GDBPort = Int32.Parse(GDBPort.Text);
        }
    }
} // namespace SDE_GUI
