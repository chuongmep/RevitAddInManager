using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using Autodesk.Revit.DB;
using RevitElementBipChecker.Model;
using RevitElementBipChecker.View;

namespace RevitElementBipChecker.Viewmodel;

public class CompareBipViewModel : ViewmodeBase
{
    private Element Element1 { get; set; }
    private Element Element2 { get; set; }
    private List<ParameterBase> diffParameters1 = new List<ParameterBase>();
    private List<ParameterBase> diffParameters2 = new List<ParameterBase>();
    private bool IsToggle { get; set; }
    RevitEvent revitEvent = new RevitEvent();
    public FrmCompareBip FrmCompareBip { get; set; }
    public ICommand CloseCommand { get; set; }
    public ICommand ToggleCommand { get; set; }
    public ICommand ExportCommand { get; set; }
    private ObservableCollection<ParameterDifference> differences;
    public ObservableCollection<ParameterDifference> Differences
    {
        get
        {
            if (differences == null)
            {
                differences = new ObservableCollection<ParameterDifference>();
            }
            return differences;
        }
        set => OnPropertyChanged(ref differences, value);
    }
    private ICollectionView itemsView;
    public ICollectionView ItemsView
    {
        get
        {
            if (itemsView == null)
            {
                itemsView = CollectionViewSource.GetDefaultView(Differences);
                itemsView.Filter = filterSearchText;
            }
            return itemsView;
        }
        set => OnPropertyChanged(ref itemsView, value);
    }
    private bool filterSearchText(object item)
    {
        var data = (ParameterDifference)item;
        if (SearchText != null || SearchText != "")
        {
            return SearchText != null && data.Name.ToUpper().Contains(SearchText.ToUpper());
        }
        return true;
    }
    private string _searchText;
    public string SearchText
    {
        get
        {
            if (_searchText == null)
            {
                _searchText = "";
            }
            return _searchText;
        }
        set
        {
            OnPropertyChanged(ref _searchText, value);
            ItemsView.Refresh();
        }
    }
    public CompareBipViewModel(Element element1, Element element2)
    {
        this.Element1 = element1;
        this.Element2 = element2;
        InitData();
        ToggleCompare();
        ToggleCommand = new RelayCommand(() => revitEvent.Run(this.ToggleCompare, true, null));
        CloseCommand = new RelayCommand(() => revitEvent.Run(FrmCompareBip.Close, true, null));
        ExportCommand = new RelayCommand(() => revitEvent.Run(ExportCsv, true, null));
    }

    void ToggleCompare()
    {
        IsToggle = !IsToggle;
        if (IsToggle)
        {
            differences =
                ParameterComparer.CompareParameters(diffParameters1, diffParameters2)
                    .OrderBy(x => x.Name).ToObservableCollection();
            foreach (var item in differences)
            {
                if (diffParameters2.All(x => x.Name != item?.Name))
                {
                    item!.RowColor = System.Windows.Media.Brushes.Firebrick;
                }
            }
        }
        else
        {
            differences =
                ParameterComparer.CompareParameters(diffParameters2, diffParameters1)
                .OrderBy(x => x.Name).ToObservableCollection();
            foreach (var item in differences)
            {
                if (diffParameters1.All(x => x.Name != item?.Name))
                {
                    item!.RowColor = System.Windows.Media.Brushes.Firebrick;
                }
            }
        }
        OnPropertyChanged(nameof(Differences));
        ItemsView = CollectionViewSource.GetDefaultView(Differences);
        ItemsView.Refresh();
        ItemsView.Filter = filterSearchText;
        OnPropertyChanged(nameof(SearchText));
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

    private void ExportCsv()
    {
        string filename = "result";
        if (IsToggle)
        {
            filename = Element1.Id.ToString() + "_" + Element2.Id.ToString() + ".csv";
        }
        else
        {
            filename = Element2.Id.ToString() + "_" + Element1.Id.ToString() + ".csv";
        }
        string exportCsv = FrmCompareBip.dataGrid.Items.Cast<ParameterDifference>()
            .ToList()
            .ToDataTable()
            .ExportCsv(filename);
        Process.Start(exportCsv);
    }
}