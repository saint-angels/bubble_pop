using UnityEngine;

public class ConfigManager : MonoBehaviour
{
    public AnimationCfg Animation => animationCfg;
    public BubblesConfig Bubbles => bubblesCfg;

    [SerializeField] private AnimationCfg animationCfg = null;
    [SerializeField] private BubblesConfig bubblesCfg = null;
}
