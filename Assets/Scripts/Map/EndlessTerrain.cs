using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mesh;
using UnityEngine.Rendering;
using System;

public class EndlessTerrain : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrtViwerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate* viewerMoveThresholdForChunkUpdate;
    //얼마나 멀리 떨어져 있는지에 대해 보여주기 위함 => 정적변수로 변경하여 런타임 수준에서 변경할 수 있도록함.
    public static float maxViewDist; 
    Dictionary<Vector2, TerrainChunk> terrainChunk;
    List<TerrainChunk> terrainChunksVisibleLastUpdate;

    public LODInfo[] detailLevels;
    public Material mapMaterial;
    public Transform viewer; //뷰어의 트랜스포머
    public static Vector2 viewerPosition; //위치
    public static Vector2 viewerPositionOld; //이전 위치
    public int chunkSize;
    int chunkVisibleInViewDst; //청크를 볼 수 있는 view distance
    static MapGenerator generator;
    //240x240의 끝없는 지형
    void Start()
    {
        terrainChunk = new Dictionary<Vector2, TerrainChunk>();
        chunkSize = MapGenerator.mapChunkSize-1;
        terrainChunksVisibleLastUpdate=new List<TerrainChunk>();
        chunkVisibleInViewDst = Mathf.RoundToInt(maxViewDist / chunkSize); //볼수있는 시야 범위/청크크기
        maxViewDist = detailLevels[detailLevels.Length - 1].visibleDstThreadhold;
        generator =FindObjectOfType<MapGenerator>();
        //맵데이터를 생성될때마다 받겠다는 의미의 delegate 연결(subscribe)
        UpdateVisibleChunks();
    }

    void Update()
    {

        //상하(z축) 좌우(x축) 높이(y축) 
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        
        if((viewerPositionOld-viewerPosition).sqrMagnitude> sqrtViwerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }
    void UpdateVisibleChunks()
    {
        for(int i=0;i<terrainChunksVisibleLastUpdate.Count;i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear(); //계속적으로 업데이트하면 생성하는걸 막음

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize); //0,0 =>0,0으로 나오겠지
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize); //0,0 =>0,0으로 나오겠지

        
        //주변에 있는 모든 청크 
        for(int yOffset=-chunkVisibleInViewDst;yOffset<=chunkVisibleInViewDst;yOffset++)
        {
            for(int xOffset=-chunkVisibleInViewDst;xOffset<=chunkVisibleInViewDst; xOffset++)
            {
                //300/240 => 1임. 즉, -1 0 1 을 청크 가져오는 것
                //- - - 
                //- -(현재위치) -
                //- - -
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                
                if(terrainChunk.ContainsKey(viewedChunkCoord))
                {
                    //있으면 사용
                    terrainChunk[viewedChunkCoord].UpdateTerrainChunk();
                    if (terrainChunk[viewedChunkCoord].IsVisible)
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunk[viewedChunkCoord]);
                    }
                }
                else
                {
                    //없으면 생성
                    terrainChunk.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord,chunkSize,detailLevels,transform,mapMaterial));
                }
                
            }
        }
    }


    //평면의 generator 필요
    public class TerrainChunk
    {
        Vector2 position;
        GameObject meshObject;
        Bounds bounds; //현재 점에서 가장 가까운 점을 찾기 위해서 사용
        
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        LODInfo[] detailLevels;
        LODMesh[] lodMeshs;

        MapData mapData;
        bool mapDataReceived;

        int previousLODIndex = -1;
        public TerrainChunk(Vector2 coord,int size,LODInfo[] detailLevels,Transform parent,Material material)
        {
            this.detailLevels = detailLevels;
            position =coord*size; //결국 현재 좌표에서 chunksize=240을 곱해서 하나의 메쉬를 만든다.
            //높이는 저장안함
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            bounds = new Bounds(position,Vector2.one*size);
            meshObject = new GameObject("Terrain Chunk"); //새로운 오브젝트 생성.
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();

            meshRenderer.material = material;
            //generator 송신자로부터 메시를 전달받아서 적용할 예정.
            meshObject.transform.position=positionV3;
            meshObject.transform.parent = parent;
            SetVisible(false);
            lodMeshs=new LODMesh[detailLevels.Length];
            
            for(int i=0;i<detailLevels.Length;i++)
            {
                lodMeshs[i] = new LODMesh(detailLevels[i].lod,UpdateTerrainChunk);
            }
            //terrain에서 필요한 내용, 왜냐면 mesh 데이터 생성은 여기서 수행
            generator.RequestMapData(position,OnMapDataReceived); //chunk 생성할때마다 구독자 연결  
        }

        public void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            this.mapDataReceived= true;

            Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            if(mapDataReceived) //mapData가 있을떄만 해당된다. 안받았으면 이를 할 필요없음.
            {
                //최대 보기 거리를 초과하면 메시 객체를 비활성
                float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition)); //distance 를 제곱근 수행

                bool isVisible = viewerDstFromNearestEdge <= maxViewDist;

                if (isVisible)
                {
                    int lodIndex = 0;
                    for (int i = 0; i < detailLevels.Length-1; i++)
                    {
                        if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreadhold)
                        {
                            lodIndex = i + 1; //거리가 멀어져도 된다(즉 자세하게 표현하지 않아도됨을 의미함)
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (previousLODIndex != lodIndex)
                    {
                        LODMesh lodMesh = lodMeshs[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            //현재 메시 설정
                            meshFilter.mesh = lodMesh.mesh; ;
                        }
                        else if (!lodMesh.hasRequestedMesh)
                        {
                            //현재 요청 중이 아니라면 즉 없으면을 의미
                            lodMesh.RequestMesh(mapData); //mapData에 해당하는 메시를 전달해달라고 요청
                        }
                    }
                }
                SetVisible(isVisible);
            }
           
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible
        {
            get { return meshObject.activeSelf; }
        }
    }

    //detail mesh
    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        Action UpdateCallback;
        //level or detail 의미
        public LODMesh(int lod,Action UpdateCallback)
        {
            this.lod=lod;
            this.UpdateCallback = UpdateCallback;

        }

        void OnMeshDataRecived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh= true;

            UpdateCallback();
        }
        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            generator.RequestMeshData(mapData,lod, OnMeshDataRecived);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        //거리에 대한 부동소수점, 임계값을 넘어가면 낮은 수준의 lod로 변경 예정
        public float visibleDstThreadhold;
    }
}
