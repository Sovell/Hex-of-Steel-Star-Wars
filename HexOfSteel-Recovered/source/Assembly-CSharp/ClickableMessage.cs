using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableMessage : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private TextMeshProUGUI _text;

	private string _linkColorHex = "#0096FF";

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			int num = TMP_TextUtilities.FindIntersectingLink(_text, Input.mousePosition, null);
			if (num > -1)
			{
				Application.OpenURL(_text.textInfo.linkInfo[num].GetLinkID());
			}
		}
	}

	private void Start()
	{
		FormatLink();
	}

	private void FormatLink()
	{
		if (ContainsLink(_text.text))
		{
			string linkStyle = "<color=" + _linkColorHex + "><u><b>{0}</b></u></color>";
			string text = Regex.Replace(_text.text, "(https?://\\S+)", (Match match) =>
			{
				string value = match.Groups[0].Value;
				return "<link=" + value + ">" + string.Format(linkStyle, value) + "</link>";
			});
			_text.text = text;
		}
	}

	private bool ContainsLink(string p_message)
	{
		if (!p_message.Contains("http://"))
		{
			return p_message.Contains("https://");
		}
		return true;
	}
}
