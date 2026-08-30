using System.ComponentModel;

namespace QuickMsiBuilder.UI
{
    /// <summary>
    /// One tick box in the Revit versions list.
    /// </summary>
    public class RevitYearOption : INotifyPropertyChanged
    {
        private bool _isSelected;

        public RevitYearOption(string year)
        {
            Year = year;
        }

        public string Year { get; private set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                var handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs("IsSelected"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
