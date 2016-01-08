using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace cours_work_test6
{
    /// <summary>
    /// Логика взаимодействия для Page3.xaml
    /// </summary>
    public partial class Page3 : Page
    {

        string lastData;

        public Page3()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListParams.Items.Clear();
            ListAeq.Items.Clear();
            Dictionary<string, double> regretionF = new Dictionary<string, double>();
            regretionF = Connector.regressionDictionary.ElementAt(0).Value;
            for (int i = 1; i < regretionF.Count - 1; i++)
            {
                ListParams.Items.Add(regretionF.ElementAt(i).Key.ToString());
                ListAeq.Items.Add("0");
            }
        }

        private void ClearAllLimits(object sender, RoutedEventArgs e)
        {
            Connector.Aeq.Clear();
            Connector.Beq.Clear();
        }

        private void SaveOneParam(object sender, RoutedEventArgs e)
        {
            ListAeq.Items.Insert(ListAeq.Items.IndexOf(lastData), TextBoxOneParamFromAeq.Text.ToString());
        }

        private void ListAeqDoubleClick (object sender, MouseEventArgs e)
        {
            TextBoxOneParamFromAeq.Text = ListAeq.SelectedItem.ToString();
            lastData = ListAeq.SelectedItem.ToString();           
        }

        private void SaveLimits(object sender, RoutedEventArgs e)
        {
            List<double> aqArr = new List<double>();
            foreach (var temp in ListAeq.Items)
            {
                aqArr.Add(System.Convert.ToDouble(temp));
            }
            Connector.Aeq.Add(aqArr);
            Connector.Beq.Add(System.Convert.ToDouble(ElementBeq));
        }

        private void ButtonContinue(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("Page2.xaml", UriKind.Relative));
        }

       

    }
}
