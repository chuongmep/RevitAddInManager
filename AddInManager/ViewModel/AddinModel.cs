using RevitAddinManager.Model;

namespace RevitAddinManager.ViewModel
{
    public class AddinModel : ViewModelBase
    {

        bool? _isChecked = false;
        AddinModel _parent;

        public AddinModel(string name)
        {
            this.Name = name;
            this.Children = new List<AddinModel>();
        }

        public void Initialize()
        {
            foreach (AddinModel child in this.Children)
            {
                child._parent = this;
                child.Initialize();
            }
        }
        public List<AddinModel> Children { get; set; }

        public Addin Addin { get; set; }
        public AddinItem AddinItem { get; set; }
        public bool IsInitiallySelected { get; set; }

        public string Name { get; private set; }

        public bool? IsChecked
        {
            get => _isChecked;
            set => this.SetIsChecked(value, true, true);
        }

        public bool? IsParentTree { get; set; } = false;

        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
                this.Children.ForEach(c => c.SetIsChecked(_isChecked, true, false));

            if (updateParent && _parent != null)
                _parent.VerifyCheckState();

            this.OnPropertyChanged(nameof(IsChecked));
        }

        void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
        }
    }
}