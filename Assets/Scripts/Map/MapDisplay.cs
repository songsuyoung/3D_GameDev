using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//노이즈 맵을 가져와 텍스처로 변환 하기 위한 용도
public class MapDisplay : MonoBehaviour
{
    [SerializeField]
    Renderer textureRenderer;
    [SerializeField]
    MeshFilter meshFilter;
    [SerializeField]
    MeshRenderer meshRenderer;
    //map을 실제로 전달하여 Renderer에 텍스처를 입혀줌 
    public void DrawTexture(Texture2D texture)
    {
        
        //플레이 버튼 없이도(즉 편집기에서 생성할 수 있도록 하는 코드)
        #region 플레이 버튼 없이, 즉 편집기에서 texture을 입힐 수 있는 코드
        //밑 방법은 플레이 버튼을 눌러야지만 볼 수 있음
        //textureRenderer.material.SetTexture("NoiseColorMap", texture);
        textureRenderer.sharedMaterial.mainTexture = texture;
        #endregion

        #region 나중에 삭제 용도
        //나중에 비행기를 띄우기 위한 용도라는데 나는 굳이 필요할까? 
        //나중에 삭제!
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
        #endregion
    }

    public void DrawMesh(Mesh meshData,Texture2D texture)
    {
        meshFilter.sharedMesh= meshData;
        meshRenderer.sharedMaterial.mainTexture= texture; 
    }

}
