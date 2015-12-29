using System.Linq;
using System.Collections.Generic;

public class Team : LocalComponent
{
	public bool isInParty;

	public Team(bool isInPartyi, LocalObject o) : base(o)
	{
		isInParty = isInPartyi;
	}

	public bool IsFriendTo(LocalObject u) { return u.initiative != null && !IsEnemyTo(u); }
	public bool IsEnemyTo(LocalObject u)
	{
		LocalObject reference = t;

		if (t.effects != null)
		{
			if (t.effects.Has("Mind Controlled")) reference = t.effects.Get("Mind Controlled").parameter as LocalObject;
			else if (t.effects.Has("Mind Tricked")) reference = t.effects.Get("Mind Tricked").parameter as LocalObject;
		}

		if (u == t || u.initiative == null) return false;
		else return reference.team.isInParty != u.team.isInParty;
	}

	public List<LocalObject> Enemies { get { return (from c in B.ActiveObjects where IsEnemyTo(c) select c).ToList(); } }
	public List<LocalObject> Friends { get { return (from c in B.ActiveObjects where IsFriendTo(c) select c).ToList(); } }
}
