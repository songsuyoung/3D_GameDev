using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//맵 생성기를 이용해 맵을 만들 때, 육지가 바다로 전체 둘러싸여 있음을 표현하고 싶을 때 
//사용하는 맵 전체 지도 생성 

/*
 
 f(x) = (x^a)/(x^a+(1-x)^a)의 값을 이용한다면 
 곡선 그래프를 얻을 수 있고, 0에 가까우면 검은색 표현, 1에 가까우면 흰색 표현을 한다.
 즉, 곡선의 방적식을 이용해서 해당 패턴을 다양하게 만들 수 있다는 소리.
 */
public static class FalloffGenerator
{
    //fall off(가장자리가 0이고 중앙으로 갈 수록 1로 변화하는 맵 생성)
    //현재 사이즈 240
    public static float[,] GenerateFalloffMap(int size)
    {
        float[,] falloffMap=new float[size,size];

        //각 값이 점의 좌표라고 생각 했을 때,
        for(int i = 0; i<size;i++)
        {
            for(int j=0;j<size;j++)
            {
                //해당 좌표를 -1~1사이의 범위로 생성 
                //높이가 -1이면 검정색으로 표현되어서?

                float x = i /(float) size*2-1; //0~1사이의 값을 가짐
                //0.4*2=> 0.8-1; -0.2를 가짐..즉 음수로 만들기 정규화 과정 
                float y = j / (float)size*2-1;
                
                //x,y가 사각형의 가장자리에 가까운가를 알아보기 위함.
                //둘 다 0에 가까울 수록, 중앙에 가깝고 1에 가까울 수록 바깥을 향함을 알 수 있음
                //그렇기 때문에 둘 중 가장 큰값을 가지고 위치를 알아보는 과정.
                //val값의 가장자리가 1에, 중앙이 0에 가까워짐.. 하지만, noise맵을 해당 값과 빼면 반대의 결과가 나옴
                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                falloffMap[i,j] = Evaluate(value); //가장 큰 값만 삽입
            }
        }
        return falloffMap;
    }

    static float Evaluate(float value)
    {
        float a = 3f; //순탄함? 가파름의 정도
        float b = 2.2f; //커짐 정도를 나타냄

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b*value, a));
    }
}
