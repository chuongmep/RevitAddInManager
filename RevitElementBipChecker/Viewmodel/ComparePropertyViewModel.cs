using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitElementBipChecker.Model;
using Color = System.Windows.Media.Color;

namespace RevitElementBipChecker.Viewmodel
{
    public class ComparePropertyViewModel : BaseElementCompare
    {
        private ObservableCollection<ComparisonResult> differences;

        public ObservableCollection<ComparisonResult> Differences
        {
            get
            {
                if (differences == null)
                {
                    differences = new ObservableCollection<ComparisonResult>();
                }
                return differences;
            }
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
            var data = (ComparisonResult) item;
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
        public bool IsToggle { get; set; } = false;
        public ICommand ToggleCommand { get; set; }
        RevitEvent revitEvent = new RevitEvent();
        public ICommand HelpCommand { get; set; }
        public ComparePropertyViewModel(UIApplication uiApp, Autodesk.Revit.DB.Element element1,Autodesk.Revit.DB.Element element2)
        {
            UiApp = uiApp;
            UiDoc = uiApp.ActiveUIDocument;
            this.Element1 = element1;
            this.Element2  = element2;
            Toggle();
            ToggleCommand = new RelayCommand(() => revitEvent.Run(Toggle, true, null));
            HelpCommand = new RelayCommand(() => revitEvent.Run(HelpClick, true, null));
        }
        
        private void Toggle()
        {
            IsToggle = !IsToggle;
            if(differences==null) differences = new ObservableCollection<ComparisonResult>();
            if (IsToggle)
            {
                PropertyViewModel propertyViewModel1 = new PropertyViewModel(Element1);
                List<BaseDataCompare> list1 = propertyViewModel1.GetPropertyData();
                PropertyViewModel propertyViewModel2 = new PropertyViewModel(Element2);
                List<BaseDataCompare> list2 = propertyViewModel2.GetPropertyData();
                differences = MetadataCompareDiff.CompareDifferences(list1, list2)
                  .OrderBy(x=>x.Name).ToObservableCollection();
            }
            else
            {
                PropertyViewModel propertyViewModel1 = new PropertyViewModel(Element2);
                List<BaseDataCompare> list1 = propertyViewModel1.GetPropertyData();
                PropertyViewModel propertyViewModel2 = new PropertyViewModel(Element1);
                List<BaseDataCompare> list2 = propertyViewModel2.GetPropertyData();
                differences = MetadataCompareDiff.CompareDifferences(list1, list2)
                    .OrderBy(x=>x.Name)
                    .ToObservableCollection();
            }
            foreach (var difference in differences)
            {
                if(difference.Type.FullName != null && difference.Type.FullName.Contains("Autodesk"))
                {
                    // set color #0000FF
                    difference.RowColor = Brushes.Beige;
                }
                else if (difference.StateCompare == StateCompare.InList1Only)
                {
                    difference.RowColor = System.Windows.Media.Brushes.Firebrick;
                }
                else if (difference.StateCompare == StateCompare.InList2Only)
                {
                    difference.RowColor = System.Windows.Media.Brushes.SeaGreen;
                }
                else if (difference.StateCompare == StateCompare.SameValue)
                {
                    difference.RowColor = System.Windows.Media.Brushes.Gray;
                }
                else if (difference.StateCompare == StateCompare.DifferentValue)
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
        private void HelpClick()
        {
            Process.Start("https://github.com/chuongmep/RevitAddInManager/wiki/How-to-use-Compare-Parameter-Element");
        }
    }
}
