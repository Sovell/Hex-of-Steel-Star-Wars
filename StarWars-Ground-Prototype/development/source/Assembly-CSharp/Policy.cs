using System;

[Serializable]
public class Policy
{
	[NonSerialized]
	public string Name;

	[NonSerialized]
	public string Description;

	public Policies.Type Type { get; set; }

	public Policies.Categories Category { get; set; }

	public byte HighCommandPointsCost { get; set; }

	public Policy(string p_name, string p_description, Policies.Categories p_category, Policies.Type p_type, byte p_cost)
	{
		Name = p_name;
		Description = p_description;
		Category = p_category;
		Type = p_type;
		HighCommandPointsCost = p_cost;
	}
}
