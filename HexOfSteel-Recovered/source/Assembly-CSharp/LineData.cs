using System;
using UnityEngine;

[Serializable]
public class LineData
{
	[NonSerialized]
	public GameObject lineObject;

	public byte Width { get; set; }

	public byte[] Color { get; set; }

	public float[][,] Points { get; set; }
}
