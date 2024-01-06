using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

//Noise 객체를 오브젝트에 붙여서 사용할 것이 아니기 때문에 Monobehavior 클래스를 상속받을 필요가 없음
//하나의 클래스로 유지하기 위해서 정적 클래스 생성
public static class Noise
{
    public enum NormalizeMode{
        Local, //지역값
        Global, //전역
    };
    //부동 소수점 2차원 배열
    //noise 규모 : scale
    //사용할 옥타브 개수(예를 들어, 메인 경계선, 큰 돌, 작은 돌을 의미)
    //주기 : Lacurarity, 증폭을 의미하는 Persistance 
    //Lacurarity^n , Persistence^n
    public static float[,] GenerateNoiseMap(int mapWidth,int mapHeight,int seed,float scale,int octaves,float persistance, float lacunarity,Vector2 offset, NormalizeMode normalizeMode)
    {

        System.Random prng = new System.Random(seed);
        float[,] noiseMap = new float[mapWidth,mapHeight];
        Vector2[] octaveOffsets = new Vector2[octaves];
        float maxPossibleHeight = 0;
        //외부에서 진폭과 주기가 생성되어짐.
        float amplitude = 1;
        float frequency = 1;
        for (int i=0;i<octaves; i++)
        {
            //-100000~100000 값?
            float offsetX = prng.Next(-100000,100000)+ offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y; //이동 방향을 반대로 변경하기 위해서 
            maxPossibleHeight += amplitude;
            //진폭에 지속성을 곱함
            amplitude *= persistance; //소수점을 곱하여 각 오타브가 감소하도록

            octaveOffsets[i]= new Vector2(offsetX, offsetY);
        }
        //sampleX,sampleY를 0으로 나누는 문제를 해결하기 위해 최소 0.0001f로 설정
        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float minLocalNoiseHeight=float.MaxValue, maxLocalNoiseHeight=float.MinValue;
        float halfWidth=mapWidth/2f;
        float halfHeight = mapHeight / 2f; //중앙 위치

        for(int y=0;y<mapHeight;y++)
        {
            for(int x=0;x<mapWidth;x++)
            {
                //외부에서 진폭과 주기가 생성되어짐.
                amplitude = 1;
                frequency = 1;
                //현재 높이 값을 추적하여 float noise생성
                float noiseHeight = 0; //초기에는 0

                for(int i=0;i< octaves;i++)
                {
                    //lacurarity와 persistence는 옥타브마다 배치되어야함.
                    //항상 동일한 값을 가지기 위해서, 정수를 사용할 예정
                    //이동할 때 모양이 변하는 문제점을 바꾸기 위해서, offset에 영향받지 않도록 ()삽입
                    float sampleX = (x-halfWidth + octaveOffsets[i].x) / scale * frequency; //주파수 적용
                    float sampleY = (y-halfHeight + octaveOffsets[i].y) / scale * frequency;
                    //주파수가 높을수록 샘플 포인트가 더 멀리 떨어져 있음을 의미 => 높이 값이 더 빠르게 변경된다는 것을 의미
                    //높이 값을 결정하길 원함 => -1~1사이의 값을 가질 수 있도록 한다.
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY)*2-1;
                     //현재 얻을 수 있는 최대 높이값을 알려면 noiseHeight의 최대는 아는것.
                    noiseHeight+= perlinValue*amplitude; //임의의 perlin 노이즈 수(0~1.0f)*진폭 
                    
                    frequency *= lacunarity; //소수점을 곱해서 주파수가 각 오타브를 증가시키도록
                    amplitude *= persistance; //소수점을 곱하여 각 오타브가 감소하도록
                    //흥미로운 값을 얻기 위해서 노이즈 높이가 감소하는 것이 좋음.
                }
                //옥타브의 perlinValue만큼 noiseHeight를 늘릴 예정 => why 3d구현을 위해
                noiseMap[x, y] = noiseHeight;
                minLocalNoiseHeight = Mathf.Min(minLocalNoiseHeight, noiseHeight);
                maxLocalNoiseHeight = Mathf.Max(maxLocalNoiseHeight, noiseHeight);

            }
        }
        //노이즈 맵은 노이즈의 높이를 의미한다.
        //원래 값으로 돌아가기 위해서는 min값과 max값을 추적해야한다.
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if(normalizeMode==NormalizeMode.Local)
                {
                    //0과 1로 정규화하고 있어서 => 완벽하게 정렬되지 않은 이유 : 
                    //최소~최고점을 중심으로 선형보간법을 이용하여 noiseMap의 비율만큼 보간한다.
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                    //0~1사이의 값을 반환. minHeight 0, maxHeight같으면 1, 그 중간에 있다면 point 5을 의미
                }
                else
                {
                    //0~1사이로 정규화
                    float normalizeHeight = (noiseMap[x, y]+1)/(2f*maxPossibleHeight/2f);
                    noiseMap[x, y] = Mathf.Clamp(normalizeHeight, 0, int.MaxValue);
                    //문제 => 값이 전체적으로 줄어 들어서, 전부 바다가 되어버림...!
                }
            }
        }

        return noiseMap;
    }
}
