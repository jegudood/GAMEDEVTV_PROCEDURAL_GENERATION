using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum HallwayDirection
{
    NONE,
    LEFT,
    RIGHT,
    TOP,
    BOTTOM
}

public static class HallwayDirectionExtension
{
    private static readonly Color yellow = new Color(1, 1, 0, 1);
    
    private static readonly Dictionary<HallwayDirection, Color> DirectionToColorMap = new Dictionary<HallwayDirection, Color>
    {
        { HallwayDirection.LEFT, yellow },
        { HallwayDirection.RIGHT, Color.magenta },
        { HallwayDirection.TOP, Color.cyan },
        { HallwayDirection.BOTTOM, Color.green }
    };

    public static Color GetColor(this HallwayDirection direction)
    {
        return DirectionToColorMap.TryGetValue(direction, out Color color) ? color : Color.grey;
    }

    public static Dictionary<Color, HallwayDirection> GetColorDirectionMap()
    {
        return DirectionToColorMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    }

    public static HallwayDirection GetOppositeDirection(this HallwayDirection direction)
    {
        Dictionary<HallwayDirection, HallwayDirection> oppositeDirectionMap = new Dictionary<HallwayDirection, HallwayDirection>()
        {
            { HallwayDirection.LEFT, HallwayDirection.RIGHT },
            { HallwayDirection.RIGHT, HallwayDirection.LEFT },
            { HallwayDirection.TOP, HallwayDirection.BOTTOM },
            { HallwayDirection.BOTTOM, HallwayDirection.TOP }
        };
        return oppositeDirectionMap.GetValueOrDefault(direction, HallwayDirection.NONE);
    }
}
