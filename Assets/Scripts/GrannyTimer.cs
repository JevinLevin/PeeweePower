using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrannyTimer : MonoBehaviour
{
    [SerializeField] private Image timerImage;

    public void SetTimeProgress(float t)
    {
        timerImage.fillAmount = t;
    }
}
