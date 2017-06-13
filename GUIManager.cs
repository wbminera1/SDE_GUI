using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDE_GUI
{
    public class GUIManager
    {
        public delegate void InputCompletedCallbackDelegate(string input);

        private GUI.GDBWindow m_GDBWindow = new GUI.GDBWindow();
        private GUI.GDBRegistersWindow m_RegistersWindow = new GUI.GDBRegistersWindow();

        public void ShowGDBWindow(InputCompletedCallbackDelegate onGDBInputCompleted)
        {
            m_GDBWindow.InputCompletedCallback = onGDBInputCompleted;
            m_GDBWindow.Show();
        }

        public void CloseGDBWindow()
        {
            m_GDBWindow.Close();
        }

        public void ProcessGDBOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                m_GDBWindow?.AddTextToConsole(outLine.Data);
            }
        }

    }
}
