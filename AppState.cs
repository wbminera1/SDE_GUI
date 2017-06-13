using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDE_GUI
{
    public class AppState
    {
        private Settings m_Settings = new Settings();
        public GDBManager GDB { get; set; }  = new GDBManager();
        public GUIManager GUI { get; set; } = new GUIManager();

        private static AppState m_Instance;
        public static AppState Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new AppState();
                }
                return m_Instance;
            }
        }

        public Settings GetSettings()
        {
            return m_Settings;
        }
        public bool LoadSettings()
        {
            return Settings.Load(out m_Settings);
        }
        public bool SaveSettings()
        {
            return m_Settings.Save();
        }
    }
}
