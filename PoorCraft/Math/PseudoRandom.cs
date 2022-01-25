using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoorCraft.Math
{
    public class PseudoRandom
    {
        private static Random rnd;
        private static float[][][] hashField;
        private static int seed;
        private static int seedA, seedB, seedC, seedD;

        private static bool useCSRandom;
        private static float scale;

        public static void Initiate(int s)
        {
            useCSRandom = true;
            scale = 1f;
            //initiate c# pseudo random
            rnd = new Random(s);

            var f = new int[] { 251, 251, 5 };
            float[][][] jaggedArray = default;

            hashField = CreateJaggedArray(jaggedArray.GetType(), 0, f);
            for (int i = 0; i < 251; i++)
            {
                for (int j = 0; j < 251; j++)
                {
                    for (int k = 0; k < 5; k++)
                    {
                        hashField[i][j][k] = ((float)rnd.Next(0, 1000000) - 500000f) / 1000000f; //prn ranging -1 to +1
                    }
                }
            }
            //initiate custom prn hash
            seed = s;
            seed = (40000 + s % 100000) * 4000;
            seedA = seed + 43876431;
            seedB = seed + 81256937;
            seedC = seed + 124532173;
            seedD = seed + 159683467;
        }

        static float[][][] CreateJaggedArray(Type t, int c, int[] l)
        {
            Array a = Array.CreateInstance(t, l[c]);
            Type type = t.GetElementType();
            if (type != null)
            {
                for (int i = 0; i < l[c]; i++)
                {
                    a.SetValue(CreateJaggedArray(type, c + 1, l), i);
                }
            }
            return (float[][][])a;
        }

        public static void SetCSharpRandomClasUsed(bool o)
        {
            useCSRandom = o;
        }
        public static void SetScale(float s)
        {
            scale = s;
        }

        public static float HashRandom(float x, float y, int index)
        {
            x *= scale; x += 25100;
            y *= scale; y += 25100;
            if (useCSRandom == true)
            {
                return hashField[(int)(x % 251f)][(int)(y % 251f)][index];
            }
            else
            {
                return HashF((int)x, (int)y, index);
            }
        }

        public static float HashF(int i, int j, int index) //Returns a float -1 to 1. Not necessarily well weighted all over range
        {
            return (float)Hash(i, j, index) / 2147483648;
        }

        public static int Hash(int i, int j, int index) //Returns a integer 0 to 2147483647
        {
            int a = i;
            int b = j;
            for (int r = 0; r < 3; r++)
            {
                a = Rotate((a ^ seedA) + (b ^ seedC), 25 - index - index);
                b = Rotate((a ^ seedB) + (b ^ seedD), 3 + index + index);
            }
            return a ^ b;
        }

        private static int Rotate(int x, int b)
        {
            return (x << b) ^ (x >> (32 - b));
        }

    }
}
