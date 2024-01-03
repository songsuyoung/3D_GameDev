using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    //얼마나 멀리 떨어져 있는지에 대해 보여주기 위함
    public const float maxViewDist=300f;
    Dictionary<Vector2, TerrainChunk> terrainChunk;
    List<TerrainChunk> terrainChunksVisibleLastUpdate;

    public Transform viewer; //뷰어의 트랜스포머
    public static Vector2 viewerPosition; //위치
    int chunkSize;
    int chunkVisibleInViewDst; //청크를 볼 수 있는 view distance

    //240x240의 끝없는 지형
    void Start()
    {
        terrainChunk = new Dictionary<Vector2, TerrainChunk>();
        chunkSize = MapGenerator.mapChunkSize-1;
        terrainChunksVisibleLastUpdate=new List<TerrainChunk>();
        chunkVisibleInViewDst = Mathf.RoundToInt(maxViewDist / chunkSize); //볼수있는 시야 범위/청크크기
    }

    void Update()
    {

        //상하(z축) 좌우(x축) 높이(y축) 
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
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
                    terrainChunk.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord,chunkSize,transform));
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
        public TerrainChunk(Vector2 coord,int size,Transform parent)
        {
            position=coord*size; //결국 현재 좌표에서 chunksize=240을 곱해서 하나의 메쉬를 만든다.
            //높이는 저장안함
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            bounds = new Bounds(position,Vector2.one*size);
            meshObject= GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position=positionV3;
            meshObject.transform.localPosition = Vector3.one * size / 10f;

            meshObject.transform.parent = parent;
            SetVisible(false);
        }

        public void UpdateTerrainChunk()
        {
            //최대 보기 거리를 초과하면 메시 객체를 비활성
            float viewerDstFromNearestEdge=Mathf.Sqrt(bounds.SqrDistance(viewerPosition)); //distance 를 제곱근 수행

            bool isVisible = viewerDstFromNearestEdge <= maxViewDist;
            SetVisible(isVisible);
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
}
