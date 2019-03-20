using System;

namespace Framework
{
    public static class Mathi
    {
        public const double PI = 3.1415926D;


        public static int Abs(int value)
        {
            return value >= 0? value : -value;
        }
        public static float Abs(float value)
        {
            return value >= 0f? value : -value;
        }

        public static int Max(int a, int b)
        {
            return a >= b? a : b;
        }
        public static float Max(float a, float b)
        {
            return a >= b? a : b;
        }

        public static int Min(int a, int b)
        {
            return a < b? a : b;
        }
        public static float Min(float a, float b)
        {
            return a < b? a : b;
        }

        public static int Clamp(int value, int min, int max)
        {
            if( min > max )
                return Clamp(value, max, min);

            if( value < min ) return min;
            else if( value > max ) return max;

            return value;
        }

        public static float Clamp(float value, float min, float max)
        {
            if( min > max )
                return Clamp(value, max, min);

            if( value < min ) return min;
            else if( value > max ) return max;

            return value;
        }

        public static double Sqrt(double value)
        {
            return Math.Sqrt(value);
        }
        public static double Pow(double x, double y)
        {
            return Math.Pow(x, y);
        }

        public static float Pow2(float value)
        {
            return value * value;
        }
        public static int Pow2(int value)
        {
            return value * value;
        }

        public static int CeilToInt(float value)
        {
            return (int)Math.Ceiling(value);
        }
        public static int FloorToInt(float value)
        {
            return (int)Math.Floor(value);
        }

        public static double Acos(double value)
        {
            return Math.Acos(value);
        }
    }
}
