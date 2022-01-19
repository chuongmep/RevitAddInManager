using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddInManager.Model
{
    public class RevitAddin : ViewModelBase
    {
        public string Assembly { get; set; }
        public string ClientId { get; set; }
        public string Name { get; set; }
        public string FullClassName { get; set; }
        public string Text { get; set; }
        public string VisibilityMode { get; set; }
        public string LanguageType { get; set; }
        public string VendorId { get; set; }
        public string VendorDescription { get; set; }
        public AddinType AddinType { get; set; }

        private bool _IsChecked;
        public bool IsChecked
        {
            get => _IsChecked;
            set => OnPropertyChanged(ref _IsChecked, value);
        }

    }


}
