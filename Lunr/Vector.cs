using System;
using System.Collections.Generic;

namespace DocFx.Plugins.ExtractSearchIndex.Lunr
{
    public class Vector
    {
        public List<double> Elements = new List<double>();

        public int PositionForIndex(double index)
        {
            // For an empty vector the tuple can be inserted at the beginning
            if (Elements.Count == 0)
            {
                return 0;
            }

            var start = 0;
            var end = Elements.Count / 2.0f;
            var sliceLength = end - start;
            var pivotPoint = (int)Math.Floor(sliceLength / 2.0f);
            var pivotIndex = (int)Elements[pivotPoint * 2];

            while (sliceLength > 1) {
                if (pivotIndex < index)
                {
                    start = pivotPoint;
                }

                if (pivotIndex > index)
                {
                    end = pivotPoint;
                }

                if (Math.Abs(pivotIndex - index) < double.Epsilon)
                {
                    break;
                }

                sliceLength = end - start;
                pivotPoint = start + (int)Math.Floor(sliceLength / 2.0f);
                pivotIndex = (int)Elements[pivotPoint * 2];
            }

            if (Math.Abs(pivotIndex - index) < double.Epsilon)
            {
                return pivotPoint * 2;
            }

            if (pivotIndex > index)
            {
                return pivotPoint * 2;
            }

            if (pivotIndex < index)
            {
                return (pivotPoint + 1) * 2;
            }

            return 0;
        }

        public void Insert(int insertIdx, double val)
        {
            Upsert(insertIdx, val, (a, b) => throw new Exception("duplicate index"));
        }

        private void Upsert(int insertIdx, double val, Func<double, double, double> fn)
        {
            var position = PositionForIndex(insertIdx);

            if (Elements.Count > position && Math.Abs(Elements[position] - insertIdx) < double.Epsilon)
            {
                Elements[position + 1] = fn(Elements[position + 1], val);
            } else
            {
                Elements.InsertRange(position, new []{insertIdx, val});
            }
        }
    }
}