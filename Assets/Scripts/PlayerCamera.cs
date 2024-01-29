using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using Cinemachine;

public class PlayerCamera : MonoBehaviour
{

    private static CinemachineVirtualCamera vcam;
    private static Transform cameraTransform;

    private static Tween shakeTween;
    private static Tweener vibrateTween;
    private static Tween zoomTween;

    private static float defaultFOV;

    private void Awake()
    {
        cameraTransform = transform;

        vcam = GetComponent<CinemachineVirtualCamera>();
        defaultFOV = vcam.m_Lens.FieldOfView;
    }

    // NEED TO FIX
    public static void ShakeCamera(float strength = 1.25f)
    {
        shakeTween?.Complete();
        shakeTween = cameraTransform.DOShakeRotation(0.25f, strength, 15, 90, true, ShakeRandomnessMode.Harmonic).SetRelative();
    }

    public static void ChangeFOV(float fov, float length)
    {
        float startingFov = vcam.m_Lens.FieldOfView;
        
       if (fov == -1)
           fov = defaultFOV;

       zoomTween?.Complete();
       zoomTween = DOVirtual.Float(startingFov, fov, length, value => vcam.m_Lens.FieldOfView = value);
    }
}
