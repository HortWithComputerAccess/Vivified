using TMPro;
using UnityEngine;

public class DifficultyInfo : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField bpmField;

	[SerializeField]
	private TMP_InputField reactionTimeField;

	[SerializeField]
	private TMP_InputField halfJumpDurationField;

	[SerializeField]
	private TMP_InputField jumpDistanceField;

	[SerializeField]
	private TMP_InputField njsField;

	[SerializeField]
	private TMP_InputField songBeatOffsetField;

	public void Start()
	{
		njsField.onValueChanged.AddListener(delegate
		{
			UpdateValues();
		});
		songBeatOffsetField.onValueChanged.AddListener(delegate
		{
			UpdateValues();
		});
		bpmField.onValueChanged.AddListener(delegate
		{
			UpdateValues();
		});
		jumpDistanceField.onValueChanged.AddListener(delegate
		{
			UpdateValuesFromJumpDistance();
		});
		halfJumpDurationField.onValueChanged.AddListener(delegate
		{
			UpdateValuesFromHalfJumpDuration();
		});
		reactionTimeField.onValueChanged.AddListener(delegate
		{
			UpdateValuesFromReactionTime();
		});
		reactionTimeField.onSelect.AddListener(delegate
		{
			RemoveMsFromText();
		});
		jumpDistanceField.onDeselect.AddListener(delegate
		{
			UpdateValues();
		});
		halfJumpDurationField.onDeselect.AddListener(delegate
		{
			UpdateValues();
		});
		reactionTimeField.onDeselect.AddListener(delegate
		{
			UpdateValues();
		});
	}

	private void UpdateValues()
	{
		float.TryParse(bpmField.text, out var result);
		float.TryParse(njsField.text, out var result2);
		float.TryParse(songBeatOffsetField.text, out var result3);
		float num = SpawnParameterHelper.CalculateHalfJumpDuration(result2, result3, result);
		float num2 = 60f / result;
		float num3 = result2 * num2 * num * 2f;
		float num4 = 60000f / result * num;
		if (!halfJumpDurationField.isFocused)
		{
			halfJumpDurationField.SetTextWithoutNotify(num.ToString());
		}
		if (!jumpDistanceField.isFocused)
		{
			jumpDistanceField.SetTextWithoutNotify(num3.ToString("0.00"));
		}
		if (!reactionTimeField.isFocused)
		{
			reactionTimeField.SetTextWithoutNotify(num4.ToString("N0") + " ms");
		}
	}

	private void RemoveMsFromText()
	{
		reactionTimeField.text = reactionTimeField.text.Split()[0];
	}

	private void UpdateValuesFromReactionTime()
	{
		float.TryParse(bpmField.text, out var result);
		float.TryParse(reactionTimeField.text, out var result2);
		SetSongBeatOffset(Mathf.Max(0.25f, result2 / (60000f / result)));
	}

	private void UpdateValuesFromJumpDistance()
	{
		float.TryParse(bpmField.text, out var result);
		float.TryParse(njsField.text, out var result2);
		float.TryParse(jumpDistanceField.text, out var result3);
		SetSongBeatOffset(Mathf.Max(0.25f, result3 / (60f / result * result2 * 2f)));
	}

	private void UpdateValuesFromHalfJumpDuration()
	{
		float.TryParse(halfJumpDurationField.text, out var result);
		SetSongBeatOffset(Mathf.Max(0.25f, result));
	}

	private void SetSongBeatOffset(float hjdAfterOffset)
	{
		float.TryParse(bpmField.text, out var result);
		float.TryParse(njsField.text, out var result2);
		float num = SpawnParameterHelper.CalculateHalfJumpDuration(result2, 0f, result);
		songBeatOffsetField.text = (hjdAfterOffset - num).ToString();
	}
}
