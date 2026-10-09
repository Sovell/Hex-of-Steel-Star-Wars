using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileTransferSlotGO : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private Button _slot_button;

	[SerializeField]
	private Image _image;

	[SerializeField]
	private TMP_Text _text;

	[SerializeField]
	private Button _transfer_button;

	[SerializeField]
	private Button _request_button;

	[SerializeField]
	private Button _annex_button;

	[SerializeField]
	private Button _delete_button;

	[Header("DEBUG")]
	[SerializeField]
	private TileGO _tileGO;

	[SerializeField]
	private GameObject _highlight;

	private void Start()
	{
		_slot_button.onClick.AddListener(() =>
		{
			Focus();
		});
		_delete_button.onClick.AddListener(() =>
		{
			Delete();
		});
		_request_button.onClick.AddListener(() =>
		{
			Request();
		});
		_transfer_button.onClick.AddListener(() =>
		{
			Transfer();
		});
		_annex_button.onClick.AddListener(() =>
		{
			Annex();
		});
	}

	public void Setup(TileGO p_tileGO)
	{
		_tileGO = p_tileGO;
		if (string.IsNullOrEmpty(_tileGO.tile.CityName))
		{
			_text.text = _tileGO.tile.GetLocalizedDisplayName();
		}
		else
		{
			_text.text = _tileGO.tile.CityName + " - (" + _tileGO.tile.GetLocalizedDisplayName() + ")";
		}
		if (_tileGO.tile.ownerPlayer == null)
		{
			_transfer_button.gameObject.SetActive(value: false);
			_request_button.gameObject.SetActive(value: false);
			_annex_button.gameObject.SetActive(value: false);
			return;
		}
		_annex_button.gameObject.SetActive(_tileGO.tile.ownerPlayer == TurnManager.currPlayer && _tileGO.tile.OriginalCountryOwner != TurnManager.currPlayer.Country);
		_transfer_button.gameObject.SetActive(_tileGO.tile.ownerPlayer == TurnManager.currPlayer);
		_request_button.gameObject.SetActive(_tileGO.tile.ownerPlayer != TurnManager.currPlayer);
		if (_annex_button.gameObject.activeSelf && _transfer_button.gameObject.activeSelf)
		{
			RectTransform component = _annex_button.gameObject.GetComponent<RectTransform>();
			RectTransform component2 = _transfer_button.gameObject.GetComponent<RectTransform>();
			float width = component.rect.width;
			float x = ((Vector3)component.anchoredPosition).x - width - 5f;
			Vector2 anchoredPosition = new Vector2(x, component2.anchoredPosition.y);
			component2.anchoredPosition = anchoredPosition;
		}
		_image.sprite = Utils.GetFlagSprite(_tileGO.tile.ownerPlayer.Country);
		_highlight = Object.Instantiate(Resources.Load<GameObject>("Prefabs/Highlight ring 2"), _tileGO.transform);
		_highlight.transform.localScale = Vector3.zero;
		LeanTween.scale(_highlight, Vector3.one * 1.3f, 0.25f).setEaseInOutBack();
		LeanTween.rotateAroundLocal(_highlight, Vector3.forward, 360f, 5f).setRepeat(-1).setDelay(0.25f);
	}

	public TileGO GetTileGO()
	{
		return _tileGO;
	}

	public bool IsLinkedToTile(TileGO p_tileGO)
	{
		return _tileGO == p_tileGO;
	}

	private void Annex()
	{
		SoundManager.instance.PlayUIClickSound();
		int annexTileCost = _tileGO.tile.GetAnnexTileCost(TurnManager.currPlayer);
		if (TurnManager.currPlayer.MoneyAmount < annexTileCost)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-request not enough funds") + annexTileCost);
		}
		else
		{
			UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-annex tile cost") + _tileGO.tile.GetAnnexTileCost(TurnManager.currPlayer), ConfirmAnnex);
		}
	}

	private void ConfirmAnnex()
	{
		TilesManagerGO.instance.AnnexTile(_tileGO, TurnManager.currPlayer);
	}

	private void Transfer()
	{
		SoundManager.instance.PlayUIClickSound();
		GameObject transferPanel = TilesManagerGO.instance.GetTransferPanel();
		Transform[] componentsInChildren = transferPanel.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform != transferPanel.transform)
			{
				Object.Destroy(transform.gameObject);
			}
		}
		GameObject transferSlot = TilesManagerGO.instance.GetTransferSlot();
		foreach (Player player in GameData.Instance.listOfPlayers)
		{
			if (player.Name != TurnManager.currPlayer.Name && !player.isAtWarWithCurrentPlayer)
			{
				GameObject gameObject = Object.Instantiate(transferSlot, transferPanel.transform);
				gameObject.name = player.Name;
				gameObject.GetComponent<Button>().onClick.AddListener(() =>
				{
					TilesManagerGO.instance.TransferTile(_tileGO, player);
				});
				gameObject.GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.GetLocalizedCountry(player.Country);
				transferPanel.SetActive(value: true);
			}
		}
		if (Input.mousePosition.y > (float)(Screen.height / 2))
		{
			transferPanel.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);
		}
		else
		{
			transferPanel.GetComponent<RectTransform>().pivot = new Vector2(0f, 0f);
		}
		transferPanel.transform.position = new Vector3(Input.mousePosition.x + 50f, Input.mousePosition.y);
	}

	private void Request()
	{
		SoundManager.instance.PlayUIClickSound();
		if (!_tileGO.tile.ownerPlayer.IsComputer)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-request tile ownership"));
			return;
		}
		int requestOwnershipCost = _tileGO.tile.GetRequestOwnershipCost(TurnManager.currPlayer);
		if (TurnManager.currPlayer.MoneyAmount < requestOwnershipCost)
		{
			UIManager.ShowMessage(LocalizationManager.Translate("error-request not enough funds") + requestOwnershipCost);
		}
		else
		{
			UIManager.ShowConfirmationWindow(LocalizationManager.Translate("info-request tile cost") + _tileGO.tile.GetRequestOwnershipCost(TurnManager.currPlayer), ConfirmRequest);
		}
	}

	private void ConfirmRequest()
	{
		TilesManagerGO.instance.RequestTile(_tileGO);
	}

	private void Focus()
	{
		SoundManager.instance.PlayUIClickSound();
		CameraGO.instance.MoveCameraToPosition(new Vector3(_tileGO.transform.position.x, _tileGO.transform.position.y, -80f));
	}

	private void Delete()
	{
		SoundManager.instance.PlayUIClickSound();
		TilesManagerGO.instance.ClickedOnTile(_tileGO);
	}

	private void OnDestroy()
	{
		Object.Destroy(_highlight, 0.25f);
		LeanTween.scale(_highlight, Vector3.zero, 0.25f).setEaseInBack();
	}
}
