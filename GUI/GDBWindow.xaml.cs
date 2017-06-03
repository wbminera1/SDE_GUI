using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SDE_GUI.GUI
{
    /// <summary>
    /// Interaction logic for GDBWindow.xaml
    /// </summary>
    public partial class GDBWindow : Window
    {
        public delegate void InputCompletedCallbackDelegate(string input);

        private Paragraph m_InputBlock = new Paragraph();
        public InputCompletedCallbackDelegate InputCompletedCallback { get; set; }

        public GDBWindow()
        {
            InitializeComponent();
            m_InputBlock.Inlines.Add(">");
            RichTextBox_Console.Document.Blocks.Clear();
            AddInputBlock();
        }

        private void RemoveInputBlock()
        {
            RichTextBox_Console.Document.Blocks.Remove(m_InputBlock);
        }

        private void AddInputBlock()
        {
            RichTextBox_Console.Document.Blocks.Add(m_InputBlock);
        }

        public void AddTextToConsole(string str)
        {
            App.Current.Dispatcher.Invoke(delegate
            {
                RemoveInputBlock();
                RichTextBox_Console.Document.Blocks.Add(new Paragraph());
                RichTextBox_Console.AppendText(str);
                AddInputBlock();
                RichTextBox_Console.CaretPosition = RichTextBox_Console.CaretPosition.DocumentEnd;
            });
        }

        private void RichTextBox_Console_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void RichTextBox_Console_TextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void RichTextBox_Console_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if(InputCompletedCallback != null)
                {
                    foreach(Inline inline in m_InputBlock.Inlines)
                    {
                        TextPointer text = inline.ContentStart;
                        string str = text.GetTextInRun(LogicalDirection.Forward);
                        string line = str.Substring(1);
                        AddTextToConsole(line);
                        InputCompletedCallback(line);
                        break;
                    }
                }
                m_InputBlock.Inlines.Clear();
                m_InputBlock.Inlines.Add(">");
                RichTextBox_Console.CaretPosition = RichTextBox_Console.CaretPosition.DocumentEnd;
                e.Handled = true;
            }
        }
    }
}
