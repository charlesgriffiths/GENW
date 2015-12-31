using System.Xml;

static class MyXml
{
	public static XmlNode FirstChild(string fileName) {
		XmlDocument xdoc = new XmlDocument();
		xdoc.Load(fileName);
		return xdoc.FirstChild;	}

	public static XmlNode SecondChild(string fileName) { return FirstChild(fileName).FirstChild; }

	public static string GetString(XmlNode xmlNode, string attribute) {	return ((XmlElement)xmlNode).GetAttribute(attribute); }
	public static char GetChar(XmlNode xmlNode, string attribute) {	return GetString(xmlNode, attribute)[0]; }

	public static int GetInt(XmlNode xmlNode, string attribute, int dflt = 0) {
		string s = GetString(xmlNode, attribute);
		return s == "" ? dflt : int.Parse(s); }

	public static float GetFloat(XmlNode xmlNode, string attribute, float dflt = 0)	{
		string s = GetString(xmlNode, attribute);
		return s == "" ? dflt : float.Parse(s);	}

	public static bool GetBool(XmlNode xmlNode, string attribute) {
		string s = GetString(xmlNode, attribute);
		return s == "" ? false : bool.Parse(s);	}
}