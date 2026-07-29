using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomLevelLayoutConfiguration", menuName = "Custom/Procedural Generation/RoomLevelLayoutConfiguration")]
public class RoomLevelLayoutConfiguration : ScriptableObject
{
    [SerializeField] private int width = 64;
    [SerializeField] private int length = 64;
    
    [SerializeField] private RoomTemplate[] roomTemplates;
    [SerializeField] private int doorDistanceFromEdge = 1;
    [SerializeField] private int hallwayLengthMin = 3;
    [SerializeField] private int hallwayLengthMax = 5;
    [SerializeField] private int roomCountMax = 5;
    [SerializeField] private int minRoomDistance = 1;
    
    public int Width => width;
    public int Length => length;
    
    public int DoorDistanceFromEdge => doorDistanceFromEdge;
    public int HallwayLengthMin => hallwayLengthMin;
    public int HallwayLengthMax => hallwayLengthMax;
    public int RoomCountMax => roomCountMax;
    public int MinRoomDistance => minRoomDistance;

    public Dictionary<RoomTemplate, int> GetAvailableRooms()
    {
        Dictionary<RoomTemplate, int> availableRooms = roomTemplates.ToDictionary(t => t, t => t.NumberOfRooms);

        availableRooms = availableRooms.Where(kvp => kvp.Value > 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        return availableRooms;
    }
}
