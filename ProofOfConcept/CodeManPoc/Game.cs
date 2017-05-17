using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeManPoc
{
    public class Game
    {
        public int Turn;
        public GameMap Map;
        public PlayerState[] Players;
        public int CookiesLeft;

        public Game(GameMap map)
        {
            Map = map;
            Players = new PlayerState[]
            {
                new PlayerState(), new PlayerState()
            };
            for (int row = 0; row < map.Height; row++)
            {
                for (int col = 0; col < map.Width; col++)
                {
                    switch(map[row, col])
                    {
                        case MapTileType.PLAYER0:
                            Players[0].Agents.Add(new MapPoint(row, col));
                            map[row, col] = MapTileType.EMPTY;
                            break;
                        case MapTileType.PLAYER1:
                            Players[1].Agents.Add(new MapPoint(row, col));
                            map[row, col] = MapTileType.EMPTY;
                            break;
                        case MapTileType.POINT:
                            CookiesLeft++;
                            break;
                    }
                }
            }
            for (int i = 0; i < Players.Length; i++)
            {
                Players[i].Target = new MapPoint?[Players[i].Agents.Count];
            }
        }
    }

    public class PlayerState
    {
        public List<MapPoint> Agents;
        public int Score;

        public MapPoint?[] Target;

        public PlayerState()
        {
            Agents = new List<MapPoint>();
        }
    }

    public class GameSight
    {
        public MapPoint Position;
        public int Distance;
        public MapTileType Obstacle;
    }

}
