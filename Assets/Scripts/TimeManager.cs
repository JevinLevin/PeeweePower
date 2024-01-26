using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private float startingTime;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerBar;
    [SerializeField] private float timerTweenScale = 1.05f;
    [SerializeField] private Color timerTweenColor;
    private Color defaultColor;

    private float maxTime;

    private float currentTime;
    private float totalTime;
    private int previousSeconds;
    private Tween scaleTween;
    private Tween colorTween;

    private void Awake()
    {
        maxTime = startingTime;
        totalTime = 0.0f;
        defaultColor = timerBar.color;
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

        if (seconds != previousSeconds)
            ScaleText();

        previousSeconds = seconds;
    }

    private void ScaleText()
    {
        scaleTween.Complete();
        scaleTween = timerText.rectTransform.DOPunchScale(Vector3.one * timerTweenScale, 0.25f, 10, 1f);

    }

    public void AddTime(float time)
    {
        currentTime = Mathf.Max(currentTime - time,0);
        
        colorTween.Complete();
        colorTween = timerBar.DOColor(timerTweenColor, 0.25f).OnComplete(() => timerBar.DOColor(defaultColor, 0.5f));
    }

    private void EndTime()
    {
        GameManager.Instance.FailGame(totalTime);
        
    }
}
