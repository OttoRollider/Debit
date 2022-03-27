using Debit.DB;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Debit.Helper
{
    /// <summary>
    /// Логика взаимодействия для WndFilter.xaml
    /// </summary>
    public partial class WndFilter : Window
    {
        List<StructDb> structDbs = new List<StructDb>();
        public WndFilter(List<StructDb> DepCodes)
        {
            InitializeComponent();
            GrDep1.Header = DepCodes[0].fdep_code;
            GrDep2.Header = DepCodes[1].fdep_code;
            structDbs = DepCodes;
            this.WindowStyle = WindowStyle.ToolWindow;
        }

        private void SelectFilter(object sender, SelectionChangedEventArgs e)
        {
            var PropName = DbWriter.GetStructDBProperty()[cboxTableDiff.SelectedIndex].Name;
            tbDep1.Text = structDbs[0].GetType().GetProperty(PropName).GetValue(structDbs[0]).ToString();
            tbDep2.Text = structDbs[1].GetType().GetProperty(PropName).GetValue(structDbs[1]).ToString();

            CheckDiff(tbDep1.Text, tbDep2.Text);
        }

        void CheckDiff(string dep1_, string dep2_)
        {
            var better = new SolidColorBrush(Color.FromArgb(255, 43, 155, 28)); //green
            var less = new SolidColorBrush(Color.FromArgb(255, 153, 7, 22)); //red
            var @default = new SolidColorBrush(Color.FromArgb(255, 0, 107, 153)); //blue

            try
            {
                var dep1 = Convert.ToDecimal(dep1_);
                var dep2 = Convert.ToDecimal(dep2_);

                if (dep1 > dep2) { lblDifference.Content = ">"; GrDep1.BorderBrush = better; GrDep2.BorderBrush = less; }
                else if (dep1 < dep2) { lblDifference.Content = "<"; GrDep2.BorderBrush = better; GrDep1.BorderBrush = less; }
                else { lblDifference.Content = "=="; GrDep1.BorderBrush = @default; GrDep2.BorderBrush = @default; }
            }
            catch { }
        }
    }
}
