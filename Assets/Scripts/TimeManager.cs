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
    private float totalTime;

    private void Awake()
    {
        maxTime = startingTime;
        totalTime = 0.0f;
    }

    private void OnEnable()
    {
        GameManager.timeManager = this;
    }

    private void Update()
    {
        if (!GameManager.Active)
            return;
        
        currentTime += Time.deltaTime;
        totalTime += Time.deltaTime;

        float t = currentTime / maxTime;
        
        SetDisplay(t);
        
        if(t >= 1)
            EndTime();
    }

    private void SetDisplay(float t)
    {
        // Alter progress bar
        timerBar.fillAmount = 1 - t;

        // Format time into a readable string
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

    private void EndTime()
    {
        GameManager.Instance.FailGame(totalTime);
        
    }
}
