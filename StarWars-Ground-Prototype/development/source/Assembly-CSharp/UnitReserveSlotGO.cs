using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitReserveSlotGO : MonoBehaviour
{
	public Button slot_button;

	public Button transfer_button;

	public Button delete_button;

	public Button upgrade_button;

	public Image background_image;

	public Image representation_image;

	public Image productionTakesTime_image;

	public Image coreUnitIndicator_image;

	public Image level_image;

	public Image horse_image;

	public Image truck_image;

	public Image mechanize_image;

	public Image bomb_image;

	public Image rocket_image;

	public Image torpedo_image;

	public Image dropTank_image;

	public Image atomBomb_image;

	public Image mountaineer_image;

	public Image winter_image;

	public Image political_image;

	public Sprite SS_sprite;

	public Sprite guards_sprite;

	public Sprite censoredSS_sprite;

	public Sprite squareCoreUnit_sprite;

	public Sprite roundCoreUnit_sprite;

	public Image recon_image;

	public Image flamethrower_image;

	public Image seaplane_image;

	public Image navalAircraft_image;

	public TextMeshProUGUI unitName_text;

	public TextMeshProUGUI displayName_text;

	public TextMeshProUGUI productionTime_text;

	public TextMeshProUGUI HP_text;

	public Unit unit;

	public void SetUpSlot()
	{
		name = unit.ID.ToString();
		slot_button.onClick.RemoveAllListeners();
		transfer_button.gameObject.SetActive(!MainMenu.isEditor && !unit.TileBuilding);
		upgrade_button.gameObject.SetActive(!MainMenu.isEditor);
		transfer_button.onClick.RemoveAllListeners();
		delete_button.onClick.RemoveAllListeners();
		upgrade_button.onClick.RemoveAllListeners();
		upgrade_button.enabled = unit.CanBeUpgraded();
		coreUnitIndicator_image.gameObject.SetActive(unit.IsCoreUnit);
		if (unit.IsCoreUnit)
		{
			coreUnitIndicator_image.sprite = ((unit.Type == "Boat" || unit.Type == "Plane") ? roundCoreUnit_sprite : squareCoreUnit_sprite);
		}
		if (!unit.IsPoliticalUnit || unit.IsCommander || unit.MaxMP <= 1 || (unit.Type == "Plane" && !unit.IsParatrooper) || unit.Type == "Boat")
		{
			political_image.gameObject.SetActive(value: false);
		}
		else
		{
			political_image.gameObject.SetActive(value: true);
			if (unit.Country == "Nazi Germany")
			{
				if (PlayerSettings.Instance.CensoredGermany)
				{
					political_image.sprite = censoredSS_sprite;
				}
				else
				{
					political_image.sprite = SS_sprite;
				}
			}
			else if (unit.Country == "Soviet Union")
			{
				political_image.sprite = guards_sprite;
			}
			else
			{
				political_image.gameObject.SetActive(value: false);
			}
		}
		unitName_text.text = unit.GetUnitName();
		displayName_text.text = unit.GetCounterDisplayName();
		displayName_text.gameObject.SetActive(!string.IsNullOrEmpty(displayName_text.text));
		productionTime_text.text = unit.TurnToProduce.ToString();
		productionTakesTime_image.gameObject.SetActive(PlayerSettings.Instance.ProductionTakesTime && unit.TurnToProduce != 0 && !MainMenu.isEditor);
		if (productionTakesTime_image.gameObject.activeSelf)
		{
			if (unit.TurnToProduce > 9)
			{
				productionTime_text.fontSize = 20f;
			}
			else
			{
				productionTime_text.fontSize = 32f;
			}
		}
		representation_image.sprite = Utils.GetUnitSprite(unit);
		background_image.sprite = Utils.GetUnitBackground(unit, TurnManager.currPlayer.Country);
		winter_image.gameObject.SetActive(unit.IsWinterSpecialized);
		mountaineer_image.gameObject.SetActive(unit.IsMountaineer);
		mechanize_image.gameObject.SetActive(unit.IsMechanized);
		truck_image.gameObject.SetActive(unit.IsMotorized);
		horse_image.gameObject.SetActive(unit.IsHorsed);
		atomBomb_image.gameObject.SetActive(unit.HasABomb);
		dropTank_image.gameObject.SetActive(unit.HasDropTank);
		bomb_image.gameObject.SetActive(unit.HasBombs);
		rocket_image.gameObject.SetActive(unit.HasRockets);
		torpedo_image.gameObject.SetActive(unit.HasTorpedo);
		recon_image.gameObject.SetActive(unit.IsRecon);
		flamethrower_image.gameObject.SetActive(unit.HasFlamethrower);
		navalAircraft_image.gameObject.SetActive(unit.CanBeRefueledOnCarrier);
		seaplane_image.gameObject.SetActive(unit.IsCatapultPlane || unit.CanCatapultPlanes);
		level_image.gameObject.SetActive(unit.Lvl > 1);
		level_image.sprite = Resources.Load<Sprite>("UI/Ranks/Outline/" + unit.Lvl);
		if (unit.MaxHP > 0)
		{
			HP_text.text = Utils.Truncate(unit.GetHPPercent(p_forDamagePurposes: false) * 100f, 0) + "%";
		}
		else
		{
			HP_text.gameObject.transform.parent.gameObject.SetActive(value: false);
		}
	}
}
