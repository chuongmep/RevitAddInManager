using System.Collections.Generic;
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
                        if (item is Autodesk.Windows.RibbonButton button)
                        {
                            // Heuristic check: check button ID, text, or tooltips for the assembly name
                            if (button.Id != null && (button.Id.Contains(assemblyName) || button.Id.Contains(assemblyPath)))
                            {
                                shouldRemove = true;
                                break;
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
                    try
                    {
                        string panelName = panel.Source.Title;
                        tab.Panels.Remove(panel);

                        // Revit API internal cleanup to avoid "Panel already exists" error on re-load
                        var uiApplicationType = typeof(UIApplication);
                        var ribbonItemsProperty = uiApplicationType.GetProperty("RibbonItemDictionary",
                            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
                        if (ribbonItemsProperty != null)
                        {
                            var ribbonItems = (Dictionary<string, Dictionary<string, Autodesk.Revit.UI.RibbonPanel>>)
                                ribbonItemsProperty.GetValue(null);
                            if (ribbonItems != null)
                            {
                                foreach (var tabItem in ribbonItems.Values)
                                {
                                    tabItem.Remove(panelName);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error removing panels: {ex.Message}");
        }
    }
}
