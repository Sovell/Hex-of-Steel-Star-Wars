using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PoliciesMenu : MonoBehaviour
{
	public static PoliciesMenu instance;

	public Transform policiesSlots_parent;

	public PolicySlotGO policiesGO_prefab;

	public GameObject tooltip;

	public TextMeshProUGUI highCommandPoints_text;

	public List<Transform> categories;

	private Policy _confirmationPolicy;

	private void Awake()
	{
		instance = this;
		foreach (Transform category in categories)
		{
			if (category.name == "Conscription")
			{
				category.gameObject.SetActive(PlayerSettings.Instance.Manpower);
			}
			else if (category.name == "Economy")
			{
				category.gameObject.SetActive(PlayerSettings.Instance.Manpower || PlayerSettings.Instance.TechTree);
			}
			else if (category.name == "Occupation")
			{
				category.gameObject.SetActive(PlayerSettings.Instance.Partisans);
			}
		}
	}

	private void OnEnable()
	{
		Policies.InitializePolicies();
		RefreshPolicies();
		RefreshHighCommandpoints();
		UIManager.instance.HandleMenuStacking(gameObject);
	}

	public void ManagePoliciesPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		if (!PlayerSettings.Instance.Policies)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("info-disabled policies"));
		}
		else if (MultiplayerManager.IsRealtimeMultiplayer() && !TurnManager.currPlayer.IsLocalClientPlayer())
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-not your turn"));
		}
		else
		{
			if (!gameObject.activeSelf && TurnManager.currPlayer.IsComputer && !MapGO.IsAIOnly)
			{
				return;
			}
			gameObject.SetActive(!gameObject.activeSelf);
			if (gameObject.activeSelf)
			{
				TutorialManager.PerformAction(TutorialActions.OPEN_POLICIES_MENU);
				if (TurnManager.currPlayer.ListActivePolicies.Count > 0)
				{
					TutorialManager.PerformAction(TutorialActions.PICK_POLICY);
				}
			}
		}
	}

	public void ClosePoliciesPanel()
	{
		SoundManager.instance.PlayUIClickSound();
		gameObject.SetActive(value: false);
	}

	private void RefreshPolicies()
	{
		List<Policy> allPolicies = Policies.GetAllPolicies();
		DeleteAllPoliciesSlots();
		foreach (Transform category in categories)
		{
			foreach (Policy item in allPolicies)
			{
				if (item.Category.ToString() == category.name)
				{
					if (item.Type == Policies.Type.Winter_soldiers && (TurnManager.currPlayer.Country == "Finland" || TurnManager.currPlayer.Country == "Sweden" || TurnManager.currPlayer.Country == "Norway"))
					{
						item.HighCommandPointsCost = 0;
					}
					PolicySlotGO policySlotGOInstance = Object.Instantiate(policiesGO_prefab, category.transform.Find("Container"));
					policySlotGOInstance.SetUpPolicySlot(item);
					policySlotGOInstance.button.onClick.AddListener(() =>
					{
						ClickOnPolicy(policySlotGOInstance);
					});
				}
			}
		}
		Canvas.ForceUpdateCanvases();
		LayoutRebuilder.ForceRebuildLayoutImmediate(policiesSlots_parent.GetComponent<RectTransform>());
		RefreshHighCommandpoints();
	}

	public void ClickOnPolicy(PolicySlotGO p_policySlotGO)
	{
		SoundManager.instance.PlayUIClickSound();
		_confirmationPolicy = p_policySlotGO.policy;
		if (TurnManager.currPlayer.HasPolicy(p_policySlotGO.policy.Type))
		{
			UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-confirmation remove policy") + LocalizationManager.GetPolicyName(p_policySlotGO.policy) + " ?", OnConfirmCancelPolicy);
		}
		else if (p_policySlotGO.policy.HighCommandPointsCost > TurnManager.currPlayer.HighCommandPoints)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-not enough hq points"));
		}
		else if (TurnManager.currPlayer.HasPolicy(p_policySlotGO.policy.Category))
		{
			UIManager.ShowConfirmationWindow(LocalizationManager.Translate("confirmation.pickNewPolicy"), OnConfirmChoosePolicy);
		}
		else
		{
			OnConfirmChoosePolicy();
		}
	}

	private void OnConfirmChoosePolicy()
	{
		TurnManager.currPlayer.PurchasePolicy(_confirmationPolicy, p_sendRPC: true);
		RefreshPolicies();
		UIManager.instance.RefreshManpower();
		UIManager.instance.RefreshIncomePerTurnUI();
		TutorialManager.PerformAction(TutorialActions.PICK_POLICY);
	}

	private void OnConfirmCancelPolicy()
	{
		TurnManager.currPlayer.RemovePolicy(_confirmationPolicy, p_sendRPC: true);
		RefreshPolicies();
		UIManager.instance.RefreshManpower();
		UIManager.instance.RefreshIncomePerTurnUI();
	}

	private void DeleteAllPoliciesSlots()
	{
		foreach (Transform category in categories)
		{
			PolicySlotGO[] componentsInChildren = category.GetComponentsInChildren<PolicySlotGO>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Object.Destroy(componentsInChildren[i].gameObject);
			}
		}
	}

	private void RefreshHighCommandpoints()
	{
		highCommandPoints_text.text = LocalizationManager.Translate("txt-HQ points") + TurnManager.currPlayer.HighCommandPoints + " / " + (TurnManager.currPlayer.HasPolicy(Policies.Type.Bigger_command_centers) ? "200" : "100");
	}
}
