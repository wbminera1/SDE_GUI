using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace SDE_GUI.GUI
{
    public class ColorButton : Button
    {
        public delegate bool OnSwitchedDelegate(bool newState);

        private bool m_On = false;
        public Brush m_ColorOn = Brushes.Red;
        public Brush m_ColorOff = Brushes.Green;
        public OnSwitchedDelegate OnSwitched { get; set; }

        protected override void OnClick()
        {
            base.OnClick();
            Switch();
            SetColor();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            SetColor();
        }

        protected void SetColor()
        {
            Background = m_On ? m_ColorOn : m_ColorOff;
        }

        protected void Switch()
        {
            if(OnSwitched != null && OnSwitched(!m_On))
            {
                m_On = !m_On;
            }
        }

    }
}
