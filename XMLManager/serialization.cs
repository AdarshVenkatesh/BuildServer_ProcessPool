////////////////////////////////////////////////////////////////////////////////////////
// serialization.cs : This package is used for serialization and deserialization.     //
// ver 1.0                                                                            //
//                                                                                    //
//Language:     Visual C#                                                             //
// Platform    : Lenovo 510S Ideapad, Win Pro 10, Visual Studio 2017                  //
// Application : CSE-681 SMA Project 4                                                //
// Author      : Dr. Jim Fawcett, EECS, SU                                            //
////////////////////////////////////////////////////////////////////////////////////////

/*
* Module Operations:
*===================
* This package serializes an instance of some type to an XML string
* and deserialize back to an instance of that type.
*
* public Interfaces:
* =================
* ToXml(): serializes an object into an XML.
* FromXml(): deserializes an XMl into an object
* 
* Required Files:
* ===============
* 
* Maintainance History:
* =====================
* ver 1.0
*
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace FederationServers
{
    public static class ToAndFromXml
    {
        //----< serialize object to XML >--------------------------------

        static public string ToXml(this object obj)
        {

            XmlSerializerNamespaces nmsp = new XmlSerializerNamespaces();
            nmsp.Add("", "");

            var sb = new StringBuilder();
            try
            {
                var serializer = new XmlSerializer(obj.GetType());
                using (StringWriter writer = new StringWriter(sb))
                {
                    serializer.Serialize(writer, obj, nmsp);
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n  exception thrown:");
                Console.Write("\n  {0}", ex.Message);
            }
            return sb.ToString();
        }
        //----< deserialize XML to object >------------------------------

        static public T FromXml<T>(this string xml)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(new StringReader(xml));
            }
            catch (Exception ex)
            {
                Console.Write("\n  deserialization failed\n  {0}", ex.Message);
                return default(T);
            }
        }

        public class Widget
        {
            public Widget() { }
            public Widget(string nm)
            {
                name = nm;
            }
            public string getName()
            {
                return name;
            }
            public string name { get; set; }
        }
#if (SERIALIZATION)
        static void Main(string[] args)
        {
        Console.Write("\n  attempting to serialize Widget object:");
      Widget widget = new Widget("Jim");
      string xml = widget.ToXml();
      if(xml.Count() > 0)
        Console.Write("\n  widget:\n{0}\n", xml);

      Console.Write("\n  attempting to deserialize Widget object:");
      Widget newWidget = xml.FromXml<Widget>();
      if(newWidget == null)
      {
        Console.Write("\n  deserialized object is null");
      }
      else
      {
        string type = newWidget.GetType().Name;
        Console.Write("\n  retrieved object of type: {0}", type);

        string test = newWidget.getName();
        Console.Write("\n  reconstructed widget's name = \"{0}\"", test);
      }


        }
#endif
    }

}
