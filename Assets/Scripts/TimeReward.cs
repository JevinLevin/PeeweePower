using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TimeReward : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    public float distance;
    public float length;
    public CanvasGroup canvasGroup;
    private Camera camera1;

    private void Awake()
    {
        camera1 = Camera.main;
        canvasGroup.alpha = 0.0f;
    }

    private void Update()
    {
        transform.LookAt(transform.position - (camera1!.transform.position - transform.position));
    }

    public void Spawn(float time, float multiplier)
    {
        canvasGroup.alpha = 1.0f;

        string newText = "+" + System.Math.Round(time,2) + "s";
        if (multiplier > 1.0f)
            newText += " x " + System.Math.Round(multiplier,2);

        timeText.text = newText;
        canvasGroup.DOFade(0.0f, length/1.2f).SetEase(Ease.InSine);
        transform.DOMove(Vector3.up * distance, length).SetRelative().OnComplete(() => Destroy(gameObject));
    }
}
