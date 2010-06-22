using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;
using System.IO;

namespace LeagueMaster
{
    public class Position
    {
        public struct positionType
        {
            int _x, _y;

            public int x
            {
                get { return _x; }
            }

            public int y
            {
                get { return _y; }
            }

            public positionType(int x, int y)
            {
                this._x = x;
                this._y = y;
            }

            public override string ToString()
            {
                return (String.Format("({0},{1})", _x, _y));
            }
        }

        private string _resolution;
        public string resolution
        {
            get
            {
                return _resolution;
            }
        }

        XmlDocument resolutionDoc;

        public Position()
        {
            _resolution = ConfigurationManager.AppSettings["resolution"].ToString();
            //store positions from resolution

            resolutionDoc = new XmlDocument();
            try 
	        {
                resolutionDoc.Load("config\\" + _resolution + ".xml");
	        }
	        catch (FileNotFoundException)
	        {
                Base.Write("Error: Resolution File Missing ( " + "config\\" + _resolution + ".xml )", ConsoleColor.Red);
		        throw;
	        }
        }

        public positionType Get(string name)
        {
            string xPath = @"/positions/position[@id='" + name + @"']";
            XmlNode node = resolutionDoc.SelectSingleNode(xPath);

            int x = System.Convert.ToInt32(node.ChildNodes[0].InnerText, 10);
            int y = System.Convert.ToInt32(node.ChildNodes[1].InnerText, 10);


#if DEBUG
            Console.WriteLine(node.InnerXml);
#endif
            return new positionType(x, y);
        }
    }
}
