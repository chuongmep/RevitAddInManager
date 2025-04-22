using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RevitElementBipChecker.Model
{
    public static class IEnumerableUtils
    {
        /// <summary>
        /// Convert From T IEnumerable To ObservableCollection
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
