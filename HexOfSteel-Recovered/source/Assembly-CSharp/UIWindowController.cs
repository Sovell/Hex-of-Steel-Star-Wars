using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public abstract class UIWindowController : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IPointerEnterHandler, IPointerExitHandler
{
	private enum Actions
	{
		None,
		Drag,
		Resize
	}

	private enum States
	{
		Maximized,
		Minimized
	}

	[Header("Minimization/Maximization")]
	public bool canMinimizeMaximize;

	public Vector2 minimizedSize = new Vector2(500f, 80f);

	public GameObject minimizeButton;

	public GameObject maximizeButton;

	[Header("Behaviour")]
	[Tooltip("Pixels from the edge that trigger a resize.")]
	public float resizeBorder = 25f;

	[Tooltip("Pixels from the edge (outside the resize band) that trigger a drag.")]
	public float dragBorderUpDown = 100f;

	public float dragBorderLeftRight = 50f;

	[Header("Constraints")]
	public Vector2 minWindowSize = new Vector2(500f, 300f);

	[Header("Optional custom cursors")]
	public bool useCustomCursors;

	public Texture2D regularCursor;

	public Texture2D cursorMove;

	public Texture2D cursorResizeHorizontal;

	public Texture2D cursorResizeVertical;

	public Texture2D cursorResizeDiag1;

	public Texture2D cursorResizeDiag2;

	private Actions currentAction;

	private States currentState;

	private bool left;

	private bool right;

	private bool top;

	private bool bottom;

	private Canvas rootCanvas;

	private RectTransform rect;

	private bool pointerInside;

	private Vector2 prevPointerPos;

	private Vector2 prevSizeDelta;

	public event Action OnMinimized;

	public event Action OnMaximized;

	protected virtual void Awake()
	{
		rect = (RectTransform)transform;
		rootCanvas = GetComponentInParent<Canvas>();
	}

	protected virtual void Start()
	{
		if (canMinimizeMaximize)
		{
			OnMinimized += OnStateChanged;
			OnMaximized += OnStateChanged;
		}
	}

	public void Close()
	{
		SoundManager.instance.PlayUIClickSound();
		gameObject.SetActive(value: false);
	}

	public void Open()
	{
		SoundManager.instance.PlayUIClickSound();
		gameObject.SetActive(value: true);
	}

	public void Minimize()
	{
		if (canMinimizeMaximize)
		{
			SoundManager.instance.PlayUIClickSound();
			currentState = States.Minimized;
			prevSizeDelta = rect.sizeDelta;
			SetPivotAndHeightKeepTop(rect, 1f, minimizedSize);
			OnMinimized?.Invoke();
		}
	}

	public void Maximize()
	{
		if (canMinimizeMaximize)
		{
			SoundManager.instance.PlayUIClickSound();
			currentState = States.Maximized;
			SetPivotAndHeightKeepTop(rect, 0.5f, minWindowSize);
			OnMaximized?.Invoke();
		}
	}

	private void OnStateChanged()
	{
		RefreshSizeButtons();
		if (currentState == States.Maximized)
		{
			rect.sizeDelta = prevSizeDelta;
		}
		ClampToParent();
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)rect.parent);
	}

	private void SetPivotAndHeightKeepTop(RectTransform rt, float pivotY, Vector2 newSize)
	{
		float height = rt.rect.height;
		float y = rt.pivot.y;
		float num = rt.anchoredPosition.y + (1f - y) * height;
		rt.pivot = new Vector2(rt.pivot.x, pivotY);
		rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSize.y);
		rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSize.x);
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, num - (1f - pivotY) * newSize.y);
	}

	private void RefreshSizeButtons()
	{
		minimizeButton.SetActive(currentState == States.Maximized);
		maximizeButton.SetActive(currentState == States.Minimized);
	}

	public void OnPointerEnter(PointerEventData e)
	{
		pointerInside = true;
		UpdateCursor(e);
	}

	public void OnPointerExit(PointerEventData e)
	{
		pointerInside = false;
		ResetCursor();
	}

	public void OnPointerDown(PointerEventData e)
	{
		prevPointerPos = e.position;
	}

	public void OnBeginDrag(PointerEventData e)
	{
		ResolveActionAndEdges(e, out currentAction, out left, out right, out top, out bottom);
		if (currentAction == Actions.Resize && currentState == States.Minimized)
		{
			currentAction = Actions.Drag;
		}
	}

	public void OnDrag(PointerEventData e)
	{
		if (currentAction != Actions.None)
		{
			Vector2 vector = (e.position - prevPointerPos) / rootCanvas.scaleFactor;
			prevPointerPos = e.position;
			if (currentAction == Actions.Drag)
			{
				rect.anchoredPosition += vector;
			}
			else if (currentAction == Actions.Resize)
			{
				Resize(vector);
			}
			ClampToParent();
			UpdateCursor(e);
		}
	}

	public void OnPointerUp(PointerEventData e)
	{
		currentAction = Actions.None;
		ResetEdges();
		ResetCursor();
	}

	private void Update()
	{
		if (pointerInside && currentAction == Actions.None)
		{
			PointerEventData e = new PointerEventData(EventSystem.current)
			{
				position = Input.mousePosition
			};
			UpdateCursor(e);
		}
	}

	private void Resize(Vector2 pointerDelta)
	{
		Vector2 sizeDelta = rect.sizeDelta;
		Vector2 anchoredPosition = rect.anchoredPosition;
		Vector2 vector = sizeDelta;
		if (left)
		{
			vector.x -= pointerDelta.x;
		}
		if (right)
		{
			vector.x += pointerDelta.x;
		}
		if (bottom)
		{
			vector.y -= pointerDelta.y;
		}
		if (top)
		{
			vector.y += pointerDelta.y;
		}
		RectTransform rectTransform = rect.parent as RectTransform;
		Vector2 vector2 = (rectTransform ? rectTransform.rect.size : new Vector2(float.MaxValue, float.MaxValue));
		vector.x = Mathf.Clamp(vector.x, minWindowSize.x, vector2.x);
		vector.y = Mathf.Clamp(vector.y, minWindowSize.y, vector2.y);
		Vector2 vector3 = vector - sizeDelta;
		Vector2 anchoredPosition2 = anchoredPosition;
		if (left)
		{
			anchoredPosition2.x -= vector3.x * (1f - rect.pivot.x);
		}
		if (right)
		{
			anchoredPosition2.x += vector3.x * rect.pivot.x;
		}
		if (bottom)
		{
			anchoredPosition2.y -= vector3.y * (1f - rect.pivot.y);
		}
		if (top)
		{
			anchoredPosition2.y += vector3.y * rect.pivot.y;
		}
		rect.sizeDelta = vector;
		rect.anchoredPosition = anchoredPosition2;
	}

	private void ClampToParent()
	{
		RectTransform rectTransform = rect.parent as RectTransform;
		if ((bool)rectTransform)
		{
			Vector3[] array = new Vector3[4];
			Vector3[] array2 = new Vector3[4];
			rect.GetWorldCorners(array);
			rectTransform.GetWorldCorners(array2);
			Vector3 zero = Vector3.zero;
			if (array[0].x < array2[0].x)
			{
				zero.x = array2[0].x - array[0].x;
			}
			else if (array[2].x > array2[2].x)
			{
				zero.x = array2[2].x - array[2].x;
			}
			if (array[0].y < array2[0].y)
			{
				zero.y = array2[0].y - array[0].y;
			}
			else if (array[2].y > array2[2].y)
			{
				zero.y = array2[2].y - array[2].y;
			}
			rect.position += zero;
			if (rect.rect.width > rectTransform.rect.width || rect.rect.height > rectTransform.rect.height)
			{
				Vector2 sizeDelta = rect.sizeDelta;
				sizeDelta.x = Mathf.Min(sizeDelta.x, rectTransform.rect.width);
				sizeDelta.y = Mathf.Min(sizeDelta.y, rectTransform.rect.height);
				rect.sizeDelta = new Vector2(Mathf.Max(sizeDelta.x, minWindowSize.x), Mathf.Max(sizeDelta.y, minWindowSize.y));
			}
		}
	}

	private void UpdateCursor(PointerEventData e)
	{
		if (!useCustomCursors)
		{
			return;
		}
		if (currentAction == Actions.Drag)
		{
			Cursor.SetCursor(cursorMove, Vector2.zero, CursorMode.Auto);
			return;
		}
		if (currentAction == Actions.Resize)
		{
			SetResizeCursor(left, right, top, bottom);
			return;
		}
		ResolveActionAndEdges(e, out var act, out var l, out var r, out var t, out var b);
		switch (act)
		{
		case Actions.Resize:
			SetResizeCursor(l, r, t, b);
			break;
		case Actions.Drag:
			Cursor.SetCursor(cursorMove, Vector2.zero, CursorMode.Auto);
			break;
		default:
			ResetCursor();
			break;
		}
	}

	private void SetResizeCursor(bool l, bool r, bool t, bool b)
	{
		if (useCustomCursors)
		{
			if ((l & t) || (r & b))
			{
				Cursor.SetCursor(cursorResizeDiag2, Vector2.zero, CursorMode.Auto);
			}
			else if ((l & b) || (r & t))
			{
				Cursor.SetCursor(cursorResizeDiag1, Vector2.zero, CursorMode.Auto);
			}
			else if (l | r)
			{
				Cursor.SetCursor(cursorResizeHorizontal, Vector2.zero, CursorMode.Auto);
			}
			else if (t | b)
			{
				Cursor.SetCursor(cursorResizeVertical, Vector2.zero, CursorMode.Auto);
			}
		}
	}

	private void ResetCursor()
	{
		if (useCustomCursors)
		{
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		}
	}

	private void ResolveActionAndEdges(PointerEventData e, out Actions act, out bool l, out bool r, out bool t, out bool b)
	{
		act = Actions.None;
		l = (r = (t = (b = false)));
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, e.position, e.pressEventCamera, out var localPoint);
		Vector2 size = rect.rect.size;
		float num = size.x * 0.5f;
		float num2 = size.y * 0.5f;
		bool flag = localPoint.x <= 0f - num + resizeBorder;
		bool flag2 = localPoint.x >= num - resizeBorder;
		bool flag3 = localPoint.y <= 0f - num2 + resizeBorder;
		bool flag4 = localPoint.y >= num2 - resizeBorder;
		if (flag | flag2 | flag4 | flag3)
		{
			act = Actions.Resize;
			l = flag;
			r = flag2;
			t = flag4;
			b = flag3;
		}
		else if (Mathf.Abs(localPoint.x) >= num - dragBorderLeftRight || Mathf.Abs(localPoint.y) >= num2 - dragBorderUpDown)
		{
			act = Actions.Drag;
		}
	}

	private void ResetEdges()
	{
		left = (right = (top = (bottom = false)));
	}
}
