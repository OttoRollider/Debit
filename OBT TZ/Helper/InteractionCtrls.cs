using Debit.DB;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using Image = System.Drawing.Image;

namespace Debit.Helper
{
    //TODO: Подумать над названием этого класса. Возможно, придётся даже разбить этот класс на несколько.
    public class InteractionCtrls
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

        /// <summary>
        /// Форма отображающая процесс фонового выполнения
        /// </summary>
        /// <param name="progressBarImage"></param>
        /// <returns></returns>
        public Form ProgressBarInitialization(Image progressBarImage)
        {
            var backColor = System.Drawing.Color.White;
            Form progressBarForm = new Form();

            //TODO: При реализации гифки, как фонового изображения, она перестаёт работать и остаётся статичной. Пофиксить.
            progressBarForm.Size = new System.Drawing.Size(200, 200);
            progressBarForm.FormBorderStyle = FormBorderStyle.None;
            progressBarForm.BackgroundImage = progressBarImage;
            progressBarForm.BackColor = backColor;
            progressBarForm.TransparencyKey = backColor;

            return progressBarForm;
        }
    }
}
