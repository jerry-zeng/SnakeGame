using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public static class VectorExtensions 
    {
        public static Vector3 ToVector3(this Vector2 value, float z = 0f)
        {
            return new Vector3(value.x, value.y, z);
        }


        public static Vector2 ToVector2(this Vector3 value)
        {
            return new Vector2(value.x, value.y);
        }


    }
}
