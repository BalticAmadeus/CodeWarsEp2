using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeManPoc
{
    public enum MapTileType
    {
        EMPTY,
        WALL,
        POINT,
        PLAYER0,
        PLAYER1
    }

    public struct MapPoint
    {
        public int Row;
        public int Col;

        public MapPoint(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public override bool Equals(object obj)
        {
            MapPoint other = (MapPoint)obj;
            return other.Row == this.Row && other.Col == this.Col;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            unchecked
            {
                hash = hash * 23 + Row.GetHashCode();
                hash = hash * 23 + Col.GetHashCode();
            }
            return hash;
        }

        public override string ToString()
        {
            return string.Format("[{0},{1}]", Row, Col);
        }
    }

    public class GameMap
    {
        public int Width;
        public int Height;
        public MapTileType[,] Tiles;

        public MapTileType this[int row, int col]
        {
            get
            {
                return Tiles[row, col];
            }
            set
            {
                Tiles[row, col] = value;
            }
        }

        public MapTileType this[MapPoint point]
        {
            get
            {
                return Tiles[point.Row, point.Col];
            }
            set
            {
                Tiles[point.Row, point.Col] = value;
            }
        }

        public GameMap(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public GameMap(string fileName)
        {
            string[] line = File.ReadAllLines(fileName);
            Width = line[0].Length;
            Height = line.Length;
            Tiles = new MapTileType[Height, Width];
            for (int i = 0; i < line.Length; i++)
            {
                ParseRow(i, line[i]);
            }
        }

        private void ParseRow(int row, string line)
        {
            for (int col = 0; col < Width; col++)
            {
                MapTileType tile = parseCell(line[col]);
                Tiles[row, col] = tile;
            }
        }

        private MapTileType parseCell(char cell)
        {
            switch (cell)
            {
                case ' ':
                    return MapTileType.EMPTY;
                case '#':
                    return MapTileType.WALL;
                case '.':
                    return MapTileType.POINT;
                case '0':
                    return MapTileType.PLAYER0;
                case '1':
                    return MapTileType.PLAYER1;
                default:
                    return MapTileType.EMPTY;
            }
        }
    }
}
