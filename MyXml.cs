// просто синтаксический сахар для работы с XML
using System.Xml;

static class MyXml
{
	public static XmlNode FirstChild(string fileName)
	{
		XmlDocument xdoc = new XmlDocument();
		xdoc.Load(fileName);
		return xdoc.FirstChild;
	}

	public static XmlNode SecondChild(string fileName)
	{
		return FirstChild(fileName).FirstChild;
	}

	public static string GetString(XmlNode xmlNode, string attribute)
	{
		XmlElement xmlElement = (XmlElement)(xmlNode);
		return xmlElement.GetAttribute(attribute);
    }

	public static int GetInt(XmlNode xmlNode, string attribute)
	{
        return int.Parse(GetString(xmlNode, attribute));
	}
}