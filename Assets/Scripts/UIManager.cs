using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public event System.Action<bool> OnGamePauseShown = (isShown) => { };

    [SerializeField] Canvas canvas;
    [SerializeField] private Button pauseButton = null;

    [Header("Panels")]
    [SerializeField] private RectTransform bubbleHudsContainer = null;
    [SerializeField] private RectTransform topPanel = null;
    [SerializeField] private RectTransform centerPanel = null;
    [SerializeField] private PausePanel pausePanel = null;

    [Header("Prefabs")]
    [SerializeField] private ScoreLabel scoreLabelPrefab = null;
    [SerializeField] private ComboLabel comboLabelPrefab = null;
    [SerializeField] private BubbleHud bubbleHudPrefab = null;
    [SerializeField] private PerfectLabel perfectLabelPrefab = null;

    private Dictionary<Bubble, BubbleHud> bubbleHuds = new Dictionary<Bubble, BubbleHud>();
    private ScoreLabel currentScoreLabel;
    private ComboLabel currentComboLabel;

    private AnimationCfg animationCfg;

    public void AddHudToBubble(Bubble bubble)
    {
        BubbleHud newHud = ObjectPool.Spawn(bubbleHudPrefab, Vector3.zero, Quaternion.identity, bubbleHudsContainer);
        newHud.Init(bubble.Power);

        UpdateBubbleHudPosition(bubble, newHud);

        bubbleHuds.Add(bubble, newHud);

        bubble.OnDeath -= Bubble_OnDeath;
        bubble.OnDeath += Bubble_OnDeath;

        bubble.OnUpgrade -= Bubble_OnUpgrade;
        bubble.OnUpgrade += Bubble_OnUpgrade;
    }

    private void Bubble_OnUpgrade(Bubble bubble)
    {
        bubbleHuds[bubble].Init(bubble.Power);
    }

    private void Bubble_OnDeath(Bubble bubble)
    {
        bubbleHuds[bubble].Fade(TextLabelFader.FadeType.MoveUp);
        bubbleHuds.Remove(bubble);
    }

    private void OnScoreUpdated(ulong newScore)
    {
        if (currentScoreLabel != null)
        {
            currentScoreLabel.Fade(TextLabelFader.FadeType.MoveUp, false);
        }
        
        currentScoreLabel = ObjectPool.Spawn(scoreLabelPrefab, Vector3.zero, Quaternion.identity, topPanel);
        currentScoreLabel.Init(NumberFormatHelper.FormatNumberScore(newScore));
    }

    private void OnComboUpdated(uint newCombo)
    {
        if (currentComboLabel != null)
        {
            currentComboLabel.Fade(TextLabelFader.FadeType.MoveUp, false);
        }

        currentComboLabel = ObjectPool.Spawn(comboLabelPrefab, Vector3.zero, Quaternion.identity, topPanel);
        currentComboLabel.Init($"x{newCombo}");
    }

    private void OnGridCleared()
    {
        PerfectLabel newPerfectLabel = ObjectPool.Spawn(perfectLabelPrefab, Vector3.zero, Quaternion.identity, centerPanel);
        newPerfectLabel.Init("PERFECT");
        newPerfectLabel.Fade(TextLabelFader.FadeType.MoveUp, false);
    }

    private void UpdateBubbleHudPosition(Bubble bubble, BubbleHud hud)
    {
        Vector2 screenPoint = Root.Instance.CameraController.WorldToScreenPoint(bubble.transform.position);
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(bubbleHudsContainer, screenPoint, null, out localPoint))
        {
            hud.transform.localPosition = localPoint;
        }
    }

    private void Start()
    {
        animationCfg = Root.Instance.ConfigManager.Animation;

        pauseButton.onClick.AddListener(() =>
        {
            pausePanel.gameObject.SetActive(true);
            OnGamePauseShown(true);
        });

        pausePanel.OnResumePressed += () => 
        {
            pausePanel.gameObject.SetActive(false);
            OnGamePauseShown(false);
        };


        Root.Instance.GameController.OnScoreUpdated += OnScoreUpdated;
        Root.Instance.GameController.OnComboUpdated += OnComboUpdated;
        Root.Instance.GameController.OnGridCleared += OnGridCleared;
        Root.Instance.GameController.OnMergeCombo += OnMergeCombo;

        //Clear the score & combo on start
        OnScoreUpdated(0);
        OnComboUpdated(0);
    }

    private void OnMergeCombo(uint mergeCombo)
    {
        if (mergeCombo >= 2)
        {
            PerfectLabel newPerfectLabel = ObjectPool.Spawn(perfectLabelPrefab, Vector3.zero, Quaternion.identity, centerPanel);
            newPerfectLabel.Init($"x{mergeCombo}");
            newPerfectLabel.Fade(TextLabelFader.FadeType.ZoomIn, false);
        }
    }

    void LateUpdate()
    {
        foreach (var bubbleHudPair in bubbleHuds)
        {
            Bubble bubble = bubbleHudPair.Key;
            BubbleHud hud = bubbleHudPair.Value;
            UpdateBubbleHudPosition(bubble, hud);
        }
    }

}
