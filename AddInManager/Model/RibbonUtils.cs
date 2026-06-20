using System.Diagnostics;
using System.Reflection;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using RibbonPanel = Autodesk.Revit.UI.RibbonPanel;

namespace RevitAddinManager.Model;

public static class RibbonUtils
{
    private static readonly FieldInfo RibbonPanelField = typeof(RibbonPanel).GetField("m_RibbonPanel", BindingFlags.Instance | BindingFlags.NonPublic);

    public static Autodesk.Windows.RibbonPanel GetRibbonPanel(RibbonPanel panel)
    {
        return RibbonPanelField.GetValue(panel) as Autodesk.Windows.RibbonPanel;
    }

    public static void RemovePanels(string assemblyPath)
    {
        try
        {
            var ribbon = ComponentManager.Ribbon;
            string assemblyName = System.IO.Path.GetFileNameWithoutExtension(assemblyPath);
            foreach (var tab in ribbon.Tabs)
            {
                var panelsToRemove = new List<Autodesk.Windows.RibbonPanel>();
                foreach (var panel in tab.Panels)
                {
                    bool shouldRemove = false;
                    foreach (var item in panel.Source.Items)
                    {
                        if (item is RibbonButton button)
                        {
                            // Heuristic check: check button ID, text, or tooltips for the assembly name
                            if (button.Id != null && (button.Id.Contains(assemblyName) || button.Id.Contains(assemblyPath)))
                            {
                                shouldRemove = true;
                                break;
                            }
                            if (button.Text != null && button.Text.Contains(assemblyName))
                            {
                                // dangerous, but might work
                            }
                        }
                    }

                    if (shouldRemove)
                    {
                        panelsToRemove.Add(panel);
                    }
                }

                foreach (var panel in panelsToRemove)
                {
                    tab.Panels.Remove(panel);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error removing panels: {ex.Message}");
        }
    }
}
