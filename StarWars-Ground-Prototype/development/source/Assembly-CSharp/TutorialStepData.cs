using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tutorial step data", menuName = "Tutorial/Tutorial step data")]
public class TutorialStepData : ScriptableObject
{
	public List<TutorialPage> pages;
}
