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
        MeshMap,
    };

    public const int mapChunkSize = 241; //(241-1)/i;
    [Range(0,6)] //2의 곱셈
    public int levelOfDetail; //240의 인수, 1,2,4,6,8,10,12 => 자세하게 나옴

    public DrawMode drawMode;
    // Vector2 -float형 ( mapWidth,mapHeight )

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

    [SerializeField]
    float meshHeightMultiplier;

    public AnimationCurve meshHeightCurve; //LOD
    public bool autoUpdate;

    public int seed;
    public Vector2 offset;
    [SerializeField]
    TerrainType[] regions; //지역을 나타냄.
    public void GenerateMap()
    {
        //노이즈를 생성한 맵을 가져온다.
        float[,] noiseMap=Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves,persistance,lacunarity,offset);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        for (int y=0;y< mapChunkSize; y++)
        {
            for(int x=0;x< mapChunkSize; x++)
            {
                //regions에서 지정한 height과 같은 map은 색상을 변경
                float currentHeight=noiseMap[x, y];

                for(int i=0;i< regions.Length; i++) {
                    //높이가 작거나 같으면 색상 변경 
                    if (currentHeight <= regions[i].height)
                    {
                        //(현재행)*열+현재열
                        colorMap[y * mapChunkSize + x] = regions[i].color;
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
            mapDisplay.DrawTexture(TextureGenerator.TextureFromColourMap(colorMap, mapChunkSize, mapChunkSize));
        }else if(drawMode==DrawMode.MeshMap)
        {
            //컬러 색상 전달 , 높이값을 가진 noiseMap을 이용해 Mesh를 생성하여 그린다.
            mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier,meshHeightCurve,levelOfDetail).CreateMesh(), TextureGenerator.TextureFromColourMap(colorMap, mapChunkSize, mapChunkSize));
            
        }

    }

    /*유니티 에디터에서 스크립트 변경사항을 적용하기 위해서 로드되는데, 인스펙터 상 수정될 때 호출하여
     가질 수 없는 값을 막는데 사용한다.*/
    void OnValidate()
    {
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

