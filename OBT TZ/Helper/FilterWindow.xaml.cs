using Debit.DB;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Debit.Helper
{
    /// <summary>
    /// Логика взаимодействия для FilterWindow.xaml
    /// </summary>
    public partial class FilterWindow : Window
    {
        private List<StructDb> _structDbs = new List<StructDb>();
        public FilterWindow(List<StructDb> DepCodes)
        {
            InitializeComponent();
            GrDep1.Header = DepCodes[0].fdep_code;
            GrDep2.Header = DepCodes[1].fdep_code;
            _structDbs = DepCodes;
            this.WindowStyle = WindowStyle.ToolWindow;
        }

        private void SelectFilter(object sender, SelectionChangedEventArgs e)
        {
            int IndexFirstSummProperty = 4; // Пропускаем первые 4 свойства, в которых хранятся данные об учреждении
            var propertyName = DbWriter.GetStructDBProperty()[cboxTableDiff.SelectedIndex + IndexFirstSummProperty].Name;

            tbDep1.Text = _structDbs[0].GetType().GetProperty(propertyName).GetValue(_structDbs[0]).ToString();
            tbDep2.Text = _structDbs[1].GetType().GetProperty(propertyName).GetValue(_structDbs[1]).ToString();

            ColumnsComparison(tbDep1.Text, tbDep2.Text);
        }

        private void ColumnsComparison(string firstColumn, string secondColumn)
        {
            var better = new SolidColorBrush(Color.FromArgb(255, 43, 155, 28)); //green
            var less = new SolidColorBrush(Color.FromArgb(255, 153, 7, 22)); //red
            var equal = new SolidColorBrush(Color.FromArgb(255, 0, 107, 153)); //blue

            var firstValue = Convert.ToDecimal(firstColumn);
            var secondValue = Convert.ToDecimal(secondColumn);

            if (firstValue > secondValue)
                ChangeGroupBoxBorderColor(">", better, less);
            else if (firstValue < secondValue)
                ChangeGroupBoxBorderColor("<", less, better);
            else
                ChangeGroupBoxBorderColor("==", equal, equal);
        }

        private void ChangeGroupBoxBorderColor(string comparisonSign, SolidColorBrush firstGrBorderColor, SolidColorBrush secondGrBorderColor)
        {
            lblDifference.Content = comparisonSign;
            GrDep1.BorderBrush = firstGrBorderColor;
            GrDep2.BorderBrush = secondGrBorderColor;
        }
    }
}
