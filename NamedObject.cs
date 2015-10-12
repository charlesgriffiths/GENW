using System.Xml;
using System.Collections.ObjectModel;

abstract class NamedObject
{
	public string name;

	//	private void Load(XmlElement xelement) {}
	public abstract void Load(XmlNode xnode);
}

class GeneralBase<T> where T : NamedObject, new ()
{
	public Collection<T> data;
	public bool loaded;

	public GeneralBase()
	{
		data = new Collection<T>();
		loaded = false;
	}

	public int Size { get { return data.Count; } }

	public void Add(T t)
	{
		data.Add(t);
	}

	public T Get(string name)
	{
		Log.Assert (loaded, "base is not loaded yet");
		foreach (T d in data)
		{
			if (d.name == name) return d;
		}

		Log.Error("No name " + name + " found in base");
		return null;
	}

	public void Load(string filename)
	{
		XmlNode xnode = MyXml.SecondChild(filename);

		while (xnode != null)
		{
			T t = new T();
			t.Load(xnode);
			Add(t);
			xnode = xnode.NextSibling;
		}

		loaded = true;
	}
}
