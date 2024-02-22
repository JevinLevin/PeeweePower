using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DamageTint : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        GameManager.playerController.OnStun += PlayTint;
    }

    private void OnDisable()
    {
        GameManager.playerController.OnStun -= PlayTint;
    }

    private void PlayTint()
    {
        canvasGroup.DOFade(1.0f, 0.25f).OnComplete(() => canvasGroup.DOFade(0.0f, 2.0f));
    }
}
