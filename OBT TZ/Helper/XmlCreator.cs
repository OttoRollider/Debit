using Debit.DB;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Debit.Helper
{
    public class XmlCreator
    {
        /// <summary>
        /// Создание XML
        /// </summary>
        /// <param name="path"></param>
        public void CreateXml(string path)
        {
            XDocument xDocument = new XDocument(new XElement("RootXml",
                         new XElement("Report", new XAttribute("Code", "042"), new XAttribute("AlbumCode", "МЕС_К"),
                             new XElement("FormVariant", new XAttribute("Number", "1"), new XAttribute("NsiVariantCode", "0000"),
                             new XElement("Table", new XAttribute("Code", "Строка"))))));
            xDocument.Save(path);
        }

        /// <summary>
        /// Добавление табличных данных в XML
        /// </summary>
        /// <param name="structDb"></param>
        /// <param name="path"></param>
        public async void AddXmlData(StructDb structDb, string path)
        {
            XDocument xDocument = XDocument.Load(path);
            int columnNumber = 2;


            XElement root = xDocument.Root.Element("Report").Element("FormVariant").Element("Table");

            root.Add(new XElement("Data"));

            root = xDocument.Descendants("Data").Last();

            root.Add(new[] { new XAttribute("СинтСчёт", structDb.dep_code3), new XAttribute("КОСГУ", structDb.dep_code4) });

            var properties = structDb.GetType().GetProperties().
                Where(
                prop => prop.Name != "dep_code"
                    & prop.Name != "dep_code2"
                    & prop.Name != "dep_code3"
                    & prop.Name != "dep_code4"
                    & prop.Name != "fdep_code");

            foreach (var property in properties)
            {
                if (property.GetValue(structDb).ToString() != "0" & property.GetValue(structDb).ToString() != "-")
                {
                    root.Add(new[] { new XAttribute($"_x{columnNumber}", property.GetValue(structDb).ToString()) });
                }
                columnNumber++;
            }

            xDocument.Save(path);
            await Task.Delay(50);
        }
    }
}
