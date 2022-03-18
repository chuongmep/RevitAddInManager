using RevitAddinManager.Model;

namespace RevitAddinManager.ViewModel;

public class AddinModel : ViewModelBase
{
    private bool? isChecked = false;
    private AddinModel parent;

    public AddinModel(string name)
    {
        Name = name;
        Children = new List<AddinModel>();
    }

    public void Initialize()
    {
        foreach (var child in Children)
        {
            child.parent = this;
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
        get => isChecked;
        set => SetIsChecked(value, true, true);
    }

    public bool? IsParentTree { get; set; } = false;

    private void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
    {
        if (value == isChecked)
            return;

        isChecked = value;

        if (updateChildren && isChecked.HasValue)
            Children.ForEach(c => c.SetIsChecked(isChecked, true, false));

        if (updateParent && parent != null)
            parent.VerifyCheckState();

        OnPropertyChanged(nameof(IsChecked));
    }

    private void VerifyCheckState()
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