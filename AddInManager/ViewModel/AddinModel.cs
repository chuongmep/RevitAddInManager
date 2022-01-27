using RevitAddinManager.Model;

namespace RevitAddinManager.ViewModel;

public class AddinModel : ViewModelBase
{

    bool? _isChecked = false;
    AddinModel _parent;

    public AddinModel(string name)
    {
        Name = name;
        Children = new List<AddinModel>();
    }

    public void Initialize()
    {
        foreach (var child in Children)
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
        set => SetIsChecked(value, true, true);
    }

    public bool? IsParentTree { get; set; } = false;

    void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
    {
        if (value == _isChecked)
            return;

        _isChecked = value;

        if (updateChildren && _isChecked.HasValue)
            Children.ForEach(c => c.SetIsChecked(_isChecked, true, false));

        if (updateParent && _parent != null)
            _parent.VerifyCheckState();

        OnPropertyChanged(nameof(IsChecked));
    }

    void VerifyCheckState()
    {
        bool? state = null;
        for (var i = 0; i < Children.Count; ++i)
        {
            var current = Children[i].IsChecked;
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
        SetIsChecked(state, false, true);
    }
}