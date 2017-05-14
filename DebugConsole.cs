using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FrontEnd
{
    public class DebugConsole
    {
        private TextBox m_ConsoleWindow;
        private int m_ConsoleStrCount = 0;

        public DebugConsole(TextBox consoleWindow)
        {
            m_ConsoleWindow = consoleWindow;
        }

        public void HexDumpToConsole(object obj)
        {
            byte[] bytes = ObjectToByteArray(obj);
            WriteLine(BitConverter.ToString(bytes));
        }

        public void HexDumpToConsole(int val)
        {
            WriteLine(BitConverter.ToString(BitConverter.GetBytes(val)));
        }

        public void WriteLine(byte[] data)
        {
            WriteLine(BitConverter.ToString(data));
        }

        public void WriteLine(string str)
        {
            Console.WriteLine(str);

            m_ConsoleWindow.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            new Action(delegate ()
            {
                m_ConsoleWindow.AppendText(m_ConsoleStrCount + " " + str + "\n");
                m_ConsoleWindow.ScrollToEnd();
                ++m_ConsoleStrCount;
            }));
        }

        public void WriteLineAsString(byte[] data, int dataMax = int.MaxValue)
        {
            string str = "";
            for(int idx = 0; idx < data.Length && idx < dataMax; ++idx) {
                byte b = data[idx];
                if (b == 0) {
                    break;
                }
                str += (char)b;
            }
            WriteLine(str);
        }

        // Convert an object to a byte array
        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                try
                {
                    bf.Serialize(ms, obj);
                }
                catch (SerializationException)
                {

                }
                return ms.ToArray();
            }
        }

    }
}
