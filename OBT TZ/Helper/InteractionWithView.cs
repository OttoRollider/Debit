using System.Collections.Generic;
using System.Windows;

namespace Debit.Helper
{
    //TODO: Подумать над названием этого класса. Возможно, придётся даже разбить этот класс на несколько.
    public class InteractionWithView
    {
        /// <summary>
        /// Поиск контролов в контейнере
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mainWindow"></param>
        /// <returns></returns>
        public static IEnumerable<T> FindTextBoxes<T>(DependencyObject mainWindow) where T : DependencyObject
        {
            foreach (object chieldObject in LogicalTreeHelper.GetChildren(mainWindow))
            {
                if (chieldObject is T)
                    yield return (T)chieldObject;
            }
        }
    }
}
