using System;
using UnityEngine;
using Random = System.Random;

[ExecuteAlways]
[DisallowMultipleComponent]
public class SharedLevelData : MonoBehaviour
{
    public static SharedLevelData Instance { get; private set; }

    [SerializeField] private int scale = 1;
    [SerializeField] private int seed = Environment.TickCount;

    private Random random;
    public int Scale => scale;
    public Random Rand => random;

    [InspectorButtonAttribute]
    public void GeneratedSeed()
    {
        seed = Environment.TickCount;
        ResetRandom();
    }

    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            enabled = false;
            Debug.LogWarning("Duplicate SharedLevelData detected and disabled.", this);
        }
        Debug.Log(Instance.GetInstanceID());
        ResetRandom();
    }

    public void ResetRandom()
    {
        random = new Random(seed);
    }
}
