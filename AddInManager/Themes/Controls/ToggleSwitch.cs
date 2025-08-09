using System.Windows;
using System.Windows.Controls;
using CheckBox = System.Windows.Controls.CheckBox;

namespace RevitAddinManager.Themes.Controls
{
    public class ToggleSwitch : CheckBox
    {
        static ToggleSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleSwitch), new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
        }
    }
}
