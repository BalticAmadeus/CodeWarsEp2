using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic
{
    public class MapData : ICloneable
    {
        public int Width;
        public int Height;
        public TileType[,] Tiles;
        public List<Point>[] RatPositions;
        public int RemainingCookies;

        public TileType this[int row, int col, List<MapChange> changeLog = null]
        {
            get
            {
                return Tiles[row, col];
            }

            set
            {
                Tiles[row, col] = value;
                if (changeLog != null)
                    changeLog.Add(new MapChange(row, col, value));
            }
        }

        public TileType this[Point point, List<MapChange> changeLog = null]
        {
            get
            {
                return Tiles[point.Row, point.Col];
            }

            set
            {
                Tiles[point.Row, point.Col] = value;
                if (changeLog != null)
                    changeLog.Add(new MapChange(point, value));
            }
        }

        public object Clone()
        {
            return new MapData
            {
                Width = this.Width,
                Height = this.Height,
                Tiles = (TileType[,])this.Tiles.Clone(),
                RatPositions = this.RatPositions.Select(p => new List<Point>(p)).ToArray()
            };
        }

        public bool InBounds(Point point)
        {
            return point.Col >= 0
                && point.Row >= 0
                && point.Col < Width
                && point.Row < Height;
        }
    }

    public enum TileType : byte
    {
        Empty = 0,
        Wall,
        Cookie,
        Start0,
        Start1,
        Start2,
        Start3,
        Start4,
        Start5,
        Start6,
        Start7
    }
}
