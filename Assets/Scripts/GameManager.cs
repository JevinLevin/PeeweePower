using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    private void Awake()
    {
        CreateSingleton();


    }
}
