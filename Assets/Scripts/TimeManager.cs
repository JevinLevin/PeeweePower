using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private float startingTime;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerBar;

    private float maxTime;

    private float currentTime;

    private void Awake()
    {
        maxTime = startingTime;
    }

    private void OnEnable()
    {
        GameManager.timeManager = this;
    }

    private void Update()
    {
        currentTime += Time.deltaTime;

        float t = currentTime / maxTime;

        timerBar.fillAmount = 1 - t;

        int seconds = (int)(maxTime - currentTime);
        int minutes = seconds / 60;
        seconds = seconds % 60;
        string text = string.Format("{0:00}:{1:00}", minutes, seconds);
        timerText.text = "Time: " + text;


    }

    public void AddTime(float time)
    {
        currentTime = Mathf.Max(currentTime - time,0);
    }
}
