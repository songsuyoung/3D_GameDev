using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Noise 객체를 오브젝트에 붙여서 사용할 것이 아니기 때문에 Monobehavior 클래스를 상속받을 필요가 없음
//하나의 클래스로 유지하기 위해서 정적 클래스 생성
public static class Noise
{
    //부동 소수점 2차원 배열
    //noise 규모 : scale
    public static float[,] GenerateNoiseMap(int mapWidth,int mapHeight,float scale)
    {
        float[,] noiseMap = new float[mapWidth,mapHeight];

        //sampleX,sampleY를 0으로 나누는 문제를 해결하기 위해 최소 0.0001f로 설정
        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        for(int y=0;y<mapHeight;y++)
        {
            for(int x=0;x<mapWidth;x++)
            {
                //항상 동일한 값을 가지기 위해서, 정수를 사용할 예정
                float sampleX = x/scale;
                float sampleY = y/scale;
                //높이 값을 결정하길 원함
                float perlinValue=Mathf.PerlinNoise(sampleX, sampleY);

                noiseMap[x,y] = perlinValue;
            }
        }
        return noiseMap;
    }
}
