using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ActionTextDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private float displayTime;
    
    public void StartDisplay(string text, float displayTime)
    {
        this.text.text = text;

        this.displayTime = displayTime;
        
        StartCoroutine(nameof(DisplayText));

    }


    public void StopDisplay()
    {
        text.gameObject.SetActive(false);
        StopCoroutine(nameof(DisplayText));
    }


    private IEnumerator DisplayText()
    {
        text.gameObject.SetActive(true);

        yield return new WaitForSeconds(displayTime);
        
        text.gameObject.SetActive(false);

        yield return new WaitForSeconds(displayTime);

        StartCoroutine(nameof(DisplayText));
    }

}
