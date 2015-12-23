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
using MathWorks.MATLAB.NET.Arrays;
using MathWorks.MATLAB.NET.Utility;
using intlinprogNative;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;


namespace cours_work_test6
{
    /// <summary>
    /// Логика взаимодействия для Page2.xaml
    /// </summary>
    /// 

    public partial class Page2 : Page
    {

       
        MILP ml = new MILP();
        public Dictionary<string, double> regressionDictionary;
        double[] F;
        double const_f;
        double[,] A; //
        double[] B;
        double[,] Aeq;
        List<double> Beq;
        double[] BeqArr;
        
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

            F = new double[Connector.regressionDictionary.ElementAt(0).Value.Count - 2];
            A = new double[(Connector.regressionDictionary.Count -1) * 2, Connector.regressionDictionary.ElementAt(1).Value.Count - 1];
            B = new double[A.GetLength(0)];
            ub = new double[Connector.MinMaxDictionary.Count];
            lb = new double[Connector.MinMaxDictionary.Count];
            intcon = new List<double>();

            Beq = new List<double>();
            DictVars = new Dictionary<string, bool>();
            //intcon = new int[Connector.MinMaxDictionary.Count];
            NumVarStat = new List<int>();
            //intVars = new List<string>();
        }


        #region MatlabFunctions
        private void InsertArrayF()
        {
            Dictionary<string, double> regretionF = new Dictionary<string, double>();
            regretionF = Connector.regressionDictionary.ElementAt(0).Value;
            //F = new double[regretionF.Count];
            const_f = regretionF.ElementAt(0).Value;

            for (int i = 1; i < regretionF.Count - 1; i++)
            {
                //F[count] = element.Value;
                F[i-1] = regretionF.ElementAt(i).Value;
                DataList.Items.Add(regretionF.ElementAt(i - 1).Key.ToString());
            }
        }

        //private void InsertArrayA()
        //{
        //    Dictionary<string, double> regretionF = new Dictionary<string, double>();
        //    regretionF = Connector.regressionDictionary.ElementAt(0).Value;
            
        //    for (int i = 0; i < Connector.regressionDictionary.Count-1; i++)
        //    {
        //        regretionF = Connector.regressionDictionary.ElementAt(i+1).Value;
        //        for (int j = 1; j < regretionF.Count; j++)
        //            A[i, j - 1] = regretionF.ElementAt(j).Value;
        //    }
        //    for (int i = Connector.regressionDictionary.Count, k = 0; i < (Connector.regressionDictionary.Count - 1) * 2; i++, k++)
        //    {
        //        regretionF = Connector.regressionDictionary.ElementAt(k+1).Value;
        //        for (int j = 1; j < regretionF.Count; j++)
        //            A[i, j - 1] = -regretionF.ElementAt(j).Value;
        //    }
        //}

        private void InsertArrayA()
        {
            for (int i = 1; i < Connector.regressionDictionary.Count; i++)
            {
                for (int j = 1; j < Connector.regressionDictionary.ElementAt(i).Value.Count; j++)
                {
                    A[i - 1, j - 1] = Connector.regressionDictionary.ElementAt(i).Value.ElementAt(j).Value;
                }
            }

            for (int i = Connector.regressionDictionary.Count, k = 1; k < Connector.regressionDictionary.Count; i++, k++)
            {
                for(int j = 1; j < Connector.regressionDictionary.ElementAt(k).Value.Count; j++)
                {
                    A[i - 1, j - 1] = - Connector.regressionDictionary.ElementAt(k).Value.ElementAt(j).Value;
                }
            }
        }
        private void InsertArrayB()
        {
            //Dictionary<string, double> regretionF = new Dictionary<string, double>();
            //regretionF = Connector.regressionDictionary.ElementAt(0).Value;
            
            for (int count = 0; count < B.Length / 2; count++)//foreach (var temp in Connector.regressionDictionary)
            {
                B[count] = (double)(Connector.MinMaxDictionary.ElementAt(count).Value.max) - Connector.regressionDictionary.ElementAt(count + 1).Value.ElementAt(0).Value; //+ ограничение добавить от пользователя
            }
            
            for (int count = B.Length / 2, i = 0; count < B.Length; count++, i++ )//foreach (var temp in Connector.regressionDictionary)
            {
                B[count] = Connector.regressionDictionary.ElementAt(i + 1).Value.ElementAt(0).Value - (double)(Connector.MinMaxDictionary.ElementAt(i).Value.min);
               
            }
        }

