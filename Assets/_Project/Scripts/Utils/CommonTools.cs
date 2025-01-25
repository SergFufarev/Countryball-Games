using UnityEngine;

namespace FunnyBlox.Tools
{
    [System.Serializable]
    public struct CustomVector3
    {
        public float x;
        public float y;
        public float z;

        public CustomVector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public class CommonTools
    {
        public static CustomVector3 ToCustomVector3(Vector3 vector) => new CustomVector3(vector.x, vector.y, vector.z);

        public static Vector3 FromCustomVector3(CustomVector3 vector) => new Vector3(vector.x, vector.y, vector.z);
    }
}