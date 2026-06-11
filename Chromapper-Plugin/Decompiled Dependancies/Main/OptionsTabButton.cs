using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class OptionsTabButton : UIBehaviour, IPointerExitHandler, IEventSystemHandler, IPointerEnterHandler
{
	[FormerlySerializedAs("hovering")]
	[HideInInspector]
	public bool Hovering;

	[FormerlySerializedAs("textMeshTabName")]
	public TextMeshProUGUI TextMeshTabName;

	[FormerlySerializedAs("discordPopout")]
	public RectTransform DiscordPopout;

	[FormerlySerializedAs("discordPopoutCanvas")]
	public CanvasGroup DiscordPopoutCanvas;

	[FormerlySerializedAs("icon")]
	public Image Icon;

	private readonly Color iconColorHover = new Color(0f, 0.5f, 1f, 1f);

	private readonly Color iconColorSelected = new Color(0.78f, 0.47f, 0f, 1f);

	private Coroutine discordPopoutCoroutine;

	private TabManager tabManager;

	protected override void Start()
	{
		tabManager = base.transform.GetComponentInParent<TabManager>();
	}

	private void LateUpdate()
	{
		if (tabManager.SelectedTab == this)
		{
			Icon.color = iconColorSelected;
		}
		else if (!Hovering)
		{
			Icon.color = Color.white;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (tabManager.SelectedTab != this)
		{
			Icon.color = iconColorHover;
		}
		Hovering = true;
		discordPopoutCoroutine = StartCoroutine(SlideText());
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (tabManager.SelectedTab != this)
		{
			Icon.color = Color.white;
		}
		Hovering = false;
		discordPopoutCoroutine = StartCoroutine(SlideText());
	}

	public void RefreshWidth()
	{
		Vector2 sizeDelta = DiscordPopout.sizeDelta;
		DiscordPopout.sizeDelta = new Vector2(TextMeshTabName.preferredWidth + 5f, sizeDelta.y);
	}

	public void ChangeTab()
	{
		tabManager.OnTabSelected(this);
	}

	private IEnumerator SlideText()
	{
		if (discordPopoutCoroutine != null)
		{
			StopCoroutine(discordPopoutCoroutine);
		}
		float startTime = Time.time;
		Vector3 zero = new Vector3(0f, 1f, 1f);
		Vector3 one = new Vector3(1f, 1f, 1f);
		while (true)
		{
			Vector3 localScale = DiscordPopout.localScale;
			localScale = Vector3.MoveTowards(localScale, Hovering ? one : zero, Time.time / startTime * 0.2f);
			DiscordPopout.localScale = localScale;
			DiscordPopoutCanvas.alpha = localScale.x;
			if (localScale.x >= 1f)
			{
				DiscordPopout.localScale = one;
				DiscordPopoutCanvas.alpha = 1f;
				yield break;
			}
			if (localScale.x <= 0f)
			{
				break;
			}
			yield return new WaitForFixedUpdate();
		}
		DiscordPopout.localScale = zero;
		DiscordPopoutCanvas.alpha = 0f;
	}
}
