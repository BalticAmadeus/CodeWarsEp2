using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Linq;
using GameLogic;

namespace Game.WebService.Model
{
    [DataContract]
    public class EnMapData
    {
        [DataMember]
        public int Width;

        [DataMember]
        public int Height;

        [DataMember]
        public List<string> Rows;

        public EnMapData()
        {
            // default
        }

        public EnMapData(MapData md)
        {
            Width = md.Width;
            Height = md.Height;
            Rows = new List<string>();
            for (int row = 0; row < Height; row++)
            {
                Rows.Add(BuildRow(md, row));
            }
        }

        public MapData ToMapData()
        {
            MapData md = new MapData();
            md.Width = Width;
            md.Height = Height;

            if (md.Width > Settings.MapSizeLimit || md.Height > Settings.MapSizeLimit)
                throw new ApplicationException("Map too big");

            md.Tiles = new TileType[Height, Width];

            md.RatPositions = new List<Point>[8];
            for (int i = 0; i < md.RatPositions.Length; i++)
            {
                md.RatPositions[i] = new List<Point>();
            }

            for (int row = 0; row < Height; row++)
            {
                ParseRow(row, Rows[row], md);
            }

            md.RatPositions = md.RatPositions.Where(p => p.Count > 0).ToArray();

            return md;
        }

        private void ParseRow(int row, string data, MapData md)
        {
            for (int col = 0; col < Width; col++)
            {
                TileType tileType = parseTile(data[col]);
                switch (tileType)
                {
                    case TileType.Start0:
                    case TileType.Start1:
                    case TileType.Start2:
                    case TileType.Start3:
                    case TileType.Start4:
                    case TileType.Start5:
                    case TileType.Start6:
                    case TileType.Start7:
                        int playerIndex = (int)tileType - (int)TileType.Start0;
                        md.RatPositions[playerIndex].Add(new Point(row, col));
                        break;
                    default:
                        md.Tiles[row, col] = tileType;
                        break;
                }
            }
        }

        private TileType parseTile(char code)
        {
            switch (code)
            {
                case '#':
                    return TileType.Wall;
                case ' ':
                    return TileType.Empty;
                case '.':
                    return TileType.Cookie;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                    return (TileType)((int)TileType.Start0 + (int)code - (int)'0');
                default:
                    throw new ApplicationException(string.Format("Invalid map tile character '{0}'", code.ToString()));
            }
        }

        public static string BuildRow(MapData md, int row)
        {
            StringBuilder sb = new StringBuilder();
            for (int col = 0; col < md.Width; col++)
            {
                sb.Append(BuildTile(md.Tiles[row, col]));
            }
            return sb.ToString();
        }

        public static char BuildTile(TileType tile)
        {
            switch (tile)
            {
                case TileType.Empty:
                    return ' ';
                case TileType.Wall:
                    return '#';
                case TileType.Cookie:
                    return '.';
                default:
                    return ' ';
            }
        }
    }
}
