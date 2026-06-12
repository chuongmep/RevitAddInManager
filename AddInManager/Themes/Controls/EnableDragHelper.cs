using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using WpfScrollBar = System.Windows.Controls.Primitives.ScrollBar;

namespace RevitAddinManager.Themes.Controls;

public class EnableDragHelper
{
    public static readonly DependencyProperty EnableDragProperty = DependencyProperty.RegisterAttached(
        "EnableDrag",
        typeof(bool),
        typeof(EnableDragHelper),
        new PropertyMetadata(default(bool), OnLoaded));

    private static void OnLoaded(DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
        var uiElement = dependencyObject as UIElement;
        if (uiElement == null || (dependencyPropertyChangedEventArgs.NewValue is bool) == false)
        {
            return;
        }

        if ((bool) dependencyPropertyChangedEventArgs.NewValue)
        {
            uiElement.MouseMove += UIElementOnMouseMove;
        }
        else
        {
            uiElement.MouseMove -= UIElementOnMouseMove;
        }
    }

    private static void UIElementOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
    {
        var uiElement = sender as UIElement;
        if (uiElement != null)
        {
            if (mouseEventArgs.LeftButton == MouseButtonState.Pressed)
            {
                var source = mouseEventArgs.OriginalSource as DependencyObject;
                if (IsMouseCapturedByAnotherElement(uiElement) || (source != null && IsScrollBarInteraction(source)))
                {
                    return;
                }

                DependencyObject parent = uiElement;
                // Search up the visual tree to find the first parent window.
                while (parent != null && parent is not Window)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                if (parent is Window window)
                {
                    window.DragMove();
                }
            }
        }
    }

    private static bool IsMouseCapturedByAnotherElement(UIElement uiElement)
    {
        // When a child control (for example a ScrollBar thumb) captures mouse input,
        // avoid turning that gesture into a window drag.
        var captured = Mouse.Captured;
        return captured != null && !ReferenceEquals(captured, uiElement);
    }

    private static bool IsScrollBarInteraction(DependencyObject source)
    {
        while (source != null)
        {
            if (source is WpfScrollBar or Thumb or Track or RepeatButton)
            {
                return true;
            }

            source = VisualTreeHelper.GetParent(source);
        }

        return false;
    }

    public static void SetEnableDrag(DependencyObject element, bool value)
    {
        element.SetValue(EnableDragProperty, value);
    }

    public static bool GetEnableDrag(DependencyObject element)
    {
        return (bool) element.GetValue(EnableDragProperty);
    }
}