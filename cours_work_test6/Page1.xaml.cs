using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
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
//using Logic;
using Microsoft.Win32;
using RDotNet;
using Excel = Microsoft.Office.Interop.Excel;
using RDotNet.NativeLibrary;
using intlinprogNative;


namespace cours_work_test6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        private object[,] dataObjects;
        private ObservableCollection<object> varList;
        private ObservableCollection<object> controlVarList;
        private ObservableCollection<object> staticVarList;
        private ObservableCollection<object> optimVarList;
        private ObservableCollection<object> parameterList; 
        private Dictionary<string, string> columnRStringDictionary;

        /// <summary>
        /// конструктор формы
        /// </summary>
        public Page1()
        {
            InitializeComponent();

            controlVarList = new ObservableCollection<object>();
            staticVarList = new ObservableCollection<object>();
            optimVarList = new ObservableCollection<object>();
            parameterList=new ObservableCollection<object>();

            StaticVariablesList.ItemsSource = staticVarList;
            OptimVariable.ItemsSource = optimVarList;
            ControlVariablesList.ItemsSource = controlVarList;
            parameterListBox.ItemsSource = parameterList;
            VariablesList.SelectionMode = SelectionMode.Extended;


            ConfigurePath();
        }

        /// <summary>
        /// Получаем данные из экселя в виде
        /// двухмерного массива
        /// </summary>
        /// <returns>object[,]</returns>
        private object[,] GetFromExcel(string filePath)
        {
            try
            {
                Excel.Application ExcelInstance = new Excel.Application();
                Excel._Workbook WorkBook = ExcelInstance.Workbooks.Open(filePath);
                Excel._Worksheet WorkSheet = WorkBook.Sheets[1];
                Excel.Range usedRange = WorkSheet.UsedRange;

                var data = (object[,])usedRange.get_Value();

                for (int i = 1; i <= data.GetLength(0); i++)
                {
                    for (int j = 1; j <= data.GetLength(1); j++)
                    {
                        if (data[i, j] == null)
                            data[i, j] = 0;
                    }
                }

                ExcelInstance.Quit();
                return data;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            return null;
        }

        /// <summary>
        /// Превращение массива данных экселя в таблицу данных
        /// </summary>
        /// <param name="array">
        /// Массив, который надо конвертировать
        /// </param>
        /// <param name="tryGetColumnName">
        /// Указывает, надо ли получить имена колонок из первой строки
        /// массива
        /// </param>
        /// <returns>DataTable</returns>
        private DataTable ExcelArrayToDataTable(object[,] array, bool tryGetColumnName)
        {

            DataTable dataTable = new DataTable();
            // Добавляем первую пустую колонку, что бы согласовать
            // нумерацию грида и экселевской таблици.
            dataTable.Columns.Add("n", typeof(string));

            if (tryGetColumnName)
            {
                // Получаем имена и добавляем новые колонки из первой строки
                for (int c = 1; c < array.GetLength(1); c++)
                    dataTable.Columns.Add(array[1, c].ToString());

                for (int i = 2; i < array.GetLength(0) + 1; i++)
                {
                    // Получаем значения построчно и добавляем в грид
                    object[] tempObjects = new object[array.GetLength(1)];
                    for (int j = 1; j < array.GetLength(1); j++)
                    {
                        if (array[i, j] == null)
                            tempObjects[j] = 0;
                        else
                            tempObjects[j] = array[i, j];
                    }
                    dataTable.Rows.Add(tempObjects);
                }
            }
            else
            {
                //Заполняем имена рядом чисел
                for (int c = 1; c < array.GetLength(0) + 1; c++)
                    dataTable.Columns.Add(c.ToString(), typeof(double));


                for (int i = 1; i < array.GetLength(1) + 1; i++)
                {
                    // Получаем значения построчно и добавляем в грид
                    object[] tempObjects = new object[array.GetLength(0) + 1];
                    for (int j = 1; j < array.GetLength(0) + 1; j++)
                    {
                        if (array[i, j] == null)
                            tempObjects[j] = 0;
                        else
                            tempObjects[j] = array[i, j];
                    }
                    dataTable.Rows.Add(tempObjects);
                }
            }
            return dataTable;
        }

        /// <summary>
        /// Получаем список переменных из таблицы
        /// </summary>
        /// <param name="arrayObjects"></param>
        /// <returns></returns>
        private ObservableCollection<object> GetVariableList(object[,] arrayObjects)
        {
            ObservableCollection<object> list = new ObservableCollection<object>();
            for (int i = 1; i < arrayObjects.GetLength(1); i++)
            {
                list.Add(arrayObjects[1, i]);
            }

            return list;
        }

        /// <summary>
        /// создаем словарь
        /// </summary>
        /// <param name="array"></param>
        /// <param name="dictionary"></param>
        private void FillColumnDictionary(IEnumerable<object> array, out Dictionary<string, string> dictionary)
        {
            dictionary = new Dictionary<string, string>();

            foreach (var item in array)
            {
                var columnIndex = FindByColumnName(dataObjects, item.ToString());
                if (columnIndex != null)
                {
                    dictionary.Add(item.ToString(), MakeRString(dataObjects, columnIndex.Value, true));
                }
            }
        }



        #region CollectionMethods
        private int? FindByColumnName(object[,] array, string name)
        {
            for (int i = 1; i < array.GetLength(1); i++)
            {
                if (name == array[1, i].ToString())
                    return i;
            }
            return null;
        }

        /// <summary>
        /// получение строки массива как список
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="rowNumber"></param>
        /// <param name="ExcelArray"></param>
        /// <returns></returns>
        private List<T> GetRowFromArray<T>(T[,] array, int rowNumber, bool ExcelArray)
        {
            List<T> list = new List<T>();
            if (ExcelArray)
            {
                for (int i = 1; i <= array.GetLength(1); i++)
                {
                    list.Add(array[rowNumber, i]);
                }
            }
            else
            {
                for (int i = 0; i < array.GetLength(1); i++)
                {
                    list.Add(array[rowNumber, i]);
                }
            }
            return list;
        }

        /// <summary>
        /// получение колонки массива как список
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="columnNumber"></param>
        /// <param name="ExcelArray"></param>
        /// <returns></returns>
        private List<T> GetColumnFromArray<T>(T[,] array, int columnNumber, bool ExcelArray)
        {

            List<T> list = new List<T>();
            if (ExcelArray)
            {
                for (int i = 2; i <= array.GetLength(1); i++)
                {
                    list.Add(array[i, columnNumber]);
                }
            }
            else
            {
                for (int i = 0; i < array.GetLength(1); i++)
                {
                    list.Add(array[i, columnNumber]);
                }
            }
            return list;

        }


        private List<double> ConvertListToDouble(List<object> list)
        {
            List<double> outputList = new List<double>();
            try
            {
                foreach (var element in list)
                {
                    outputList.Add(System.Convert.ToDouble(element));

                }
            }
            catch (InvalidCastException exception)
            {
                MessageBox.Show("Invalid cast!");
                outputList.Add(Double.Epsilon);
            }
            return outputList;
        }

        private Dictionary<string, MinMax> CalculateMinMax(IEnumerable<object> list)
        {
            Dictionary<string, MinMax> outputDictionary = new Dictionary<string, MinMax>();
            for (int i = 0; i < list.Count(); i++)
            {
                var colList = GetColumnFromArray<object>(dataObjects
                    , FindByColumnName(dataObjects, list.ElementAt(i).ToString()).Value, true);
                var doubleList = colList.ConvertAll(e => Convert.ToDouble(e, CultureInfo.InvariantCulture));
                outputDictionary.Add(list.ElementAt(i).ToString(), new MinMax(doubleList.Min(), doubleList.Max()));
            }
            return outputDictionary;
        }

        #endregion





        #region RMethods
        /// <summary>
        /// Получение директории с основными библиотеками R
        /// и установка ее в PATH
        /// </summary>
        /// <param name="Rversion"></param>
        private void ConfigurePath()
        {
            var installPath = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\R-core\R").GetValue("InstallPath").ToString();
            var oldPath = System.Environment.GetEnvironmentVariable("PATH");
            var rPath = System.Environment.Is64BitProcess ?
                                   string.Format(@"{0}\bin\x64",installPath) :
                                   string.Format(@"{0}\bin\i386", installPath);
            if (!Directory.Exists(rPath))
                throw new DirectoryNotFoundException(
                  string.Format(" R.dll not found in : {0}", rPath));
            var newPath = string.Format("{0}{1}{2}", rPath,
                                    System.IO.Path.PathSeparator, oldPath);
            System.Environment.SetEnvironmentVariable("PATH", newPath);
        }
        /// <summary>
        /// Формирования вектора для использования в R
        /// Передаеться в Evaluate как строка
        /// </summary>
        /// <param name="arrayObjects">Массив с данными</param>
        /// <param name="columnNumber">номер колонки в массиве</param>
        /// <param name="ExcelArray">Являеться ли массив полученым из Экселя</param>
        /// <returns>строку</returns>
        private string MakeRString(object[,] arrayObjects, int columnNumber, bool ExcelArray)
        {
            string result = "c(";
            if (ExcelArray)
            {
                for (int i = 2; i <= arrayObjects.GetLength(1); i++)
                {
                    string num = arrayObjects[i, columnNumber].ToString();
                    var sp = num.Replace(",", ".");
                    if (i == arrayObjects.GetLength((1)))
                        result = string.Concat(result, sp);
                    else
                        result = string.Concat(result, sp, ",");
                }
            }
            else
            {
                for (int i = 0; i < arrayObjects.GetLength(1); i++)
                {
                    string.Concat(arrayObjects[i, columnNumber].ToString(), ",");
                }
            }

            result = string.Concat(result, ")");

            return result;
        }

        private Dictionary<string, double> CalculateStatisticData(Dictionary<string, string> columnRStringDictionary,
            string columnName, REngine engine)
        {
            try
            {

                string toEvaluate = null;
                string columns = null;
                Dictionary<string, double> regressionDictionary = new Dictionary<string, double>();
                int regrColumnPostion = 0;

                for (int i = 0; i < columnRStringDictionary.Count; i++)
                {
                    var column = columnRStringDictionary.ElementAt(i);
                    toEvaluate = string.Concat(toEvaluate, column.Key, "<-", column.Value, Environment.NewLine);
                    if (column.Key == columnName)
                    {
                        regrColumnPostion = i+1;
                        if (i == columnRStringDictionary.Count - 1)
                            columns = columns.Remove(columns.Count() - 1);
                        continue;
                    }
                    if (i == columnRStringDictionary.Count - 1)
                        columns = string.Concat(columns, column.Key);
                    else
                    {
                        columns = string.Concat(columns, column.Key, "+");
                    }
                }


                toEvaluate = string.Concat(toEvaluate, "out<-", "lm(", columnName, "~", columns, ")", Environment.NewLine, "summary(out)");

                var result = engine.Evaluate(toEvaluate).AsCharacter().ToArray();
                //foreach (var var in result)
                //{
                //    MessageBox.Show(var.ToString());
                //}

                var marker = result[4].Split(',');
                var values = result[3].Split(',');

                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = values[i].Trim('c', '(');
                    if (values[i].Contains('\n'))
                        values[i] = values[i].Remove(0, 1);
                    values[i].Replace(',', '.');
                }

                var col = columns.Split('+');

                regressionDictionary.Add("const", Convert.ToDouble(values[0], CultureInfo.InvariantCulture));

                for (int i = 0; i < marker.Count(); i++)
                {
                    var cName = columnRStringDictionary.ElementAt(i).Key;
                    if (marker[i].Contains("TRUE") || i == regrColumnPostion-1||(staticVarList.Contains(cName)&&optimVarList.Contains(columnName)))
                        regressionDictionary.Add(cName, 0.0);
                    else
                    {
                        try
                        {
                            regressionDictionary.Add(cName, Convert.ToDouble(values[i+1], CultureInfo.InvariantCulture));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
                return regressionDictionary;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Exeption: " + ex.Message);
            }
            MessageBox.Show("Success!");

            return null;
        }


        #endregion



        #region Buttons
        private void GetButton_Click(object sender, RoutedEventArgs e)
        {
            dataObjects = GetFromExcel(FilePath.Text);
            varList = GetVariableList(dataObjects);
            VariablesList.ItemsSource = varList;
            var table = ExcelArrayToDataTable(dataObjects, true);
            OuterDataGrid.ItemsSource = table.DefaultView;
        }

        private void ChooseOptimButton_Click(object sender, RoutedEventArgs e)
        {
            if (VariablesList.SelectedItems.Count > 1)
                MessageBox.Show("Нельзя добавить больше одной критериальной переменной!", "Ошибка!");
            else
            {
                foreach (var item in VariablesList.SelectedItems)
                {
                    if (!controlVarList.Contains(item) &&
                    !optimVarList.Contains(item) &&
                    !staticVarList.Contains(item) &&
                    !parameterList.Contains(item)&&
                    optimVarList.Count < 1)
                        optimVarList.Add(item);
                    else
                    {
                        MessageBox.Show("Выбранная переменная уже присутствует в другой группе!", "Ошибка!"); ;
                    }
                }
            }
        }

        private void ChooseControlButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in VariablesList.SelectedItems)
            {
                if (!controlVarList.Contains(item) &&
                    !optimVarList.Contains(item) &&
                    !staticVarList.Contains(item) &&
                    !parameterList.Contains(item))
                    controlVarList.Add(item);
                else
                {
                    MessageBox.Show("Выбранная переменная уже присутствует в другой группе!", "Ошибка!"); ;
                }
            }
        }

        private void ChooseStaticButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in VariablesList.SelectedItems)
            {
                if (!controlVarList.Contains(item) &&
                     !optimVarList.Contains(item) &&
                     !staticVarList.Contains(item) &&
                     !parameterList.Contains(item))
                    staticVarList.Add(item);
                else
                {
                    MessageBox.Show("Выбранная переменная уже присутствует в другой группе!", "Ошибка!");
                }
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.ShowDialog();
            fd.AddExtension = true;
            FilePath.Text = fd.FileName;
        }
        private void RegressionButton_Click(object sender, RoutedEventArgs e)
        {
            REngine engine = REngine.CreateInstance("RDotNet");
            engine.Initialize();
            var keyToRemove = optimVarList.ElementAt(0).ToString();
            var valueToremove = columnRStringDictionary[optimVarList.ElementAt(0).ToString()];
            Dictionary<string, Dictionary<string, double>> regressionList = new Dictionary<string, Dictionary<string, double>>();
            var val = CalculateStatisticData(columnRStringDictionary, optimVarList.ElementAt(0).ToString(), engine);

            regressionList.Add(optimVarList.ElementAt(0).ToString(), val);

            var tempRStringDicitonary = columnRStringDictionary;

            foreach (var param in parameterList.Union(optimVarList))
            {
                tempRStringDicitonary.Remove(param.ToString());
            }

            foreach (var column in staticVarList)
            {
                var sval = CalculateStatisticData(columnRStringDictionary, column.ToString(), engine);

                regressionList.Add(column.ToString(), sval);
            }

            columnRStringDictionary.Add(keyToRemove, valueToremove);

            Connector.regressionDictionary = regressionList;
            Connector.MinMaxDictionary = CalculateMinMax(staticVarList.Union(controlVarList).Union(optimVarList));
            engine.Dispose();

            NavigationService.Navigate(new Uri("Page2.xaml", UriKind.Relative));
        }
        private void ChoiseButton_Click(object sender, RoutedEventArgs e)
        {
            var choosenColumns = staticVarList.Union(controlVarList).Union(parameterList).Union(optimVarList);
            FillColumnDictionary(choosenColumns, out columnRStringDictionary);

            Connector.staticVars = staticVarList;
            Connector.optimVarList = optimVarList;
            Connector.controlVarList = controlVarList;
            Connector.parameterList = parameterList;
        }

        private void ParameterButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in VariablesList.SelectedItems)
            {
                if (!controlVarList.Contains(item) &&
                    !optimVarList.Contains(item) &&
                    !staticVarList.Contains(item)&&
                    !parameterList.Contains(item))
                    parameterList.Add(item);
                else
                {
                    MessageBox.Show("Выбранная переменная уже присутствует в другой группе!", "Ошибка!");
                }
            }
        }
        #endregion

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            controlVarList.Clear();
            staticVarList.Clear();
            optimVarList.Clear();
            parameterList.Clear();
        }
    }
}
