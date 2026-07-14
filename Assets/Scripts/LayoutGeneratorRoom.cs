using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class LayoutGeneratorRoom : MonoBehaviour
{
    [SerializeField] private int width = 64;
    [SerializeField] private int length = 64;
    
    [SerializeField] private int roomWidthMin = 3;
    [SerializeField] private int roomWidthMax = 5;
    [SerializeField] private int roomLengthMin = 3;
    [SerializeField] private int roomLengthMax = 5;
    [SerializeField] private int hallwayLengthMin = 3;
    [SerializeField] private int hallwayLengthMax = 5;
    [SerializeField] private int roomCountMax = 5;

    [SerializeField] private GameObject levelLayoutDisplay;
    [SerializeField] private List<Hallway> openDoorways;

    private System.Random random;
    private Level level;

    [InspectorButton]
    public void GenerateLevel()
    {
        random = new System.Random();
        openDoorways = new List<Hallway>();
        level = new Level(width, length);
        RectInt roomRect = GetStartRoomRect();
        Debug.Log(roomRect);
        Room room = new Room(roomRect);
        List<Hallway> hallways = room.CalculateAllPossibleDoorways(room.Area.width, room.Area.height, 1);
        
        hallways.ForEach(h=> h.StartRoom = room);
        hallways.ForEach(h => openDoorways.Add(h));
        level.AddRoom(room);

        Hallway selectedEntryway = openDoorways[random.Next(openDoorways.Count)];
        AddRoom();
        
        DrawLayout(selectedEntryway, roomRect);
    }

    private RectInt GetStartRoomRect()
    {
        int roomWidth = random.Next(roomWidthMin, roomWidthMax);
        int availableWidthX = width / 2 - roomWidth;
        int randomX = random.Next(0, availableWidthX);
        int roomX = randomX + (width / 4);

        int roomLength = random.Next(roomLengthMin, roomLengthMax);
        int availableLengthY = length / 2 - roomLength;
        int randomY = random.Next(0, availableLengthY);
        int roomY = randomY + (length / 4);

        return new RectInt(roomX, roomY, roomWidth, roomLength);
    }

    private void DrawLayout(Hallway selectedEntryWay = null, RectInt roomCandidateRect = new RectInt())
    {
        Renderer rendererComponent = levelLayoutDisplay.GetComponent<Renderer>();
        Texture2D layoutTexture = (Texture2D)rendererComponent.sharedMaterial.mainTexture;

        layoutTexture.Reinitialize(width, length);
        levelLayoutDisplay.transform.localScale = new Vector3(width, length, 1.0f);
        layoutTexture.FillWithColor(Color.black);
        
        Array.ForEach(level.Rooms, room => layoutTexture.DrawRectangle(room.Area, Color.white));
        Array.ForEach(level.Hallways, hallway => layoutTexture.DrawLine(hallway.StartPositionAbsolute, hallway.EndPositionAbsolute, Color.white));
        
        layoutTexture.DrawRectangle(roomCandidateRect, Color.steelBlue);

        foreach (Hallway hallway in openDoorways)
        {
            layoutTexture.SetPixel(hallway.StartPositionAbsolute.x, hallway.StartPositionAbsolute.y, hallway.StartDirection.GetColor());
        }

        if (selectedEntryWay != null)
        {
            layoutTexture.SetPixel(selectedEntryWay.StartPositionAbsolute.x, selectedEntryWay.StartPositionAbsolute.y, Color.darkRed);
        }
        
        layoutTexture.SaveAsset();
    }

    private Hallway SelectHallwayCandidate(RectInt roomCandidateRect, Hallway entryway)
    {
        Room room = new Room(roomCandidateRect);
        List<Hallway> candidates = room.CalculateAllPossibleDoorways(room.Area.width, room.Area.height, 1);
        HallwayDirection requiredDirection = entryway.StartDirection.GetOppositeDirection();
        List<Hallway> filteredHallwayCandidates = candidates.Where(hallwayCandidate => hallwayCandidate.StartDirection == requiredDirection).ToList();

        return filteredHallwayCandidates.Count > 0 ? filteredHallwayCandidates[random.Next(filteredHallwayCandidates.Count)] : null;
    }

    private Vector2Int CalculateRoomPosition(Hallway entryway, int roomWidth, int roomLength, int distance, Vector2Int endPosition)
    {
        Vector2Int roomPosition = entryway.StartPositionAbsolute;
        switch (entryway.StartDirection)
        {
            case HallwayDirection.LEFT :
                roomPosition.x -= distance + roomWidth;
                roomPosition.y -= endPosition.y;
                break;
            case HallwayDirection.TOP :
                roomPosition.x -= endPosition.x;
                roomPosition.y += distance + 1;
                break;
            case HallwayDirection.RIGHT :
                roomPosition.x += distance + 1;
                roomPosition.y -= endPosition.y;
                break;
            case HallwayDirection.BOTTOM :
                roomPosition.x -= endPosition.x;
                roomPosition.y -= distance + roomLength;
                break;
        }
        return roomPosition;
    }

    private Room ConstructAdjacentRoom(Hallway selectedEntryway)
    {
        RectInt roomCandidateRect = new RectInt
        {
            width = random.Next(roomWidthMin, roomWidthMax),
            height = random.Next(roomLengthMin, roomLengthMax)
        };
        
        Hallway selectedExit = SelectHallwayCandidate(roomCandidateRect, selectedEntryway);
        if (selectedExit == null) return null;

        int hallwayDistance = random.Next(hallwayLengthMin, hallwayLengthMax);
        
        Vector2Int roomCandidatePosition = CalculateRoomPosition(selectedEntryway, roomCandidateRect.width, roomCandidateRect.height, hallwayDistance, selectedExit.StartPosition);
        roomCandidateRect.position = roomCandidatePosition;

        if (!IsRoomCandidateValid(roomCandidateRect)) return null;
        
        Room newAdjacentRoom = new Room(roomCandidateRect);
        selectedEntryway.EndRoom = newAdjacentRoom;
        selectedEntryway.EndPosition = selectedExit.StartPosition;

        return newAdjacentRoom;
    }

    private void AddRoom()
    {
        while (openDoorways.Count > 0 && level.Rooms.Length < roomCountMax)
        {
            Hallway selectedEntryway = openDoorways[random.Next(0, openDoorways.Count)];
            Room newRoom = ConstructAdjacentRoom(selectedEntryway);

            if (newRoom == null)
            {
                openDoorways.Remove(selectedEntryway);
                continue;
            }
            
            level.AddRoom(newRoom);
            level.AddHallway(selectedEntryway);
            selectedEntryway.EndRoom = newRoom;
            
            List<Hallway> newOpenHallways = newRoom.CalculateAllPossibleDoorways(newRoom.Area.width, newRoom.Area.height, 1);
            newOpenHallways.ForEach( h => h.StartRoom = newRoom);
            
            openDoorways.Remove(selectedEntryway);
            openDoorways.AddRange(newOpenHallways);
        }
    }

    private bool IsRoomCandidateValid(RectInt roomCandidateRect)
    {
        RectInt levelRect = new RectInt(1, 1, width - 2, length - 2);
        return levelRect.Contains(roomCandidateRect) && !CheckRoomOverlap(roomCandidateRect, level.Rooms, level.Hallways, 1);
    }

    private bool CheckRoomOverlap(RectInt roomCandidateRect, Room[] rooms, Hallway[] hallways, int minRoomDistance)
    {
        RectInt paddedRoomRect = new RectInt
        {
            x = roomCandidateRect.x - minRoomDistance,
            y = roomCandidateRect.x - minRoomDistance,
            width = roomCandidateRect.width + 2 * minRoomDistance,
            height = roomCandidateRect.height +2 * minRoomDistance
        };
        
        return rooms.Any(room => paddedRoomRect.Overlaps(room.Area)) || hallways.Any(hallway => paddedRoomRect.Overlaps(hallway.Area));
    }
}
