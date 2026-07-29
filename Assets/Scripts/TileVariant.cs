using System;
using UnityEngine;
using Random = System.Random;

[Serializable]
public class TileVariant
{
    [SerializeField] private GameObject[] variants = Array.Empty<GameObject>();

    public GameObject GetRandomTile()
    {
        if (variants.Length == 0) return null;
        Random random = SharedLevelData.Instance.Rand;
        int randomIndex = random.Next(0, variants.Length);
        return variants[randomIndex];
    }
}
