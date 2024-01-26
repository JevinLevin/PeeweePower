using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyVariants : MonoBehaviour
{
    [SerializeField] private Material[] textures;
    private SkinnedMeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
    }

    public void SetTexture()
    {
        meshRenderer.material = textures[Random.Range(0, textures.Length)];
    }
}
