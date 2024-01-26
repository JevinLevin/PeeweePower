using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }


    private void CreateSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
    }
    #endregion

    public static PlayerController playerController;
    public static PlayerAttacker playerAttacker;
    public static TimeManager timeManager;
    public static EnemySpawner enemySpawner;

    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private Highscores highscores;

    public Light directionalLight;
    public Light spotLight;

    public static bool Active;
    private float finalTime;
    
    private void Awake()
    {
        CreateSingleton();
    }

    private void Start()
    {
        StartGame();
    }

    private void StartGame()
    {
        Active = true;
    }



    public void FailGame(float finalTime)
    {
        enemySpawner.Freeze();
        playerController.Die();
        Active = false;

        this.finalTime = finalTime;
        
        
        StartCoroutine(DeathAnimation());
    }

    private IEnumerator DeathAnimation()
    {
        directionalLight.DOIntensity(0, 3.0f);
        spotLight.gameObject.SetActive(true);
        spotLight.DOIntensity(30, 3.0f);
        RenderSettings.ambientIntensity = 0.0f;

        yield return new WaitForSeconds(3.0f);
        
        FadeIn();
        
        yield return new WaitForSeconds(2.0f);
        
        Cursor.lockState = CursorLockMode.None;

        highscores.CheckHighscore(finalTime);
        highscores.FadeIn();
        
    }

    public IEnumerator Retry(float delay = 0.0f)
    {
        yield return new WaitForSeconds(delay);
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        FadeOut(0.5f);
        StartGame();

    }

    private void FadeIn(float length = 2.0f)
    {
        fadeCanvas.DOFade(1.0f, length);
    }
    
    private void FadeOut(float length = 2.0f)
    {
        fadeCanvas.DOFade(0.0f, length);
    }
}
