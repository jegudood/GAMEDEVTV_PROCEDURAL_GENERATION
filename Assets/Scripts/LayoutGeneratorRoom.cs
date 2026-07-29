using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class LayoutGeneratorRoom : MonoBehaviour
{
    [SerializeField] private int seed = Environment.TickCount;
    [SerializeField] private RoomLevelLayoutConfiguration levelConfig; 

    [SerializeField] private GameObject levelLayoutDisplay;
    [SerializeField] private List<Hallway> openDoorways;

    private Random random;
    private Level level;
    private Dictionary<RoomTemplate, int> availableRooms;

    [InspectorButton]
    public Level GenerateLevel()
    {
        SharedLevelData.Instance.ResetRandom();
        random = SharedLevelData.Instance.Rand;
        availableRooms = levelConfig.GetAvailableRooms();
        openDoorways = new List<Hallway>();
        level = new Level(levelConfig.Width, levelConfig.Length);
        
        RoomTemplate startRoomTemplate = availableRooms.Keys.ElementAt(random.Next(0, availableRooms.Count));
        RectInt roomRect = GetStartRoomRect(startRoomTemplate);
        Room room = CreateNewRoom(roomRect, startRoomTemplate);
        List<Hallway> hallways = room.CalculateAllPossibleDoorways(room.Area.width, room.Area.height, levelConfig.DoorDistanceFromEdge);
        
        hallways.ForEach(h=> h.StartRoom = room);
        hallways.ForEach(h => openDoorways.Add(h));
        level.AddRoom(room);

        Hallway selectedEntryway = openDoorways[random.Next(openDoorways.Count)];
        AddRoom();
        
        DrawLayout(selectedEntryway, roomRect);
        
        return level;
    }

    [InspectorButton]
    public void GenerateNewSeed()
    {
        SharedLevelData.Instance.GeneratedSeed();
    }

    [InspectorButton]
    public void GenerateNewSeedAndLevel()
    {
        GenerateNewSeed();
        GenerateLevel();
    }

    private RectInt GetStartRoomRect(RoomTemplate roomTemplate)
    {
        RectInt roomSize = roomTemplate.GenerateRoomCandidateRect(random);

        int roomWidth = roomSize.width;
        int availableWidthX = levelConfig.Width / 2 - roomWidth;
        int randomX = random.Next(0, availableWidthX);
        int roomX = randomX + (levelConfig.Width / 4);

        int roomLength = roomSize.height;
        int availableLengthY = levelConfig.Length / 2 - roomLength;
        int randomY = random.Next(0, availableLengthY);
        int roomY = randomY + (levelConfig.Length / 4);

        return new RectInt(roomX, roomY, roomWidth, roomLength);
    }

    private void DrawLayout(Hallway selectedEntryWay = null, RectInt roomCandidateRect = new RectInt(), bool isDebug = false)
    {
        Renderer rendererComponent = levelLayoutDisplay.GetComponent<Renderer>();
        Texture2D layoutTexture = (Texture2D)rendererComponent.sharedMaterial.mainTexture;

        layoutTexture.Reinitialize(levelConfig.Width, levelConfig.Length);
        int scale = SharedLevelData.Instance.Scale;
        
        levelLayoutDisplay.transform.localScale = new Vector3(levelConfig.Width * scale, levelConfig.Length * scale, 1.0f);
        float xPos = level.Width * scale / 2 - scale;
        float zPos = level.Length * scale / 2 - scale;
        levelLayoutDisplay.transform.position = new Vector3(xPos, -2.0f, zPos);
        layoutTexture.FillWithColor(Color.black);
        
        foreach (Room room in level.Rooms)
        {
            if (room.LayoutTexture != null) layoutTexture.DrawTexture(room.LayoutTexture, room.Area);
            else layoutTexture.DrawRectangle(room.Area, Color.white);
        }
        
        Array.ForEach(level.Hallways, hallway => layoutTexture.DrawLine(hallway.StartPositionAbsolute, hallway.EndPositionAbsolute, Color.white));
        layoutTexture.ConvertToBlackAndWhite();
        if(isDebug)
        {
            layoutTexture.DrawRectangle(roomCandidateRect, Color.steelBlue);
            openDoorways.ForEach(h => layoutTexture.SetPixel(h.StartPositionAbsolute.x, h.StartPositionAbsolute.y, h.StartDirection.GetColor()));
        }

        if (isDebug && selectedEntryWay != null)
        {
            layoutTexture.SetPixel(selectedEntryWay.StartPositionAbsolute.x, selectedEntryWay.StartPositionAbsolute.y, Color.darkRed);
        }
        
        layoutTexture.SaveAsset();
    }

    private Hallway SelectHallwayCandidate(RectInt roomCandidateRect, RoomTemplate roomTemplate, Hallway entryway)
    {
        Room room = CreateNewRoom(roomCandidateRect, roomTemplate, false);
        List<Hallway> candidates = room.CalculateAllPossibleDoorways(room.Area.width, room.Area.height, levelConfig.DoorDistanceFromEdge);
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
        RoomTemplate roomTemplate = availableRooms.Keys.ElementAt(random.Next(0, availableRooms.Count));
        RectInt roomCandidateRect = roomTemplate.GenerateRoomCandidateRect(random);
        
        Hallway selectedExit = SelectHallwayCandidate(roomCandidateRect, roomTemplate, selectedEntryway);
        if (selectedExit == null) return null;

        int hallwayDistance = random.Next(levelConfig.HallwayLengthMin, levelConfig.HallwayLengthMax);
        Vector2Int roomCandidatePosition = CalculateRoomPosition(selectedEntryway, roomCandidateRect.width, roomCandidateRect.height, hallwayDistance, selectedExit.StartPosition);
        roomCandidateRect.position = roomCandidatePosition;

        if (!IsRoomCandidateValid(roomCandidateRect)) return null;
        Room newAdjacentRoom = CreateNewRoom(roomCandidateRect, roomTemplate);
        selectedEntryway.EndRoom = newAdjacentRoom;
        selectedEntryway.EndPosition = selectedExit.StartPosition;

        return newAdjacentRoom;
    }

    private void AddRoom()
    {
        while (openDoorways.Count > 0 && level.Rooms.Length < levelConfig.RoomCountMax && availableRooms.Count > 0)
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
            
            List<Hallway> newOpenHallways = newRoom.CalculateAllPossibleDoorways(newRoom.Area.width, newRoom.Area.height, levelConfig.DoorDistanceFromEdge);
            newOpenHallways.ForEach( h => h.StartRoom = newRoom);
            
            openDoorways.Remove(selectedEntryway);
            openDoorways.AddRange(newOpenHallways);
        }
    }

    private bool IsRoomCandidateValid(RectInt roomCandidateRect)
    {
        RectInt levelRect = new RectInt(1, 1, levelConfig.Width - 2, levelConfig.Length - 2);
        return levelRect.Contains(roomCandidateRect) && !CheckRoomOverlap(roomCandidateRect, level.Rooms, level.Hallways, levelConfig.MinRoomDistance);
    }

    private bool CheckRoomOverlap(RectInt roomCandidateRect, Room[] rooms, Hallway[] hallways, int minRoomDistanceValue)
    {
        RectInt paddedRoomRect = new RectInt
        {
            x = roomCandidateRect.x - minRoomDistanceValue,
            y = roomCandidateRect.y - minRoomDistanceValue,
            width = roomCandidateRect.width + 2 * minRoomDistanceValue,
            height = roomCandidateRect.height +2 * minRoomDistanceValue
        };
        
        return rooms.Any(room => paddedRoomRect.Overlaps(room.Area)) || hallways.Any(hallway => paddedRoomRect.Overlaps(hallway.Area));
    }

    private void UseUpRoomTemplate(RoomTemplate roomTemplate)
    {
        availableRooms[roomTemplate] -= 1;
        if (availableRooms[roomTemplate] == 0)
        {
            availableRooms.Remove(roomTemplate);
        }
    }

    private Room CreateNewRoom(RectInt roomCandidateRect, RoomTemplate roomTemplate, bool useUp = true)
    {
        if(useUp) UseUpRoomTemplate(roomTemplate);
        return roomTemplate.LayoutTexture == null ? new Room(roomCandidateRect) : new Room(roomCandidateRect.x, roomCandidateRect.y, roomTemplate.LayoutTexture);
    }
}
