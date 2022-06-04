using System.Windows;
using System.Windows.Controls;

namespace RevitAddinManager.Controls
{
    public class ToggleSwitch : CheckBox
    {
        static ToggleSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleSwitch), new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
        }
    }
}
