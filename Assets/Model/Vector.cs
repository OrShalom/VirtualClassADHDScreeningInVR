using UnityEngine;

namespace Assets.Model
{
    public class Vector
    {
        public float x;
        public float y;
        public float z;
        public Vector(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }
    }
}
