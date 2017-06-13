using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SDE_GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        App() 
        {
        }

        protected override void OnStartup( StartupEventArgs e )
        {
            base.OnStartup(e);

            if (!AppState.Instance.LoadSettings())
            {
                GUI.Settings win2 = new GUI.Settings();
                win2.ShowDialog();
                AppState.Instance.SaveSettings();
            }

        }
    }
}
