using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Highscores : MonoBehaviour
{
    private HighScoreItem[] highscores = new HighScoreItem[5];

    private CanvasGroup canvasGroup;
    
    [SerializeField] private TMP_InputField textField;
    [SerializeField] private GameObject newHighscore;
    [SerializeField] private GameObject highscoreDisplay;
    [SerializeField] private HighscoreDisplay[] displays;
    [SerializeField] private HighscoreDisplay yourScore;

    private string tempName;
    private float tempScore;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        LoadHighScores();
        canvasGroup.alpha = 0.0f;
    }

    public void FadeIn()
    {
        canvasGroup.DOFade(1.0f, 1.0f);
    }

    public void FadeOut()
    {
        canvasGroup.DOFade(0.0f, 1.0f).OnComplete(() => { newHighscore.SetActive(false); highscoreDisplay.SetActive(false);}
    );
}

    public void CheckHighscore(float score)
    {
        tempName = "";
        tempScore = score;
        
        if ( score > highscores.Min(item => item.score))
        {
            StartCoroutine(WaitForName());
            return;
        }
        
        DisplayHighScores(false);
    }

    private void AddHighscore(string playerName)
    {
        highscores[^1].name = playerName;
        highscores[^1].score = tempScore;

        highscores = highscores.OrderByDescending(item => item.score).ToArray();
        
        SaveHighscore();
        DisplayHighScores(true);
    }

    private IEnumerator WaitForName()
    {
        highscoreDisplay.SetActive(false);
        newHighscore.SetActive(true);

        while (tempName == "")
            yield return null;

        AddHighscore(tempName);
    }

    public void AcceptName()
    {
        string text = textField.text;
        if (text.Length == 0)
            return;
        
        tempName = text;
    }

    private void SaveHighscore()
    {
        string json = JsonUtility.ToJson(new HighScoreList { highscoresSerialized = highscores });

        PlayerPrefs.SetString("HighScores", json);
        PlayerPrefs.Save();
    }

    private void DisplayHighScores(bool newHighscore)
    {
        if (!newHighscore)
        {
            yourScore.score.text = tempScore.ToString(CultureInfo.CurrentCulture);
            yourScore.gameObject.SetActive(true);
        }
        else
            yourScore.gameObject.SetActive(false);
        
        this.newHighscore.SetActive(false);
        highscoreDisplay.SetActive(true);


        for (int i = 0; i < highscores.Length; i++)
        {
            
            displays[i].playerName.text = highscores[i].name;
            if(highscores[i].score != 0.0f)
                displays[i].score.text = highscores[i].score.ToString();
        }
    }
    
    public void LoadHighScores()
    {
        if (!PlayerPrefs.HasKey("HighScores"))
        {
            highscores = new HighScoreItem[5];
            SaveHighscore();
            return;
        }
        
        string json = PlayerPrefs.GetString("HighScores");

        
        HighScoreList loadedScores = JsonUtility.FromJson<HighScoreList>(json);
        highscores = loadedScores.highscoresSerialized;
        
    }

    public void Retry()
    {
        FadeOut();
        StartCoroutine(GameManager.Instance.Retry(1.0f));
    }

    public void MainMenu()
    {
        FadeOut();
        StartCoroutine(GameManager.Instance.ReturnToMenu(1.0f));
    }
}

[System.Serializable]
public class HighScoreItem
{
    public string name;
    public float score;
}

[System.Serializable]
public class HighScoreList
{
    public HighScoreItem[] highscoresSerialized;
}
