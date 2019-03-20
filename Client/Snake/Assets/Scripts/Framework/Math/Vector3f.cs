
namespace Framework
{
    [System.Serializable]
    public struct Vector3f 
    {
        public float x;
        public float y;
        public float z;

        // default parameterless constructor

        public Vector3f(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float Length()
        {
            return (float)Mathi.Sqrt(x * x + y * y + z * z);
        }

        public static float Distance(Vector3f a, Vector3f b)
        {
            return (float)Mathi.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) + (a.z - b.z) * (a.z - b.z));
        }

        /// <summary>
        /// Pow of distance the specified a and b.
        /// </summary>
        public static float Distance2(Vector3f a, Vector3f b)
        {
            return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) + (a.z - b.z) * (a.z - b.z);
        }

        public static double Dot(Vector3f a, Vector3f b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public static double GetAngle(Vector3f a, Vector3f b)
        {
            double cos = (Dot(a, b)) / (a.Length() * b.Length());
            return Mathi.Acos(cos);
        }
        public static double GetAngleDegree(Vector3f a, Vector3f b)
        {
            double cos = (Dot(a, b)) / (a.Length() * b.Length());
            return Mathi.Acos(cos) * 180d / Mathi.PI;
        }

        public Vector3f Normalize()
        {
            float u = (float) (1f / Mathi.Sqrt(x * x + y * y + z * z));
            return new Vector3f(x * u, y * u, z * u);
        }

        public Vector3f Mult(float value)
        {
            return new Vector3f(x * value, y * value, z * value);
        }

        public Vector3f GetDirection(Vector3f dest)
        {
            Vector3f distance = new Vector3f(dest.x - x, dest.y - y, dest.z - z);
            return distance.Normalize();
        }

        public static Vector3f operator +(Vector3f a, Vector3f b)
        {
            return new Vector3f(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3f operator -(Vector3f a, Vector3f b)
        {
            return new Vector3f(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3f operator *(Vector3f a, float value)
        {
            return new Vector3f(a.x * value, a.y * value, a.z * value);
        }

        public static Vector3f operator /(Vector3f a, float value)
        {
            return new Vector3f(a.x / value, a.y / value, a.z / value);
        }


        public static bool IsClose(Vector3f a, Vector3f b)
        {
            if(Distance(a, b) < 0.2f){
                return true;
            }
            return false;
        }
    }
}
