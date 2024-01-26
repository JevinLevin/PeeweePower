using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLight : MonoBehaviour
{
    private Light sceneLight;

    private void Awake()
    {
        sceneLight = GetComponent<Light>();
    }

    private void OnEnable()
    {
        GameManager.Instance.directionalLight = sceneLight;
    }
}
