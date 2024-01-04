using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.Profiling.Memory.Experimental;
using System.Linq;
using static UnityEngine.EventSystems.EventTrigger;


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
    public int editorPreviewLOD; //240의 인수, 1,2,4,6,8,10,12 => 자세하게 나옴

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

    //맵에 대한 색상, 높이 값
    Queue<MapThreadInfo<MapData>> mapDatas=new Queue<MapThreadInfo<MapData>>();   
    //mesh에 대한 정보
    Queue<MapThreadInfo<MeshData>> meshDatas=new Queue<MapThreadInfo<MeshData>>();
    
    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);
        MapDisplay mapDisplay = this.GetComponent<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            mapDisplay.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.MeshMap)
        {
            //컬러 색상 전달 , 높이값을 가진 noiseMap을 이용해 Mesh를 생성하여 그린다.
            mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD).CreateMesh(), TextureGenerator.TextureFromColourMap(mapData.colorMap, mapChunkSize, mapChunkSize));

        }
    }

    public void RequestMeshData(MapData mapData,int lod,Action<MeshData> callback)
    {
        //함수 연결 
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData,lod,callback); //파라미터로 받은 callback(구독자를 연결)
        };

        new Thread(threadStart).Start();    
    }

    //해당 맵에 대한, mesh 정보를 가짐.
    void MeshDataThread(MapData mapData,int lod,Action<MeshData> callback)
    {
        //메시 데이터 생성
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
        lock(meshDatas)
        {
            //생성된 메시 데이터를 스레드 하나만 건드릴 수 있도록 롹해놓음
            meshDatas.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }
    public void RequestMapData(Vector2 centre,Action<MapData> callback)
    {
        //mapData가 완성되면 콜백 호출 하는 함수 
        //callback 함수 전달 -> event - receive : publishers - subscribers 패턴
        ThreadStart threadStart = delegate
        {
            //callback으로 전달된 함수로 수신해줄 것 ( 콜백 등록 )
            MapDataThread(centre,callback);
        }; //thread 생성

        new Thread(threadStart).Start(); //쓰레드 목록 삽입
    }

    //MAP 데이터를 계산할 데이터 스레드 : 왜 필요한가? 다른작업과 동시에 진행되어야하는 일이기때문에 thread로 생성
    void MapDataThread(Vector2 centre,Action<MapData> callback)
    {
        //새로운 맵을 생성 => generateMap 이 쓰레드 내부에서 생성될 것.
        MapData mapData = GenerateMapData(centre);
        //다른 스레드가 침범하지 않도록 lock을 함
        //즉 한 스레드가 아래 작업을 하는 동안 다른 스레드는 접근 할 수 없음을 의미
        lock(mapDatas)
        {
            //호출할때마다 호출한 함수,생성된 맵을 큐에 삽입
            mapDatas.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
            // 1. mapData를 생성하여 콜백 큐에 추가한다.
            //생성이 완료되면 requestMapData 전달
        }
    }
    MapData GenerateMapData(Vector2 centre)
    {
        //노이즈를 생성한 맵을 가져온다.
        float[,] noiseMap=Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves,persistance,lacunarity, centre+offset);

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

        MapData mapData = new MapData(noiseMap,colorMap);
        
        return mapData;

    }

    private void Update()
    {
        if(mapDatas.Count>0)
        {
            for(int i=0;i<mapDatas.Count;i++)
            {
                //call back 맵데이터와 함께 콜백 수행
                MapThreadInfo<MapData> threadInfo = mapDatas.Dequeue();
                //해당 callback(함수 레퍼런스로 함수 파라미터 전달 => delegate 함수표현)
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if(meshDatas.Count>0)
        {
            for(int i=0;i<meshDatas.Count;i++)
            {
                MapThreadInfo<MeshData> threadInfo= meshDatas.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
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
    //스레드 정보 매핑과 같은 구조체, Thread 뿐만 아니라 다른 정보에서도 사용하기 위해 generic 하게 사용
    //MapThreadInfo 자체를 Queue처리 할예정인가봄...
    struct MapThreadInfo<T>
    {
        //읽기만 가능
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}


//Noise Map, 색상 맵을 담을 수 있는 구조체가 필요
public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap,Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}