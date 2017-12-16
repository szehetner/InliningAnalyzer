using System.Runtime.CompilerServices;

namespace ex
{
    public struct Cell
    {
        public int X;
        public int Y;
    }
    public sealed unsafe class CellArray1
    {
        private Cell* _data;
        Cell[] cells;

        public CellArray1(int size)
        {
            cells = new Cell[size];
            fixed (Cell* c = &cells[0])
            {
                _data = c;
            }
        }

        public ref Cell this[int pos]
        {
            get { return ref *(_data + pos); }
        }

        public ref Cell At(int pos)
        {
            return ref *(_data + pos);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var m1 = new CellArray1(4096);

            var sum1 = 0;

            for (var i = 0; i < 4096; i++)
            {
                m1[i].X = i;
                m1.At(i).Y = -1;
                sum1 += m1[i].X;
            }
        }
    }
}
