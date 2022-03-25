using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Debit.DB
{
    [Serializable]
    public class StructDb : INotifyPropertyChanged
    {
        [Key]
        public string? dep_code { get; set; }
        public string? dep_code2 { get; set; }
        public string? dep_code3 { get; set; }
        public string? dep_code4 { get; set; }
        public string? start_year_full { get; set; }
        public string? start_year_long_term { get; set; }
        public string? start_year_overdue { get; set; }
        public string? increase_full { get; set; }
        public string? increase_nonmoney { get; set; }
        public string? decrease_full { get; set; }
        public string? decrease_nonmoney { get; set; }
        public string? end_report_period_full { get; set; }
        public string? end_report_period_long_term { get; set; }
        public string? end_report_period_overdue { get; set; }
        public string? end_previous_period_full { get; set; }
        public string? end_previous_period_long_term { get; set; }
        public string? end_previous_period_overdue { get; set; }

        [NotMapped]
        public string fdep_code
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(dep_code))
                    return $"{dep_code} {dep_code2} {dep_code3} {dep_code4}";
                else return "NO_DATA";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
