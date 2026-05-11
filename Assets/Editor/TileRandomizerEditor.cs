using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileRandomizerEditor : EditorWindow
{
    private Tilemap targetTilemap;
    private TileBase[] randomTiles = new TileBase[4];
    private string excludeName = "ground_large"; // 要排除的地块名字

    [MenuItem("Tools/Tilemap 随机化工具")]
    public static void ShowWindow() => GetWindow<TileRandomizerEditor>("Tile 随机化");

    private void OnGUI()
    {
        targetTilemap = (Tilemap)EditorGUILayout.ObjectField("目标 Tilemap", targetTilemap, typeof(Tilemap), true);
        excludeName = EditorGUILayout.TextField("排除的 Tile 名称", excludeName);

        EditorGUILayout.LabelField("随机池中的 Tile:", EditorStyles.boldLabel);
        for (int i = 0; i < 4; i++)
            randomTiles[i] = (TileBase)EditorGUILayout.ObjectField($"Tile {i + 1}", randomTiles[i], typeof(TileBase), false);

        if (GUILayout.Button("开始随机化(跳过特定块)") && targetTilemap != null)
        {
            RandomizeExistingTiles();
        }
    }

    private void RandomizeExistingTiles()
    {
        BoundsInt bounds = targetTilemap.cellBounds;
        Undo.RecordObject(targetTilemap, "Randomize Tiles Specific");

        int count = 0;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase currentTile = targetTilemap.GetTile(pos);

                // 只有原本有格子，且名字不是 ground_large 的才替换
                if (currentTile != null && currentTile.name != excludeName)
                {
                    TileBase randomTile = randomTiles[Random.Range(0, randomTiles.Length)];
                    targetTilemap.SetTile(pos, randomTile);
                    count++;
                }
            }
        }
        Debug.Log($"随机化完成！共替换了 {count} 个格子。已跳过名为 '{excludeName}' 的格子。");
    }
}