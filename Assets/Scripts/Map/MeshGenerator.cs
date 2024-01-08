using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

//지금 Plane 1개 MeshData 1개 사용
public class MeshData
{
    //삼각형을 이루는 3개의 정점 
    Vector3[] vertices;
    //3개의 정점을 연결한 triangle
    int[] triangles;
    int triangleIndex; //배열 인덱스 관함
    //0과 1의 백분율을 가짐
    Vector2[] uvs; //색상 입히기 위해서

    //법선 벡터 계산을 위한 bordervertex
    Vector3[] borderVertices;
    int[] borderTriangles;

    int borderTriangleIndex;
    bool useFlatShading;
    Vector3[] bakedNormals; //미리 생성된 벡터 사용
    public MeshData(int verticesPerline,bool useFlatShading)
    {
        this.useFlatShading = useFlatShading;

        //배열은 W*H 크기
        vertices = new Vector3[verticesPerline * verticesPerline];
        uvs = new Vector2[verticesPerline * verticesPerline];

        //그 배열 내 삼각형 이룸
        triangles = new int[(verticesPerline-1) * (verticesPerline - 1) * 6];
        borderVertices = new Vector3[verticesPerline * 4 + 4];
        borderTriangles = new int[24*verticesPerline];
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 uv,int vertexIdx)
    {
        if (vertexIdx < 0)
        {
            //경계선에 있는 정점을 포함하는 경우
            //음수~-1 =>양수로 바꾸기 위해서 양수화 시켜서 -1을 빼주면 0~1까지의 값을 가짐
            borderVertices[-vertexIdx-1] = vertexPosition;
            return;
        }
        else
        {
            vertices[vertexIdx] = vertexPosition;
            uvs[vertexIdx] = uv;
        }

    }
    public void AddTriangle(int a,int b,int c)
    {
        if(a<0||b<0||c<0)
        {
            //경계선 위치
            borderTriangles[borderTriangleIndex] = a;
            borderTriangles[borderTriangleIndex + 1] = b;
            borderTriangles[borderTriangleIndex + 2] = c;

            borderTriangleIndex += 3;

        }
        else
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;

            triangleIndex += 3;

        }
    }

    //Mesh 데이터를 메쉬로 만들기

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        //법선벡터를 계산하는 부분인데, 이 부분에 의해서 색상 조명이 잘못 표현되는 것.
        //이를 해결하기 위해서 자체 구현을 해야함 (삼각형의 각 정점마다 법선벡터를 구현)
        //mesh.RecalculateNormals();
        if(useFlatShading)
        {
            mesh.RecalculateNormals(); //컴퓨터에서 계산
        }
        else
        {
            mesh.normals = bakedNormals;
        }
        //RecalculateNormals();
        return mesh;
    }
    //해당 index를 차지하는 법선 벡터 계산 후 전달
    Vector3 SurfaceNormalFromIndices(int indexA,int indexB,int indexC)
    {
        //0보다 작을 경우 경계선에 해당 index가 음수값을 가지면 거꾸로 바꿔, -1을 빼서 0~n으로 정규화해야한다.
        Vector3 pointA= (indexA < 0)? borderVertices[-indexA-1] : vertices[indexA]; //해당 index를 포함하는 삼각형의 좌표
        Vector3 pointB= (indexB < 0)? borderVertices[-indexB-1] : vertices[indexB];
        Vector3 pointC= (indexC < 0) ? borderVertices[-indexC-1] : vertices[indexC];
        //vertexA-B 내적
        
        //두 사이의 벡터
        Vector3 sideAB = pointA - pointB;
        Vector3 sideAC = pointA - pointC;

        //두 벡터간의 외적을 진행했을 때 법선 벡터를 구할 수 있음.
        Vector3 normal=Vector3.Cross(sideAB, sideAC);
        //그리고 법선 벡터의 경우 색상(즉 조명을 표현할 때 사용)
        return normal.normalized; //단위벡터로 정규화
    }

    public void Finalize()
    {
        if(useFlatShading)
        {
            FlatShading();
        }
        else
        {   
            BakedNormals();
        }
    }

    void BakedNormals()
    {
        bakedNormals = CalculateNormals();
    }
    Vector3[] CalculateNormals()
    {
        //정점의 개수만큼  법선 벡터 생성이 가능
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triangleCount=triangles.Length/3;//삼각형의 개수, 삼각형의 점의 개수를 3으로 나누면 삼각형의 개수를 알 수 있음.
        
        //3개의 점이 삼각형을 이루고, 3으로 나누면 삼각형 개수 만큼 알 수 있음.
        //그리고 정점 A B C는 i*3으로 곱한 만큼 idx를 차지(3만큼 증가, 0,1,2)로 알 수 있음
        for(int i=0; i< triangleCount; i++)
        {
            //삼각형의 배열의 인덱스 => triangleCount; (즉 vertexNormals의 개수)
            int normalTriangleIndex = i * 3; //삼각형의 개수 
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex+1];
            int vertexIndexC = triangles[normalTriangleIndex+2];
            //ABC에 대한 법선 벡터

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

            //실제 vertex의 중심값을 A가짐
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }
        triangleCount = borderTriangles.Length / 3;//삼각형의 개수, 삼각형의 점의 개수를 3으로 나누면 삼각형의 개수를 알 수 있음.

        //border을 중심으로 법선 벡터 생성
        for (int i = 0; i < triangleCount; i++)
        {
            //삼각형의 배열의 인덱스 => triangleCount; (즉 vertexNormals의 개수)
            int normalTriangleIndex = i * 3; //삼각형의 개수 
            int vertexIndexA = borderTriangles[normalTriangleIndex];
            int vertexIndexB = borderTriangles[normalTriangleIndex + 1];
            int vertexIndexC = borderTriangles[normalTriangleIndex + 2];
            //ABC에 대한 법선 벡터

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

            //실제 vertex의 중심값을 A가짐
            if(vertexIndexA>=0)
            {
                vertexNormals[vertexIndexA] += triangleNormal;
            }
            if(vertexIndexB>=0)
            {
                vertexNormals[vertexIndexB] += triangleNormal;
            }
            if(vertexIndexC>=0)
            {
                vertexNormals[vertexIndexC] += triangleNormal;
            }
        }

        for (int i=0;i<vertexNormals.Length;i++)
        {
            vertexNormals[i].Normalize(); // 각 벡터값을 단위벡터로 정규화
        }
        return vertexNormals;

    }

    //평면 음영 모드로 전환 
    void FlatShading()
    {
        Vector3[] flatShadedVertices = new Vector3[triangles.Length];
        Vector2[] flatShadedUvs = new Vector2[triangles.Length];    
        for(int i=0;i<triangles.Length;i++)
        {
            flatShadedVertices[i] = vertices[triangles[i]];
            flatShadedUvs[i] = uvs[triangles[i]];
            triangles[i] = i;
        }

        vertices= flatShadedVertices;
        uvs= flatShadedUvs; //해당 메시의 평면음영을 사용할 것인지를 확인
    }
}
public static class MeshGenerator 
{
    //높이에 대한 승수 역할을 하는 변수를 추가해줘야ㅕ함,. => 부동 높이 승수를 추가해줌
    //
    //높이를 가진 heightMap => noiseMap에 float형으로 정의 되어있음
    public static MeshData GenerateTerrainMesh(float[,] heightMap,float heightMultiplier, AnimationCurve _heightCurve,int levelOfDetail,bool useFlatShading)
    {
        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;

        AnimationCurve heightCurve =new AnimationCurve(_heightCurve.keys);
        //사각형을 이루는 정점의 개수(6개)
        //(witdh-1)*(height-1) 맵 크기에서 이루어지는 사각형 개수
        //즉 삼각형의 정점 수 (width-1)*(height-1)*6
        int borderedSize=heightMap.GetLength(0);
        int meshSize = borderedSize - 2 * meshSimplificationIncrement;
        int meshSizeUnsimplified = borderedSize - 2; 

        //중앙값에 배치시키기 위해서 
        float topLeftX = (meshSizeUnsimplified - 1) / -2f;
        float topLeftZ = (meshSizeUnsimplified - 1) / 2;

        //현재 정점 인덱스
        int veticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;

        MeshData data = new MeshData(borderedSize, useFlatShading);

        int[,] vertexIndiceMap = new int[borderedSize, borderedSize];//경계선에 해당하는 값을 생성
        int meshVertexIndex = 0;
        // -1  -2  -3  -4  -5
        // -6   0   1   2  -7
        // -8   3   4   5  -9
        // -10  6   7   8  -11
        // -12 -13 -14 -15 -16
        // 음수가 포함되어 있다면, 경계선에 해당 
        int borderVertexIndex = -1;
        for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            {
                //경계선에 해당한다.
                bool isBorderVertex = y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1;
                
                if(isBorderVertex)
                {
                    vertexIndiceMap[x, y] = borderVertexIndex--; //음수로 하나씩 줄여나간다.
                }
                else
                {
                    //경계선이 아님을 의미~
                    vertexIndiceMap[x, y] = meshVertexIndex++;
                }
            }
        }
        
