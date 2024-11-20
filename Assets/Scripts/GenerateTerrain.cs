using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateTerrain : MonoBehaviour
{
    [Header("Basic Configuration")]
    public int Seed;
    public int ChunkSize;
    public int ChunkNumSize;

    [Header("Aditional Configuration")]
    [Range(0.0f, 1.0f)]
    public float centerFactor;// Movimiento desde el centro (0.0 - 1.0)
    [Range(0.0f, 1.0f)]
    public float pathIrregularity; // Irregularidad del camino (0.0 - 1.0)

    [Header("Prefabs")]
    public GameObject gFloor;
    public GameObject gWall;

    Dictionary<Vector3, Chunk> map3D = new Dictionary<Vector3, Chunk>();

    void SetRandomSeed()
    {
        UnityEngine.Random.InitState(Seed);
    }

    public void Generate(){
        SetRandomSeed(); // Inicializar la semilla

        Vector2Int vStart = new Vector2Int(ChunkSize / 2, ChunkSize / 2);
        Vector3 currentPosition = Vector3.zero; // Posición inicial en el editor
        float bifurcationProbability = 0.05f; // Probabilidad inicial de bifurcaciones

        for (int i = 0; i < ChunkNumSize; i++)
        {
            Chunk cChunk = 
                new Chunk(ChunkSize,new List<Vector2Int>{ vStart }, bifurcationProbability, centerFactor, pathIrregularity, this);

            map3D.Add(currentPosition, cChunk);

            // Colocar el chunk en la posición actual
            cChunk.GChunk.transform.position = currentPosition;

            // Obtener el punto final del chunk actual
            Vector2Int vEnd = cChunk.GetEnd().First();

            // Determinar la dirección del movimiento basado en la posición de vEnd
            Vector3 direction = Vector3.zero;
            if (vEnd.x == 0)
                direction = Vector3.left;
            else if (vEnd.x == ChunkSize - 1)
                direction = Vector3.right;
            else if (vEnd.y == 0)
                direction = Vector3.back;
            else if (vEnd.y == ChunkSize - 1)
                direction = Vector3.forward;

            // Actualizar la posición del siguiente chunk
            currentPosition += direction * ChunkSize;
           
            // Actualizar vStart para el siguiente chunk
            vStart = new Vector2Int(
                direction == Vector3.left ? ChunkSize - 1 : (direction == Vector3.right ? 0 : vEnd.x),
                direction == Vector3.back ? ChunkSize - 1 : (direction == Vector3.forward ? 0 : vEnd.y)
            );

            bifurcationProbability = Mathf.Min(0.6f, bifurcationProbability + i * 0.05f);
        }
    }

    private void Start()
    {
        Generate();
    }
}
