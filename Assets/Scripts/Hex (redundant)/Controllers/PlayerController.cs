using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static int currentSteps { get; private set; } = 0;
    private int stepsUsedInLastMove = 0;

    public static Vector3Int coords = new Vector3Int(0, 0, 0);

    private Vector3Int newCoords = new Vector3Int(0, 0, 0);

    public static bool isDragging { get; private set; } = false;

    RaycastHit hitInfo;
    RaycastHit _hitInfo;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                if (hitInfo.transform.tag.Equals("Character"))
                {
                    transform.GetComponent<Collider>().enabled = false;
                    if (!isDragging)
                        isDragging = true;
                }
            }
        } 

        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                transform.GetComponent<Collider>().enabled = true;
                if (newCoords != coords)
                {
                    coords = newCoords;
                    transform.position = GridManager.hexToWorldCoords(coords) + Vector3.up * 10f;
                }
                GridManager.ClearAllColoredCells();
                transform.position = GridManager.hexToWorldCoords(coords);
                currentSteps -= stepsUsedInLastMove;
                stepsUsedInLastMove = 0;
                isDragging = false;
            }
        }

        if (isDragging)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _hitInfo))
            {
                Debug.DrawRay(_hitInfo.point, Vector3.up * 10f, Color.red);
                if (_hitInfo.transform.tag.Equals("Cell"))
                {
                    transform.position = GridManager.hexToWorldCoords(_hitInfo.transform.GetComponent<Cell>().getPositionVector()) + Vector3.up * 10f;
                    if (_hitInfo.transform.GetComponent<Cell>().walkable)
                    {
                        newCoords = GridManager.findPathTo(coords, _hitInfo.transform.GetComponent<Cell>().getPositionVector(), out stepsUsedInLastMove, currentSteps);
                    }
                }
            }
        } else
        {
            
        }
    }

    public static void DropTheDice(int min, int max, bool forced = false)
    {
        if (currentSteps <= 0 || forced)
        {
            currentSteps = Random.Range(min, max);
        }
    }
}
