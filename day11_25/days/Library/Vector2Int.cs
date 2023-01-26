namespace Library
{
    public class Vector2Int : IEquatable<Vector2Int>
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Vector2Int(int x, int y) => (X, Y) = (x, y);

        public static Vector2Int operator+(Vector2Int v1, Vector2Int v2) => new Vector2Int(v1.X + v2.X, v1.Y + v2.Y);
        public static Vector2Int operator-(Vector2Int v1, Vector2Int v2) => new Vector2Int(v1.X - v2.X, v1.Y - v2.Y);
        public static bool operator ==(Vector2Int a, Vector2Int b)
        {
            if (a is null)
                return b is null;

            if (b is null)
                return false;

            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Vector2Int a, Vector2Int b) => !(a == b);

        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

        public bool Equals(Vector2Int other) => this == other;

        public override bool Equals(object o)
        {
            if (o == null || GetType() != o.GetType())
                return false;

            var other = (Vector2Int)o;
            return this == other;
        }
    }
}