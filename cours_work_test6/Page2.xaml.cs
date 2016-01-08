using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using intlinprogNative;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;



namespace cours_work_test6
{
    /// <summary>
    /// Логика взаимодействия для Page2.xaml
    /// </summary>
    /// 

    public partial class Page2 : Page
    {

        public Dictionary<string, double> regressionDictionary;
        private Dictionary<string, double> _parameterRegressionValuesDictionary;
        private Dictionary<string, double?> _parameterValuesDictionary;
        double[] F;
        double const_f;
        double[,] A; //
        double[] B;

        double[] ub; //max
        double[] lb; //min
        List<double> intcon;
        double[] intconArr;
        //List<string> intVars; 
        Dictionary<string, bool> DictVars;
        List<int> NumVarStat;



        string changedVar = null;

        public Page2()
        {
            InitializeComponent();
            F = new double[Connector.regressionDictionary.ElementAt(0).Value.Count - (2+Connector.parameterList.Count)];
            A = new double[(Connector.regressionDictionary.Count - 1) * 2, Connector.regressionDictionary.ElementAt(1).Value.Count - 1];
            B = new double[A.GetLength(0)];
            ub = new double[Connector.MinMaxDictionary.Count - 1];
            lb = new double[Connector.MinMaxDictionary.Count - 1];
            intcon = new List<double>();
            DictVars = new Dictionary<string, bool>();
            NumVarStat = new List<int>();
            _parameterRegressionValuesDictionary=new Dictionary<string, double>();
            _parameterValuesDictionary= new Dictionary<string, double?>();
        }


        #region MatlabFunctions
        private void InsertArrayF()
        {
            Dictionary<string, double> regressionF = new Dictionary<string, double>();
            regressionF = Connector.regressionDictionary.ElementAt(0).Value;
            //F = new double[regressionF.Count];
            const_f = regressionF.ElementAt(0).Value;

            for (int i = 1; i < regressionF.Count - 1; i++)
            {
                if (Connector.parameterList.Contains(regressionF.ElementAt(i).Key))
                {
                    _parameterRegressionValuesDictionary.Add(regressionF.ElementAt(i).Key,regressionF.ElementAt(i).Value);
                    _parameterValuesDictionary.Add(regressionF.ElementAt(i).Key, null);
                }
                else
                {
                    F[i - 1] = regressionF.ElementAt(i).Value;
                    DataList.Items.Add(regressionF.ElementAt(i).Key.ToString());
                }
            }
        }

        /// <summary>
        /// Заполнение матрици А
        /// </summary>
        private void InsertArrayA()
        {         
            for (int i = 1; i < Connector.regressionDictionary.Count; i++)
            {
                for (int j = 1; j < Connector.regressionDictionary.ElementAt(i).Value.Count; j++)
                {
                    A[i - 1, j - 1] = Connector.regressionDictionary.ElementAt(i).Value.ElementAt(j).Value;
                }
            }

            for (int i = Connector.regressionDictionary.Count, k = 1;
                k < Connector.regressionDictionary.Count;
                i++, k++)
            {
                for (int j = 1; j < Connector.regressionDictionary.ElementAt(k).Value.Count; j++)
                {
                    A[i - 1, j - 1] = -Connector.regressionDictionary.ElementAt(k).Value.ElementAt(j).Value;
                }
            }
        }

        /// <summary>
        /// Заполнение вектора B
        /// </summary>
        private void InsertArrayB()
        {
            for (int count = 0; count < B.Length / 2; count++)
            {
                B[count] = (double)(Connector.MinMaxDictionary.ElementAt(count).Value.max) - Connector.regressionDictionary.ElementAt(count + 1).Value.ElementAt(0).Value; //+ ограничение добавить от пользователя
            }

            for (int count = B.Length / 2, i = 0; count < B.Length; count++, i++)
            {
                B[count] = Connector.regressionDictionary.ElementAt(i + 1).Value.ElementAt(0).Value - (double)(Connector.MinMaxDictionary.ElementAt(i).Value.min);

            }
        }

        private void InsertArraysLBandUB()
        {
            int i = 0;
            foreach (var element in Connector.MinMaxDictionary)
            {
                if (element.Key != Connector.regressionDictionary.ElementAt(0).Key)
                {
                    ub[i] = (double)element.Value.max;
                    lb[i] = (double)element.Value.min;
                    i++;
                }
            }
        }
        private void InsertArrayIntcon()
        {
            int i = 0;
            intconArr = new double[intcon.Count];
            foreach (var temp in intcon)
            {
                intconArr[i] = intcon.ElementAt(i);
            }
        }

