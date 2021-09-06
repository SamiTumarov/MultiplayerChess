using System.Collections;
using System.Collections.Generic;

namespace ChessWPF
{
    public class PointsCollection: IEnumerable
    {
        private List<_Point> Points;
        const int ARRAY_SIZE = 6;

        public PointsCollection()
        {
            this.Points = new List<_Point>(ARRAY_SIZE);
        }

        /// <summary> Adds a point to collection </summary>
        public void AddPoint(int row, int col)
        {
            this.Points.Add(new _Point(row, col));
        }

        /// <summary> Adds a point to collection </summary>
        public void AddPoint(_Point Point)
        {
            this.Points.Add(Point);
        }

        /// <summary> Removes a point from collection </summary>
        public bool RemovePoint(_Point Point)
        {
            return this.Points.Remove(Point);
        }

        /// <summary> Checks if a point is in collection. Not by reference but by values </summary>
        public bool PointInCollection(_Point Point)
        {
            foreach (_Point PointInCollection in this.Points)
            {
                if (PointInCollection.Equals(Point))
                    return true;
            }

            return false;
        }

        /// <summary> Checks if a point is in collection. Not by reference but by values </summary>
        public bool PointInCollection(int row, int col)
        {
            return this.PointInCollection(new _Point(row, col));
        }


        /// <summary> Allow foreach iteration </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Points.GetEnumerator();
        }

        public int Count
        {
            get { return this.Points.Count;  }
        }

        public _Point this[int index]
        {
            get { return this.Points[index]; }
        }
    }
    
    /// <summary> Point class. Cotains row and col.
    /// Replacing the need to use tuples for cells. </summary>
    public class _Point
    {
        private int row;
        private int col;

        public _Point(int row, int col)
        {
            this.row = row;
            this.col = col;
        }

        public bool Equals(_Point Other)
        {
            return (Other != null && Other.Row == this.Row && Other.Col == this.Col);
        }

        public int Row
        {
            get { return this.row; }
            set { this.row = value; }
        }

        public int Col
        {
            get { return this.col; }
            set { this.col = value; }
        }
    }
}
