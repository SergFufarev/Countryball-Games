using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FunnyBlox
{
    public class CountryCollider : MonoBehaviour
    {
        [ContextMenu("CreateMeshCollider")]
        public void CreateMeshCollider()
        {
            var child = transform.GetChild(0);
            child.tag = "Country";
            var mesh = child.gameObject.AddComponent<MeshCollider>();
            mesh.convex = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log($"Enter to {collision.gameObject.name}", collision.gameObject);
        }
    }
}