        //Не закончено
        private void MatlabWork()
        {
            MatlabMILP optimisation = new MatlabMILP();
            try
            {

                double[] aq = { 0, 0, 0, 0,0,0,0,0};
                double[] bq = { 0 };
                var result = optimisation.intlinprog(2, F, intcon.ToArray(), A, B, aq, bq, lb, ub);//Beq.ToArray()
                object[] resob = result;
                double[,] res1 = (double[,])resob[1];
                double[,] res2 = (double[,])resob[0];
                int i = 1;

                foreach (double temp in res2)
                {
                    OutputList.Content += Connector.regressionDictionary.ElementAt(0).Value.ElementAt(i++).Key.ToString() + " " + temp.ToString() + Environment.NewLine;
                }
                OutputList.Content += "Оптимальное решение: " + Connector.regressionDictionary.ElementAt(0).Key.ToString() + " = " + (res1[0, 0] + const_f).ToString() + Environment.NewLine;
                OutputList.Content += "Оптимальное решение: " + Connector.regressionDictionary.ElementAt(0).Key.ToString() + " = " + (res1[0, 0]).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        #endregion MatlabFunctions



        #region Buttons
        //Не закончено
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            OutputList.Content = "";
            DataList.Items.Clear();
            InsertArrayF();
            InsertArrayA();
            InsertArrayB();
            InsertArraysLBandUB();
            InsertArrayIntcon();
            ParamaterListBox.ItemsSource = Connector.parameterList;
            ParamaterListBox.SelectionMode = SelectionMode.Single;
        }

        private void OutputData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ButtonChangeMinMax(object sender, RoutedEventArgs e)
        {
            foreach (string item in DataList.SelectedItems)
            {
                if (DataList.SelectedItems.Count == 1)
                {
                    if (item == Connector.MinMaxDictionary.ElementAt(0).Key)
                    {
                        Max.Text = (B[0] - const_f).ToString();
                        Min.Text = (B[A.GetLength(0) / 2 - 1] + const_f).ToString();
                    }
                    else
                    {
                        Min.Text = Connector.MinMaxDictionary[item.ToString()].min.ToString();
                        Max.Text = Connector.MinMaxDictionary[item.ToString()].max.ToString();
                    }
                }
                else
                {
                    MessageBox.Show("Выбрано более одной переменной или не одной!", "Ошибка!"); ;
                }
                changedVar = item.ToString();
            }
        }

        private void ButtonSaveMinMax(object sender, RoutedEventArgs e)
        {

            if (changedVar == null)
                MessageBox.Show("Возможно Вы ошиблись кнопкой!", "Ошибка!");
            else
            {
                if (changedVar == Connector.MinMaxDictionary.ElementAt(0).Key)
                {
                    B[0] = Convert.ToDouble(Max.Text) - const_f;
                    B[A.GetLength(0) / 2 - 1] = -Convert.ToDouble(Min.Text) + const_f;
                }
                else
                {
                    Connector.MinMaxDictionary[changedVar].min = Convert.ToDouble(Min.Text);
                    Connector.MinMaxDictionary[changedVar].max = Convert.ToDouble(Max.Text);
                }
            }
        }

        private void ButtonIntDataInput(object sender, RoutedEventArgs e)
        {
            //IndexBox.Content = " ";
            Dictionary<string, int> NameNom = new Dictionary<string, int>();

            int i = 1;

            foreach (var temp in Connector.regressionDictionary.ElementAt(0).Value)
            {
                if ((temp.Key != "const") && (temp.Key != Connector.regressionDictionary.ElementAt(0).Key))
                {
                    NameNom.Add(temp.Key.ToString(), i);
                    i++;
                }
            }
            i = 0;

            foreach (string temp in DataList.SelectedItems)
            {
                foreach (var temp2 in NameNom)
                    if (temp2.Key == temp)
                    {
                        i = 0;
                        IndexBox.Content = IndexBox.Content + temp2.Value.ToString() + " ";
                        //if (intcon.Exists(temp2.Value) == false)
                        foreach (var temp3 in intcon)
                        {
                            if (temp3 == temp2.Value)
                                i = 1;
                        }
                        if (i == 0)
                            intcon.Add(temp2.Value);
                    }
            }

        }

        private void ClearIntcon(object sender, RoutedEventArgs e)
        {
            intcon.Clear();
            IndexBox.Content = "";
        }

        private void RunMatlab(object sender, RoutedEventArgs e)
        {
            if (Connector.parameterList.Count > 0)
            {
                if (_parameterValuesDictionary.ContainsValue(null))
                {
                    MessageBox.Show("Были введены не все значения параметров, дальнейший рассчет не возможен!","Ошибка!");
                }
                else
                {
                    foreach (var param in Connector.parameterList)
                    {
                        B[0] += _parameterValuesDictionary[param.ToString()].Value
                            *_parameterRegressionValuesDictionary[param.ToString()];
                    }
                    MatlabWork();
                }
            }
            else
                MatlabWork();
        }

        private void AddLimitParams(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("Page3.xaml", UriKind.Relative));
        }

        private void SaveParameterValueButton_Click(object sender, RoutedEventArgs e)
        {
            _parameterValuesDictionary[ParamaterListBox.SelectedItems[0].ToString()] =
                double.Parse(ParameterValueTextBox.Text,CultureInfo.InvariantCulture);
        }

        private void ParamaterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ParameterValueTextBox.Text = _parameterRegressionValuesDictionary[ParamaterListBox.SelectedItems[0].ToString()].ToString();
        }
    }
}
#endregion Buttons