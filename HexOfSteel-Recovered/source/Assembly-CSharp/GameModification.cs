using System;
using UnityEngine;

public abstract class GameModification
{
	[Obsolete("Use the 'Mod' property instead.")]
	public Mod mod;

	private Mod _mod;

	public Mod Mod
	{
		get
		{
			return _mod;
		}
		protected set
		{
			_mod = value;
			mod = value;
		}
	}

	protected GameModification(Mod p_mod)
	{
		Mod = p_mod;
	}

	public abstract void OnModInitialization(Mod p_mod);

	public abstract void OnModUnloaded();

	public virtual void Log(string p_content, LogType p_logType = LogType.Log)
	{
		string message = "[MOD:" + Mod.Name + "] " + p_content;
		switch (p_logType)
		{
		default:
			Debug.Log(message);
			break;
		case LogType.Warning:
			Debug.LogWarning(message);
			break;
		case LogType.Error:
			Debug.LogError(message);
			break;
		}
	}
}
