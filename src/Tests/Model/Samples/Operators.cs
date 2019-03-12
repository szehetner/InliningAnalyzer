using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Model.Samples
{
    public class Operators
    {
        public void Main()
        {
            Amount a = new Amount(1);
            Amount b = new Amount(2);
            Amount c = a + b;
        }
    }

    public class Amount
    {
        public int Value { get; set; }

        public Amount(int value)
        {
            Value = value;
        }

        public static Amount operator +(Amount a, Amount b)
        {
            return new Amount(a.Value + b.Value);
        }
    }

    public class BM
    {
        public VecWithFields TestVecWithFields()
        {
            var p = new VecWithFields(12f, 5f, 1.5f);
            var r = new VecWithFields(12f, 5f, 1.5f);
            for (int i = 0; i < 100000; i++)
            {
                r = p + r;
                //r = p * r;
            }
            return r;
        }

        public VecWithProperties TestVecWithProperties()
        {
            var p = new VecWithProperties(12f, 5f, 1.5f);
            var r = new VecWithProperties();
            for (int i = 0; i < 100000; i++)
            {
                r = p + r;
                //r = p * r;
            }
            return r;
        }

        public struct VecWithFields
        {
            public float X;
            public float Y;
            public float Z;

            public VecWithFields(float x, float y, float z)
            {
                this.X = x;
                this.Y = y;
                this.Z = z;
            }

            public static VecWithFields operator *(VecWithFields q, VecWithFields r)
            {
                return new VecWithFields(q.X * r.X, q.Y * r.Y, q.Z * r.Z);
            }

            public static VecWithFields operator +(VecWithFields q, VecWithFields r)
            {
                return new VecWithFields(q.X + r.X, q.Y + r.Y, q.Z + r.Z);
            }
        }


        public struct VecWithProperties
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }

            public VecWithProperties(float x, float y, float z)
            {
                this.X = x;
                this.Y = y;
                this.Z = z;
            }

            public static VecWithProperties operator *(VecWithProperties q, VecWithProperties r)
            {
                return new VecWithProperties(q.X * r.X, q.Y * r.Y, q.Z * r.Z);
            }

            public static VecWithProperties operator +(VecWithProperties q, VecWithProperties r)
            {
                return new VecWithProperties(q.X + r.X, q.Y + r.Y, q.Z + r.Z);
            }

        }
    }
}
