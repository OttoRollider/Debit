using Debit.DB;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Drawing.Image;

namespace Debit.Helper
{
    public class InteractionCtrls
    {
        /// <summary>
        /// Поиск блоков в контейнере
        /// </summary>
        /// <typeparam name="T">Обобщённый тип для выбора контрола</typeparam>
        /// <param name="depObj">Контейнер поиска</param>
        /// <returns></returns>
        public static IEnumerable<T> FindVisualChilds<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield return (T)Enumerable.Empty<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject ithChild = VisualTreeHelper.GetChild(depObj, i);
                if (ithChild == null) continue;
                if (ithChild is T t) yield return t;
                foreach (T childOfChild in FindVisualChilds<T>(ithChild)) yield return childOfChild;
            }
        }

        /// <summary>
        /// Поиск контролов в контейнере
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="depObj"></param>
        /// <returns></returns>
        public static IEnumerable<T> FindLogicalChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                foreach (object rawChild in LogicalTreeHelper.GetChildren(depObj))
                {
                    if (rawChild is DependencyObject)
                    {
                        DependencyObject child = (DependencyObject)rawChild;
                        if (child is T)
                        {
                            yield return (T)child;
                        }

                        foreach (T childOfChild in FindLogicalChildren<T>(child))
                        {
                            yield return childOfChild;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Метод преобразования картинки в массив байт
        /// </summary>
        /// <param name="imageIn"></param>
        /// <returns></returns>
        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }

        /// <summary>
        /// Метод преобразования массива байт в картинкиу 
        /// </summary>
        /// <param name="byteArrayIn"></param>
        /// <returns></returns>
        public static Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

        /// <summary>
        /// Конвертер из выбранного формата в BitMap. Метод перегрузки
        /// </summary>
        /// <param name="img">Исходное изображение</param>
        /// <param name="fic">Выбор формата</param>
        /// <returns></returns>
        public static BitmapImage Convert(Image img, FormatImageConverter fic)
        {
            ImageFormat IF = ImageFormat.Png;

            switch (fic)
            {
                case FormatImageConverter.PNG:
                    IF = ImageFormat.Png;
                    break;
                case FormatImageConverter.JPEG:
                    IF = ImageFormat.Jpeg;
                    break;
                case FormatImageConverter.BMP:
                    IF = ImageFormat.Bmp;
                    break;
                case FormatImageConverter.GIF:
                    IF = ImageFormat.Gif;
                    break;
                case FormatImageConverter.TIFF:
                    IF = ImageFormat.Tiff;
                    break;
            }
            using (var memory = new MemoryStream())
            {
                img.Save(memory, IF);
                memory.Position = 0;

                var bImage = new BitmapImage();
                bImage.BeginInit();
                bImage.StreamSource = memory;

                bImage.CacheOption = BitmapCacheOption.OnLoad;
                bImage.DecodePixelHeight = 32;
                bImage.DecodePixelWidth = 32;
                bImage.EndInit();
                return bImage;
            }
        }


        /// <summary>
        /// Список свойств класса StructDb
        /// </summary>
        /// <returns></returns>
        public static List<PropertyInfo> get_PropertyStructDb()
        {
            List<PropertyInfo> properties = new List<PropertyInfo>();
            PropertyInfo[] myPropertyInfo;
            Type myType = typeof(StructDb);

            myPropertyInfo = myType.GetProperties();

            for (int i = 0; i < myPropertyInfo.Length; i++)
                properties.Add(myPropertyInfo[i]);

            return properties;
        }

        /// <summary>
        /// Форма отображающая процесс фонового выполнения
        /// </summary>
        /// <param name="imgLoading"></param>
        /// <returns></returns>
        public Form FormUpdate(Image imgLoading)
        {
            var backColor = System.Drawing.Color.White;
            Form f1 = new();

            f1.Size = new System.Drawing.Size(200, 200);
            f1.FormBorderStyle = FormBorderStyle.None;
            f1.BackgroundImage = imgLoading;
            f1.BackColor = backColor;
            f1.TransparencyKey = backColor;

            return f1;
        }
    }

    public class ViewCtrls
    {
        public static MainWindow mainWindow { get; set; }

        /// <summary>
        /// Метод добавления данных в коллекцию Obsrver
        /// </summary>
        /// <param name="list"></param>
        public void AddToObserverCollection(List<StructDb> list)
        {
            foreach (var l in list)
                mainWindow.ocStructDb.Add(l);

            mainWindow.dbListView.ItemsSource = mainWindow.ocStructDb;
        }
    }
}
