using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class Chunk
{
    private GenerateTerrain generateTerrain;
    private int iSize;

    private List<Vector2Int> vStartPoints; // Lista de puntos de inicio
    private List<Vector2Int> vEndPoints = new List<Vector2Int>(); // Lista de puntos de inicio

    private List<Vector2Int> mChunkData;// Guardar las casillas con los caminos 

    public GameObject GChunk;

    float bifurcationProbability;
    float centerFactor;
    float pathIrregularity;

    // Constructor
    public Chunk(int _iSize, List<Vector2Int> _vStartPoint,
        float _bifurcationProbability,float _centerFactor, float _pathIrregularity, GenerateTerrain _generateTerrain)
    {
        iSize = _iSize;
        vStartPoints = _vStartPoint;
        generateTerrain = _generateTerrain;
        bifurcationProbability = _bifurcationProbability;
        centerFactor = _centerFactor;
        pathIrregularity = _pathIrregularity;

        mChunkData = new List<Vector2Int>();

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
        
        var vEndPoint = GetValidEndPoint(); //Generar Borde
        if (vEndPoint != new Vector2Int())
        {
            vEndPoints.Add(vEndPoint); //Si hay posible borde
            vStartPoints.Add(vEndPoint); //Tratar todos los puntos como start

            if (Random.value < bifurcationProbability) //Si hay bifulcación
            {
                var newEnd = GetValidEndPoint();

                if (newEnd != new Vector2Int()){ 
                    vEndPoints.Add(vEndPoint);
                    vStartPoints.Add(newEnd);
                }
            }
        }

        vEndPoint = vStartPoints[vStartPoints.Count-1];

        for (int i = vStartPoints.Count - 2; i >=0; i--)
        {
            List<Vector2Int> path = GeneratePath(vStartPoints[i], vEndPoint);
            path.Add(vStartPoints[i]); // Asegurarse de agregar el punto de inicio al camino

            // Marcar el camino en mChunkData
            mChunkData.AddRange(path);

            vEndPoint = vStartPoints[i];
        }
    }

    // Obtener un borde libre aleatorio para el punto final
    Vector2Int GetValidEndPoint()
    {
        List<Vector2Int> possibleEdges = new List<Vector2Int> {
            new Vector2Int(-1, 0), // ARRIBA
            new Vector2Int(0, iSize-1), // ABAJO
            new Vector2Int(iSize-1, 0), // DERECHA
            new Vector2Int(0, -1), // IZQUIERDA
        };

        // Eliminar el borde del que proviene el punto de inicio
        foreach (var startPoint in vStartPoints)
        {
            if (startPoint.x == 0 && startPoint.y != 0) possibleEdges.Remove(new Vector2Int(0, -1)); // izquierda
            else if (startPoint.y == 0 && startPoint.x != 0) possibleEdges.Remove(new Vector2Int(-1, 0)); // superior
            else if (startPoint.y == iSize - 1) possibleEdges.Remove(new Vector2Int(0, iSize - 1)); // inferior
            else if (startPoint.x == iSize - 1) possibleEdges.Remove(new Vector2Int(iSize - 1, 0)); // derecha
        }

        if (possibleEdges.Count == 0) return new Vector2Int(); // Verifica que hay bordes válidos

        // Selección aleatoria del borde
        var EDGE = possibleEdges[Random.Range(0, possibleEdges.Count)];

        // Ajustar las coordenadas del borde
        if (EDGE.y == -1)
            EDGE.y = Random.Range(1, iSize - 1);
        else if (EDGE.x == -1)
            EDGE.x = Random.Range(1, iSize - 1);
        else if (EDGE.x == iSize - 1)
            EDGE.y = Random.Range(1, iSize - 1);
        else if (EDGE.y == iSize - 1)
            EDGE.x = Random.Range(1, iSize - 1);

        return EDGE;
    }

    //A*
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
        fScore[start] = gScore[start] + Heuristic(start, end);

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

            foreach (Vector2Int neighbor in GetNeighbors(current, end))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                // Calcular el costo G con el factor de irregularidad
                float centerCost = CalculateCenterCost(neighbor); // Costo basado en la distancia al centro
                float randomFactor = UnityEngine.Random.Range(1 - pathIrregularity, 1 + pathIrregularity); // Factor aleatorio
                float tentativeGScore = gScore.GetValueOrDefault(current, float.MaxValue) + 1 + centerCost * randomFactor;

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

    // Calcular el costo basado en la distancia al centro
    float CalculateCenterCost(Vector2Int point)
    {
        float centerX = iSize / 2;
        float centerY = iSize / 2;
        float distance = Mathf.Sqrt(Mathf.Pow(centerX - point.x, 2) + Mathf.Pow(centerY - point.y, 2));
        float maxDistance = Mathf.Sqrt(Mathf.Pow(centerX, 2) + Mathf.Pow(centerY, 2));

        // Escalar la distancia al centro en función del factor de centro
        return (distance / maxDistance) * centerFactor;
    }

    // Obtener los vecinos adyacentes (evitar bordes)
    List<Vector2Int> GetNeighbors(Vector2Int point, Vector2Int endPoint)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>
    {
        new Vector2Int(point.x - 1, point.y), // Izquierda
        new Vector2Int(point.x + 1, point.y), // Derecha
        new Vector2Int(point.x, point.y - 1), // Arriba
        new Vector2Int(point.x, point.y + 1)  // Abajo
    };

        // Filtrar vecinos que estén dentro de los límites válidos (evitar bordes)
        neighbors = neighbors.Where(n =>
        (!IsBlocked(n) || n == endPoint) // Permitimos el nodo final aunque esté en el borde
        ).ToList();

        return neighbors;
    }

    // Ver si la celda es un posible camino
    public bool IsBlocked(Vector2Int current)
    {
        // Combinamos todas las condiciones de los bordes en una sola verificación
        return current.x <= 0 || current.y <= 0 || current.x >= iSize - 1 || current.y >= iSize - 1;
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
            if (fScore.GetValueOrDefault(node, float.MaxValue) < fScore.GetValueOrDefault(lowest, float.MaxValue))
                lowest = node;
        }
        return lowest;
    }

    // Generar el mesh del chunk
    void GenerateMesh()
    {
        for (int i = 0; i < iSize; i++)
        {
            for (int j = 0; j < iSize; j++)
            {
                GameObject go = GameObject.Instantiate(generateTerrain.gFloor, GChunk.transform, false);
                go.transform.SetLocalPositionAndRotation(new Vector3(i, 0, j), GChunk.transform.rotation);

                if (!mChunkData.Contains(new Vector2Int(i,j)))
                {
                    go = GameObject.Instantiate(generateTerrain.gWall, GChunk.transform, false);
                    go.transform.SetLocalPositionAndRotation(new Vector3(i, generateTerrain.gWall.transform.localScale.y,j), GChunk.transform.rotation);
                }
            }
        }
    }

    public List<Vector2Int> GetEnd()
    {
        return vEndPoints;
    }
}
