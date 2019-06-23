using UnityEngine;

public class ConfigManager : MonoBehaviour
{
    public AnimationCfg Animation => animationCfg;
    public BubblesConfig Bubbles => bubblesCfg;
    public GridConfig Grid => gridCfg;

    [SerializeField] private AnimationCfg animationCfg = null;
    [SerializeField] private BubblesConfig bubblesCfg = null;
    [SerializeField] private GridConfig gridCfg = null;
}
