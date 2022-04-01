using Debit.DB;
using System.Collections.Generic;

namespace Debit.Helper
{
    public class ListViewContent
    {
        public static MainWindow mainWindow { get; set; }

        /// <summary>
        /// Метод добавления данных в ObservableCollection
        /// </summary>
        /// <param name="dbData"></param>
        public void AddDataToObservableCollection(List<StructDb> dbData)
        {
            foreach (var line in dbData)
                mainWindow.DbDataCollection.Add(line);
        }

        /// <summary>
        /// Метод отображения данных в ListView
        /// </summary>
        /// <returns></returns>
        public void AddDataToListView()
        {
            mainWindow.dbListView.ItemsSource = mainWindow.DbDataCollection;
        }
    }
}
