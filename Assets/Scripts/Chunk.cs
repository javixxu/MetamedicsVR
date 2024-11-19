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
    private GameObject GChunk;

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

    // Método para verificar si una celda está bloqueada
    public bool IsBlocked(int x, int y)
    {
        return x < 0 || y < 0 || x >= iSize || y >= iSize || mChunkData[x, y];
    }

    // Generación del camino dentro del chunk
    void GeneratePathInChunk()
    {
        // Asegurarse de que el punto final no esté en el borde
        vEndPoint = GetValidEndPoint();

        // Generar el camino desde vStartPoint hasta vEndPoint usando A*
        List<Vector2Int> path = GeneratePath(vStartPoint, vEndPoint);

        // Marcar el camino en mChunkData
        foreach (var point in path)
        {
            mChunkData[point.x, point.y] = true; // Establecer las celdas del camino como pasables
        }

        // Asegurarse de que el camino tiene la longitud mínima
        EnsureMinimumPathLength(path);
    }

    // Generar el camino usando A* (algoritmo de búsqueda)
    List<Vector2Int> GeneratePath(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new List<Vector2Int>();

        // A* pathfinding variables
        var openSet = new HashSet<Vector2Int>(); // Puntos por explorar
        var closedSet = new HashSet<Vector2Int>(); // Puntos ya explorados
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>(); // Para reconstruir el camino
        var gScore = new Dictionary<Vector2Int, float>(); // Distancia desde el punto inicial
        var fScore = new Dictionary<Vector2Int, float>(); // Estimación de distancia al objetivo

        openSet.Add(start);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, end);

        while (openSet.Count > 0)
        {
            // Encontrar el punto con el menor fScore
            Vector2Int current = GetLowestFScoreNode(openSet, fScore);

            // Si hemos llegado al final, reconstruimos el camino
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

            // Explorar los vecinos
            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor) || IsBlocked(neighbor.x, neighbor.y))
                    continue;

                float tentativeGScore = gScore[current] + 1; // Asumimos un costo de 1 por movimiento

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

        return path; // Retorna una lista vacía si no se encuentra camino
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
        return neighbors;
    }

    // Asegurarse de que el camino tenga al menos una longitud de iSize / 2
    void EnsureMinimumPathLength(List<Vector2Int> path)
    {
        if (path.Count < iSize / 2)
        {
            // Si el camino es más corto que iSize / 2, agregar puntos para alargarlo
            Vector2Int lastPoint = path[path.Count - 1];
            while (path.Count < iSize / 2)
            {
                // Aquí, simplemente agregamos un nuevo punto adyacente a la última celda
                // Evita agregar puntos fuera de la cuadrícula o en los bordes
                Vector2Int nextPoint = GetNextValidPoint(lastPoint);
                path.Add(nextPoint);
                lastPoint = nextPoint;
            }
        }
    }

    // Obtener el siguiente punto válido dentro del área del chunk (sin bordes)
    Vector2Int GetNextValidPoint(Vector2Int current)
    {
        // Ejemplo: mover un paso hacia la derecha o hacia abajo (depende de tu lógica)
        Vector2Int nextPoint = new Vector2Int(current.x + 1, current.y);

        // Asegúrate de que el punto esté dentro de los límites y no en el borde
        if (nextPoint.x > iSize || nextPoint.y > iSize || nextPoint.x == 0 || nextPoint.y == 0)
        {
            nextPoint = new Vector2Int(current.x, current.y + 1);
        }

        return nextPoint;
    }

    // Asegurarse de que el punto final no esté en el borde
    Vector2Int GetValidEndPoint()
    {
        // Lista con los 4 bordes posibles
        List<Vector2Int> possibleEdges = new List<Vector2Int> {
            new Vector2Int(1, 0), // Superior
            new Vector2Int(0, iSize - 1), // Inferior
            new Vector2Int(iSize - 1, 0), // Izquierda
            new Vector2Int(iSize - 1, iSize - 1)  // Derecha
        };

        // Eliminar el borde del que proviene el punto de inicio
        if (vStartPoint.x == 0)
            possibleEdges.Remove(new Vector2Int(0, 0)); // Borde izquierdo
        else if (vStartPoint.x == iSize - 1)
            possibleEdges.Remove(new Vector2Int(iSize - 1, 0)); // Borde derecho
        else if (vStartPoint.y == 0)
            possibleEdges.Remove(new Vector2Int(1, 0)); // Borde superior
        else if (vStartPoint.y == iSize - 1)
            possibleEdges.Remove(new Vector2Int(0, iSize - 1)); // Borde inferior

        var x = possibleEdges[Random.Range(0, possibleEdges.Count)];
        var y = Random.Range(1, iSize -1);
        var w = x * y; 

        //Seleccionar un borde aleatorio de los posibles
        return w;
    }



    // Método para generar el mesh del chunk (si es necesario)
    void GenerateMesh()
    {
        for (int i = 0; i < iSize; i++)
        {
            for (int j = 0; j < iSize; j++)
            {
                GameObject go;
                if (mChunkData[i, j])
                    go = GameObject.Instantiate(generateTerrain.gWall, GChunk.transform, false);
                else
                    go = GameObject.Instantiate(generateTerrain.gPath, GChunk.transform, false);

                go.transform.SetLocalPositionAndRotation(new Vector3(i, 0, j), GChunk.transform.rotation);
            }
        }
    }
}