        private void InsertArraysLBandUB()
        {
            int i = 0;
            foreach (var element in Connector.MinMaxDictionary.Values)
            {
                ub[i] = (double)element.max;
                lb[i] = (double)element.min;
                i++;
            }  
        }

         
        private void InsertArraysAeqBeq()
        {
            Aeq = new double[NumVarStat.Count, Connector.MinMaxDictionary.Count];

            for (int i = 0; i < NumVarStat.Count; i++)
            {
                for (int j = 0; j < Connector.MinMaxDictionary.Count; j++)
                {
                    Aeq[i, j] = 0;
                }
            } 
            int k = 0;
            foreach (var temp in NumVarStat)
            {
                Aeq[k, temp] = 1;
                k++;
            }
            BeqArr = new double[Beq.Count];
            k = 0;
            foreach (var temp in Beq)
            {
                BeqArr[k] = temp;
            }

        }


        private void InsertArrayInrconst()
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
            MILP optimisation = new MILP();
            try
            {
                var result = optimisation.intlinprog(2, F, intcon.ToArray(), A, B, Aeq, Beq.ToArray(), lb, ub);
                object[] resob = (object[])result;
                double[,] res1 = (double[,])resob[1];
                double[,] res2 = (double[,])resob[0];
                int i = 0;
                
                foreach (double temp in res2)
                {
                    OutputList.Content += Connector.regressionDictionary.ElementAt(0).Value.ElementAt(i).Key.ToString() + temp.ToString() + Environment.NewLine;
                }
                OutputList.Content += "Оптимальное решение: " + Connector.regressionDictionary.ElementAt(0).Key.ToString() + (res1[0, 0] + const_f).ToString();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
        
        #endregion MatlabFunctions

        

        #region Buttons
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        //Не закончено
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            OutputList.Content = "";
            DataList.Items.Clear();
            InsertArrayF();
            InsertArrayA();
            InsertArrayB();
            InsertArraysLBandUB();
            InsertArraysAeqBeq();
            InsertArrayInrconst();
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
                    B[A.GetLength(0)/2 - 1] = -Convert.ToDouble(Min.Text) + const_f;
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
            Dictionary<string, int> NameNom = new Dictionary<string,int>();
            Dictionary<int, string> NomName = new Dictionary<int, string>();
             
            int i = 1;
            
            foreach (var temp in Connector.regressionDictionary.ElementAt(0).Value)
            {
                if (temp.Key != "const")
                {
                    NameNom.Add(temp.Key.ToString(), i);
                    NomName.Add(i, temp.Key.ToString());
                    i++;
                }
            }
            i = 0;
            //Connector.regressionDictionary.ElementAt(0).Value.ElementAt(i);
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void ClearIntcon(object sender, RoutedEventArgs e)
        {
            intcon.Clear();
            IndexBox.Content = "";
        }

        private void ButtonAddStatVar(object sender, RoutedEventArgs e)
        {
            Dictionary<string, int> NameNom = new Dictionary<string, int>();
            Dictionary<int, string> NomName = new Dictionary<int, string>();

            int i = 1;

            foreach (var temp in Connector.regressionDictionary.ElementAt(0).Value)
            {
                if (temp.Key != "const")
                {
                    NameNom.Add(temp.Key.ToString(), i);
                    NomName.Add(i, temp.Key.ToString());
                    i++;
                }
            }
           // foreach (string item in DataList.SelectedItems)
           

           if (DataList.SelectedItems.Count == 1)
           {
               foreach (var temp in Connector.staticVars)
               {
                   if (DataList.SelectedItems[0] == temp.ToString())
                   {
                       Beq.Add(Convert.ToDouble(StatVarBox.Text));
                       foreach (var temp1 in NameNom)
                       {
                           if (temp1.Key.ToString() == DataList.SelectedItems[0].ToString())
                               NumVarStat.Add(temp1.Value);
                       }

                   }

               }
           }
           else
           {
               MessageBox.Show("Выбрано более одной переменной или не одной!", "Ошибка!"); ;
           }
           }

        private void ClearStatVars(object sender, RoutedEventArgs e)
        {
            Beq.Clear();
            NumVarStat.Clear();
        }

        private void RunMatlab(object sender, RoutedEventArgs e)
        {
            MatlabWork();
        }
        
    }
}
        #endregion Buttons