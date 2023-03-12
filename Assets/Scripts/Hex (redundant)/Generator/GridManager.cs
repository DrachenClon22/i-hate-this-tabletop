using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Priority_Queue;

public class GridManager : MonoBehaviour
{
    private static List<Cell> cells = new List<Cell>();

    private static Dictionary<Cell, Color> coloredCells = new Dictionary<Cell, Color>();
    public static void AddCell(Cell cell)
    {
        cells.Add(cell);
    }

    public static Cell getCell(int x, int y)
    {
        foreach (Cell cell in cells)
        {
            if (cell.q == x && cell.r == y)
                return cell;
        }

        return null;
    }

    public static Cell getCell(int q, int r, int s)
    {
        foreach (Cell cell in cells)
        {
            if (cell.q == q && cell.r == r && cell.s == s)
                return cell;
        }

        return null;
    }

    public static Cell getCell(Vector3Int coords)
    {
        foreach (Cell cell in cells)
        {
            if (cell.getPositionVector() == coords)
                return cell;
        }

        return null;
    }

    public static Vector3 hexToWorldCoords(Vector3Int cellPos)
    {
        return getCell(cellPos).worldPosition;
    }

    public static Vector3 hexToWorldCoords(Cell cell)
    {
        return cell.worldPosition;
    }

    public static Cell[] getNeighbors(Vector3Int coords, int radius)
    {
        List<Cell> neighbors = new List<Cell>();
        int i = 0;
        int _q;
        int _r;
        int _s;
        for (int q = -radius; q <= radius; q++)
        {
            for (int r = Mathf.Max(-radius, -q - radius); r <= Mathf.Min(radius, -q + radius); r++)
            {
                _q = q + coords.x;
                _r = r + coords.y;
                _s = -_q - _r;

                if (new Vector3Int(_q, _r, _s) == coords)
                    continue;

                if (getCell(_q, _r, _s))
                {
                    if (getCell(_q, _r, _s).walkable)
                        neighbors.Add(getCell(_q, _r, _s));
                }
                i++;
            }
        }
        return neighbors.ToArray();
    }

    public static Vector3Int findPathTo(Vector3Int from, Vector3Int to, out int stepsUsed, int maxSteps = 100)
    {
        if (maxSteps > 0)
        {
            stepsUsed = 0;
            ClearAllColoredCells();
            coloredCells.Add(getCell(from), getCell(from).GetComponent<Renderer>().materials[1].color);
            getCell(from).GetComponent<Renderer>().materials[1].color = Color.yellow;

            IDictionary<Vector3Int, Vector3Int> nodeParents = new Dictionary<Vector3Int, Vector3Int>();
            IEnumerable<Cell> validNodes = cells;

            IDictionary<Vector3Int, int> heuristicScore = new Dictionary<Vector3Int, int>();
            IDictionary<Vector3Int, int> distanceFromStart = new Dictionary<Vector3Int, int>();

            foreach (Cell vertex in validNodes)
            {
                heuristicScore.Add(new KeyValuePair<Vector3Int, int>(vertex.getPositionVector(), int.MaxValue));
                distanceFromStart.Add(new KeyValuePair<Vector3Int, int>(vertex.getPositionVector(), int.MaxValue));
            }

            heuristicScore[from] = HeuristicCostEstimate(from, to);
            distanceFromStart[from] = 0;

            SimplePriorityQueue<Vector3Int, int> priorityQueue = new SimplePriorityQueue<Vector3Int, int>();
            priorityQueue.Enqueue(from, heuristicScore[from]);

            while (priorityQueue.Count > 0)
            {
                Vector3Int curr = priorityQueue.Dequeue();

                if (stepsUsed >= maxSteps)
                {
                    print("WTH?? max steps reached");
                    Vector3Int _curr = curr;
                    while (_curr != from)
                    {
                        coloredCells.Add(getCell(_curr), getCell(_curr).GetComponent<Renderer>().materials[1].color);
                        getCell(_curr).GetComponent<Renderer>().materials[1].color = Color.magenta;
                        _curr = nodeParents[_curr];
                    }
                    return curr;
                }
                if (curr == to)
                {
                    Vector3Int _curr = to;
                    while (_curr != from)
                    {
                        coloredCells.Add(getCell(_curr), getCell(_curr).GetComponent<Renderer>().materials[1].color);
                        getCell(_curr).GetComponent<Renderer>().materials[1].color = Color.magenta;
                        _curr = nodeParents[_curr];
                    }
                    return to;
                }

                IList<Cell> neighbors = getNeighbors(curr, 1);

                foreach (Cell node in neighbors)
                {
                    int currScore = distanceFromStart[curr] + node.distanceCost;
                    if (currScore < distanceFromStart[node.getPositionVector()])
                    {
                        nodeParents[node.getPositionVector()] = curr;
                        distanceFromStart[node.getPositionVector()] = currScore;

                        int hScore = distanceFromStart[node.getPositionVector()] + HeuristicCostEstimate(node.getPositionVector(), to);
                        heuristicScore[node.getPositionVector()] = hScore;
                        if (!priorityQueue.Contains(node.getPositionVector()))
                        {
                            priorityQueue.Enqueue(node.getPositionVector(), hScore);
                        }
                    }
                }
                stepsUsed++;
            }

            return from;
        }
        else
        {
            stepsUsed = 0;
            return from;
        }
    }

    private static int HeuristicCostEstimate(Vector3Int node, Vector3Int goal)
    {
        return (int)(Mathf.Abs(node.x - goal.x) +
                Mathf.Abs(node.y - goal.y) +
                Mathf.Abs(node.z - goal.z));
    }

    public static void ClearAllColoredCells()
    {
        if (coloredCells.Count > 0)
        {
            foreach (Cell cell in coloredCells.Keys)
            {
                cell.GetComponent<Renderer>().materials[1].color = coloredCells[cell];
            }
            coloredCells.Clear();
        }
    }
}
