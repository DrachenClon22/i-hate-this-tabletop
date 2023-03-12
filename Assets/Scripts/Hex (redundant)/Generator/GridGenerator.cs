using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridManager))]
public class GridGenerator : MonoBehaviour
{
    private readonly float WIDTH = 17.3f;
    private readonly float HEIGHT = 20f;

    public Vector2Int gridSize = new Vector2Int(50, 50);
    public float perlinSampleMultiplier = 50f;
    public float perlinDivider = 10f;

    public GameObject cell;

    private Transform l_parent;
    void Start()
    {
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
        float seed = UnityEngine.Random.value;

        l_parent = gameObject.transform;
        gridSize.y = Mathf.FloorToInt(gridSize.y * 1.25f);

        Vector3 startOffset = transform.position - new Vector3(WIDTH * gridSize.x / 2f, 0f, HEIGHT * 0.75f * gridSize.y / 2f);

        for (int width = 0; width < gridSize.x; width++)
        {
            for (int height = 0; height < gridSize.y; height++)
            {
                float xPosition = WIDTH * width + ((height % 2 == 0) ? (WIDTH / 2f) : 0);
                float yPosition = Mathf.PerlinNoise(width / perlinDivider + seed, height / perlinDivider + seed) * perlinSampleMultiplier;
                float zPosition = (HEIGHT * height) * 0.75f;
                Quaternion rotation = Quaternion.identity;
                Vector3 cellPosition = new Vector3(xPosition, yPosition, zPosition);

                GameObject go = Instantiate(cell, position: cellPosition + startOffset, rotation: rotation);
                go.transform.parent = l_parent;
                go.name = $"HEX: X:{height} || Y:{width}";
                GridManager.AddCell(go.AddComponent<Cell>().init(width, height, go.transform.position, (UnityEngine.Random.Range(0,3) > 0) ? true : false));
            }
        }
    }
}
