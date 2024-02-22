using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StunIndicator : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image progressBar;
    [SerializeField] private PlayerController player;

    private float defaultAlpha;
    private void Start()
    {
        defaultAlpha = canvasGroup.alpha;
        canvasGroup.alpha = 0.0f;
    }

    private void OnEnable()
    {
        player.OnStunTime += StartStun;
    }

    private void OnDisable()
    {
        player.OnStunTime -= StartStun;
    }

    private void StartStun(float length)
    {
        StartCoroutine(PlayStun(length));
    }

    private IEnumerator PlayStun(float length)
    {
        float time = 0.0f;
        canvasGroup.DOFade(defaultAlpha, 0.1f);

        while (time < length)
        {
            time += Time.deltaTime;

            progressBar.fillAmount = Mathf.Lerp(1,0, time / length);
            
            yield return null;
        }
        
        canvasGroup.DOFade(0.0f, 0.25f);
    }
}
