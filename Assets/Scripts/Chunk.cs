using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Chunk
{
    public static Vector2Int[] vDirections = new Vector2Int[]{
        Vector2Int.up,   // Arriba
        Vector2Int.down, // Abajo
        Vector2Int.right,// Derecha
        Vector2Int.left  // Izquierda
    };

    GenerateTerrain generateTerrain;

    int iSize;
    float fProbabilityBifurcation;

    Vector2Int vStartPoint;
    Vector2Int vEndPoint;

    List<Vector2Int> lPath = new List<Vector2Int>();

    GameObject GChunk;

    public Chunk(int _iSize, Vector2Int _vStartPoint, float _fProbabilityBifurcation, GenerateTerrain _generateTerrain)
    {
        iSize = _iSize;
        vStartPoint = _vStartPoint;
        generateTerrain = _generateTerrain;
        fProbabilityBifurcation = _fProbabilityBifurcation;

        GeneratePathInChunk();
        GChunk = new GameObject("Chunk");
        GChunk.transform.SetParent(generateTerrain.transform);

        GenerateMesh();
    }

    Vector2Int EndPath()
    {
        Vector2 vStartSide = vStartPoint; 
        vStartSide.Normalize(); //lado de entrada
        
        List<Vector2Int> path = (List<Vector2Int>)vDirections.Clone(); // lista con los posibles bordes
        path.Remove(new Vector2Int((int)vStartSide.x, (int)vStartSide.y)); //quitamos la entrada

        int rnd = Random.Range(0, path.Count);
        return path[rnd] * Random.Range(0, iSize);
    }

    void GeneratePathInChunk()
    {
        Vector2Int current = vStartPoint;
        lPath = new List<Vector2Int> { current };

        int iMinSteps =  iSize / 2; // longitud mínima del camino

        while (lPath.Count < iMinSteps || current != vEndPoint)
        {
            List<Vector2Int> possibleDirections = new List<Vector2Int>();

            foreach (Vector2Int dir in vDirections)
            {
                Vector2Int next = current + dir;
                if (CheckMovement(next) && !lPath.Contains(next) && (!IsOnEdge(next) || next == vEndPoint))
                {
                    possibleDirections.Add(dir);
                    next = current;
                }
            }

            if (possibleDirections.Count == 0)
            {
                break; // No hay más movimientos posibles
            }
        }
    }

    bool CheckMovement(Vector2Int vPos)
    {
        return vPos.x >= 0 && vPos.x < iSize && vPos.y >= 0 && vPos.y < iSize;
    }
    bool IsOnEdge(Vector2Int vPos)
    {
        return vPos.x == 0 || vPos.x == iSize - 1 || vPos.y == 0 || vPos.y == iSize - 1;
    }
    void GenerateMesh()
    {
        for (int i = 0; i < iSize; i++)
        {
            for (int j = 0; j < iSize; j++)
            {
                GameObject go;
                if (lPath.Contains(new Vector2Int(i, j)))
                    go = GameObject.Instantiate(generateTerrain.gWall, GChunk.transform, false);
                else
                    go = GameObject.Instantiate(generateTerrain.gPath, GChunk.transform, false);

                go.transform.SetLocalPositionAndRotation(new Vector3(i, 0, j), GChunk.transform.rotation);
            }
        }
    }

    public Vector2Int GetEnd() { return lPath.Last(); }

}