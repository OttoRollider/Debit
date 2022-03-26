using Debit.Helper;
using Debit.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Debit.DB
{
    public class HelperDb
    {
        List<string> listDb = null;

        Form update = new InteractionCtrls().ProgressBarInitialization((System.Drawing.Image)Resources.update);

        /// <summary>
        /// Метод считывания 
        /// </summary>
        /// <param name="path"></param>
        public async void db_Import(string[] path)
        {
            update.TopMost = true;
            update.StartPosition = FormStartPosition.CenterScreen;
            update.Show();
            foreach (string s_path in path)
            {
                listDb = new List<string>();

                List<string> list = File.ReadAllLines(s_path).ToList(); //Считываем путь до файла

                foreach (string line in list) //Пройдёмся по строкам в файле
                {
                    Match match = Regex.Match(line.ToString(), "^[0-9]+.*"); //Регулярное выражение для отсвеивания ненужных строк
                    if (match.Success)
                        listDb.AddRange(list.Where(item => item == line)); //Добавляем в Лист строки которые потом разберём и отправим в БД
                }

                foreach (string s_list in listDb) //Разъеденим строки сепаратор |
                {
                    using (DbConnector db = new DbConnector())
                    {
                        StructDb structDb = new StructDb();

                        for (int i = 0; i < s_list.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Length; i++) //Узнаем сколько разделителей и засунем в цикл
                            structDb.GetType().GetProperty(InteractionCtrls.GetStructDBProperty()[i].Name).SetValue(structDb, $"{s_list.Split('|')[i]}");

                        db.money_debit.Add(structDb);
                        db.SaveChanges();

                        await Task.Delay(1);
                    }
                }
            }
            update.Close();
        }
    }
}
