using System.Xml;
using System.Collections;
using System.Collections.ObjectModel;

abstract class NamedObject
{
	public string name;

//	public NamedObject() {}
	private void Load(XmlElement xelement) {}
}

//class GeneralBase<T> where T : NamedObject, new()
class GeneralBase<T> where T : NamedObject
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

		Log.Error("No name found in base");
		return null;
	}
}
