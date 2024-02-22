using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerBar;
    [SerializeField] private Image flashBar;
    [SerializeField] private RectTransform peeweeIcon;
    [SerializeField] private Image peeweeSprite;

    [Header("Sprites")] 
    [SerializeField] private Sprite peeweeStage1;
    [SerializeField] private Sprite peeweeStage2;
    [SerializeField] private Sprite peeweeStage3;

    [Header("Config")]
    [SerializeField] private float timerTweenScale = 1.05f;
    [SerializeField] private float startingTime;
    [SerializeField] private Color flashTweenColor;
    [SerializeField] private Gradient colors;
    
    [Header("Reward")] 
    [SerializeField] private GameObject timeRewardObject;
    [SerializeField] private Canvas timeRewardCanvas;
    
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
        defaultColor = flashBar.color;
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
        flashBar.fillAmount = 1 - t;

        // Format time into a readable string
        int seconds = (int)(maxTime - currentTime);
        seconds = seconds % 60;
        timerText.text = "Time:" + seconds;

        timerBar.color = colors.Evaluate(t);

        if (seconds != previousSeconds)
            ScaleText();
        
        // Set peewee position
        Vector3 leftSide = timerBar.rectTransform.anchoredPosition;      
        Vector3 rightSide = timerBar.rectTransform.anchoredPosition;
        rightSide.x += timerBar.rectTransform.rect.width;

        peeweeSprite.sprite = t < 0.50 ? peeweeStage1 : t < 1f ? peeweeStage2 : peeweeStage3; 

        peeweeIcon.anchoredPosition = Vector3.Lerp(rightSide, leftSide, t);

        previousSeconds = seconds;
    }

    private void ScaleText()
    {
        scaleTween.Complete();
        scaleTween = timerText.rectTransform.DOPunchScale(Vector3.one * timerTweenScale, 0.25f, 10, 1f);

    }

    public void AddTime(float time, Vector3 worldPosition)
    {
        float comboMultiplier = 1 + ((float)GameManager.playerAttacker.GetCombo()-1)/100;

        float finalTime = time * comboMultiplier;
        
        currentTime = Mathf.Max(currentTime - finalTime,0);
        
        colorTween.Complete();
        colorTween = flashBar.DOColor(flashTweenColor, 0.25f).OnComplete(() => flashBar.DOColor(defaultColor, 0.5f));

        TimeReward tempReward = Instantiate(timeRewardObject, timeRewardCanvas.transform).GetComponent<TimeReward>();
        tempReward.transform.position = worldPosition;
        tempReward.Spawn(time,comboMultiplier);
    }

    private void EndTime()
    {
        GameManager.Instance.FailGame(totalTime);
        
    }
}
