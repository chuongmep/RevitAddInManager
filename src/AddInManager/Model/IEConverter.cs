using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddInManager.Model
{
    /// <summary>
    /// Quick convert collection
    /// </summary>
    public static class IEConverter
    {
        /// <summary>
        /// Convert IEnumerableUtils To ObservableCollection 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            ObservableCollection<T> newSource = new ObservableCollection<T>();
            foreach (T t in source)
            {
                newSource.Add(t);
            }
            return newSource;
        }
    }
}
