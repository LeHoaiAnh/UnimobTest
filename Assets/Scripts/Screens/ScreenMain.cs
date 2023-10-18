using Hara.GUI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScreenMain : ScreenBase
{
    [Header("Btn Groups")]
    [SerializeField] private Button spawn20;
    [SerializeField] private Button spawn50;
    [SerializeField] private Button spawn100;

    [Header("FPS Text")]
    [SerializeField] private TextMeshProUGUI fpsText;
    private float poolingTime = 1f;
    private float time;
    private int frameCount;
    private int curFrameCount;

    private void Start()
    {
        spawn20.onClick.AddListener(() => { GameController.instance.SpawnSpiders(20); }) ;
        spawn50.onClick.AddListener(() => { GameController.instance.SpawnSpiders(50); }) ;
        spawn100.onClick.AddListener(() => { GameController.instance.SpawnSpiders(100); }) ;

        curFrameCount = -1;
    }


    private void Update()
    {
        time += Time.deltaTime;
        frameCount++;

        if (time >= poolingTime)
        {
            int frameRate = Mathf.CeilToInt(frameCount / time);

            if (frameRate != curFrameCount && frameRate > 0)
            {
                curFrameCount = frameRate;
                fpsText.text = frameRate.ToString() + " FPS";
            }
        }
    }
}
