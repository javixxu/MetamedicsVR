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

    void Generate(){
        SetRandomSeed();//inicializar la semilla

        Vector2Int vStart = new Vector2Int(iChunSize/2, iChunSize / 2);
        float probabilityBifurcation = Mathf.Pow(Mathf.Clamp01((0 - 1) / (float)iChunkNumSize), 2);
        Chunk cChunk = new Chunk(iChunSize, vStart, this);

        for (int i = 0; i < iChunkNumSize; i++) {


            //map3D.Add(new Vector2(), cChunk);
            //vStart = cChunk.GetEnd();
        }
    }

    private void Start()
    {
        Generate();
    }
}
