using UnityEngine;

namespace Ability.Editor.Combo
{
    static class ComboGraphLayout
    {
        public const float NodeWidth = 300f;
        public const float NodeHeight = 220f;
        public const float GapX = 380f;
        public const float GapY = 300f;
        public const float StartX = 80f;
        public const float StartY = 120f;
        public const int ColumnCount = 4;

        public static Rect GetDefaultPosition(int index)
        {
            if (index < 0)
            {
                index = 0;
            }

            var row = index / ColumnCount;
            var column = index % ColumnCount;
            return new Rect(StartX + (column * GapX), StartY + (row * GapY), NodeWidth, NodeHeight);
        }

        public static Rect GetPositionAt(Vector2 position)
        {
            return new Rect(position.x, position.y, NodeWidth, NodeHeight);
        }
    }
}
