using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLight : MonoBehaviour
{
    private Light sceneLight;

    private void OnEnable()
    {
        sceneLight = GetComponent<Light>();
    }

    private void Start()
    {
        GameManager.Instance.directionalLight = sceneLight;
    }
}
