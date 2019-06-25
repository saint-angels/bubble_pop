using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TextLabelFader))]
public abstract class FadingTextBase : MonoBehaviour
{
    [SerializeField] protected TMPro.TextMeshProUGUI textLabel = null;

    protected RectTransform rectTransform;

    private TextLabelFader textFader;

    public virtual void Init(string newText)
    {
        textLabel.text = newText;
        textFader.Init(textLabel);
    }

    public void Fade(TextLabelFader.FadeType fadeType, bool withRandomOffset = true)
    {
        textFader.StartFade(withRandomOffset, fadeType);
    }

    private void Awake()
    {
        rectTransform = textLabel.GetComponent<RectTransform>();
        textFader = textLabel.GetComponent<TextLabelFader>();

        textFader.OnFadeCompleted += OnFadeCompleted;
    }

    protected abstract void OnFadeCompleted();
}
