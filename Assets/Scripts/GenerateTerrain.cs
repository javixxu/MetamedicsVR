using System;
using System.Collections.Generic;
using UnityEngine;

public class GenerateTerrain : MonoBehaviour
{
    public int iSeed;
    public int iChunSize;
    public int iChunkNumSize;
    public GameObject gPath;
    public GameObject gWall;

    Dictionary<Vector2, Chunk> map3D = new Dictionary<Vector2, Chunk>();

    /// <summary>
    /// Inicializar la semilla global para generar el terreno
    /// </summary>
    void SetRandomSeed()
    {
        UnityEngine.Random.InitState(iSeed);
    }

    public void Generate(){
        SetRandomSeed(); // Inicializar la semilla

        Vector2Int vStart = new Vector2Int(iChunSize / 2, iChunSize / 2);
        Vector3 currentPosition = Vector3.zero; // Posición inicial en el editor

        for (int i = 0; i < iChunkNumSize; i++)
        {
            Chunk cChunk = new Chunk(iChunSize, vStart, this);
            map3D.Add(vStart * i, cChunk);

            // Colocar el chunk en la posición actual
            cChunk.GChunk.transform.position = currentPosition;

            // Obtener el punto final del chunk actual
            Vector2Int vEnd = cChunk.GetEnd();

            // Determinar la dirección del movimiento basado en la posición de vEnd
            Vector3 direction = Vector3.zero;
            if (vEnd.x == 0)
                direction = Vector3.left;
            else if (vEnd.x == iChunSize - 1)
                direction = Vector3.right;
            else if (vEnd.y == 0)
                direction = Vector3.back;
            else if (vEnd.y == iChunSize - 1)
                direction = Vector3.forward;

            // Actualizar la posición del siguiente chunk
            currentPosition += direction * iChunSize;

            // Actualizar vStart para el siguiente chunk
            vStart = new Vector2Int(
                direction == Vector3.left ? iChunSize - 1 : (direction == Vector3.right ? 0 : vEnd.x),
                direction == Vector3.back ? iChunSize - 1 : (direction == Vector3.forward ? 0 : vEnd.y)
            );
        }
    }

    private void Start()
    {
        Generate();
    }
}
