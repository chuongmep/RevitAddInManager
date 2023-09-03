using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using RevitElementBipChecker.Model;

namespace RevitElementBipChecker.View
{
    /// <summary>
    /// Interaction logic for FrmCompareBip.xaml
    /// </summary>
    public partial class FrmCompareBip
    {
        private Element Element1 { get; set; }
        private Element Element2 { get; set; }
        private List<ParameterBase> diffParameters1 = new List<ParameterBase>();
        private List<ParameterBase> diffParameters2 = new List<ParameterBase>();
        private bool IsToggle { get; set; }
        RevitEvent revitEvent = new RevitEvent();
        public string FirstValue { get; set; } = "Hello";
        public FrmCompareBip(Element element1,Element element2)
        {
            InitializeComponent();
            this.Element1 = element1;
            this.Element2 = element2;
            InitData();
            ToggleCompare();
            
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            try
            {
                revitEvent.Run(this.Close, true, null);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void ToggleClick(object sender, RoutedEventArgs e)
        {
            try
            {
                revitEvent.Run(this.ToggleCompare, true, null);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        void ToggleCompare()
        {
            //clear items
            IsToggle = !IsToggle;
            dataGrid.ItemsSource = null;
            dataGrid.Items.Clear();
            //get new items
            if (IsToggle)
            {
                List<ParameterDifference> differences = ParameterComparer.CompareParameters(diffParameters1, diffParameters2);
                dataGrid.ItemsSource = differences.OrderBy(x=>x.Name);
                foreach (var item in dataGrid.Items)
                {
                    ParameterDifference parameterDifference = item as ParameterDifference;
                    if (diffParameters2.All(x => x.Name != parameterDifference?.Name))
                    {
                        parameterDifference.BackgroundColor = System.Windows.Media.Brushes.Firebrick;
                    }
                }
            }
            else
            {
                List<ParameterDifference> differences = ParameterComparer.CompareParameters(diffParameters2, diffParameters1);
                dataGrid.ItemsSource = differences.OrderBy(x=>x.Name);
                foreach (var item in dataGrid.Items)
                {
                    ParameterDifference parameterDifference = item as ParameterDifference;
                    if (diffParameters1.All(x => x.Name != parameterDifference?.Name))
                    {
                        parameterDifference.BackgroundColor = System.Windows.Media.Brushes.Firebrick;
                    }
                }
                
                 
            }
            
        }

        void InitData()
        {
            foreach (Autodesk.Revit.DB.Parameter parameter in Element1.ParametersMap)
            {
                diffParameters1.Add(new ParameterBase()
                {
                    Name = parameter.Definition.Name,
                    Value = parameter.AsValueString(),
                    Type = parameter.StorageType.ToString()
                });
            }

            if (Element1.CanHaveTypeAssigned())
            {
                var typeElement1 = Element1.Document.GetElement(Element1.GetTypeId());
                foreach (Autodesk.Revit.DB.Parameter parameter in typeElement1.ParametersMap)
                {
                    diffParameters1.Add(new ParameterBase()
                    {
                        Name = parameter.Definition.Name,
                        Value = parameter.AsValueString(),
                        Type = parameter.StorageType.ToString()
                    });
                }
            }
            foreach (Autodesk.Revit.DB.Parameter parameter in Element2.ParametersMap)
            {
                diffParameters2.Add(new ParameterBase()
                {
                    Name = parameter.Definition.Name,
                    Value = parameter.AsValueString(),
                    Type = parameter.StorageType.ToString()
                });
            }
            if (Element2.CanHaveTypeAssigned())
            {
                var typeElement2 = Element2.Document.GetElement(Element2.GetTypeId());
                foreach (Autodesk.Revit.DB.Parameter parameter in typeElement2.ParametersMap)
                {
                    diffParameters2.Add(new ParameterBase()
                    {
                        Name = parameter.Definition.Name,
                        Value = parameter.AsValueString(),
                        Type = parameter.StorageType.ToString()
                    });
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
