using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitElementBipChecker.Model;
using RevitElementBipChecker.View;

namespace RevitElementBipChecker.Viewmodel
{
    public class BipCheckerViewmodel : BaseElement
    {
        public FrmBipChecker frmmain;
        private RevitEvent revitEvent = new RevitEvent();
        public const string DefaultValue = "<null>";
        private ObservableCollection<ParameterData> data;

        public ObservableCollection<ParameterData> Data
        {
            get
            {

                return data;
            }
            set { OnPropertyChanged(ref data, value); }
        }

        private ICollectionView itemsView;
        public ICollectionView ItemsView
        {
            get
            {
                if (itemsView == null)
                {
                    itemsView = CollectionViewSource.GetDefaultView(Data);
                    itemsView.Filter = filterSearchText;
                }
                return itemsView;
            }
            set => OnPropertyChanged(ref itemsView, value);
        }

        private bool isInstance = true;
        public bool IsInstance
        {
            get => isInstance;
            set => OnPropertyChanged(ref isInstance, value);
        }

        private bool isType;
        public bool IsType
        {
            get => isType;
            set => OnPropertyChanged(ref isType, value);
        }


        private bool filterSearchText(object item)
        {
            ParameterData paradata = (ParameterData)item;
            if (SearchText != null || SearchText != "")
            {
                return paradata.Name.ToUpper().Contains(SearchText.ToUpper());

            }
            else { return true; }
        }
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                OnPropertyChanged(ref _searchText, value);
                ItemsView.Refresh();
            }
        }

        public ICommand Copy => new CopyCommand(this);
        public ICommand OpenCsv => new ExportCsvCommand(this);
        public ExportJsonCommand OpenJson  => new ExportJsonCommand(this);
        private PickFirstCommand PickFirstCommand { get; set; }
        public ICommand SnoopCommand => new RelayCommand(()=>revitEvent.Run(PickFirstCommand.Toggle, true, null, null, false));
        public ICommand CheckTypeInstance => new RelayCommand(PickFirstCommand.ToggleTypeInstance);
        public BipCheckerViewmodel(UIDocument uidoc)
        {
            this.UIDoc = uidoc;
            SelectedCommand SelectedCommand = new SelectedCommand(this);
            PickFirstCommand = new PickFirstCommand(this);
            
        }

        public void GetDatabase()
        {

            Data = new ObservableCollection<ParameterData>();
            if(Elements==null || Elements.Count==0) return;
            Elements.ForEach(x => GetParametersData(x));
            ObservableCollection<ParameterData> list = Data.GroupBy(x => x.Parameter.Id).Select(x => x.First()).ToObservableCollection();
            Data = list;
            //Sort
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(data);
            view.SortDescriptions.Add(new SortDescription("ElementId", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription("TypeOrInstance", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription("ParameterName", ListSortDirection.Ascending));
        }

        ObservableCollection<ParameterData> GetParametersData(Autodesk.Revit.DB.Element element)
        {
            if (IsInstance)
            {
                foreach (Parameter parameter in element.Parameters)
                {

                    var parameterData = new ParameterData(element, parameter, element.Document);
                    Data.Add(parameterData);
                }
            }

            if (IsType && element.CanHaveTypeAssigned())
            {
                Element elementType = element.Document.GetElement(element.GetTypeId());
                foreach (Parameter parameter in elementType.Parameters)
                {
                    var parameterData = new ParameterData(element,parameter, UIDoc.Document, false);
                    Data.Add(parameterData);
                }
            }
            return Data;
        }
    }
}
