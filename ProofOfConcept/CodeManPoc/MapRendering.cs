﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeManPoc
{
    public class MapRendering
    {
        private class RenderContext : IDisposable
        {
            private MapRendering rendering;
            private Graphics graphics;

            public Bitmap Image { get { return rendering.image; } }
            public Graphics Graphics { get { return graphics; } }

            public RenderContext(MapRendering rendering)
            {
                this.rendering = rendering;
                Bitmap image = rendering.image;
                if (image == null || image.Size != rendering.target.ClientSize)
                {
                    image = new Bitmap(rendering.target.ClientSize.Width, rendering.target.ClientSize.Height);
                    rendering.image = image;
                }
                graphics = Graphics.FromImage(image);
            }

            public void Dispose()
            {
                graphics.Dispose();
                Bitmap view = rendering.image;
                rendering.target.Image = view;
                rendering.image = rendering.backup;
                rendering.backup = view;
            }
        }

        private PictureBox target;
        private Bitmap image;
        private Bitmap backup;

        public MapRendering(PictureBox target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            this.target = target;
        }

        public void Render(Game game)
        {
            GameMap map = game.Map;
            using (var ctx = new RenderContext(this))
            {
                Graphics g = ctx.Graphics;
                int picWidth = ctx.Image.Width;
                int picHeight = ctx.Image.Height;
                int cellSize = Math.Min(picWidth / map.Width, picHeight / map.Height);
                if (cellSize < 8)
                {
                    g.FillRectangle(Brushes.Gray, 0, 0, picWidth, picHeight);
                    return;
                }
                g.FillRectangle(Brushes.White, 0, 0, picWidth, picHeight);
                int imgWidth = map.Width * cellSize;
                int imgHeight = map.Height * cellSize;
                g.TranslateTransform((picWidth - imgWidth) / 2, (picHeight - imgHeight) / 2);
                Rectangle box = new Rectangle
                {
                    Width = cellSize,
                    Height = cellSize
                };
                int inset = 0;
                for (int row = 0; row < map.Height; row++)
                {
                    box.Y = row * cellSize;
                    for (int col = 0; col < map.Width; col++)
                    {
                        box.X = col * cellSize;
                        MapTileType tileType = map[row, col];
                        switch (tileType)
                        {
                            case MapTileType.EMPTY:
                                g.FillRectangle(Brushes.Black, box);
                                break;
                            case MapTileType.WALL:
                                g.FillRectangle(Brushes.Blue, box);
                                break;
                            //case MapTileType.POINT:
                            //    g.FillRectangle(Brushes.Black, box);
                            //    inset = cellSize / 4;
                            //    g.FillEllipse(Brushes.White, box.X + inset, box.Y + inset, cellSize - inset * 2, cellSize - inset * 2);
                            //    break;
                            default:
                                g.FillRectangle(Brushes.Black, box);
                                break;
                        }
                    }
                }
                inset = 1;
                foreach (MapPoint p in game.Players[0].Agents)
                {
                    g.FillEllipse(Brushes.Green, p.Col * cellSize + inset, p.Row * cellSize + inset, cellSize - inset * 2, cellSize - inset * 2);
                }
                foreach (MapPoint p in game.Players[1].Agents)
                {
                    g.FillEllipse(Brushes.Magenta, p.Col * cellSize + inset, p.Row * cellSize + inset, cellSize - inset * 2, cellSize - inset * 2);
                }
                HashSet<MapPoint> targets = new HashSet<MapPoint>();
                for (int i = 0; i < game.Players.Length; i++)
                {
                    targets.UnionWith(game.Players[i].Target.Where(t => t.HasValue).Select(t => t.Value));
                }
                for (int row = 0; row < map.Height; row++)
                {
                    box.Y = row * cellSize;
                    for (int col = 0; col < map.Width; col++)
                    {
                        box.X = col * cellSize;
                        if (map[row, col] == MapTileType.POINT)
                        {
                            inset = cellSize / 4;
                            Brush color;
                            if (targets.Contains(new MapPoint(row, col)))
                                color = Brushes.Yellow;
                            else
                                color = Brushes.White;
                            g.FillEllipse(color, box.X + inset, box.Y + inset, cellSize - inset * 2, cellSize - inset * 2);
                        }
                    }
                }
            }
        }
    }
}
