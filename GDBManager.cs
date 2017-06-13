using FrontEnd;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SDE_GUI
{
    public class GDBManager
    {
        private ProcessAsync m_SDEServerProcessAsync = new ProcessAsync();
        private ProcessAsync m_GDBServerProcessAsync = new ProcessAsync();
        private ProcessAsync m_GDBProcessAsync = new ProcessAsync();
        private Thread m_SDEServerThread;
        private Thread m_GDBServerThread;
        private Thread m_GDBThread;
        private Proxy m_Proxy;
        private DebugConsole m_DebugConsole;

        public int ProxyPortFrom { get; set; }
        public int ProxyPortTo { get; set; }
        public int GDBPort { get; set; }
        public int GDBServerPort { get; set; }

        public void SetDebugConsole(DebugConsole debugConsole)
        {
            m_DebugConsole = debugConsole;
        }
        public bool OnProxySwitched(bool newState)
        {
            if (newState && m_Proxy == null)
            {
                m_Proxy = new Proxy("127.0.0.1", ProxyPortFrom, "127.0.0.1", ProxyPortTo);
                m_Proxy.SetDebugOutput(m_DebugConsole);
                m_Proxy.Start();
                return true;
            }
            if (!newState && m_Proxy != null)
            {
                m_Proxy.Stop();
                m_Proxy = null;
                return true;
            }
            return false;
        }
        public bool OnSDEServerSwitched(bool newState)
        {
            if (newState && m_SDEServerThread == null)
            {
                m_SDEServerThread = new Thread(RunSDEServer);
                m_SDEServerThread.Start();
                return true;
            }
            else if (!newState && m_GDBServerThread != null)
            {
                m_SDEServerProcessAsync.Kill();
                m_SDEServerThread.Join();
                m_SDEServerThread = null;
                return true;
            }
            return false;
        }
        public bool OnGDBServerSwitched(bool newState)
        {
            if (newState && m_GDBServerThread == null)
            {
                m_GDBServerThread = new Thread(RunGDBServer);
                m_GDBServerThread.Start();
                return true;
            }
            else if (!newState && m_GDBServerThread != null)
            {
                m_GDBServerProcessAsync.Kill();
                m_GDBServerThread.Join();
                m_GDBServerThread = null;
                return true;
            }
            return false;
        }
        public bool OnGDBSwitched(bool newState)
        {
            if (newState && m_GDBThread == null)
            {
                m_GDBThread = new Thread(RunGDB);
                m_GDBThread.Start();

                AppState.Instance.GUI.ShowGDBWindow(OnGDBInputCompleted);
                return true;
            }
            else if (!newState && m_GDBThread != null)
            {
                m_GDBProcessAsync.Kill();
                m_GDBThread.Join();
                m_GDBThread = null;
                AppState.Instance.GUI.CloseGDBWindow();
                return true;
            }
            return false;
        }
        private void RunSDEServer()
        {
            Settings settings = AppState.Instance.GetSettings();
            string fullPath = settings.SDEPath;
            string fullArgs = "-debug -- " + " " + settings.CMDPath + " " + settings.Args;
            m_GDBServerProcessAsync.Run(fullPath, fullArgs, new DataReceivedEventHandler(ProcessOutputHandler));
        }
        private void RunGDBServer()
        {
            Settings settings = AppState.Instance.GetSettings();
            string fullPath = settings.GDBServerPath;
            string fullArgs = "127.0.0.1:" + "12000" + " " + settings.CMDPath + " " + settings.Args;
            m_GDBServerProcessAsync.Run(fullPath, fullArgs, new DataReceivedEventHandler(ProcessOutputHandler));
        }
        private void RunGDB()
        {
            string fullPath = AppState.Instance.GetSettings().GDBPath;
            m_GDBProcessAsync.InputLine("target remote 127.0.0.1:" + GDBPort.ToString());
            m_GDBProcessAsync.Run(fullPath, "", new DataReceivedEventHandler(AppState.Instance.GUI.ProcessGDBOutputHandler));
        }
        private void OnGDBInputCompleted(string input)
        {
            m_GDBProcessAsync.InputLine(input);
        }

        private void ProcessOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                m_DebugConsole?.WriteLine(outLine.Data);
                if (outLine.Data.Contains("on port"))
                {
                    int portIdx = outLine.Data.IndexOfAny("0123456789".ToCharArray());
                    if (portIdx >= 0)
                    {
                        string portStr = outLine.Data.Substring(portIdx);
                        ProxyPortTo = Int32.Parse(portStr);
                    }
                }
            }
        }

    }
}
