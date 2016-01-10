using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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

        private IEnumerable<object> _variableList;
        private List<double> _variableValues; 
        
        string lastData;

        public Page3()
        {
            _variableList = Connector.staticVars.Union(Connector.controlVarList);    
            InitializeComponent();
            ListAeq.SelectionMode = SelectionMode.Single;
            _variableValues=new List<double>();
            foreach (var item in _variableList)
            {
                _variableValues.Add(0);
            }
            ParamsListBox.ItemsSource = _variableList;
            ListAeq.ItemsSource = _variableValues;
        }

        private void ParamsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }

        private void ClearAllLimits(object sender, RoutedEventArgs e)
        {
            Connector.Aeq.Clear();
            Connector.Beq.Clear();
        }

        private void SaveElementButton_Click(object sender, RoutedEventArgs e)
        {
            var doub = double.Parse(ElementBeq.Text, CultureInfo.InvariantCulture);
            _variableValues[ListAeq.SelectedIndex]=doub;         
        }

        private void ListAeqDoubleClick (object sender, MouseEventArgs e)
        {
            TextBoxOneParamFromAeq.Text = ListAeq.SelectedItem.ToString();
            lastData = ListAeq.SelectedItem.ToString();           
        }

        private void ListAeq_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ElementBeq.Text = ListAeq.SelectedItems[0].ToString();
        }

        private void SaveLimits(object sender, RoutedEventArgs e)
        {
            List<double> aqArr = new List<double>();
            foreach (var temp in ListAeq.Items)
            {
                aqArr.Add(System.Convert.ToDouble(temp));
            }
            Connector.Aeq.Add(aqArr);
            Connector.Beq.Add(System.Convert.ToDouble(ElementBeq,CultureInfo.InvariantCulture));
        }

        private void ButtonContinue(object sender, RoutedEventArgs e)
        {
            List<double> aqArr = new List<double>();
            foreach (var temp in ListAeq.Items)
            {
                aqArr.Add(System.Convert.ToDouble(temp));
            }
            Connector.Aeq.Add(aqArr);
            Connector.Beq.Add(double.Parse(ElementBeq.Text));
            NavigationService.Navigate(new Uri("Page2.xaml", UriKind.Relative));
        }

       

    }
}
