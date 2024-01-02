using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//직렬화
[System.Serializable]
//TerrainType  산, 바닥, 물, 빙하인지 등.
public struct TerrainType
{
    public string type; //name water, soil..등
    public float height;
    public Color color;
}
//Map 생성용 클래스

public class MapGenerator : MonoBehaviour
{
    // 색상 모드 or 노이즈 모드일지 결정하기 위해서 enum클래스
    public enum DrawMode {
        ColorMap,
        NoiseMap,
    };

    public DrawMode drawMode;
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
    [SerializeField]
    TerrainType[] regions; //지역을 나타냄.
    public void GenerateMap()
    {
        //노이즈를 생성한 맵을 가져온다.
        float[,] noiseMap=Noise.GenerateNoiseMap(mapWidth, mapHeight,seed, noiseScale, octaves,persistance,lacunarity,offset);

        Color[] colorMap = new Color[mapWidth * mapHeight];
        for (int y=0;y<mapHeight;y++)
        {
            for(int x=0;x<mapWidth;x++)
            {
                //regions에서 지정한 height과 같은 map은 색상을 변경
                float currentHeight=noiseMap[x, y];

                for(int i=0;i< regions.Length; i++) {
                    //높이가 작거나 같으면 색상 변경 
                    if (currentHeight <= regions[i].height)
                    {
                        //(현재행)*열+현재열
                        colorMap[y * mapWidth + x] = regions[i].color;
                        break;
                    }
                }
            }
        }
        MapDisplay mapDisplay = this.GetComponent<MapDisplay>();
        if(drawMode==DrawMode.NoiseMap)
        {
            mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if(drawMode==DrawMode.ColorMap)
        {
            mapDisplay.DrawTexture(TextureGenerator.TextureFromColourMap(colorMap, mapWidth, mapHeight));
        }
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

