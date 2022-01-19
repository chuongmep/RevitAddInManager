using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddInManager.Model
{
    public class ProductType
    {
        /// <summary>
        /// Use for feature develop mix flaform support multiple software autodesk
        /// </summary>
        public enum MyEnum
        {
            Revit = 1,
            Autocad = 2,
            Naviswork = 3,
            Unknown = -1
        }
    }
}