        for (int y=0; y< borderedSize; y+=meshSimplificationIncrement)
        {
            for(int x=0; x< borderedSize; x+=meshSimplificationIncrement) {

                int vertexIndex = vertexIndiceMap[x, y];
                Vector2 percent = new Vector2((x - meshSimplificationIncrement) / (float)meshSize, (y - meshSimplificationIncrement) / (float)meshSize);

                float height = heightMultiplier * heightCurve.Evaluate(heightMap[x, y]);
                //유니티는 y가 높이임... 언리얼은 z가 높이고..0~1의 값이여서 높이 차가 별로 나지않음.
                Vector3 vertexPosition = new Vector3(topLeftX + percent.x* meshSizeUnsimplified, height, topLeftZ - percent.y* meshSizeUnsimplified);
                //지도의 총 높이 너비 => mesh 크기보다 하나 더 키운것이 -> borderSize 
                //meshSimplificationIncrement를 뺀 값을 0/ 
                
                data.AddVertex(vertexPosition, percent, vertexIndex);
                if (y< borderedSize - 1&&x<borderedSize-1)
                {
                    int a = vertexIndiceMap[x, y];
                    int b = vertexIndiceMap[x+ meshSimplificationIncrement, y];
                    int c = vertexIndiceMap[x,y+ meshSimplificationIncrement];
                    int d = vertexIndiceMap[x+ meshSimplificationIncrement, y+ meshSimplificationIncrement];

                    //삼각형 adc / 삼각형 dab를 만듬
                    data.AddTriangle(a,d,c);
                    data.AddTriangle(d,a,b);
                }

                //위치 추적용, 어디 배열에 위치하는가
                vertexIndex++;
            }
        }
        //mesh생성과 함께 baked Normal 생성, => 이 함수가 thread에 의해 생성됨으로 별도의 스레드 생성 전까지 이전 normal 벡터를 상요함
        data.Finalize();
        return data;
    }


}
