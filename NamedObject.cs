using System.Xml;
using System.Collections.Generic;

public abstract class NamedObject
{
	public string name;

	public abstract void Load(XmlNode xnode);

	protected static MainScreen M { get { return MainScreen.Instance; } }
	protected static BigBase BB { get { return BigBase.Instance; } }
}

public class GeneralBase<T> where T : NamedObject, new ()
{
	public List<T> data = new List<T>();
	private string filename;
	//public bool loaded;

	/*public GeneralBase()
	{
		data = new List<T>();
		loaded = false;
	}*/

	//public int Size { get { return data.Count; } }

	/*public void Add(T t)
	{
		data.Add(t);
	}*/

	public T Get(string name)
	{
		//Log.Assert (loaded, "base is not loaded yet");
		foreach (T d in data) if (d.name == name) return d;
		Log.Error("No name " + name + " found in the base " + filename);
		return null;
	}

	/*public string SafeName(string name)
	{
		foreach (T t in data) if (t.name == name) return name;
		Log.Error("No name " + name + " found in the base " + filename);
		return name;
	}*/

	public void Load(string _filename)
	{
		filename = _filename;
		for (XmlNode xnode = MyXml.SecondChild("Data/" + filename); xnode != null; xnode = xnode.NextSibling)
		{
			T t = new T();
			t.Load(xnode);
			data.Add(t);
		}
		//loaded = true;
	}
}
