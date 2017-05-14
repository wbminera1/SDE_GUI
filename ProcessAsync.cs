using System;
using System.Text;
using System.Diagnostics;

namespace SDE_GUI
{
    class ProcessAsync
    {
        private StringBuilder m_Output = null;
        private Process m_Process;

        public void Run(string fileName, string arguments, DataReceivedEventHandler handler)
        {
            m_Process = new Process();
            m_Process.StartInfo.FileName = fileName;
            m_Process.StartInfo.Arguments = arguments;
            m_Process.StartInfo.UseShellExecute = false;
            m_Process.StartInfo.RedirectStandardOutput = true;
            m_Process.StartInfo.RedirectStandardError = true;
            m_Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            m_Process.StartInfo.CreateNoWindow = true;

            m_Output = new StringBuilder("");

            m_Process.OutputDataReceived += handler;
            m_Process.ErrorDataReceived += handler;

            m_Process.Start();

            m_Process.BeginOutputReadLine();
            m_Process.BeginErrorReadLine();

            while(!m_Process.WaitForExit(100))
            {

            }

            m_Process.Close();
        }

        public void Kill()
        {
            m_Process.Kill();
        }

    }
}
