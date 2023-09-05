using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitElementBipChecker.Model;
using RevitElementBipChecker.View;

namespace RevitElementBipChecker.Viewmodel;

public class ParameterCompareViewModel : BaseElementCompare
{
   
    private List<ParameterDifference> diffParameters1 = new List<ParameterDifference>();
    private List<ParameterDifference> diffParameters2 = new List<ParameterDifference>();
    private bool IsToggle { get; set; } = false;
    RevitEvent revitEvent = new RevitEvent();
    public FrmCompareBip FrmCompareBip { get; set; }
    public ICommand CloseCommand { get; set; }
    public ICommand HelpCommand { get; set; }
    public ICommand PropertyCommand { get; set; }
    public ICommand ToggleCommand { get; set; }
    public ICommand ExportCommand { get; set; }
    public ICommand SelectElement1Command { get; set; }
    public ICommand SelectElement2Command { get; set; }
    public ICommand SnoopCommand { get; set; }
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
        var data = (ParameterDifference) item;
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

    public ParameterCompareViewModel(UIApplication uiApp, Element element1, Element element2)
    {
        this.UiApp = uiApp;
        this.UiDoc = uiApp.ActiveUIDocument;
        this.Element1 = element1;
        this.Element2 = element2;
        InitData();
        ToggleCompare();
        ToggleCommand = new RelayCommand(() => revitEvent.Run(this.ToggleCompare, true, null));
        CloseCommand = new RelayCommand(() => revitEvent.Run(FrmCompareBip.Close, true, null));
        HelpCommand = new RelayCommand(() => revitEvent.Run(HelpClick, true, null));
        PropertyCommand = new RelayCommand(() => revitEvent.Run(PropertyClick, true, null));
        ExportCommand = new RelayCommand(() => revitEvent.Run(ExportCsv, true, null));
        SelectElement1Command = new RelayCommand(() => revitEvent.Run(SelectElement1Click, true, null));
        SelectElement2Command = new RelayCommand(() => revitEvent.Run(SelectElement2Click, true, null));
        SnoopCommand = new RelayCommand(() => revitEvent.Run(SnoopCommandClick, true, null));
    }

    private void SelectElement1Click()
    {
        UiDoc.RefreshActiveView();
        if (IsToggle)
        {
            UiDoc.Selection.SetElementIds(new List<ElementId>() {Element1.Id});
        }
        else
        {
            UiDoc.Selection.SetElementIds(new List<ElementId>() {Element2.Id});
        }
    }

    private void SelectElement2Click()
    {
        UiDoc.RefreshActiveView();
        if (IsToggle)
        {
            UiDoc.Selection.SetElementIds(new List<ElementId>() {Element2.Id});
        }
        else
        {
            UiDoc.Selection.SetElementIds(new List<ElementId>() {Element1.Id});
        }
    }

    private void SnoopCommandClick()
    {
        List<Element> newElements = new List<Element>() {Element1, Element2};
        ICollection<ElementId> elementIds = newElements.Select(x => x.Id).ToList();
        UiDoc.Selection.SetElementIds(elementIds);
        RevitCommandId lookupCommandId =
            RevitCommandId.LookupCommandId("CustomCtrl_%CustomCtrl_%Add-Ins%Explorer%RevitDBExplorer.Command");
        if (lookupCommandId == null)
        {
            TaskDialog.Show("Error", "Please install RevitDBExplorer");
            Process.Start("https://github.com/NeVeSpl/RevitDBExplorer");
            MessageBox.Show("Please install RevitDBExplorer");
            return;
        }

        // execute the command
        UiApp.PostCommand(lookupCommandId);
    }

    void ToggleCompare()
    {
        IsToggle = !IsToggle;
        if (IsToggle)
        {
            differences =
                CompareParameter.CompareParameters(diffParameters1, diffParameters2)
                    .OrderBy(x => x.Name).ToObservableCollection();
        }
        else
        {
            differences =
                CompareParameter.CompareParameters(diffParameters2, diffParameters1)
                    .OrderBy(x => x.Name).ToObservableCollection();
        }
        foreach (ParameterDifference difference in differences)
        {
            if (difference.State == StateParameter.NotExistIn1)
            {
                difference.RowColor = System.Windows.Media.Brushes.Firebrick;
            }
            else if (difference.State == StateParameter.NotExistIn2)
            {
                difference.RowColor = System.Windows.Media.Brushes.SeaGreen;
            }
            else if (difference.State == StateParameter.SameValue)
            {
                difference.RowColor = System.Windows.Media.Brushes.Gray;
            }
            else if (difference.State == StateParameter.DifferentValue)
            {
                difference.RowColor = System.Windows.Media.Brushes.Yellow;
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
            diffParameters1.Add(new ParameterDifference(Element1, parameter, Element1.Document, true));
        }

        if (Element1.CanHaveTypeAssigned())
        {
            var typeElement1 = Element1.Document.GetElement(Element1.GetTypeId());
            foreach (Autodesk.Revit.DB.Parameter parameter in typeElement1.ParametersMap)
            {
                diffParameters1.Add(new ParameterDifference(Element1, parameter, Element1.Document, false));
            }
        }

        foreach (Autodesk.Revit.DB.Parameter parameter in Element2.ParametersMap)
        {
            diffParameters2.Add(new ParameterDifference(Element2, parameter, Element2.Document, true));
        }

        if (Element2.CanHaveTypeAssigned())
        {
            var typeElement2 = Element2.Document.GetElement(Element2.GetTypeId());
            foreach (Autodesk.Revit.DB.Parameter parameter in typeElement2.ParametersMap)
            {
                diffParameters2.Add(new ParameterDifference(Element2, parameter, Element2.Document, false));
            }
        }
    }

    private void HelpClick()
    {
        Process.Start("https://github.com/chuongmep/RevitAddInManager/wiki/How-to-use-Compare-Parameter-Element");
    }

    private void PropertyClick()
    {
        ComparePropertyViewModel viewModel = new ComparePropertyViewModel(UiApp, Element1, Element2);
        FrmPropertyChecker frmPropertyChecker = new FrmPropertyChecker(viewModel);
        frmPropertyChecker.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        frmPropertyChecker.Owner = FrmCompareBip;
        frmPropertyChecker.Show();
    }
    private void ExportCsv()
    {
        string filename;
        if (IsToggle)
        {
            filename = Element1.Id + "_" + Element2.Id + ".csv";
        }
        else
        {
            filename = Element2.Id + "_" + Element1.Id + ".csv";
        }

        string exportCsv = FrmCompareBip.dataGrid.Items.Cast<ParameterDifference>()
            .ToList()
            .ToDataTable()
            .ExportCsv(filename);
        Process.Start(exportCsv);
    }
}