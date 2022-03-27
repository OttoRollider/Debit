using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Debit.DB
{
    public class DbWriter
    {
        private List<string> _dataForDb = null;

        /// <summary>
        /// Метод считывания строк из txt файла
        /// </summary>
        /// <param name="paths"></param>
        //TODO: Подумать, как вынести запись в прогресс бар и лейбл в отдельный класс или метод
        public async void ReadingTxt(string[] paths, ProgressBar progressBar, Label label)
        {
            foreach (string path in paths)
            {
                _dataForDb = new List<string>();

                List<string> fileAllLines = File.ReadAllLines(path).ToList(); // Считываем все строки из файла и записываем их в список.

                progressBar.Maximum = fileAllLines.Count;
                label.Content = "Считывание файла:";

                foreach (string line in fileAllLines) // Пройдёмся по строкам в файле
                {
                    Match match = Regex.Match(line.ToString(), "^[0-9]+.*"); // Регулярное выражение для отсвеивания ненужных строк
                    progressBar.Value++;

                    if (match.Success)
                        _dataForDb.AddRange(fileAllLines.Where(item => item == line)); // Добавляем в _dataForDb строки которые потом разберём и отправим в БД
                    
                    await Task.Delay(1);
                }

                progressBar.Value = 0;
                progressBar.Maximum = _dataForDb.Count;
                label.Content = "Импорт данных в БД:";

                foreach (string line in _dataForDb) // Разъеденим строки сепаратор |
                {
                    progressBar.Value++;
                    using (DbConnector dbConnector = new DbConnector())
                    {
                        StructDb structDb = new StructDb();

                        for (int i = 0; i < line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Length; i++) //Узнаем сколько разделителей и засунем в цикл
                            structDb.GetType().GetProperty(GetStructDBProperty()[i].Name).SetValue(structDb, $"{line.Split('|')[i]}");

                        dbConnector.money_debit.Add(structDb);
                        dbConnector.SaveChanges();

                        await Task.Delay(1);
                    }
                }
            }
            progressBar.Value = 0;
            label.Content = "Данные загружены:";
        }

        /// <summary>
        /// Получаем список свойств класса StructDb
        /// </summary>
        /// <returns></returns>
        public static List<PropertyInfo> GetStructDBProperty()
        {
            List<PropertyInfo> properties = new List<PropertyInfo>();
            PropertyInfo[] propertyInfo;
            Type type = typeof(StructDb);

            propertyInfo = type.GetProperties(); // Получаем все свойства типа (класса) StructDb в виде массива propertyInfo.

            for (int i = 0; i < propertyInfo.Length; i++)
                properties.Add(propertyInfo[i]);

            return properties;
        }
    }
}
