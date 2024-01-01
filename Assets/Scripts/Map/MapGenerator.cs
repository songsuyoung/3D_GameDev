using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Map 생성용 클래스
public class MapGenerator : MonoBehaviour
{
    // Vector2 -float형 ( mapWidth,mapHeight )
    [SerializeField]
    int mapWidth;
    [SerializeField] 
    int mapHeight;
    [SerializeField]
    float noiseScale;

    public bool autoUpdate;
    public void GenerateMap()
    {
        //노이즈를 생성한 맵을 가져온다.
        float[,] noiseMap=Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale);

        MapDisplay mapDisplay = this.GetComponent<MapDisplay>();
        mapDisplay.DrawNoiseMap(noiseMap);
    }

}
