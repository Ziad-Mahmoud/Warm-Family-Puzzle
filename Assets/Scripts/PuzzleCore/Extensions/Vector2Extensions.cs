namespace PuzzleCore.Extensions
{
    public static class Vector2Extensions
    {
        public static Vector2 SetX(this Vector2 vector, int x)
        {
            vector.x = x;
            return vector;
        }
        public static Vector2 SetY(this Vector2 vector, int y)
        {
            vector.y = y;
            return vector;
        }
    }
}
