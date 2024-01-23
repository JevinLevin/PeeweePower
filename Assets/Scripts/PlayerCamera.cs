using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using Cinemachine;

public class PlayerCamera : MonoBehaviour
{

    [SerializeField] private Transform cameraOrigin;
    private static Transform cameraTransform;

    private void Awake()
    {
        cameraTransform = cameraOrigin;
    }

    // NEED TO FIX
    public static void ShakeCamera()
    {
        //cameraTransform.DOShakeRotation(0.25f, 200, 10, 90, true, ShakeRandomnessMode.Harmonic);
    }
}
