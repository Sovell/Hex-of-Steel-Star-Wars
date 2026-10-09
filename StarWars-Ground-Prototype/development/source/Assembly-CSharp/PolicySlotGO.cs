using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PolicySlotGO : MonoBehaviour
{
	public TextMeshProUGUI policyName_text;

	public Image representation_image;

	public Button button;

	public Policy policy;

	[SerializeField]
	private Color _active_color;

	[SerializeField]
	private Color _tooExpensive_color;

	public void SetUpPolicySlot(Policy p_policy)
	{
		policy = p_policy;
		name = policy.Name;
		policyName_text.text = LocalizationManager.GetPolicyName(policy);
		representation_image.sprite = Policies.GetPolicyIcon(policy);
		if (TurnManager.currPlayer.HasPolicy(policy.Type))
		{
			representation_image.color = _active_color;
			button.interactable = true;
		}
		else if (policy.HighCommandPointsCost > TurnManager.currPlayer.HighCommandPoints)
		{
			representation_image.color = _tooExpensive_color;
			button.interactable = false;
		}
	}
}
