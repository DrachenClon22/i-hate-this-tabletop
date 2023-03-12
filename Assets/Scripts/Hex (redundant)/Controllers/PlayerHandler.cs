using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : MonoBehaviour
{
    public GameObject prefab;

    public static GameObject player;

    private static PlayerHandler instance;

    private void Start()
    {
        instance = this;
    }
    public void SpawnPlayer()
    {
        if (SelectHex.focus && prefab)
        {
            if (!player)
            {
                player = Instantiate(prefab, SelectHex.focus.transform.position + prefab.transform.position, Quaternion.identity);
                PlayerController.coords = SelectHex.focus.GetComponent<Cell>().getPositionVector();
            }
        }
    }
}
