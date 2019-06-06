using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Progressbar : MonoBehaviour
{
    private TextMeshProUGUI textValue;
    private TextMeshProUGUI TextValue
    {
        get
        {
            if (textValue == null)
            {
                textValue = GetComponentInChildren<TextMeshProUGUI>();
            }
            return textValue;
        }
    }

    private Slider slider;
    private Slider Slider
    {
        get
        {
            if (slider == null)
            {
                slider = GetComponent<Slider>();
            }
            return slider;
        }
    }

    private RectTransform rectTransform;
    public RectTransform RectTransform
    {
        get
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
            return rectTransform;
        }
    }

    private int PrevValue;
    private int PrevMaxValue;

    public void SetValue01(float value)
    {
        SetValue(value, 1f);
    }

    public void SetValue(float value, float maxValue)
    {
        float normValue = 0f;
        if (maxValue > 0f)
        {
            normValue = (value / maxValue);
        }

        // TODO setting Slider.value allocates 20B of memory
        Slider.value = Mathf.Clamp(normValue, 0f, maxValue);
        if (TextValue != null)
        {
            int intValue = (int)value;
            int intMaxValue = (int)maxValue;

            if (intValue != PrevValue || intMaxValue != PrevMaxValue)
            {
                TextValue.text = string.Format("{0}/{1}", intValue, intMaxValue);
                PrevValue = intValue;
                PrevMaxValue = intMaxValue;
            }
        }
    }
}