using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cours_work_test6
{
    public class MinMax
    {
        public double min { get; set; }
        public double max { get; set; }

        public MinMax(){}
        public MinMax(double min,double max)
        {
            this.min = min;
            this.max = max;
        }
    }

    /// <summary>
    /// Костыль-костылевич. В будущих версиях будет убран
    /// </summary>
    public static class Connector
    {
        public static Dictionary<string, Dictionary<string, double>> regressionDictionary { get; set; }
        public static Dictionary<string, MinMax> MinMaxDictionary { get; set; }
        public static ObservableCollection<object> staticVars = new ObservableCollection<object>();
    }
}
