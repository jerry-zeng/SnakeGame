
namespace Framework
{
    [System.Serializable]
    public struct Vector2f
    {
        public float x;
        public float y;

        public Vector2f(float x, float y)
        {
            this.x = x;
            this.y = y;
        }


        public static double Distance(Vector2f a, Vector2f b)
        {
            return Mathi.Sqrt( (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) );
        }

        public static float Distance2(Vector2f a, Vector2f b)
        {
            return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
        }

        public static Vector2f operator +(Vector2f a, Vector2f b)
        {
            return new Vector2f(a.x + b.x, a.y + b.y);
        }

        public static Vector2f operator -(Vector2f a, Vector2f b)
        {
            return new Vector2f(a.x - b.x, a.y - b.y);
        }

        public static Vector2f operator *(Vector2f a, float value)
        {
            return new Vector2f(a.x * value, a.y * value);
        }

        public static Vector2f operator /(Vector2f a, float value)
        {
            return new Vector2f(a.x / value, a.y / value);
        }
    }

    [System.Serializable]
    public struct Vector2i 
    {
        public int x;
        public int y;

        // default parameterless constructor

        public Vector2i(int x, int y)
        {
            this.x = x;
            this.y = y;
        }


        public static double Distance(Vector2i a, Vector2i b)
        {
            return Mathi.Sqrt( (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) );
        }

        public static int Distance2(Vector2i a, Vector2i b)
        {
            return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
        }

        public static Vector2i operator +(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.x + b.x, a.y + b.y);
        }

        public static Vector2i operator -(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.x - b.x, a.y - b.y);
        }

        public static Vector2i operator *(Vector2i a, int value)
        {
            return new Vector2i(a.x * value, a.y * value);
        }

    }

}
