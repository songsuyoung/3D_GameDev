using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TextureGenerator
{
    public static Texture2D TextureFromColourMap(Color[] colourMap, int width,int height)
    {
        //color에 해당하는 texture을 생성하여 리턴
        Texture2D texture=new Texture2D(width,height);

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();

        return texture;
    }

    //높이가 있는 맵
    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        //float [,]배열은 GetLength를 이용해서 값을 가져옴
        //2D Array는 행렬을 의미한다. 
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        //각 픽셀이 얻어야 하는 색상을 설정하고 단계를 이동 => noiseMap (0~1사이의 색상을 가짐)
        //Texture.SetPixel을 사용하면 x,y에 대한 색상이 입혀지는데 
        //위 방법 (반복문을 도는것보다) 색상 배열을 생성하는 것이 훨씬 더 빠른 작업
        Color[] noiseColor = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                //noiseColor은 1차원 배열, 
                int idx = y * width + x;
                //1차원 유니크한 값=행 idx * 너비(칼럼 개수) + 현재 칼럼 위치
                //색상 선형 보간법으로 (noiseMap에 나온 부동소수점)을 이용하여 0~1사이의 값를 적절히
                ////섞어 색상을 입힌다.
                noiseColor[idx] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }

        return TextureFromColourMap(noiseColor,width,height);
    }
}