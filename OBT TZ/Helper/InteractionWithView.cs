using System.Collections.Generic;
using System.Windows;

namespace Debit.Helper
{
    public class InteractionWithView
    {
        /// <summary>
        /// Finding text boxes in main window
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rootElement"></param>
        /// <returns></returns>
        public static IEnumerable<T> FindTextBoxes<T>(DependencyObject rootElement) where T : DependencyObject
        {
            foreach (object childElement in LogicalTreeHelper.GetChildren(rootElement))
            {
                if (childElement is DependencyObject) // if childObject have a child objects...
                {
                    DependencyObject child = (DependencyObject)childElement; // ...then is DependencyObject

                    if (child is T)
                        yield return (T)childElement;

                    foreach (var cildOfChild in FindTextBoxes<T>(child)) // Passing through lower-level child elements
                        yield return cildOfChild;
                }
            }
        }
    }
}
