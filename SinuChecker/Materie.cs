using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SinuChecker
{
    public class Materie
    {
        private string _name;
        private string _date;
        private string _nota;

        public string Nota
        {
            get { return _nota; }
            set { _nota = value; }
        }

        public string Date
        {
            get { return _date; }
            set { _date = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Materie(string name, string date, string nota)
        {
            _name = name;
            _date = date;
            _nota = nota;
        }

        public static List<Materie> ParseHtml(string text)
        {
            List<Materie> materii = new List<Materie>();
            XmlDocument document = new XmlDocument();
            XmlNodeList node;
            MemoryStream stream = new MemoryStream();
            byte[] buffer = Encoding.UTF8.GetBytes(text);

            stream.Write(buffer, 0, buffer.Length);
            document.Load(stream);

            document.GetElementsByTagName("tr");

            return materii;
        }
    }
}
