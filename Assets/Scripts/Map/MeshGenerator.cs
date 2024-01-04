using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//지금 Plane 1개 MeshData 1개 사용
public class MeshData
{
    //삼각형을 이루는 3개의 정점 
    public Vector3[] vertices;
    //3개의 정점을 연결한 triangle
    public int[] triangles;
    int triangleIndex; //배열 인덱스 관리

    //0과 1의 백분율을 가짐
    public Vector2[] uvs; //색상 입히기 위해서
    public MeshData(int meshWidth, int meshHeight)
    {
        //배열은 W*H 크기
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];

        //그 배열 내 삼각형 이룸
        triangles = new int[(meshWidth-1) * (meshHeight-1) * 6];
    }

    public void AddTriangle(int a,int b,int c)
    {
        if(triangleIndex > triangles.Length)
        {
            return;
        }
        triangles[triangleIndex] = a;
        triangles[triangleIndex+1] = b;
        triangles[triangleIndex+2] = c;

        triangleIndex +=3;
    }

    //Mesh 데이터를 메쉬로 만들기

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();

        return mesh;
    }
}
public static class MeshGenerator 
{
    //높이에 대한 승수 역할을 하는 변수를 추가해줘야ㅕ함,. => 부동 높이 승수를 추가해줌
    //
    //높이를 가진 heightMap => noiseMap에 float형으로 정의 되어있음
    public static MeshData GenerateTerrainMesh(float[,] heightMap,float heightMultiplier, AnimationCurve _heightCurve,int levelOfDetail)
    {
        AnimationCurve heightCurve=new AnimationCurve(_heightCurve.keys);
        //사각형을 이루는 정점의 개수(6개)
        //(witdh-1)*(height-1) 맵 크기에서 이루어지는 사각형 개수
        //즉 삼각형의 정점 수 (width-1)*(height-1)*6
        int width=heightMap.GetLength(0);
        int height=heightMap.GetLength(1);

        //중앙값에 배치시키기 위해서 
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2;

        //현재 정점 인덱스
        int vertexIndex = 0;
        int meshSimplificationIncrement = (levelOfDetail==0)? 1:levelOfDetail*2;
        int veticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        MeshData data = new MeshData(veticesPerLine, veticesPerLine);

        for (int y=0; y<height; y+=meshSimplificationIncrement)
        {
            for(int x=0; x< width; x+=meshSimplificationIncrement) {
               
                    //유니티는 y가 높이임... 언리얼은 z가 높이고..0~1의 값이여서 높이 차가 별로 나지않음.
               data.vertices[vertexIndex] = new Vector3(topLeftX + x, heightMap[x, y] * heightMultiplier * heightCurve.Evaluate(heightMap[x, y]), topLeftZ - y);

                

                if (y<height-1&&x<width-1)
                {
                    data.AddTriangle(vertexIndex, vertexIndex + veticesPerLine + 1, vertexIndex + veticesPerLine);
                    data.AddTriangle(vertexIndex + veticesPerLine + 1, vertexIndex, vertexIndex + 1);
                }

                //지도의 총 높이 너비
                data.uvs[vertexIndex] = new Vector2(x/(float)width, y/(float)height);
                //위치 추적용, 어디 배열에 위치하는가
                vertexIndex++;
            }
        }

        return data;
    }


}
