using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Chunk
{
    private GenerateTerrain generateTerrain;
    private int iSize;
    private Vector2Int vStartPoint;
    private Vector2Int vEndPoint;
    private bool[,] mChunkData; // Representación del chunk (pasable / no pasable)
    public GameObject GChunk;

    // Constructor
    public Chunk(int _iSize, Vector2Int _vStartPoint, GenerateTerrain _generateTerrain)
    {
        iSize = _iSize;
        vStartPoint = _vStartPoint;
        generateTerrain = _generateTerrain;

        // Inicializar los datos del chunk
        mChunkData = new bool[iSize, iSize];

        // Generar el camino dentro del chunk
        GeneratePathInChunk();

        // Crear GameObject para el chunk
        GChunk = new GameObject("Chunk");
        GChunk.transform.SetParent(generateTerrain.transform);
        GenerateMesh();
    }

    // Generación del camino dentro del chunk
    void GeneratePathInChunk()
    {
        // Asegurarse de que el punto final no esté en el borde
        vEndPoint = GetValidEndPoint();
        Debug.Log(vStartPoint);
        Debug.Log(vEndPoint);
        // Generar el camino desde vStartPoint hasta vEndPoint usando A*
        List<Vector2Int> path = GeneratePath(vStartPoint, vEndPoint);
        path.Add(vStartPoint);
        // Marcar el camino en mChunkData
        foreach (var point in path)
        {
            mChunkData[point.x, point.y] = true; // Establecer las celdas del camino como pasables
        }

    }

    List<Vector2Int> GeneratePath(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new List<Vector2Int>();

        var openSet = new HashSet<Vector2Int>();
        var closedSet = new HashSet<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float>();
        var fScore = new Dictionary<Vector2Int, float>();

        openSet.Add(start);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, end);

        while (openSet.Count > 0)
        {
            Vector2Int current = GetLowestFScoreNode(openSet, fScore);

            if (current == end)
            {
                while (cameFrom.ContainsKey(current))
                {
                    path.Add(current);
                    current = cameFrom[current];
                }
                path.Reverse();
                return path;
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                float tentativeGScore = gScore[current] + 1;

                if (!openSet.Contains(neighbor))
                    openSet.Add(neighbor);

                if (tentativeGScore < gScore.GetValueOrDefault(neighbor, float.MaxValue))
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, end);
                }
            }
        }

        return path;
    }

    // Método para verificar si una celda está bloqueada
    public bool IsBlocked(Vector2Int current)
    {
        return (current.x < 1 || current.y < 1 || current.x >= iSize || current.y >= iSize || mChunkData[current.x, current.y]) && current != vEndPoint;
    }


    // Heurística de Manhattan para A*
    float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    // Obtener el nodo con el menor fScore
    Vector2Int GetLowestFScoreNode(HashSet<Vector2Int> openSet, Dictionary<Vector2Int, float> fScore)
    {
        Vector2Int lowest = openSet.First();
        foreach (var node in openSet)
        {
            if (fScore.ContainsKey(node) && fScore[node] < fScore[lowest])
                lowest = node;
        }
        return lowest;
    }

    // Obtener los vecinos adyacentes (4 direcciones)
    List<Vector2Int> GetNeighbors(Vector2Int point)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>
        {
            new Vector2Int(point.x - 1, point.y), // Izquierda
            new Vector2Int(point.x + 1, point.y), // Derecha
            new Vector2Int(point.x, point.y - 1), // Arriba
            new Vector2Int(point.x, point.y + 1)  // Abajo
        };

        // Filtrar vecinos que estén dentro de los límites válidos
        neighbors = neighbors.Where(n =>
         (n.x >= 0 && n.x < iSize && n.y >= 0 && n.y < iSize) // Dentro de los límites
         && (!IsBlocked(n) || n == vEndPoint) // Permitimos el nodo final aunque esté en el borde
           ).ToList();

        return neighbors;
    }

    // Asegurarse de que el punto final no esté en el borde
    Vector2Int GetValidEndPoint()
    {
        // Lista con los 4 bordes posibles
        List<Vector2Int> possibleEdges = new List<Vector2Int> {
            new Vector2Int(-1, 0), // ARRIBA AUMENTAMOS LA X
            new Vector2Int(0, iSize-1), // ABAJO AUMENTAMOS LA X
            new Vector2Int(iSize-1, 0), // DERECHA AUMENTAMOS LA Y
            new Vector2Int(0, -1), // IZQUIERDA AUMENTAMOS LA Y
           
        };

        // Eliminar el borde del que proviene el punto de inicio
        if (vStartPoint.x == 0 && vStartPoint.y != 0) //izquierda
            possibleEdges.Remove(new Vector2Int(0, -1)); 

        else if (vStartPoint.y == 0 && vStartPoint.x != 0) //superior
            possibleEdges.Remove(new Vector2Int(-1, 0));

        else if (vStartPoint.y == iSize - 1) //inferior
            possibleEdges.Remove(new Vector2Int(0, iSize - 1)); 

        else if (vStartPoint.x == iSize - 1)  //derecha
            possibleEdges.Remove(new Vector2Int(iSize - 1, 0));

        var EDGE = possibleEdges[Random.Range(0, possibleEdges.Count)];

        if(EDGE.x == 0 && EDGE.y!=-1)EDGE.y = Random.Range(1, iSize - 1);
        else if(EDGE.y == 0 && EDGE.x != -1)EDGE.x = Random.Range(1, iSize - 1);

        else if (EDGE.y == -1) EDGE.y = Random.Range(1, iSize - 1);
        else if (EDGE.x == -1) EDGE.x = Random.Range(1, iSize - 1);


        //Seleccionar un borde aleatorio de los posibles
        return EDGE;
    }

    // Método para generar el mesh del chunk (si es necesario)
    void GenerateMesh()
    {
        for (int i = 0; i < iSize; i++)
        {
            for (int j = 0; j < iSize; j++)
            {
                GameObject go = GameObject.Instantiate(generateTerrain.gPath, GChunk.transform, false);
                go.transform.SetLocalPositionAndRotation(new Vector3(i, 0, j), GChunk.transform.rotation);

                if (!mChunkData[i, j])
                {
                    go = GameObject.Instantiate(generateTerrain.gWall, GChunk.transform, false);
                    go.transform.SetLocalPositionAndRotation(new Vector3(i, generateTerrain.gWall.transform.localScale.y, j), GChunk.transform.rotation);
                }
            }
        }
    }
    public Vector2Int GetEnd()
    {
        return vEndPoint;
    }
}
