using System.ComponentModel;
using QuickMsiBuilder.CLI;

namespace QuickMsiBuilder.UI
{
    /// <summary>
    /// One tick box in the entry points list.
    /// </summary>
    public class AddinEntryOption : INotifyPropertyChanged
    {
        private bool _isSelected;

        public AddinEntryOption(AddinCandidate candidate, bool isSelected)
        {
            Candidate = candidate;
            _isSelected = isSelected;
        }

        public AddinCandidate Candidate { get; private set; }

        public string FullClassName
        {
            get { return Candidate.FullClassName; }
        }

        /// <summary>e.g. "Contoso.MyCommand  (Command)".</summary>
        public string DisplayName
        {
            get { return string.Format("{0}  ({1})", Candidate.FullClassName, Candidate.AddinType); }
        }

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
