﻿using Debit.DB;
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
using System.Windows.Forms;
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
        public readonly ObservableCollection<StructDb> ocStructDb = new ObservableCollection<StructDb>();
        ViewCtrls viewCtrls = new();
        internal CollectionView view = null;

        private Application application;
        private Workbook workBook;
        private Worksheet worksheet;
        string pathExcel = string.Empty;

        public MainWindow()
        {
            DataContext = this;

            InitializeComponent();
            Loaded += (s, a) => {
                ViewCtrls.mainWindow = this;
                LoadImg();
            };

            tbSearch.GotFocus += (s, a) => tbSearch.Text = tbSearch.Text == "Динамический поиск" ? "" : tbSearch.Text;
            tbSearch.LostFocus += (s, a) => tbSearch.Text = string.IsNullOrWhiteSpace(tbSearch.Text) ? "Динамический поиск" : tbSearch.Text;
            tbSearch.Loaded += (s, a) => tbSearch.IsEnabled = ocStructDb.Count > 0 ? true : false;

            tbSearch.TextChanged += (s, a) => CollectionViewSource.GetDefaultView(dbListView.ItemsSource).Refresh();

            dbListView.MouseRightButtonUp += ContextMenu;

        }

        private void SortHeaderClick(object sender, RoutedEventArgs e)
        {
            var pos = PointToScreen(Mouse.GetPosition(this));
            var column = ((GridViewColumnHeader)e.OriginalSource).Column.Header.ToString();

        }

        private void ContextMenu(object sender, MouseButtonEventArgs e)
        {
            var FocusItem = e.OriginalSource as StructDb;

            int countSelected = dbListView.SelectedItems.Count;

            try
            {
                //Добавляем контекстное меню
                MenuItem ShowCompare = new MenuItem();
                ShowCompare.Header = $"Сравнить данные";
                ShowCompare.Click += (@sender, @event) =>
                {
                    List<StructDb> list = new List<StructDb>();
                    var items = dbListView.SelectedItems;
                    foreach (var item in items)
                        list.Add((StructDb)item);
                    var pos = PointToScreen(Mouse.GetPosition(this));
                    WndFilter wf = new WndFilter(list);
                    wf.Left = pos.X;
                    wf.Top = pos.Y;
                    wf.ShowDialog();
                };
                ShowCompare.IsEnabled = countSelected == 2 ? true : false;

                MenuItem ShowDel = new MenuItem();
                ShowDel.Header = "Удалить";
                ShowDel.Click += (@sender, @event) => RemoveData(@sender, @event);

                ContextMenu cm = new ContextMenu();
                cm.Items.Add(ShowCompare);
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
            using (DbConnector db = new DbConnector())
            {
                //// создаем объект вручную
                StructDb struct_hand = new StructDb
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
                db.money_debit.Add(struct_hand);
                db.SaveChanges();

                foreach (var textBox in InteractionCtrls.FindLogicalChildren<TextBox>(this))
                {
                    textBox.Clear();
                }
            }
        }

        private void ChangeData(object sender, RoutedEventArgs e)
        {
            ocStructDb.Clear();
            using (DbConnector db = new DbConnector())
            {
                // получаем объекты из бд и выводим в ListView
                var debit = db.money_debit.ToList();
                viewCtrls.AddToObserverCollection(debit);
            }

            view = (CollectionView)CollectionViewSource.GetDefaultView(ocStructDb);
            view.Filter = DataBaseFilter;

            tbSearch.IsEnabled = true;

            this.Title += $" Загруженных договоров {ocStructDb.Count}";
        }

        private void ImportTxt2Db(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
                Multiselect = true
            };

            if (ofd.ShowDialog().Value == true)
                new HelperDb().db_Import(ofd.FileNames);

        }

        private void RemoveData(object sender, RoutedEventArgs e)
        {
            using (DbConnector db = new DbConnector())
            {
                ocStructDb.Remove((StructDb)dbListView.SelectedItem);
                db.money_debit.Remove((StructDb)dbListView.SelectedItem);
                db.SaveChanges();
            }
        }

        private void LoadImg()
        {
            imgImport.Source = InteractionCtrls.Convert(InteractionCtrls.byteArrayToImage(Properties.Resources.file_extension_txt), FormatImageConverter.PNG);
            imgExportXML.Source = InteractionCtrls.Convert(InteractionCtrls.byteArrayToImage(Properties.Resources.xml_exports), FormatImageConverter.PNG);
            imgExportXLSX.Source = InteractionCtrls.Convert(InteractionCtrls.byteArrayToImage(Properties.Resources.file_extension_xls), FormatImageConverter.PNG);
            imgAddData.Source = InteractionCtrls.Convert(InteractionCtrls.byteArrayToImage(Properties.Resources.database_add), FormatImageConverter.PNG);
            imgChangeData.Source = InteractionCtrls.Convert(InteractionCtrls.byteArrayToImage(Properties.Resources.database_refresh), FormatImageConverter.PNG);
            imgDeleteData.Source = InteractionCtrls.Convert(InteractionCtrls.byteArrayToImage(Properties.Resources.database_delete), FormatImageConverter.PNG);
        }


        private void Any_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).Text = (sender as TextBox)?.Text.Replace('.', ',');
            (sender as TextBox).CaretIndex = (sender as TextBox).Text.Length;
        }

        private void ExportXML(object sender, RoutedEventArgs e)
        {
            string path = string.Empty;
            if (ocStructDb.Count < 1) return;

            SaveFileDialog sfd = new SaveFileDialog() { Filter = "xml table(*.xml)|*.xml|All files (*.*)|*.*" };
            if (sfd.ShowDialog().HasValue)
                path = sfd.FileName;
            else return;

            if (string.IsNullOrWhiteSpace(path)) return;

            XMLHelper xMLHelper = new XMLHelper();
            xMLHelper.CreateXmlData(path);

            foreach (var ocStruct in dbListView.Items)
                xMLHelper.AddXmlData((StructDb)ocStruct, path);

            MessageBoxResult result = MessageBox.Show("Экспорт XML выполнен!\rОткрыть файл?", "Выполнено", MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
                Process.Start("notepad.exe", path);

        }

        private void ExportExcel(object sender, RoutedEventArgs e)
        {

            SaveFileDialog sfd = new SaveFileDialog() { Filter = "Excel table(*.xlsx)|*.xlsx|All files (*.*)|*.*" };
            if (sfd.ShowDialog().HasValue)
                pathExcel = sfd.FileName;
            else return;

            if (string.IsNullOrWhiteSpace(pathExcel)) return;

            const string template = "template.xlsx";

            using (MemoryStream fileOut = new MemoryStream(Properties.Resources.template))
            using (FileStream fileSave = File.Create(template))
            using (System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(fileOut, System.IO.Compression.CompressionMode.Decompress))
                gz.CopyTo(fileSave);

            application = new Application
            {
                Visible = false,
                DisplayAlerts = false
            };

            workBook = application.Workbooks.Open(Path.Combine(Environment.CurrentDirectory, template));
            worksheet = workBook.ActiveSheet as Worksheet;


            int index = 7;

            foreach (var item in dbListView.Items)
            {
                Range line = (Range)worksheet.Rows[index];
                line.Insert();

                worksheet.Range[$"A{index}"].Value = (item as StructDb).fdep_code;
                worksheet.Range[$"B{index}"].Value = (item as StructDb).start_year_full;
                worksheet.Range[$"C{index}"].Value = (item as StructDb).start_year_long_term;
                worksheet.Range[$"D{index}"].Value = (item as StructDb).start_year_overdue;
                worksheet.Range[$"E{index}"].Value = (item as StructDb).increase_full;
                worksheet.Range[$"F{index}"].Value = (item as StructDb).increase_nonmoney;
                worksheet.Range[$"G{index}"].Value = (item as StructDb).decrease_full;
                worksheet.Range[$"H{index}"].Value = (item as StructDb).decrease_nonmoney;
                worksheet.Range[$"I{index}"].Value = (item as StructDb).end_report_period_full;
                worksheet.Range[$"J{index}"].Value = (item as StructDb).end_report_period_long_term;
                worksheet.Range[$"K{index}"].Value = (item as StructDb).end_report_period_overdue;
                worksheet.Range[$"L{index}"].Value = (item as StructDb).end_previous_period_full;
                worksheet.Range[$"M{index}"].Value = (item as StructDb).end_previous_period_long_term;
                worksheet.Range[$"N{index}"].Value = (item as StructDb).end_previous_period_overdue;


                index++;
            }

            saveExcel();
            Topmost = true;
        }

        void saveExcel()
        {
            string savedFileName = pathExcel;
            workBook.SaveAs(Path.Combine(Environment.CurrentDirectory, savedFileName));
            CloseExcel();
        }

        /// <summary>
        /// Закрытие 
        /// </summary>
        private void CloseExcel()
        {
            if (application != null)
            {


                int excelProcessId = -1;
                GetWindowThreadProcessId(application.Hwnd, ref excelProcessId);

                Marshal.ReleaseComObject(worksheet);
                workBook.Close();
                Marshal.ReleaseComObject(workBook);
                application.Quit();
                Marshal.ReleaseComObject(application);

                application = null;
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

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseExcel();
        }
    }


}