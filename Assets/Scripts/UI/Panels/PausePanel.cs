using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PausePanel : MonoBehaviour
{
    public event System.Action OnResumePressed = () => { };

    [SerializeField] private Button resumeButon = null;
    void Start()
    {
        resumeButon.onClick.AddListener(() =>
        {
            OnResumePressed();
        }); ;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
