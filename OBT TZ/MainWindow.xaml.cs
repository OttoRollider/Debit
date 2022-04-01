using Debit.DB;
using Debit.Helper;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Application = Microsoft.Office.Interop.Excel.Application;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Range = Microsoft.Office.Interop.Excel.Range;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using TextBox = System.Windows.Controls.TextBox;
using Window = System.Windows.Window;

namespace Debit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CollectionView _collectionView = null;
        private ListViewContent _listViewContent = new ListViewContent();
        private string _pathToSaveExcel = string.Empty;
        private Application _application;
        private Workbook _workBook;
        private Worksheet _worksheet;

        public readonly ObservableCollection<StructDb> DbDataCollection = new ObservableCollection<StructDb>();

        public MainWindow()
        {
            DataContext = this;

            InitializeComponent();
            Loaded += (s, a) => {
                ListViewContent.mainWindow = this;
            };

            tbSearch.GotFocus += (s, a) => tbSearch.Text = tbSearch.Text == "Динамический поиск" ? "" : tbSearch.Text;
            tbSearch.LostFocus += (s, a) => tbSearch.Text = string.IsNullOrWhiteSpace(tbSearch.Text) ? "Динамический поиск" : tbSearch.Text;
            tbSearch.Loaded += (s, a) => tbSearch.IsEnabled = DbDataCollection.Count > 0 ? true : false;

            tbSearch.TextChanged += (s, a) => CollectionViewSource.GetDefaultView(dbListView.ItemsSource).Refresh();

            dbListView.MouseRightButtonUp += ContextMenu;
        }

        //TODO:Метод ничего не сортирует. Доработать.
        private void SortHeaderClick(object sender, RoutedEventArgs e)
        {
            var position = PointToScreen(Mouse.GetPosition(this));
            var column = ((GridViewColumnHeader)e.OriginalSource).Column.Header.ToString();

        }

        private void ContextMenu(object sender, MouseButtonEventArgs e)
        {
            var focusItem = e.OriginalSource as StructDb;

            int countSelected = dbListView.SelectedItems.Count;

            try
            {
                //Добавляем контекстное меню
                MenuItem showComparison = new MenuItem();
                showComparison.Header = "Сравнить данные";
                showComparison.Click += (@sender, @event) =>
                {
                    List<StructDb> list = new List<StructDb>();
                    var items = dbListView.SelectedItems;

                    foreach (var item in items)
                        list.Add((StructDb)item);

                    var position = PointToScreen(Mouse.GetPosition(this));
                    FilterWindow filterWindow = new FilterWindow(list);
                    filterWindow.Left = position.X;
                    filterWindow.Top = position.Y;
                    filterWindow.ShowDialog();
                };
                showComparison.IsEnabled = countSelected == 2 ? true : false;

                MenuItem ShowDel = new MenuItem();
                ShowDel.Header = "Удалить";
                ShowDel.Click += (@sender, @event) => RemoveData(@sender, @event);

                ContextMenu cm = new ContextMenu();
                cm.Items.Add(showComparison);
                cm.Items.Add(ShowDel);
                cm.IsOpen = true;
            }
            catch { }
        }

        private bool DataBaseFilter(object item)
        {
            if (String.IsNullOrEmpty(tbSearch.Text) || tbSearch.Text == "Динамический поиск")
                return true;
            else
                return ((item as StructDb).fdep_code.IndexOf(tbSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void AddData(object sender, RoutedEventArgs e)
        {
            using (DbConnector dbConnector = new DbConnector())
            {
                // создаем объект вручную
                StructDb structDb = new StructDb
                {
                    #region Новый экземпляр класса занесённый руками
                    dep_code = tb_dep_code.Text,
                    dep_code2 = tb_dep_code2.Text,
                    dep_code3 = tb_dep_code3.Text,
                    dep_code4 = tb_dep_code4.Text,
                    start_year_full = tb_start_year_full.Text,
                    start_year_long_term = tb_start_year_long_term.Text,
                    start_year_overdue = tb_start_year_overdue.Text,
                    increase_full = tb_increase_full.Text,
                    increase_nonmoney = tb_increase_nonmoney.Text,
                    decrease_full = tb_decrease_full.Text,
                    decrease_nonmoney = tb_decrease_nonmoney.Text,
                    end_report_period_full = tb_end_report_period_full.Text,
                    end_report_period_long_term = tb_end_report_period_long_term.Text,
                    end_report_period_overdue = tb_end_report_period_overdue.Text,
                    end_previous_period_full = tb_end_previous_period_full.Text,
                    end_previous_period_long_term = tb_end_previous_period_long_term.Text,
                    end_previous_period_overdue = tb_end_previous_period_overdue.Text,
                    #endregion
                };

                // добавляем бд
                dbConnector.money_debit.Add(structDb);
                dbConnector.SaveChanges();

                foreach (var textBox in InteractionWithView.FindTextBoxes<TextBox>(this))
                {
                    textBox.Clear();
                }
            }
        }

        private void UpdateData(object sender, RoutedEventArgs e)
        {
            DbDataCollection.Clear();
            using (DbConnector dbConnector = new DbConnector())
            {
                // получаем объекты из бд и выводим в ListView
                var debit = dbConnector.money_debit.ToList();
                _listViewContent.AddDataToObservableCollection(debit);
                _listViewContent.AddDataToListView();
            }
            //TODO: Разобраться с CollectionViewSource и CollectionView. Может стоит создать сразу экземпляр класса CollectionViewSource, а не CollectionView
            _collectionView = (CollectionView)CollectionViewSource.GetDefaultView(DbDataCollection);
            _collectionView.Filter = DataBaseFilter;

            tbSearch.IsEnabled = true;

            lbRowCount.Content = $"Загруженных договоров: {DbDataCollection.Count}";
        }

        private void ImportTxt(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog().Value == true)
                new DbWriter().ReadingTxt(openFileDialog.FileNames, pbReadTxt, lbProgressReadTxt);
        }
        //TODO: При удалении одной добавленной записи из нескольких удаляются все. Добавленные записи одинаковые. Разобраться.
        private void RemoveData(object sender, RoutedEventArgs e)
        {
            using (DbConnector dbConnector = new DbConnector())
            {
                var selectedLines = dbListView.SelectedItems;
                foreach (var line in selectedLines)
                    dbConnector.money_debit.RemoveRange((StructDb)line);
                dbConnector.SaveChanges();
            }
            UpdateData(sender, e);
        }

        private void OnTextChange(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).Text = (sender as TextBox)?.Text.Replace('.', ',');
            (sender as TextBox).CaretIndex = (sender as TextBox).Text.Length;
        }

        private void ExportXML(object sender, RoutedEventArgs e)
        {
            string path = string.Empty;
            if (DbDataCollection.Count < 1) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog() { Filter = "xml table(*.xml)|*.xml|All files (*.*)|*.*" };
            if (saveFileDialog.ShowDialog().HasValue)
                path = saveFileDialog.FileName;
            else return;

            if (string.IsNullOrWhiteSpace(path)) return;

            XmlCreator xmlCreator = new XmlCreator();
            xmlCreator.CreateXml(path);

            foreach (var item in dbListView.Items)
                xmlCreator.AddXmlData((StructDb)item, path);

            MessageBoxResult result = MessageBox.Show("Экспорт XML выполнен!\rОткрыть файл?", "Выполнено", MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
                Process.Start("notepad.exe", path);

        }

        private void ExportExcel(object sender, RoutedEventArgs e)
        {

            SaveFileDialog saveFileDialog = new SaveFileDialog() { Filter = "Excel table(*.xlsx)|*.xlsx|All files (*.*)|*.*" };
            if (saveFileDialog.ShowDialog().HasValue)
                _pathToSaveExcel = saveFileDialog.FileName;
            else return;

            if (string.IsNullOrWhiteSpace(_pathToSaveExcel)) return;

            const string template = "template.xlsx";

            using (MemoryStream fileOut = new MemoryStream(Properties.Resources.template))
            using (FileStream fileSave = File.Create(template))
            using (System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(fileOut, System.IO.Compression.CompressionMode.Decompress))
                gz.CopyTo(fileSave);

            _application = new Application
            {
                Visible = false,
                DisplayAlerts = false
            };

            _workBook = _application.Workbooks.Open(Path.Combine(Environment.CurrentDirectory, template));
            _worksheet = _workBook.ActiveSheet as Worksheet;

            int index = 7;

            foreach (var item in dbListView.Items)
            {
                Range line = (Range)_worksheet.Rows[index];
                line.Insert();

                _worksheet.Range[$"A{index}"].Value = (item as StructDb).fdep_code;
                _worksheet.Range[$"B{index}"].Value = (item as StructDb).start_year_full;
                _worksheet.Range[$"C{index}"].Value = (item as StructDb).start_year_long_term;
                _worksheet.Range[$"D{index}"].Value = (item as StructDb).start_year_overdue;
                _worksheet.Range[$"E{index}"].Value = (item as StructDb).increase_full;
                _worksheet.Range[$"F{index}"].Value = (item as StructDb).increase_nonmoney;
                _worksheet.Range[$"G{index}"].Value = (item as StructDb).decrease_full;
                _worksheet.Range[$"H{index}"].Value = (item as StructDb).decrease_nonmoney;
                _worksheet.Range[$"I{index}"].Value = (item as StructDb).end_report_period_full;
                _worksheet.Range[$"J{index}"].Value = (item as StructDb).end_report_period_long_term;
                _worksheet.Range[$"K{index}"].Value = (item as StructDb).end_report_period_overdue;
                _worksheet.Range[$"L{index}"].Value = (item as StructDb).end_previous_period_full;
                _worksheet.Range[$"M{index}"].Value = (item as StructDb).end_previous_period_long_term;
                _worksheet.Range[$"N{index}"].Value = (item as StructDb).end_previous_period_overdue;

                index++;
            }

            saveExcel();
            Topmost = true;
        }

        private void saveExcel()
        {
            string savedFileName = _pathToSaveExcel;
            _workBook.SaveAs(Path.Combine(Environment.CurrentDirectory, savedFileName));
            CloseExcel();
        }

        /// <summary>
        /// Закрытие 
        /// </summary>
        private void CloseExcel()
        {
            if (_application != null)
            {
                int excelProcessId = -1;
                GetWindowThreadProcessId(_application.Hwnd, ref excelProcessId);

                Marshal.ReleaseComObject(_worksheet);
                _workBook.Close();
                Marshal.ReleaseComObject(_workBook);
                _application.Quit();
                Marshal.ReleaseComObject(_application);

                _application = null;
                // Прибиваем висящий процесс
                try
                {
                    Process process = Process.GetProcessById(excelProcessId);
                    process.Kill();
                }
                finally
                {
                    System.Windows.Forms.MessageBox.Show("Экспорт XLSX таблицы выполнен!");
                }
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(int hWnd, ref int lpdwProcessId);
    }
}
