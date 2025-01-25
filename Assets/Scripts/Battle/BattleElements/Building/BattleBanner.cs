using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBanner : MonoBehaviour
{
    [SerializeField] private MeshRenderer bannerRend;

    public void SetBannerMaterial(Material mat)
    {
        bannerRend.material = mat;
    }
}