using System.Collections.Generic;
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
    private static readonly Dictionary<HallwayDirection, Color> DirectionToColorMap = new Dictionary<HallwayDirection, Color>
    {
        { HallwayDirection.LEFT, Color.yellow },
        { HallwayDirection.RIGHT, Color.magenta },
        { HallwayDirection.TOP, Color.cyan },
        { HallwayDirection.BOTTOM, Color.green }
    };

    public static Color GetColor(this HallwayDirection direction)
    {
        return DirectionToColorMap.TryGetValue(direction, out Color color) ? color : Color.grey;
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
