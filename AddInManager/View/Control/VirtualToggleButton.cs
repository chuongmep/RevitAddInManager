using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace RevitAddinManager.View.Control;

public static class VirtualToggleButton
{
    #region attached properties

    #region IsChecked

    /// <summary>
    /// IsChecked Attached Dependency Property
    /// </summary>
    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.RegisterAttached("IsChecked", typeof(Nullable<bool>), typeof(VirtualToggleButton),
            new FrameworkPropertyMetadata((Nullable<bool>)false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                OnIsCheckedChanged));

    /// <summary>
    /// Gets the IsChecked property.  This dependency property
    /// indicates whether the toggle button is checked.
    /// </summary>
    public static Nullable<bool> GetIsChecked(DependencyObject d)
    {
        return (Nullable<bool>)d.GetValue(IsCheckedProperty);
    }

    /// <summary>
    /// Sets the IsChecked property.  This dependency property
    /// indicates whether the toggle button is checked.
    /// </summary>
    public static void SetIsChecked(DependencyObject d, Nullable<bool> value)
    {
        d.SetValue(IsCheckedProperty, value);
    }

    /// <summary>
    /// Handles changes to the IsChecked property.
    /// </summary>
    private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement pseudobutton)
        {
            var newValue = (Nullable<bool>)e.NewValue;
            if (newValue == true)
            {
                RaiseCheckedEvent(pseudobutton);
            }
            else if (newValue == false)
            {
                RaiseUncheckedEvent(pseudobutton);
            }
            else
            {
                RaiseIndeterminateEvent(pseudobutton);
            }
        }
    }

    #endregion IsChecked

    #region IsThreeState

    /// <summary>
    /// IsThreeState Attached Dependency Property
    /// </summary>
    public static readonly DependencyProperty IsThreeStateProperty =
        DependencyProperty.RegisterAttached("IsThreeState", typeof(bool), typeof(VirtualToggleButton),
            new FrameworkPropertyMetadata(false));

    /// <summary>
    /// Gets the IsThreeState property.  This dependency property
    /// indicates whether the control supports two or three states.
    /// IsChecked can be set to null as a third state when IsThreeState is true.
    /// </summary>
    public static bool GetIsThreeState(DependencyObject d)
    {
        return (bool)d.GetValue(IsThreeStateProperty);
    }

    /// <summary>
    /// Sets the IsThreeState property.  This dependency property
    /// indicates whether the control supports two or three states.
    /// IsChecked can be set to null as a third state when IsThreeState is true.
    /// </summary>
    public static void SetIsThreeState(DependencyObject d, bool value)
    {
        d.SetValue(IsThreeStateProperty, value);
    }

    #endregion IsThreeState

    #region IsVirtualToggleButton

    /// <summary>
    /// IsVirtualToggleButton Attached Dependency Property
    /// </summary>
    public static readonly DependencyProperty IsVirtualToggleButtonProperty =
        DependencyProperty.RegisterAttached("IsVirtualToggleButton", typeof(bool), typeof(VirtualToggleButton),
            new FrameworkPropertyMetadata(false,
                OnIsVirtualToggleButtonChanged));

    /// <summary>
    /// Gets the IsVirtualToggleButton property.  This dependency property
    /// indicates whether the object to which the property is attached is treated as a VirtualToggleButton.
    /// If true, the object will respond to keyboard and mouse input the same way a ToggleButton would.
    /// </summary>
    public static bool GetIsVirtualToggleButton(DependencyObject d)
    {
        return (bool)d.GetValue(IsVirtualToggleButtonProperty);
    }

    /// <summary>
    /// Sets the IsVirtualToggleButton property.  This dependency property
    /// indicates whether the object to which the property is attached is treated as a VirtualToggleButton.
    /// If true, the object will respond to keyboard and mouse input the same way a ToggleButton would.
    /// </summary>
    public static void SetIsVirtualToggleButton(DependencyObject d, bool value)
    {
        d.SetValue(IsVirtualToggleButtonProperty, value);
    }

    /// <summary>
    /// Handles changes to the IsVirtualToggleButton property.
    /// </summary>
    private static void OnIsVirtualToggleButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IInputElement element)
        {
            if ((bool)e.NewValue)
            {
                element.MouseLeftButtonDown += OnMouseLeftButtonDown;
                element.KeyDown += OnKeyDown;
            }
            else
            {
                element.MouseLeftButtonDown -= OnMouseLeftButtonDown;
                element.KeyDown -= OnKeyDown;
            }
        }
    }

    #endregion IsVirtualToggleButton

    #endregion attached properties

    #region routed events

    #region Checked

    /// <summary>
    /// A static helper method to raise the Checked event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaiseCheckedEvent(UIElement target)
    {
        if (target == null) return null;

        var args = new RoutedEventArgs();
        args.RoutedEvent = ToggleButton.CheckedEvent;
        RaiseEvent(target, args);
        return args;
    }

    #endregion Checked

    #region Unchecked

    /// <summary>
    /// A static helper method to raise the Unchecked event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaiseUncheckedEvent(UIElement target)
    {
        if (target == null) return null;

        var args = new RoutedEventArgs();
        args.RoutedEvent = ToggleButton.UncheckedEvent;
        RaiseEvent(target, args);
        return args;
    }

    #endregion Unchecked

    #region Indeterminate

    /// <summary>
    /// A static helper method to raise the Indeterminate event on a target element.
    /// </summary>
    /// <param name="target">UIElement or ContentElement on which to raise the event</param>
    internal static RoutedEventArgs RaiseIndeterminateEvent(UIElement target)
    {
        if (target == null) return null;

        var args = new RoutedEventArgs();
        args.RoutedEvent = ToggleButton.IndeterminateEvent;
        RaiseEvent(target, args);
        return args;
    }

    #endregion Indeterminate

    #endregion routed events

    #region private methods

    private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        UpdateIsChecked(sender as DependencyObject);
    }

    private static void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.OriginalSource == sender)
        {
            if (e.Key == Key.Space)
            {
                // ignore alt+space which invokes the system menu
                if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt) return;

                UpdateIsChecked(sender as DependencyObject);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && (bool)(sender as DependencyObject).GetValue(KeyboardNavigation.AcceptsReturnProperty))
            {
                UpdateIsChecked(sender as DependencyObject);
                e.Handled = true;
            }
        }
    }

    private static void UpdateIsChecked(DependencyObject d)
    {
        var isChecked = GetIsChecked(d);
        if (isChecked == true)
        {
            SetIsChecked(d, GetIsThreeState(d) ? null : false);
        }
        else
        {
            SetIsChecked(d, isChecked.HasValue);
        }
    }

    private static void RaiseEvent(DependencyObject target, RoutedEventArgs args)
    {
        if (target is UIElement)
        {
            (target as UIElement).RaiseEvent(args);
        }
        else if (target is ContentElement)
        {
            (target as ContentElement).RaiseEvent(args);
        }
    }

    #endregion private methods
}