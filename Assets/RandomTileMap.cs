using UnityEngine;
using UnityEngine.Tilemaps;

public class RandomTilemap : MonoBehaviour
{
    public Tilemap tilemap;
    public Tile cellTile;
    public int width = 100;
    public int height = 100;

    void Start()
    {
        GenerateRandomTilemap();
    }

    void GenerateRandomTilemap()
    {
        tilemap.ClearAllTiles();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                TileBase tileToPlace = Random.value > 0.5f ? cellTile : null; //Przywrócona linijka

                tilemap.SetTile(new Vector3Int(x, y, 0), tileToPlace);
            }
        }
        tilemap.CompressBounds();
    }
}