using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public bool walkable = true;
    public bool something_placed = false;

    public Vector3 worldPosition = Vector3.zero;

    public int q;
    public int r;
    public int s;

    public int distanceCost = 1;

    private MeshCollider m_collider;
    public Cell init(int x, int y, Vector3 worldPosition, bool walkable = true)
    {
        this.worldPosition = worldPosition;
        this.walkable = walkable;
        this.q = y;
        this.r = x - (y + (y&1)) / 2;
        s = -q - r;

        if (!walkable)
            gameObject.GetComponent<Renderer>().materials[1].color = Color.red;

        return this;
    }

    private void Start()
    {
        m_collider = GetComponent<MeshCollider>();
    }

    public void ChangeColor(Color color)
    {
        gameObject.GetComponent<Renderer>().materials[1].color = color;
    }

    private void OnBecameVisible()
    {
        if (!m_collider.enabled)
            m_collider.enabled = true;
    }

    private void OnBecameInvisible()
    {
        if (m_collider.enabled)
            m_collider.enabled = false;
    }

    public Vector3Int getPositionVector()
    {
        return new Vector3Int(q, r, s);
    }
}
