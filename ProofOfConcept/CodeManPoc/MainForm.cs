using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeManPoc
{
    public partial class MainForm : Form
    {
        private MapRendering _rendering;
        private GameMap _map;
        private Game _game;
        private Random _rnd;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _rendering = new MapRendering(boardPic);
            _rnd = new Random();
            ResetGame();
            _rendering.Render(_game);
        }

        private void ResetGame()
        {
            _map = new GameMap("map.txt");
            _game = new Game(_map);
        }

        //private int weighType(GameSight sight)
        //{
        //    switch (sight.Obstacle)
        //    {
        //        case MapTileType.POINT:
        //            return 0;
        //        case MapTileType.WALL:
        //            return 1;
        //        case MapTileType.GHOST1:
        //            return 2;
        //        default:
        //            return 3;
        //    }
        //}

        //private void lookRay(List<GameSight> rays, int rdiff, int cdiff)
        //{
        //    int row = _map.Pacman.Row + rdiff;
        //    int col = _map.Pacman.Col + cdiff;
        //    int distance = 0;
        //    while (row >= 0 && row < _map.Height && col >= 0 && col < _map.Width)
        //    {
        //        if (_map[row, col] == MapTileType.WALL)
        //        {
        //            if (distance > 0)
        //            {
        //                rays.Add(new GameSight
        //                {
        //                    Position = new MapPoint(_map.Pacman.Row + rdiff, _map.Pacman.Col + cdiff),
        //                    Distance = distance,
        //                    Obstacle = MapTileType.WALL
        //                });
        //            }
        //            return;
        //        }
        //        if (_map.Ghost.Any(g => g.Row == row && g.Col == col))
        //        {
        //            rays.Add(new GameSight
        //            {
        //                Position = new MapPoint(_map.Pacman.Row + rdiff, _map.Pacman.Col + cdiff),
        //                Distance = distance,
        //                Obstacle = MapTileType.GHOST1
        //            });
        //            return;
        //        }
        //        if (_map[row, col] == MapTileType.POINT)
        //        {
        //            rays.Add(new GameSight
        //            {
        //                Position = new MapPoint(_map.Pacman.Row + rdiff, _map.Pacman.Col + cdiff),
        //                Distance = distance,
        //                Obstacle = MapTileType.POINT
        //            });
        //            return;
        //        }
        //        distance++;
        //        row += rdiff;
        //        col += cdiff;
        //    }
        //    if (distance > 0)
        //    {
        //        rays.Add(new GameSight
        //        {
        //            Position = new MapPoint(_map.Pacman.Row + rdiff, _map.Pacman.Col + cdiff),
        //            Distance = distance,
        //            Obstacle = MapTileType.WALL
        //        });
        //    }
        //}

        private void randomPlayer(int p)
        {
            PlayerState player = _game.Players[p];
            for (int i = 0; i < player.Agents.Count; i++)
            {
                MapPoint from = player.Agents[i];
                // Eat cookie if possible
                if (_map[from] == MapTileType.POINT)
                {
                    _map[from] = MapTileType.EMPTY;
                    player.Score++;
                    _game.CookiesLeft--;
                    continue;
                }
                // Move randomly
                List<MapPoint> points = new List<MapPoint>();
                checkFreePoint(points, from, -1, 0);
                checkFreePoint(points, from, 1, 0);
                checkFreePoint(points, from, 0, -1);
                checkFreePoint(points, from, 0, 1);
                if (points.Count == 1)
                {
                    player.Agents[i] = points[0];
                }
                else if (points.Count > 0)
                {
                    player.Agents[i] = points[_rnd.Next(points.Count)];
                }
                else
                {
                    // stay
                }
            }
        }

        private void randomTargetPlayer(int p)
        {
            PlayerState player = _game.Players[p];
            for (int i = 0; i < player.Agents.Count; i++)
            {
                MapPoint from = player.Agents[i];
                // Eat cookie if possible
                if (_map[from] == MapTileType.POINT)
                {
                    _map[from] = MapTileType.EMPTY;
                    player.Score++;
                    _game.CookiesLeft--;
                    continue;
                }
                // Find target
                if (!player.Target[i].HasValue || _map[player.Target[i].Value] != MapTileType.POINT)
                {
                    List<MapPoint> options = new List<MapPoint>();
                    for (int row = 0; row < _map.Height; row++)
                    {
                        for (int col = 0; col < _map.Width; col++)
                        {
                            if (_map[row, col] != MapTileType.POINT)
                                continue;
                            MapPoint target = new MapPoint(row, col);
                            int distance = shortestPath(from, target);
                            if (distance < 1000000)
                                options.Add(target);
                        }
                    }
                    if (options.Count == 1)
                    {
                        player.Target[i] = options[0];
                    }
                    else if (options.Count > 0)
                    {
                        player.Target[i] = options[_rnd.Next(options.Count)];
                    }
                    else
                    {
                        player.Target[i] = null;
                    }
                }
                // Prepare to move
                List<MapPoint> points = new List<MapPoint>();
                checkFreePoint(points, from, -1, 0);
                checkFreePoint(points, from, 1, 0);
                checkFreePoint(points, from, 0, -1);
                checkFreePoint(points, from, 0, 1);
                // Limited options resolve immediately
                if (points.Count == 1)
                {
                    player.Agents[i] = points[0];
                    continue;
                }
                else if (points.Count == 0)
                {
                    // stay
                    continue;
                }
                // Move towards target
                if (player.Target[i].HasValue)
                {
                    MapPoint target = player.Target[i].Value;
                    Tuple<int, int>[] values = new Tuple<int, int>[points.Count];
                    for (int j = 0; j < values.Length; j++)
                    {
                        values[j] = Tuple.Create(shortestPath(points[j], target), j);
                    }
                    Array.Sort(values, (a, b) => a.Item1 - b.Item1);
                    int good = values[0].Item1;
                    values = values.Where(t => t.Item1 == good).ToArray();
                    player.Agents[i] = points[values[_rnd.Next(values.Length)].Item2];
                    continue;
                }
                // Without target, move randomly
                player.Agents[i] = points[_rnd.Next(points.Count)];
            }
        }

        private int shortestPath(MapPoint source, MapPoint target)
        {
            if (source.Equals(target))
                return 0;
            int[,] crumbs = new int[_map.Height, _map.Width];
            for (int row = 0; row < _map.Height; row++)
            {
                for (int col = 0; col < _map.Width; col++)
                {
                    crumbs[row, col] = 1000000;
                }
            }
            Queue<MapPoint> front = new Queue<MapPoint>();
            crumbs[source.Row, source.Col] = 0;
            front.Enqueue(source);
            while (front.Count > 0)
            {
                MapPoint x = front.Dequeue();
                for (int dr = -1; dr <= 1; dr++)
                {
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        if (dr * dc != 0 || dr + dc == 0)
                            continue;
                        MapPoint y = new MapPoint(x.Row + dr, x.Col + dc);
                        if (y.Row < 0 || y.Row >= _map.Height)
                            continue;
                        if (y.Col < 0 || y.Col >= _map.Width)
                            continue;
                        if (_map[y] == MapTileType.WALL)
                            continue;
                        int v = crumbs[x.Row, x.Col] + 1;
                        if (crumbs[y.Row, y.Col] <= v)
                            continue;
                        crumbs[y.Row, y.Col] = v;
                        if (y.Equals(target))
                            return v;
                        front.Enqueue(y);
                    }
                }
            }
            return 2000000;
        }

        //private void shortGhosts()
        //{
        //    for (int i = 0; i < 4; i++)
        //    {
        //        List<MapPoint> points = new List<MapPoint>();
        //        checkGhostPoint(points, i, -1, 0);
        //        checkGhostPoint(points, i, 1, 0);
        //        checkGhostPoint(points, i, 0, -1);
        //        checkGhostPoint(points, i, 0, 1);
        //        if (points.Count == 1)
        //        {
        //            _game.Ghost[i] = points[0];
        //        }
        //        else if (points.Count > 0)
        //        {
        //            Tuple<int,int>[] values = new Tuple<int,int>[points.Count];
        //            for (int j = 0; j < values.Length; j++) {
        //                values[j] = Tuple.Create(shortestPathToPacman(points[j]), j);
        //            }
        //            Array.Sort(values, (a, b) => a.Item1 - b.Item1);
        //            int good = values[0].Item1;
        //            values = values.Where(t => t.Item1 == good).ToArray();
        //            _game.Ghost[i] = points[values[_rnd.Next(values.Length)].Item2];
        //        }
        //        else
        //        {
        //            _game.Ghost[i] = _game.Trail[i];
        //        }
        //    }
        //}

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            _game.Turn++;

            if (p1RandomRadio.Checked)
                randomPlayer(0);
            else if (p1TargetRadio.Checked)
                randomTargetPlayer(0);

            if (p2RandomRadio.Checked)
                randomPlayer(1);
            else if (p2TargetRadio.Checked)
                randomTargetPlayer(1);

            // Move ghosts
            //if (ghostRandomRadio.Checked)
            //    randomGhosts();
            //else if (ghostProxyRadio.Checked)
            //    proxyGhosts();
            //else if (ghostShortRadio.Checked)
            //    shortGhosts();
            //else
            //    randomGhosts();

            //_game.Trail = _map.Ghost;
            //_map.Ghost = _game.Ghost.ToArray();

            //// Move pacman
            //if (pacmanAutoChk.Checked)
            //{
            //    // Find nearest osbtacle in a line of sight in all directions
            //    List<GameSight> rays = new List<GameSight>();
            //    lookRay(rays, -1, 0);
            //    lookRay(rays, 1, 0);
            //    lookRay(rays, 0, -1);
            //    lookRay(rays, 0, 1);
            //    rays.Sort((a, b) =>
            //    {
            //        int diff = weighType(a) - weighType(b);
            //        if (diff != 0)
            //            return diff;
            //        if (a.Obstacle == MapTileType.GHOST1)
            //            return b.Distance - a.Distance;
            //        else
            //            return a.Distance - b.Distance;
            //    });
            //    if (rays.Count > 0)
            //    {
            //        if (rays[0].Obstacle == MapTileType.WALL && rays.Count(t => t.Obstacle == MapTileType.WALL) >= 2)
            //        {
            //            rays.RemoveAll(t => t.Obstacle != MapTileType.WALL || t.Position.Equals(_game.CameFrom));
            //            _game.CameFrom = _map.Pacman;
            //            if (rays.Count == 1)
            //                _map.Pacman = rays[0].Position;
            //            else
            //                _map.Pacman = rays[_rnd.Next(rays.Count)].Position;
            //        }
            //        else
            //        {
            //            _game.CameFrom = _map.Pacman;
            //            _map.Pacman = rays[0].Position;
            //        }
            //    }
            //}
            //else
            //{
            //    _game.CameFrom = _map.Pacman;
            //    _map.Pacman = _game.Pacman;
            //}


            // Game finish conditions

            bool gameLost = false;

            // Simplified rules: game ends when all cookies are eaten
            if (_game.CookiesLeft == 0)
            {
                Text = string.Format("END. {0} : {1}", _game.Players[0].Score, _game.Players[1].Score);
                gameTimer.Enabled = false;
                gameLost = true;
            }

            // Game stats and visuals
            turnLbl.Text = _game.Turn.ToString();
            scoreLbl.Text = string.Format("{0} : {1}", _game.Players[0].Score, _game.Players[1].Score);

            _rendering.Render(_game);
        }

        private void checkFreePoint(List<MapPoint> points, MapPoint from, int rdiff, int cdiff)
        {
            MapPoint p = new MapPoint(from.Row + rdiff, from.Col + cdiff);
            if (p.Row < 0 || p.Row >= _map.Height)
                return;
            if (p.Col < 0 || p.Col >= _map.Width)
                return;
            if (_map[p] == MapTileType.WALL)
                return;
            points.Add(p);
        }

        private void playBtn_Click(object sender, EventArgs e)
        {
            ResetGame();
            _rendering.Render(_game);
            gameTimer.Enabled = true;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (_game != null)
                _rendering.Render(_game);
        }
    }
}
