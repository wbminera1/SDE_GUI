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
    /// Interaction logic for Setup.xaml
    /// </summary>
    public partial class Settings : Window
    {
        SDE_GUI.Settings m_Current = new SDE_GUI.Settings();
        public Settings()
        {
            InitializeComponent();
            if(SDE_GUI.Settings.Load(out m_Current))
            {
                textSDE.Text = m_Current.SDEPath;
                textGDBServer.Text = m_Current.GDBServerPath;
                textGDB.Text = m_Current.GDBPath;
                textCMD.Text = m_Current.CMDPath;
                textARG.Text = m_Current.Args;
                if(m_Current.Selection == 0)
                {
                    radioSDE.IsChecked = true;
                } else
                {
                    radioGDB.IsChecked = true;
                }
            }
        }

        static public string GetExePath()
        {
            string path = "";
            System.Windows.Forms.DialogResult result;
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.DefaultExt = ".exe";
            dlg.Filter = "FileType|*.exe";
            dlg.Multiselect = false;
            result = dlg.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                path = dlg.FileName;
            }

            return path;
        }


        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            m_Current.SDEPath = textSDE.Text;
            m_Current.GDBServerPath = textGDBServer.Text;
            m_Current.GDBPath = textGDB.Text;
            m_Current.CMDPath = textCMD.Text;
            m_Current.Args = textARG.Text;
            if(radioSDE.IsChecked == true)
            {
                m_Current.Selection = 0;
            }
            m_Current.Save();
            Close();
        }
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void buttonSDE_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            string path = GetExePath();
            if(path.Length > 0)
            {
                m_Current.SDEPath = path;
                textSDE.Text = path;
            }
            Show();
        }
        private void buttonGDB_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            string path = GetExePath();
            if (path.Length > 0)
            {
                m_Current.GDBPath = path;
                textGDB.Text = path;
            }
            Show();
        }

        private void buttonGDBServer_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            string path = GetExePath();
            if (path.Length > 0)
            {
                m_Current.GDBServerPath = path;
                textGDBServer.Text = path;
            }
            Show();
        }

        private void buttonCMD_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            string cmd = GetExePath();
            if (cmd.Length > 0)
            {
                m_Current.CMDPath = cmd;
                textCMD.Text = cmd;
            }
            Show();
        }
        private void radioSDE_Checked(object sender, RoutedEventArgs e)
        {
            m_Current.Selection = 0;
        }
        private void radioGDB_Checked(object sender, RoutedEventArgs e)
        {
            m_Current.Selection = 1;
        }
    }
}
