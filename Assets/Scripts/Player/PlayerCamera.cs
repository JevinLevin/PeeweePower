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

    private static Tween shakeTweenAmplitude;
    private static Tween shakeTweenFrequency;
    private static Tween shakeInTweenAmplitude;
    private static Tween shakeInTweenFrequency;
    private static Tweener vibrateTween;
    private static Tween zoomTween;

    private static float defaultFOV;

    private void Awake()
    {
        cameraTransform = transform;

        vcam = GetComponent<CinemachineVirtualCamera>();
        defaultFOV = vcam.m_Lens.FieldOfView;
    }

    //public static void ShakeCameraRotation(float strength = 1.25f)
    //{
    //    shakeTween?.Complete();
    //    shakeTween = cameraTransform.DOShakeRotation(0.25f, strength, 15, 90, true, ShakeRandomnessMode.Harmonic).SetRelative();
    //}

    public static void ShakeCamera(float amplitude, float frequency, float duration)
    {
        shakeTweenAmplitude?.Complete();
        shakeTweenFrequency?.Complete();
        shakeTweenAmplitude = DOVirtual.Float(amplitude, 0.0f, duration, value => vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = value).SetEase(Ease.InSine);
        shakeTweenFrequency = DOVirtual.Float(frequency, 0.0f, duration, value => vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = value).SetEase(Ease.InSine);
    }
    
    public static void ShakeCameraIn(float amplitude, float frequency, float duration)
    {
        shakeInTweenAmplitude?.Kill();
        shakeInTweenFrequency?.Kill();
        shakeInTweenAmplitude = DOVirtual.Float( 0.0f, amplitude, duration, value => vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = value).SetEase(Ease.InSine);
        shakeInTweenFrequency = DOVirtual.Float( 0.0f, frequency, duration, value => vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = value).SetEase(Ease.InSine);
    }

    public static void ShakeCameraInStop()
    {
        shakeInTweenAmplitude?.Kill();
        shakeInTweenFrequency?.Kill();
        vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0;
        vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 0;
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
