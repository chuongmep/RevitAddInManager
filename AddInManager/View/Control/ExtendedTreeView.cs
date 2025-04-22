using System.Windows;

namespace RevitAddinManager.View.Control;

public class ExtendedTreeView : System.Windows.Controls.TreeView
{
    public ExtendedTreeView()
    {
        SelectedItemChanged += ItemChange;
    }

    private void ItemChange(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (SelectedItem != null)
        {
            SetValue(SelectedItem_Property, SelectedItem);
        }
    }

    public object SelectedItem_
    {
        get => GetValue(SelectedItem_Property);
        set => SetValue(SelectedItem_Property, value);
    }

    public static readonly DependencyProperty SelectedItem_Property = DependencyProperty.Register("SelectedItem_", typeof(object), typeof(ExtendedTreeView), new UIPropertyMetadata(null));

    public static readonly DependencyProperty ContextMenuOpenedProperty = DependencyProperty.RegisterAttached("ContextMenuOpened", typeof(bool), typeof(ExtendedTreeView));

    public static bool GetContextMenuOpened(DependencyObject obj)
    { return (bool)obj.GetValue(ContextMenuOpenedProperty); }

    public static void SetContextMenuOpened(DependencyObject obj, bool value)
    { obj.SetValue(ContextMenuOpenedProperty, value); }
}