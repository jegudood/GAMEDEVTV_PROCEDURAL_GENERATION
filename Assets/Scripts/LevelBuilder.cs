using System;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    [SerializeField] private LayoutGeneratorRoom layoutGeneratorRoom;
    [SerializeField] private MarchingSquares marchingSquares;
    [SerializeField] private RoomDecorator roomDecorator;

    private void Start()
    {
        GenerateRandom();
    }

    [InspectorButton]
    public void GenerateRandom()
    {
        SharedLevelData.Instance.GeneratedSeed();
        Generate();
    }
    
    [InspectorButton]
    public void Generate()
    {
        Level level = layoutGeneratorRoom.GenerateLevel();
        layoutGeneratorRoom.GenerateLevel();
        marchingSquares.CreateLevelGeometry();
        
        roomDecorator.PlaceItems(level);
    }
}
