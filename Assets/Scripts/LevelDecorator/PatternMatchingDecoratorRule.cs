using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

[Serializable]
[CreateAssetMenu(fileName = "DecoratorRule", menuName = "Custom/Procedural Generation/Pattern Decorator Rule")]
public class PatternMatchingDecoratorRule : BaseDecoratorRule
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Array2DWrapper<TileType> placement;
    [SerializeField] private Array2DWrapper<TileType> fill;
    [SerializeField] private bool centerHorizontally = false;
    [SerializeField] private bool centerVertically = false;
    
    internal override bool CanBeApplied(TileType[,] levelDecorated, Room room)
    {
        return FindOccurrences(levelDecorated, room).Length > 0;
    }

    internal override void Apply(TileType[,] levelDecorated, Room room, Transform parent)
    {
        Vector2Int[] occurences = FindOccurrences(levelDecorated, room);
        if (occurences.Length == 0) return;
        Random random = SharedLevelData.Instance.Rand;
        int occurenceIndex = random.Next(0, occurences.Length);
        Vector2Int occurence = occurences[occurenceIndex];
        for (int y = 0; y < placement.Height; y++)
        {
            for (int x = 0; x < placement.Width; x++)
            {
                TileType tileType = fill[x, y];
                levelDecorated[occurence.x + x, occurence.y + y] = tileType;
            }
        }

        if (prefab == null) return;
        
        GameObject decoration = Instantiate(prefab, parent.transform);
        Vector3 center = new Vector3(occurence.x + placement.Width / 2.0f, 0.0f, occurence.y + placement.Height / 2.0f);
        int scale = SharedLevelData.Instance.Scale;
        decoration.transform.position = (center + new Vector3(-1.0f, 0.0f, -1.0f)) * scale;
        decoration.transform.localScale = Vector3.one * scale;
    }

    private Vector2Int[] FindOccurrences(TileType[,] levelDecorated, Room room)
    {
        List<Vector2Int> occurences = new List<Vector2Int>();
        float centerX = room.Area.position.x + room.Area.width / 2.0f - placement.Width / 2.0f;
        float centerY = room.Area.position.y + room.Area.height / 2.0f - placement.Height / 2.0f;

        for (int y = room.Area.position.y - 1; y < room.Area.position.y + room.Area.height + 2 - placement.Height; y++)
        {
            for (int x = room.Area.position.x - 1; x < room.Area.position.x + room.Area.width + 2 - placement.Width; x++)
            {
                if (centerHorizontally && x != centerX) continue;
                if (centerVertically && y != centerY) continue;
                if (!IsPatternAtPosition(levelDecorated, placement, x, y)) continue;
                occurences.Add(new Vector2Int(x, y));
            }
        }

        return occurences.ToArray();
    }

    private bool IsPatternAtPosition(TileType[,] levelDecorated, Array2DWrapper<TileType> pattern, int startX, int startY)
    {
        for (int y = 0; y < pattern.Height; y++)
        {
            for (int x = 0; x < pattern.Width; x++)
            {
                if (levelDecorated[startX + x, startY + y] != pattern[x, y])
                {
                    return false;
                }
            }
        }

        return true;
    }
}
