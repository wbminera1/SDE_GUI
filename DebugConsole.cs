using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace FrontEnd
{
    public class DebugConsole
    {
        private RichTextBox m_ConsoleWindow;
        private int m_ConsoleStrCount = 0;

        public DebugConsole(RichTextBox consoleWindow)
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
        public void WriteLine(byte[] data, int length)
        {
            WriteLine(BitConverter.ToString(data, 0, length));
        }

        public void WriteLine(string str, Brush brush = null)
        {
            Console.WriteLine(str);

            m_ConsoleWindow.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
            new Action(delegate ()
            {
                //Brush oldBrush = m_ConsoleWindow.Foreground;
                if (brush != null)
                {
                    if(brush != m_ConsoleWindow.Foreground)
                    {
                        if(m_ConsoleWindow.Document.Blocks.LastBlock != null)
                        {
                        }
                        m_ConsoleWindow.Document.Blocks.Add(new Paragraph());
                        m_ConsoleWindow.Document.Blocks.LastBlock.Foreground = brush;
                    }
                }
                m_ConsoleWindow.Document.Blocks.LastBlock.ContentEnd.Paragraph.Inlines.Add(m_ConsoleStrCount + " " + str);
//                m_ConsoleWindow.AppendText(m_ConsoleStrCount + " " + str + "\n");
                m_ConsoleWindow.ScrollToEnd();
                ++m_ConsoleStrCount;
                if (brush != null)
                {
                    //m_ConsoleWindow.Foreground = oldBrush;
                }
            }));
        }

        public void WriteLineAsString(byte[] data, int dataMax = int.MaxValue, Brush brush = null)
        {
            string str = "";
            for(int idx = 0; idx < data.Length && idx < dataMax; ++idx) {
                byte b = data[idx];
                if (b == 0) {
                    break;
                }
                str += (char)b;
            }
            WriteLine(str, brush);
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
