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
    [SerializeField]
    int octaves;

    //가질 수 있는 값 0~1f 사이를 가져야함.
    [Range(0f, 1f)]
    [SerializeField]
    float persistance;
    [SerializeField]
    float lacunarity;
    public bool autoUpdate;

    public int seed;
    public Vector2 offset;

    public void GenerateMap()
    {
        //노이즈를 생성한 맵을 가져온다.
        float[,] noiseMap=Noise.GenerateNoiseMap(mapWidth, mapHeight,seed, noiseScale, octaves,persistance,lacunarity,offset);

        MapDisplay mapDisplay = this.GetComponent<MapDisplay>();
        mapDisplay.DrawNoiseMap(noiseMap);
    }

    /*유니티 에디터에서 스크립트 변경사항을 적용하기 위해서 로드되는데, 인스펙터 상 수정될 때 호출하여
     가질 수 없는 값을 막는데 사용한다.*/
    private void OnValidate()
    {
        if(mapWidth<1)
        {
            mapWidth = 1;
        }
        if(mapHeight<1)
        {
            mapHeight = 1;  
        }

        if(lacunarity<1)
        {
            lacunarity = 1;
        }
        if(octaves<0)
        {
            octaves = 0;
        }

    }

}
