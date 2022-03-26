using Debit.DB;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Debit.Helper
{
    public class XMLHelper
    {
        private string _path = Path.Combine(Environment.CurrentDirectory, "export.xml");

        /// <summary>
        /// Создание XML
        /// </summary>
        /// <param name="path"></param>
        public void CreateXmlData(string path)
        {
            _path = path;
            XDocument xdoc = new XDocument(new XElement("RootXml",
                         new XElement("Report", new XAttribute("Code", "042"), new XAttribute("AlbumCode", "МЕС_К"),
                             new XElement("FormVariant", new XAttribute("Number", "1"), new XAttribute("NsiVariantCode", "0000"),
                             new XElement("Table", new XAttribute("Code", "Строка"))))));
            xdoc.Save(_path);
        }

        /// <summary>
        /// Добавление элементов  в XML
        /// </summary>
        /// <param name="structDb"></param>
        /// <param name="path"></param>
        public async void AddXmlData(StructDb structDb, string path)
        {
            _path = path;
            XDocument xdoc = XDocument.Load(_path);
            int index = 2;


            XElement root = xdoc.Element("RootXml").Element("Report").Element("FormVariant").Element("Table");

            root.Add(new XElement("Data"));

            root = xdoc.Descendants("Data").Last();

            xdoc.Descendants("Data").Last().Add(new[] { new XAttribute("СинтСчёт", structDb.dep_code3), new XAttribute("КОСГУ", structDb.dep_code4) });

            var prop_ = structDb.GetType().GetProperties().
                Where(
                prop => prop.Name != "dep_code"
                    & prop.Name != "dep_code2"
                    & prop.Name != "dep_code3"
                    & prop.Name != "dep_code4"
                    & prop.Name != "fdep_code");

            foreach (var prop in prop_)
            {
                if (prop.GetValue(structDb).ToString() != "0" & prop.GetValue(structDb).ToString() != "-")
                {
                    root.Add(new[] { new XAttribute($"_x{index}", prop.GetValue(structDb).ToString()) });
                }
                index++;
            }

            xdoc.Save(_path);
            await Task.Delay(50);
        }
    }
}
