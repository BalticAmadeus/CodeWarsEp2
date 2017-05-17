using Game.AdminClient.Converters;
using Game.AdminClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Game.AdminClient.ViewModels
{
    public class ActorSprite
    {
        protected Point _position;
        protected GeometryGroup _geometry;
        protected Drawing _drawing;
        protected TranslateTransform _offset;

        public Drawing SpriteDrawing { get { return _drawing; } }

        protected ActorSprite(Point position)
        {
            _position = position;
            _offset = new TranslateTransform(_position.X, _position.Y);
            _geometry = new GeometryGroup();
            _geometry.Transform = _offset;
        }

        public void Move(Point position)
        {
            // Movement direction
            do
            {
                int angle;
                if (position.X > _position.X)
                    angle = 0; // right
                else if (position.X < _position.X)
                    angle = 180; // left
                else if (position.Y > _position.Y)
                    angle = 90; // down
                else if (position.Y < _position.Y)
                    angle = 270; // up
                else
                    break; // stay
                SetAngle(angle);
            } while (false);
            // Actual movement
            var duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.5));
            var xanim = new DoubleAnimation(_position.X, position.X, duration);
            var yanim = new DoubleAnimation(_position.Y, position.Y, duration);
            _position = position;
            _offset.BeginAnimation(TranslateTransform.XProperty, xanim);
            _offset.BeginAnimation(TranslateTransform.YProperty, yanim);
        }

        protected virtual void SetAngle(int angle)
        {
            // custom action when overriden
        }
    }

    public class RatSprite : ActorSprite
    {
        private RotateTransform _drawingRotate;
        private Brush _baseBrush;
        private Brush _contrastBrush;
        private Pen _contrastPen;
        private GeometryGroup _monikerLayer;

        public RatSprite(Point position, int playerIndex) : base(position)
        {
            // Base colors
            _baseBrush = new SolidColorBrush(ColorStateConverter.GetPlayerColor(playerIndex));
            _contrastBrush = new SolidColorBrush(Colors.Black);
            _contrastPen = new Pen(_contrastBrush, 0.03);
            // Body
            var trunk = new EllipseGeometry(new System.Windows.Point(0.5, 0.5), 0.4, 0.2);
            var tail = new PathGeometry();
            var fig = new PathFigure();
            fig.StartPoint = new System.Windows.Point(0.0, 0.5);
            fig.Segments.Add(new LineSegment(new System.Windows.Point(0.2, 0.4), true));
            fig.Segments.Add(new LineSegment(new System.Windows.Point(0.2, 0.6), true));
            fig.IsClosed = true;
            tail.Figures.Add(fig);
            var body = new GeometryGroup();
            body.Children.Add(trunk);
            body.Children.Add(tail);
            body.FillRule = FillRule.Nonzero;
            var bodyDrawing = new GeometryDrawing(_baseBrush, null, body);

            // Eyes
            var eye1 = new EllipseGeometry(new System.Windows.Point(0.75, 0.42), 0.03, 0.05);
            var eye2 = new EllipseGeometry(new System.Windows.Point(0.75, 0.58), 0.03, 0.05);
            var eyes = new GeometryGroup();
            eyes.Children.Add(eye1);
            eyes.Children.Add(eye2);
            var eyesDrawing = new GeometryDrawing(_contrastBrush, null, eyes);

            // Ears
            var ears = new PathGeometry();
            fig = new PathFigure();
            fig.StartPoint = new System.Windows.Point(0.65, 0.35);
            fig.Segments.Add(new LineSegment(new System.Windows.Point(0.55, 0.40), true));
            fig.Segments.Add(new LineSegment(new System.Windows.Point(0.65, 0.48), true));
            ears.Figures.Add(fig);
            fig = new PathFigure();
            fig.StartPoint = new System.Windows.Point(0.65, 1 - 0.35);
            fig.Segments.Add(new LineSegment(new System.Windows.Point(0.55, 1 - 0.40), true));
            fig.Segments.Add(new LineSegment(new System.Windows.Point(0.65, 1 - 0.48), true));
            ears.Figures.Add(fig);
            var earsDrawing = new GeometryDrawing(null, _contrastPen, ears);

            // Monikers
            _monikerLayer = new GeometryGroup();
            var monikerDrawing = new GeometryDrawing(_baseBrush, null, _monikerLayer);

            // Combine
            var drawing = new DrawingGroup();
            drawing.Children.Add(bodyDrawing);
            drawing.Children.Add(eyesDrawing);
            drawing.Children.Add(earsDrawing);
            _drawingRotate = new RotateTransform(0, 0.5, 0.5);
            drawing.Transform = _drawingRotate;
            var baseDrawing = new DrawingGroup();
            baseDrawing.Children.Add(drawing);
            baseDrawing.Children.Add(monikerDrawing);
            baseDrawing.Transform = _offset;
            
            _drawing = baseDrawing;
        }

        protected override void SetAngle(int angle)
        {
            _drawingRotate.Angle = angle;
        }

        public void Die()
        {
            var duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.5));
            var danim = new DoubleAnimation(0, duration);
            _baseBrush.BeginAnimation(Brush.OpacityProperty, danim);
            _contrastBrush.BeginAnimation(Brush.OpacityProperty, danim);
        }

        public void SetMoniker(int where)
        {
            _monikerLayer.Children.Clear();
            var box = new System.Windows.Rect(0, 0, 0.08, 0.08);
            switch (where)
            {
                case 1:
                case 2:
                    box.X = 0.98 - box.Width;
                    box.Y = (box.Height + 0.02) * (where - 1) + 0.02;
                    break;
                case 3:
                case 4:
                    box.X = 0.98 - box.Width;
                    box.Y = 1.00 - (box.Height + 0.02) * (5 - where);
                    break;
                case 5:
                case 6:
                    box.X = 0.02;
                    box.Y = (box.Height + 0.02) * (where - 5) + 0.02;
                    break;
                case 7:
                case 8:
                    box.X = 0.02;
                    box.Y = 1.00 - (box.Height + 0.02) * (9 - where);
                    break;
                default:
                    return;
            }
            _monikerLayer.Children.Add(new EllipseGeometry(box));
        }
    }

    public class CookieSprite
    {
        private Geometry _geometry;
        private Drawing _drawing;

        public Drawing SpriteDrawing { get { return _drawing; } }

        public CookieSprite(int x, int y)
        {
            _geometry = new EllipseGeometry(
                new System.Windows.Point(x + 0.5, y + 0.5),
                1.0 / 6, 1.0 / 9);
            _drawing = new GeometryDrawing(Brushes.White, null, _geometry);
        }

        public void Eat()
        {
            var duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.5));
            var ranim = new DoubleAnimation(0, duration);
            _geometry.BeginAnimation(EllipseGeometry.RadiusXProperty, ranim);
            _geometry.BeginAnimation(EllipseGeometry.RadiusYProperty, ranim);
        }
    }
}
