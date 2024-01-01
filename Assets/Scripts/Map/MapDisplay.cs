using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//노이즈 맵을 가져와 텍스처로 변환 하기 위한 용도
public class MapDisplay : MonoBehaviour
{
    [SerializeField]
    Renderer textureRenderer;

    //map을 실제로 전달하여 Renderer에 텍스처를 입혀줌 
    public void DrawNoiseMap(float[,] noiseMap)
    {
        //float [,]배열은 GetLength를 이용해서 값을 가져옴
        //2D Array는 행렬을 의미한다. 
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        //각 픽셀이 얻어야 하는 색상을 설정하고 단계를 이동 => noiseMap (0~1사이의 색상을 가짐)
        //Texture.SetPixel을 사용하면 x,y에 대한 색상이 입혀지는데 
        //위 방법 (반복문을 도는것보다) 색상 배열을 생성하는 것이 훨씬 더 빠른 작업
        Color[] noiseColor = new Color[width*height];

        for(int y=0; y < height; y++)
        {
            for(int x=0; x < width; x++)
            {
                //noiseColor은 1차원 배열, 
                int idx = y * width + x;
                //1차원 유니크한 값=행 idx * 너비(칼럼 개수) + 현재 칼럼 위치
                //색상 선형 보간법으로 (noiseMap에 나온 부동소수점)을 이용하여 0~1사이의 값를 적절히
                ////섞어 색상을 입힌다.
                noiseColor[idx] = Color.Lerp(Color.black,Color.white,noiseMap[x,y]);
            }
        }

        texture.SetPixels(noiseColor);
        texture.Apply(); //색상 입히기


        //플레이 버튼 없이도(즉 편집기에서 생성할 수 있도록 하는 코드)
        #region 플레이 버튼 없이, 즉 편집기에서 texture을 입힐 수 있는 코드
        //밑 방법은 플레이 버튼을 눌러야지만 볼 수 있음
        //textureRenderer.material.SetTexture("NoiseColorMap", texture);
        textureRenderer.sharedMaterial.mainTexture = texture;
        #endregion

        #region 나중에 삭제 용도
        //나중에 비행기를 띄우기 위한 용도라는데 나는 굳이 필요할까? 
        //나중에 삭제!
        textureRenderer.transform.localScale = new Vector3(width, 1, height);
        #endregion
    }
}
