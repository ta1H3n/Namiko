using Namiko.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Namiko
{
    public static class XmlHelper
    {
        public static bool NewLineOnAttributes { get; set; }
        /// Serializes an object to an XML string, using the specified namespaces.
        public static string ToXml(object obj, XmlSerializerNamespaces ns)
        {
            Type T = obj.GetType();

            var xs = new XmlSerializer(T);
            var ws = new XmlWriterSettings { Indent = true, NewLineOnAttributes = NewLineOnAttributes, OmitXmlDeclaration = true };

            var sb = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(sb, ws))
            {
                xs.Serialize(writer, obj, ns);
            }
            return sb.ToString();
        }
        
        /// Serializes an object to an XML string.
        public static string ToXml(object obj)
        {
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            return ToXml(obj, ns);
        }
        
        /// Deserializes an object from an XML string.
        public static T FromXml<T>(string xml)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            using (StringReader sr = new StringReader(xml))
            {
                return (T)xs.Deserialize(sr);
            }
        }
        
        /// Deserializes an object from an XML string, using the specified type name.
        public static object FromXml(string xml, string typeName)
        {
            Type T = Type.GetType(typeName);
            XmlSerializer xs = new XmlSerializer(T);
            using (StringReader sr = new StringReader(xml))
            {
                return xs.Deserialize(sr);
            }
        }
        
        /// Serializes an object to an XML file.
        public static void ToXmlFile(Object obj, string filePath)
        {
            var xs = new XmlSerializer(obj.GetType());
            var ns = new XmlSerializerNamespaces();
            var ws = new XmlWriterSettings { Indent = true, NewLineOnAttributes = NewLineOnAttributes, OmitXmlDeclaration = true };
            ns.Add("", "");

            using (XmlWriter writer = XmlWriter.Create(filePath, ws))
            {
                xs.Serialize(writer, obj);
            }
        }
        
        /// Deserializes an object from an XML file.
        public static T FromXmlFile<T>(string filePath)
        {
            StreamReader sr = new StreamReader(filePath);
            try
            {
                var result = FromXml<T>(sr.ReadToEnd());
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("There was an error attempting to read the file " + filePath + "\n\n" + e.InnerException.Message);
            }
            finally
            {
                sr.Close();
            }
        }

        public static List<string> GetSpookLines()
        {
            try
            {
                return FromXmlFile<List<string>>(XmlPaths.GetSpookLinesPath());
            }
            catch
            {
                return null;
            }
        }
    }

    public static class XmlPaths
    {
        public static string GetSpookLinesPath()
        {
            return Locations.SpookyLinesXml;
        }
    }
}
