using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class FlexibleColorPicker : MonoBehaviour
{
	[Serializable]
	private struct Picker
	{
		public Image image;

		public Sprite dynamicSprite;

		public Sprite staticSpriteHor;

		public Sprite staticSpriteVer;

		public Material dynamicMaterial;
	}

	private enum PickerType
	{
		Main,
		R,
		G,
		B,
		H,
		S,
		V,
		A,
		Preview,
		PreviewAlpha
	}

	public enum MainPickingMode
	{
		HS,
		HV,
		SH,
		SV,
		VH,
		VS
	}

	[Serializable]
	public class ColorUpdateEvent : UnityEvent<Color>
	{
	}

	[Serializable]
	public class AdvancedSettings
	{
		[Serializable]
		public class PSettings
		{
			[Tooltip("Value can be used to restrict slider range")]
			public Vector2 range = new Vector2(0f, 1f);

			[Tooltip("Make the picker associated with this value act static, even in a dynamic color picker setup")]
			public bool overrideStatic;
		}

		public bool mainStatic = true;

		public PSettings r;

		public PSettings g;

		public PSettings b;

		public PSettings h;

		public PSettings s;

		public PSettings v;

		public PSettings a;

		public PSettings Get(int i)
		{
			if ((i <= 0) | (i > 7))
			{
				return new PSettings();
			}
			return (new PSettings[7] { r, g, b, h, s, v, a })[i - 1];
		}
	}

	[Serializable]
	private class BufferedColor
	{
		public Color color;

		private float bufferedHue;

		private float bufferedSaturation;

		public float r => color.r;

		public float g => color.g;

		public float b => color.b;

		public float a => color.a;

		public float h => bufferedHue;

		public float s => bufferedSaturation;

		public float v => RGBToHSV(color).z;

		public BufferedColor()
		{
			bufferedHue = 0f;
			bufferedSaturation = 0f;
			color = Color.black;
		}

		public BufferedColor(Color color)
			: this()
		{
			Set(color);
		}

		public BufferedColor(Color color, float hue, float sat)
			: this(color)
		{
			bufferedHue = hue;
			bufferedSaturation = sat;
		}

		public BufferedColor(Color color, BufferedColor source)
			: this(color, source.bufferedHue, source.bufferedSaturation)
		{
			Set(color);
		}

		public void Set(Color color)
		{
			Set(color, bufferedHue, bufferedSaturation);
		}

		public void Set(Color color, float bufferedHue, float bufferedSaturation)
		{
			this.color = color;
			Vector3 vector = RGBToHSV(color);
			if (vector.y == 0f || vector.z == 0f)
			{
				this.bufferedHue = bufferedHue;
			}
			else
			{
				this.bufferedHue = vector.x;
			}
			if (vector.z == 0f)
			{
				this.bufferedSaturation = bufferedSaturation;
			}
			else
			{
				this.bufferedSaturation = vector.y;
			}
		}

		public BufferedColor PickR(float value)
		{
			Color color = this.color;
			color.r = value;
			return new BufferedColor(color, this);
		}

		public BufferedColor PickG(float value)
		{
			Color color = this.color;
			color.g = value;
			return new BufferedColor(color, this);
		}

		public BufferedColor PickB(float value)
		{
			Color color = this.color;
			color.b = value;
			return new BufferedColor(color, this);
		}

		public BufferedColor PickA(float value)
		{
			Color color = this.color;
			color.a = value;
			return new BufferedColor(color, this);
		}

		public BufferedColor PickH(float value)
		{
			Vector3 vector = RGBToHSV(this.color);
			Color color = HSVToRGB(value, vector.y, vector.z);
			color.a = this.color.a;
			return new BufferedColor(color, value, bufferedSaturation);
		}

		public BufferedColor PickS(float value)
		{
			Vector3 vector = RGBToHSV(this.color);
			Color color = HSVToRGB(bufferedHue, value, vector.z);
			color.a = this.color.a;
			return new BufferedColor(color, bufferedHue, value);
		}

		public BufferedColor PickV(float value)
		{
			Color color = HSVToRGB(bufferedHue, bufferedSaturation, value);
			color.a = this.color.a;
			return new BufferedColor(color, bufferedHue, bufferedSaturation);
		}
	}

	[Tooltip("Connections to the FCP's picker images, this should not be adjusted unless in advanced use cases.")]
	[SerializeField]
	private Picker[] pickers;

	[Tooltip("Connection to the FCP's hexadecimal input field.")]
	[SerializeField]
	private InputField hexInput;

	[Tooltip("Connection to the FCP's mode dropdown menu.")]
	[SerializeField]
	private Dropdown modeDropdown;

	private Canvas canvas;

	[Tooltip("The (starting) 2D picking mode, i.e. the 2 color values that can be picked with the large square picker.")]
	[SerializeField]
	private MainPickingMode mode;

	[Tooltip("Sprites to be used in static mode on the main picker, one for each 2D mode.")]
	[SerializeField]
	private Sprite[] staticSpriteMain;

	private BufferedColor bufferedColor;

	private Picker focusedPicker;

	private PickerType focusedPickerType;

	private MainPickingMode lastUpdatedMode;

	private bool typeUpdate;

	private bool triggeredStaticMode;

	private bool materialsSeperated;

	[Tooltip("Color set to the color picker on Start(). If you wish to set a starting color via script please used the standard color parameter of the FCP in stead.")]
	[SerializeField]
	private Color startingColor = Color.white;

	[Tooltip("Use static mode: picker images are replaced by static images in stead of adaptive Unity shaders.")]
	public bool staticMode;

	[Tooltip("Make sure FCP seperates its picker materials so that the dynamic mode works consistently, even when multiple FPCs are active at the same time. Turning this off yields a slight performance boost.")]
	public bool multiInstance = true;

	public ColorUpdateEvent onColorChange;

	private const float HUE_LOOP = 5.9999f;

	private const string SHADER_MODE = "_Mode";

	private const string SHADER_C1 = "_Color1";

	private const string SHADER_C2 = "_Color2";

	private const string SHADER_DOUBLE_MODE = "_DoubleMode";

	private const string SHADER_HSV = "_HSV";

	private const string SHADER_HSV_MIN = "_HSV_MIN";

	private const string SHADER_HSV_MAX = "_HSV_MAX";

	[Tooltip("More specific settings for color picker. Changes are not applied immediately, but require an FCP update to trigger.")]
	public AdvancedSettings advancedSettings;

	private AdvancedSettings avs => advancedSettings;

	public Color color
	{
		get
		{
			if (bufferedColor == null)
			{
				return startingColor;
			}
			return bufferedColor.color;
		}
		set
		{
			if (bufferedColor == null)
			{
				startingColor = value;
				return;
			}
			bufferedColor.Set(value);
			UpdateMarkers();
			UpdateTextures();
			UpdateHex();
			typeUpdate = true;
			onColorChange.Invoke(value);
		}
	}

	public Color GetColor()
	{
		return color;
	}

	public void SetColor(Color color)
	{
		this.color = color;
	}

	public Color GetColorFullAlpha()
	{
		Color result = color;
		result.a = 1f;
		return result;
	}

	public void SetColorNoAlpha(Color color)
	{
		color.a = this.color.a;
		this.color = color;
	}

	private void Awake()
	{
		canvas = GetComponentInParent<Canvas>();
	}

	private void OnEnable()
	{
		if (bufferedColor == null)
		{
			bufferedColor = new BufferedColor(startingColor);
		}
		if (multiInstance && !materialsSeperated)
		{
			SeperateMaterials();
			materialsSeperated = true;
		}
		triggeredStaticMode = staticMode;
		UpdateTextures();
		MakeModeOptions();
		UpdateMarkers();
		UpdateHex();
		onColorChange.Invoke(startingColor);
	}

	private void Update()
	{
		typeUpdate = false;
		if (lastUpdatedMode != mode)
		{
			ChangeMode(mode);
		}
		if (staticMode != triggeredStaticMode)
		{
			UpdateTextures();
			triggeredStaticMode = staticMode;
		}
		if (multiInstance && !materialsSeperated)
		{
			SeperateMaterials();
			materialsSeperated = true;
		}
	}

	public void SetPointerFocus(int i)
	{
		if (i < 0 || i >= pickers.Length)
		{
			PickerType pickerType = (PickerType)i;
			Debug.LogWarning("No picker image available of type " + pickerType.ToString() + ". Did you assign all the picker images in the editor?");
		}
		else
		{
			focusedPicker = pickers[i];
		}
		focusedPickerType = (PickerType)i;
	}

	public void PointerUpdate(BaseEventData e)
	{
		Vector2 normalizedPointerPosition = GetNormalizedPointerPosition(canvas, focusedPicker.image.rectTransform, e);
		bufferedColor = PickColor(bufferedColor, focusedPickerType, normalizedPointerPosition);
		UpdateMarkers();
		UpdateTextures();
		typeUpdate = true;
		UpdateHex();
		onColorChange.Invoke(bufferedColor.color);
	}

	public void TypeHex(string input)
	{
		TypeHex(input, finish: false);
		UpdateTextures();
		UpdateMarkers();
	}

	public void FinishTypeHex(string input)
	{
		TypeHex(input, finish: true);
		UpdateTextures();
		UpdateMarkers();
	}

	public void ChangeMode(int newMode)
	{
		ChangeMode((MainPickingMode)newMode);
	}

	public void ChangeMode(MainPickingMode mode)
	{
		this.mode = mode;
		triggeredStaticMode = false;
		UpdateTextures();
		UpdateMarkers();
		UpdateMode(mode);
	}

	private void SeperateMaterials()
	{
		for (int i = 0; i < pickers.Length; i++)
		{
			Picker picker = pickers[i];
			if (IsPickerAvailable(i) & (picker.dynamicMaterial != null))
			{
				Material material = (picker.dynamicMaterial = new Material(picker.dynamicMaterial));
				pickers[i] = picker;
				if (!staticMode)
				{
					picker.image.material = material;
				}
			}
		}
	}

	public void ShiftColor(int type, float delta)
	{
		float value = GetValue1D((PickerType)type) + delta;
		bufferedColor = PickColor1D(bufferedColor, (PickerType)type, value);
		UpdateMarkers();
		UpdateTextures();
		typeUpdate = true;
		UpdateHex();
		onColorChange.Invoke(bufferedColor.color);
	}

	public void ShiftHue(float delta)
	{
		PickerType type = PickerType.H;
		float value = Mathf.Repeat(GetValue1D(type) + delta, 6f) / 6f;
		bufferedColor = PickColor1D(bufferedColor, type, value);
		UpdateMarkers();
		UpdateTextures();
		typeUpdate = true;
		UpdateHex();
		onColorChange.Invoke(bufferedColor.color);
	}

	private BufferedColor PickColor(BufferedColor color, PickerType type, Vector2 v)
	{
		switch (type)
		{
		case PickerType.Main:
			return PickColorMain(color, v);
		case PickerType.Preview:
		case PickerType.PreviewAlpha:
			return color;
		default:
			return PickColor1D(color, type, v);
		}
	}

	private BufferedColor PickColorMain(BufferedColor color, Vector2 v)
	{
		return PickColorMain(color, mode, v);
	}

	private BufferedColor PickColor1D(BufferedColor color, PickerType type, Vector2 v)
	{
		float value = (IsHorizontal(pickers[(int)type]) ? v.x : v.y);
		return PickColor1D(color, type, value);
	}

	private BufferedColor PickColorMain(BufferedColor color, MainPickingMode mode, Vector2 v)
	{
		switch (mode)
		{
		case MainPickingMode.HS:
			return PickColor2D(color, PickerType.H, v.x, PickerType.S, v.y);
		case MainPickingMode.HV:
			return PickColor2D(color, PickerType.H, v.x, PickerType.V, v.y);
		case MainPickingMode.SH:
			return PickColor2D(color, PickerType.S, v.x, PickerType.H, v.y);
		case MainPickingMode.SV:
			return PickColor2D(color, PickerType.S, v.x, PickerType.V, v.y);
		case MainPickingMode.VH:
			return PickColor2D(color, PickerType.V, v.x, PickerType.H, v.y);
		case MainPickingMode.VS:
			return PickColor2D(color, PickerType.V, v.x, PickerType.S, v.y);
		default:
			return bufferedColor;
		}
	}

	private BufferedColor PickColor2D(BufferedColor color, PickerType type1, float value1, PickerType type2, float value2)
	{
		color = PickColor1D(color, type1, value1);
		color = PickColor1D(color, type2, value2);
		return color;
	}

	private BufferedColor PickColor1D(BufferedColor color, PickerType type, float value)
	{
		AdvancedSettings.PSettings pSettings = avs.Get((int)type);
		value = Mathf.Lerp(pSettings.range.x, pSettings.range.y, value);
		switch (type)
		{
		case PickerType.R:
			return color.PickR(value);
		case PickerType.G:
			return color.PickG(value);
		case PickerType.B:
			return color.PickB(value);
		case PickerType.H:
			return color.PickH(value * 5.9999f);
		case PickerType.S:
			return color.PickS(value);
		case PickerType.V:
			return color.PickV(value);
		case PickerType.A:
			return color.PickA(value);
		default:
			throw new Exception("Picker type " + type.ToString() + " is not associated with a single color value.");
		}
	}

	private void UpdateMarkers()
	{
		for (int i = 0; i < pickers.Length; i++)
		{
			if (IsPickerAvailable(i))
			{
				PickerType type = (PickerType)i;
				Vector2 value = GetValue(type);
				UpdateMarker(pickers[i], type, value);
			}
		}
	}

	private void UpdateMarker(Picker picker, PickerType type, Vector2 v)
	{
		switch (type)
		{
		case PickerType.Main:
			SetMarker(picker.image, v, setX: true, setY: true);
			break;
		default:
		{
			bool flag = IsHorizontal(picker);
			SetMarker(picker.image, v, flag, !flag);
			break;
		}
		case PickerType.Preview:
		case PickerType.PreviewAlpha:
			break;
		}
	}

	private void SetMarker(Image picker, Vector2 v, bool setX, bool setY)
	{
		RectTransform rectTransform = null;
		RectTransform rectTransform2 = null;
		if (setX & setY)
		{
			rectTransform = GetMarker(picker, null);
		}
		else if (setX)
		{
			rectTransform = GetMarker(picker, "hor");
			rectTransform2 = GetMarker(picker, "ver");
		}
		else if (setY)
		{
			rectTransform = GetMarker(picker, "ver");
			rectTransform2 = GetMarker(picker, "hor");
		}
		if (rectTransform2 != null)
		{
			rectTransform2.gameObject.SetActive(value: false);
		}
		if (!(rectTransform == null))
		{
			rectTransform.gameObject.SetActive(value: true);
			RectTransform rectTransform3 = picker.rectTransform;
			Vector2 size = rectTransform3.rect.size;
			Vector2 vector = rectTransform.localPosition;
			if (setX)
			{
				vector.x = (v.x - rectTransform3.pivot.x) * size.x;
			}
			if (setY)
			{
				vector.y = (v.y - rectTransform3.pivot.y) * size.y;
			}
			rectTransform.localPosition = vector;
		}
	}

	private RectTransform GetMarker(Image picker, string search)
	{
		for (int i = 0; i < picker.transform.childCount; i++)
		{
			RectTransform component = picker.transform.GetChild(i).GetComponent<RectTransform>();
			string text = component.name.ToLower();
			if (text.Contains("marker") & (string.IsNullOrEmpty(search) || text.Contains(search)))
			{
				return component;
			}
		}
		return null;
	}

	private Vector2 GetValue(PickerType type)
	{
		switch (type)
		{
		case PickerType.Main:
			return GetValue(mode);
		case PickerType.Preview:
		case PickerType.PreviewAlpha:
			return Vector2.zero;
		default:
		{
			float value1D = GetValue1D(type);
			return new Vector2(value1D, value1D);
		}
		}
	}

	private float GetValue1D(PickerType type)
	{
		AdvancedSettings.PSettings pSettings = avs.Get((int)type);
		float x = pSettings.range.x;
		float y = pSettings.range.y;
		switch (type)
		{
		case PickerType.R:
			return Mathf.InverseLerp(x, y, bufferedColor.r);
		case PickerType.G:
			return Mathf.InverseLerp(x, y, bufferedColor.g);
		case PickerType.B:
			return Mathf.InverseLerp(x, y, bufferedColor.b);
		case PickerType.H:
			return Mathf.InverseLerp(x, y, bufferedColor.h / 5.9999f);
		case PickerType.S:
			return Mathf.InverseLerp(x, y, bufferedColor.s);
		case PickerType.V:
			return Mathf.InverseLerp(x, y, bufferedColor.v);
		case PickerType.A:
			return Mathf.InverseLerp(x, y, bufferedColor.a);
		default:
			throw new Exception("Picker type " + type.ToString() + " is not associated with a single color value.");
		}
	}

	private Vector2 GetValue(MainPickingMode mode)
	{
		switch (mode)
		{
		case MainPickingMode.HS:
			return new Vector2(GetValue1D(PickerType.H), GetValue1D(PickerType.S));
		case MainPickingMode.HV:
			return new Vector2(GetValue1D(PickerType.H), GetValue1D(PickerType.V));
		case MainPickingMode.SH:
			return new Vector2(GetValue1D(PickerType.S), GetValue1D(PickerType.H));
		case MainPickingMode.SV:
			return new Vector2(GetValue1D(PickerType.S), GetValue1D(PickerType.V));
		case MainPickingMode.VH:
			return new Vector2(GetValue1D(PickerType.V), GetValue1D(PickerType.H));
		case MainPickingMode.VS:
			return new Vector2(GetValue1D(PickerType.V), GetValue1D(PickerType.S));
		default:
			throw new Exception("Unkown main picking mode: " + mode);
		}
	}

	private void UpdateTextures()
	{
		foreach (PickerType value in Enum.GetValues(typeof(PickerType)))
		{
			if (staticMode | avs.Get((int)value).overrideStatic | ((value == PickerType.Main) & avs.mainStatic))
			{
				UpdateStatic(value);
			}
			else
			{
				UpdateDynamic(value);
			}
		}
	}

	private void UpdateStatic(PickerType type)
	{
		if (IsPickerAvailable(type))
		{
			Picker p = pickers[(int)type];
			bool flag = IsHorizontal(p);
			Sprite sprite = (flag ? p.staticSpriteHor : p.staticSpriteVer);
			if (sprite == null)
			{
				sprite = (flag ? p.staticSpriteVer : p.staticSpriteHor);
			}
			p.image.sprite = sprite;
			p.image.material = null;
			p.image.color = Color.white;
			Color color = this.color;
			switch (type)
			{
			case PickerType.Main:
				p.image.sprite = staticSpriteMain[(int)mode];
				break;
			case PickerType.Preview:
				color.a = 1f;
				p.image.color = color;
				break;
			case PickerType.PreviewAlpha:
				p.image.color = color;
				break;
			}
		}
	}

	private void UpdateDynamic(PickerType type)
	{
		if (!IsPickerAvailable(type))
		{
			return;
		}
		Picker picker = pickers[(int)type];
		if (!(picker.dynamicMaterial == null))
		{
			picker.image.material = picker.dynamicMaterial;
			picker.image.color = Color.white;
			picker.image.sprite = picker.dynamicSprite;
			Material materialForRendering = picker.image.materialForRendering;
			BufferedColor bufferedColor = this.bufferedColor;
			bool flag = IsAlphaType(type);
			materialForRendering.SetInt("_Mode", GetGradientMode(type));
			Color value = PickColor(bufferedColor, type, Vector2.zero).color;
			Color value2 = PickColor(bufferedColor, type, Vector2.one).color;
			if (!flag)
			{
				value = new Color(value.r, value.g, value.b);
				value2 = new Color(value2.r, value2.g, value2.b);
			}
			materialForRendering.SetColor("_Color1", value);
			materialForRendering.SetColor("_Color2", value2);
			if (type == PickerType.Main)
			{
				materialForRendering.SetInt("_DoubleMode", (int)mode);
			}
			materialForRendering.SetVector("_HSV", new Vector4(bufferedColor.h / 5.9999f, bufferedColor.s, bufferedColor.v, flag ? bufferedColor.a : 1f));
			materialForRendering.SetVector("_HSV_MIN", new Vector4(avs.h.range.x, avs.s.range.x, avs.v.range.x));
			materialForRendering.SetVector("_HSV_MAX", new Vector4(avs.h.range.y, avs.s.range.y, avs.v.range.y));
		}
	}

	private int GetGradientMode(PickerType type)
	{
		int num = ((!IsHorizontal(pickers[(int)type])) ? 1 : 0);
		switch (type)
		{
		case PickerType.Main:
			return 2;
		case PickerType.H:
			return 3 + num;
		default:
			return num;
		}
	}

	private bool IsPickerAvailable(PickerType type)
	{
		return IsPickerAvailable((int)type);
	}

	private bool IsPickerAvailable(int index)
	{
		if (index < 0 || index >= pickers.Length)
		{
			return false;
		}
		Picker picker = pickers[index];
		if (picker.image == null || !picker.image.gameObject.activeInHierarchy)
		{
			return false;
		}
		return true;
	}

	private void UpdateHex()
	{
		if (!(hexInput == null) && hexInput.gameObject.activeInHierarchy)
		{
			hexInput.SetTextWithoutNotify("#" + ColorUtility.ToHtmlStringRGB(color));
		}
	}

	private void TypeHex(string input, bool finish)
	{
		if (typeUpdate)
		{
			return;
		}
		typeUpdate = true;
		string sanitizedHex = GetSanitizedHex(input, finish);
		string sanitizedHex2 = GetSanitizedHex(input, full: true);
		int caretPosition = hexInput.caretPosition;
		hexInput.SetTextWithoutNotify(sanitizedHex);
		if (hexInput.caretPosition == 0)
		{
			hexInput.caretPosition = 1;
		}
		else if (sanitizedHex.Length == 2)
		{
			hexInput.caretPosition = 2;
		}
		else if (input.Length > sanitizedHex.Length && caretPosition < input.Length)
		{
			hexInput.caretPosition = caretPosition - input.Length + sanitizedHex.Length;
		}
		if (ColorUtility.TryParseHtmlString(sanitizedHex2, out var arg))
		{
			if (bufferedColor != null)
			{
				bufferedColor.Set(arg);
				UpdateMarkers();
				UpdateTextures();
				onColorChange.Invoke(arg);
			}
			else
			{
				startingColor = arg;
			}
		}
	}

	private void MakeModeOptions()
	{
		if (modeDropdown == null || !modeDropdown.gameObject.activeInHierarchy)
		{
			return;
		}
		modeDropdown.ClearOptions();
		List<string> list = new List<string>();
		foreach (MainPickingMode value in Enum.GetValues(typeof(MainPickingMode)))
		{
			list.Add(value.ToString());
		}
		modeDropdown.AddOptions(list);
		UpdateMode(mode);
	}

	private void UpdateMode(MainPickingMode mode)
	{
		lastUpdatedMode = mode;
		if (!(modeDropdown == null) && modeDropdown.gameObject.activeInHierarchy)
		{
			modeDropdown.value = (int)mode;
		}
	}

	private static bool IsPreviewType(PickerType type)
	{
		switch (type)
		{
		case PickerType.Preview:
			return true;
		case PickerType.PreviewAlpha:
			return true;
		default:
			return false;
		}
	}

	private static bool IsAlphaType(PickerType type)
	{
		switch (type)
		{
		case PickerType.A:
			return true;
		case PickerType.PreviewAlpha:
			return true;
		default:
			return false;
		}
	}

	private static bool IsHorizontal(Picker p)
	{
		Vector2 size = p.image.rectTransform.rect.size;
		return size.x >= size.y;
	}

	public static string GetSanitizedHex(string input, bool full)
	{
		if (string.IsNullOrEmpty(input))
		{
			return "#";
		}
		List<char> list = new List<char>();
		list.Add('#');
		int num = 0;
		char[] array = input.ToCharArray();
		while (list.Count < 7 && num < input.Length)
		{
			char c = char.ToUpper(array[num++]);
			if (IsValidHexChar(c))
			{
				list.Add(c);
			}
		}
		while (full && list.Count < 7)
		{
			list.Insert(1, '0');
		}
		return new string(list.ToArray());
	}

	private static bool IsValidHexChar(char c)
	{
		return char.IsNumber(c) | ((c >= 'A') & (c <= 'F'));
	}

	public static Color ParseHex(string input)
	{
		return ParseHex(input, Color.black);
	}

	public static Color ParseHex(string input, Color defaultColor)
	{
		if (ColorUtility.TryParseHtmlString(GetSanitizedHex(input, full: true), out var result))
		{
			return result;
		}
		return defaultColor;
	}

	private static Vector2 GetNormalizedPointerPosition(Canvas canvas, RectTransform rect, BaseEventData e)
	{
		switch (canvas.renderMode)
		{
		case RenderMode.ScreenSpaceCamera:
			if (canvas.worldCamera == null)
			{
				return GetNormScreenSpace(rect, e);
			}
			return GetNormWorldSpace(canvas, rect, e);
		case RenderMode.ScreenSpaceOverlay:
			return GetNormScreenSpace(rect, e);
		case RenderMode.WorldSpace:
			if (canvas.worldCamera == null)
			{
				Debug.LogError("FCP in world space render mode requires an event camera to be set up on the parent canvas!");
				return Vector2.zero;
			}
			return GetNormWorldSpace(canvas, rect, e);
		default:
			return Vector2.zero;
		}
	}

	private static Vector2 GetNormScreenSpace(RectTransform rect, BaseEventData e)
	{
		Vector2 position = ((PointerEventData)e).position;
		Vector2 vector = rect.worldToLocalMatrix.MultiplyPoint(position);
		float x = Mathf.Clamp01(vector.x / rect.rect.size.x + rect.pivot.x);
		float y = Mathf.Clamp01(vector.y / rect.rect.size.y + rect.pivot.y);
		return new Vector2(x, y);
	}

	private static Vector2 GetNormWorldSpace(Canvas canvas, RectTransform rect, BaseEventData e)
	{
		Vector2 position = ((PointerEventData)e).position;
		Ray ray = canvas.worldCamera.ScreenPointToRay(position);
		new Plane(canvas.transform.forward, canvas.transform.position).Raycast(ray, out var enter);
		Vector3 point = ray.origin + enter * ray.direction;
		Vector2 vector = rect.worldToLocalMatrix.MultiplyPoint(point);
		float x = Mathf.Clamp01(vector.x / rect.rect.size.x + rect.pivot.x);
		float y = Mathf.Clamp01(vector.y / rect.rect.size.y + rect.pivot.y);
		return new Vector2(x, y);
	}

	public static Color HSVToRGB(Vector3 hsv)
	{
		return HSVToRGB(hsv.x, hsv.y, hsv.z);
	}

	public static Color HSVToRGB(float h, float s, float v)
	{
		float num = s * v;
		float num2 = v - num;
		float num3 = num * (1f - Mathf.Abs(h % 2f - 1f)) + num2;
		num += num2;
		switch (Mathf.FloorToInt(h % 6f))
		{
		case 0:
			return new Color(num, num3, num2);
		case 1:
			return new Color(num3, num, num2);
		case 2:
			return new Color(num2, num, num3);
		case 3:
			return new Color(num2, num3, num);
		case 4:
			return new Color(num3, num2, num);
		case 5:
			return new Color(num, num2, num3);
		default:
			return Color.black;
		}
	}

	public static Vector3 RGBToHSV(Color color)
	{
		float r = color.r;
		float g = color.g;
		float b = color.b;
		return RGBToHSV(r, g, b);
	}

	public static Vector3 RGBToHSV(float r, float g, float b)
	{
		float num = Mathf.Max(r, g, b);
		float num2 = Mathf.Min(r, g, b);
		float num3 = num - num2;
		float x = 0f;
		if (num3 > 0f)
		{
			if (r >= b && r >= g)
			{
				x = Mathf.Repeat((g - b) / num3, 6f);
			}
			else if (g >= r && g >= b)
			{
				x = (b - r) / num3 + 2f;
			}
			else if (b >= r && b >= g)
			{
				x = (r - g) / num3 + 4f;
			}
		}
		float y = ((num == 0f) ? 0f : (num3 / num));
		float z = num;
		return new Vector3(x, y, z);
	}
}
