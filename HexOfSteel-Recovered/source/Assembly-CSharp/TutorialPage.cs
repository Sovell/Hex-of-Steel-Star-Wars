using System;
using System.Collections.Generic;

[Serializable]
public class TutorialPage
{
	public string titleLocalizationID;

	public string bodyLocalizationID;

	public List<TutorialTask> tasks;
}
