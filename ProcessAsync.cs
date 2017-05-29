using System;
using System.Text;
using System.Diagnostics;
using System.Collections;

namespace SDE_GUI
{
    class ProcessAsync
    {
        private Process m_Process;
        private Queue m_InputQueue = new Queue();

        public void Run(string fileName, string arguments, DataReceivedEventHandler handler)
        {
            m_Process = new Process();
            m_Process.StartInfo.FileName = fileName;
            m_Process.StartInfo.Arguments = arguments;
            m_Process.StartInfo.UseShellExecute = false;
            m_Process.StartInfo.RedirectStandardOutput = true;
            m_Process.StartInfo.RedirectStandardError = true;
            m_Process.StartInfo.RedirectStandardInput = true;
            m_Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //m_Process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            m_Process.StartInfo.CreateNoWindow = true;
            //m_Process.StartInfo.CreateNoWindow = false;

            m_Process.OutputDataReceived += handler;
            m_Process.ErrorDataReceived += handler;

            m_Process.Start();

            m_Process.BeginOutputReadLine();
            m_Process.BeginErrorReadLine();

            while(!m_Process.WaitForExit(100))
            {
                lock(m_InputQueue)
                {
                    if(m_InputQueue.Count > 0)
                    {
                        string str = (string)m_InputQueue.Dequeue();
                        m_Process.StandardInput.WriteLine(str);
                    }
                }
            }

            m_Process.Close();
        }

        public void InputLine(string str)
        {
            lock (m_InputQueue)
            {
                m_InputQueue.Enqueue(str);
            }
        }

        public void Kill()
        {
            m_Process.Kill();
        }

    }
}
