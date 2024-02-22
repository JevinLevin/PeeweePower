using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GrannyTimer : MonoBehaviour
{
    
    [SerializeField] private Image timerImage;
    [SerializeField] private float grannyRedFadeTime = 0.25f;

    [SerializeField] private Animator animatorMain;
    [SerializeField] private Animator animatorMask;

    [SerializeField] private TextMeshProUGUI notificationText;

    private void Start()
    {
        animatorMain.speed = 0.0f;
        animatorMask.speed = 0.0f;

        notificationText.alpha = 0.0f;
    }

    public void SetTimeProgress(float t)
    {
        timerImage.fillAmount = t;
    }

    public void SpawnGranny(float cooldownLength)
    {
        StartCoroutine(SpawnAnimation(cooldownLength));
    }

    private IEnumerator SpawnAnimation(float length)
    {
        animatorMain.speed = 1.0f;
        animatorMask.speed = 1.0f;
        
        PlayNotification();
        
        yield return new WaitForSeconds(length - grannyRedFadeTime);

        timerImage.DOFade(0.0f, grannyRedFadeTime);
        
        yield return new WaitForSeconds(grannyRedFadeTime);

        animatorMain.speed = 0.0f;
        animatorMask.speed = 0.0f;
        animatorMain.Play(animatorMain.GetCurrentAnimatorStateInfo(0).fullPathHash, -1, 0f);
        animatorMain.Update(0f);
        animatorMask.Play(animatorMask.GetCurrentAnimatorStateInfo(0).fullPathHash, -1, 0f);
        animatorMask.Update(0f);
        
        timerImage.DOFade(0.5f, 0.0f);
    }

    private void PlayNotification()
    {
        notificationText.DOFade(1.0f,0.2f).OnComplete(() =>
                notificationText.DOFade(0.0f, 1f).SetEase(Ease.InQuad));
        
        notificationText.transform.DOMoveY(-100, 0.0f).SetRelative();
        notificationText.transform.DOMoveY(100, 1.5f).SetRelative();

    }
}
