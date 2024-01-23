<<<<<<< HEAD
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

    public static void ShakeCamera()
    {
        cameraTransform.DOShakeRotation(0.25f, 200, 10, 90, true, ShakeRandomnessMode.Harmonic);
    }
}
=======
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

    public static void ShakeCamera()
    {
        cameraOrigin.DOShakeRotation(0.25f, 200, 10, 90, true, ShakeRandomnessMode.Harmonic);
    }
}
>>>>>>> 4274a5cf364e5242e1f73b3d432867b4ed3c1127
