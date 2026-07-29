using UnityEngine;

[CreateAssetMenu(fileName = "Tileset", menuName = "Custom/Procedural Generation/Tileset")]
public class Tileset : ScriptableObject
{
    [SerializeField] private Color wallColor;
    [SerializeField] private TileVariant[] tiles = new TileVariant[16];

    public Color WallColor => wallColor;

    public GameObject GetTile(int tileIndex)
    {
        return tileIndex >= tiles.Length ? null : tiles[tileIndex].GetRandomTile();
    }
}
