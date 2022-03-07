using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddinManager
{
    public class FormControl
    {
        private static FormControl instance;

        public static FormControl Instance
        {
            get
            {
                if (instance == null) instance = new FormControl();
                return instance;
            }
        }
        public bool IsOpened { get; set; }
    }
}
