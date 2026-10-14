/* Copyright (C) 2008 Gilleain Torrance <gilleain.torrance@gmail.com>
 *
 *  Contact: cdk-devel@list.sourceforge.net
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public License
 *  as published by the Free Software Foundation; either version 2.1
 *  of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA.
 */
using Microsoft.Graphics.Canvas;
using NCDK.Numerics;
using NCDK.Renderers.Elements;
using NCDK.Renderers.Elements.Path;
using NCDK.Renderers.Fonts;
using NCDK.Renderers.Generators;
using System;
using System.Collections.Generic;
using System.Globalization;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using static NCDK.Renderers.Generators.BasicBondGenerator;
using static NCDK.Renderers.Generators.BasicSceneGenerator;
using static NCDK.Renderers.Generators.Standards.VecmathUtil;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.Geometry;

namespace NCDK.Renderers.Visitors
{
    /// <summary>
    /// Implementation of the <see cref="IDrawVisitor"/> interface for the WPF,
    /// allowing molecules to be rendered with toolkits based on 
    /// </summary>
    // @cdk.module renderawt
    // @cdk.githash
    public class WPFDrawVisitor : AbstractWPFDrawVisitor
    {
        /// <summary>
        /// The font manager cannot be set by the constructor as it needs to
        /// be managed by the Renderer.
        /// </summary>
        private WPFFontManager fontManager;

        CanvasTextFormat textFormat;

        /// <summary>
        /// The renderer model cannot be set by the constructor as it needs to
        /// be managed by the Renderer.
        /// </summary>
        private RendererModel rendererModel;

        /// <summary>
        /// The current stroke map.
        /// </summary>
        public IDictionary<int, Pen> StrokeMap { get; } = new Dictionary<int, Pen>();

        /// <summary>
        /// Returns the current <see cref="RendererModel"/>.
        /// </summary>
        /// <returns>the current model</returns>
        public RendererModel GetRendererModel()
        {
            return rendererModel;
        }

        private readonly double minStroke;
        private readonly bool strokeCache;

        /// <summary>
        /// Returns the <see cref="CanvasDrawingSession"/> for for this visitor.
        /// </summary>
        /// <returns>the <see cref="CanvasDrawingSession"/> object</returns>
        public CanvasDrawingSession GetGraphics()
        {
            return graphics;
        }

        /// <summary>
        /// Constructs a new <see cref="IDrawVisitor"/> using the AWT widget toolkit,
        /// taking a <see cref="CanvasDrawingSession"/> object to which the chemical content
        /// is drawn.
        /// </summary>
        /// <param name="graphics"><see cref="CanvasDrawingSession"/> to which will be drawn</param>
        public WPFDrawVisitor(CanvasDrawingSession graphics)
            : this(graphics, true, 1.5)
        { }

        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="graphics">the graphics instance</param>
        /// <param name="strokeCache">cache strokes internally, only sizes at 0.25 increments are stored</param>
        /// <param name="minStroke">the minimum stroke, strokes smaller than this are automatically resized</param>
        private WPFDrawVisitor(CanvasDrawingSession graphics, bool strokeCache, double minStroke)
            : base(graphics)
        {
            this.fontManager = null;
            this.textFormat = null;
            this.rendererModel = null;
            this.strokeCache = strokeCache;
            this.minStroke = minStroke;
        }

        /// <summary>
        /// Create a draw visitor that will be rendering to a vector graphics output. This disables
        /// the minimum stroke size and stroke caching when drawing lines.
        /// </summary>
        /// <param name="g2">graphics environment</param>
        /// <returns>draw visitor</returns>
        public static WPFDrawVisitor ForVectorGraphics(CanvasDrawingSession g2)
        {
            return new WPFDrawVisitor(g2, false, double.NegativeInfinity);
        }

        private void Visit(ElementGroup elementGroup)
        {
            elementGroup.Visit(this);
        }

        private void Visit(LineElement line)
        {
            // scale the stroke by zoom + scale (both included in the AffineTransform)
            var width = line.width * transform.M11;  // GetScaleX()
            if (width < minStroke) width = minStroke;

            int key = (int)(width * 4); // store 2.25, 2.5, 2.75 etc to separate keys

            Pen pen;
            if (strokeCache && StrokeMap.TryGetValue(key, out pen))
            {
                // do nothing
            }
            else
            {
                Pen stroke = new Pen(line.color, width);
                pen = stroke;
                StrokeMap[key] = stroke;
            }

            var linePoints = new Vector2[] { line.firstPoint, line.secondPoint };

            linePoints[0] = Vector2.Transform(linePoints[0], transform);
            linePoints[1] = Vector2.Transform(linePoints[1], transform);
            graphics.DrawLine(linePoints[0].ToSystem(), linePoints[1].ToSystem(), pen.Color, (float)pen.Width, pen.Style);
        }

        private void Visit(OvalElement oval)
        {
            var radius = ScaleX(oval.radius);
            var diameter = ScaleX(oval.radius * 2);
            var center = Transform(oval.coord);

            if (oval.fill)
            {
                this.graphics.DrawEllipse((float)(center.X - radius), (float)(center.Y - radius), (float)diameter, (float)diameter, oval.color);
            }
            else
            {
                this.graphics.DrawEllipse(
                    (float)(center.X - radius), (float)(center.Y - radius), (float)diameter, (float)diameter,
                    oval.color, 1);
            }
        }

        private double ScaleX(double xCoord)
        {
            return xCoord * transform.M11;  // Scale X
        }

        private double ScaleY(double yCoord)
        {
            return yCoord * transform.M22; // Scale Y
        }

        private Vector2 Transform(Vector2 coord)
        {
            var result = Vector2.Transform(coord, transform);
            return result;
        }

        private Color GetBackgroundColor()
        {
            return rendererModel == null ?
                new BasicSceneGenerator.BackgroundColor().Default.Value :
                rendererModel.GetV<Color>(typeof(BasicSceneGenerator.BackgroundColor));
        }

        private void Visit(TextElement textElement)
        {
            var point = this.GetTextBasePoint(textElement.text, textElement.coord, this.fontManager.Typeface, this.fontManager.Size);
            var textBounds = this.GetTextBounds(textElement.text, textElement.coord, this.fontManager.Typeface, this.fontManager.Size);
            var backColor = GetBackgroundColor();
            this.graphics.DrawRectangle(textBounds, backColor);
            this.graphics.DrawText(
                          textElement.text,
                          point.ToSystem(),
                          textElement.color,
                          textFormat);
        }

        private void Visit(WedgeLineElement wedge)
        {
            // make the vector normal to the wedge axis
            Vector2 normal = new Vector2(wedge.firstPoint.Y - wedge.secondPoint.Y, wedge.secondPoint.X - wedge.firstPoint.X);
            normal = Vector2.Normalize(normal);
            normal *= rendererModel.GetV<double>(typeof(WedgeWidth)) / rendererModel.GetV<double>(typeof(Scale));

            // make the triangle corners
            Vector2 vertexA = new Vector2(wedge.firstPoint.X, wedge.firstPoint.Y);
            Vector2 vertexB = new Vector2(wedge.secondPoint.X, wedge.secondPoint.Y) + normal;
            Vector2 vertexC = vertexB - normal;

            var brush = new SolidColorBrush(wedge.color);

            if (wedge.type == WedgeLineElement.TYPE.Dashed)
            {
                #region this.DrawDashedWedge(vertexA, vertexB, vertexC);

                // calculate the distances between lines
                double distance = Vector2.Distance(vertexB, vertexA);
                double gapFactor = 0.1f;
                double gap = distance * gapFactor;
                double numberOfDashes = distance / gap;
                double displacement = 0;

                // draw by interpolating along the edges of the triangle
                for (int i = 0; i < numberOfDashes; i++)
                {
                    Vector2 point1 = Vector2.Lerp(vertexA, vertexB, displacement);
                    Vector2 point2 = Vector2.Lerp(vertexA, vertexC, displacement);
                    var p1T = this.TransformPoint(point1);
                    var p2T = this.TransformPoint(point2);
                    this.graphics.DrawLine(p1T.ToSystem(), p2T.ToSystem(), wedge.color, 1);
                    if (distance * (displacement + gapFactor) >= distance)
                    {
                        break;
                    }
                    else
                    {
                        displacement += gapFactor;
                    }
                }
                #endregion
            }
            else if (wedge.type == WedgeLineElement.TYPE.Wedged)
            {
                #region this.DrawFilledWedge(brush, vertexA, vertexB, vertexC);
                var pointB = this.TransformPoint(vertexB);
                var pointC = this.TransformPoint(vertexC);
                var pointA = this.TransformPoint(vertexA);

                var figure = new CanvasPathBuilder(graphics.Device);
                figure.BeginFigure(pointB.ToSystem());
                figure.AddLine(pointC.ToSystem());
                figure.AddLine(pointA.ToSystem());
                figure.EndFigure(CanvasFigureLoop.Open);

                var g = CanvasGeometry.CreatePath(figure);

                this.graphics.FillGeometry(g, wedge.color);
                #endregion
            }
            else if (wedge.type == WedgeLineElement.TYPE.Indiff)
            {
                #region  this.DrawIndiffWedge(vertexA, vertexB, vertexC);
                var pen = new Pen(wedge.color, 1);
                pen.Style.StartCap = CanvasCapStyle.Round;
                pen.Style.LineJoin = CanvasLineJoin.Round;
                pen.Style.EndCap = CanvasCapStyle.Round;

                // calculate the distances between lines
                double distance = Vector2.Distance(vertexB, vertexA);
                double gapFactor = 0.05f;
                double gap = distance * gapFactor;
                double numberOfDashes = distance / gap;
                double displacement = 0;

                // draw by interpolating along the edges of the triangle
                Vector2 point1 = Vector2.Lerp(vertexA, vertexB, displacement);
                bool flip = false;
                var p1T = this.TransformPoint(point1);
                displacement += gapFactor;
                for (int i = 0; i < numberOfDashes; i++)
                {
                    Vector2 point2;
                    if (flip)
                    {
                        point2 = Vector2.Lerp(vertexA, vertexC, displacement);
                    }
                    else
                    {
                        point2 = Vector2.Lerp(vertexA, vertexB, displacement);
                    }
                    flip = !flip;
                    var p2T = this.TransformPoint(point2);
                    this.graphics.DrawLine(p1T.ToSystem(), p2T.ToSystem(), pen.Color, (float)pen.Width, pen.Style);
                    if (distance * (displacement + gapFactor) >= distance)
                    {
                        break;
                    }
                    else
                    {
                        p1T = p2T;
                        displacement += gapFactor;
                    }
                }
                #endregion
            }
        }

        private void Visit(AtomSymbolElement atomSymbol)
        {
            var xy = this.TransformPoint(atomSymbol.coord);

            var bounds = GetTextBounds(atomSymbol.text, this.fontManager.CureentTypeface, this.fontManager.Size);

            double w = bounds.Width;
            double h = bounds.Height;

            double xOffset = bounds.X;
            double yOffset = bounds.Y + bounds.Height;

            bounds = new Rect(xy.X - (w / 2), xy.Y - (h / 2), w, h);

            var padding = (float)h / 4;
            this.graphics.FillRoundedRectangle(
                new Rect(
                    bounds.X - (padding / 2), bounds.Y - (padding / 2),
                    bounds.Width + padding, bounds.Height + padding), padding, padding,
                GetBackgroundColor());
            this.graphics.DrawText(
                atomSymbol.text,
                (float)(bounds.X - xOffset), (float)(bounds.Y + h - yOffset),
                atomSymbol.color,
                textFormat);

            int offset = 10; // XXX
            string chargeString;
            if (atomSymbol.formalCharge == 0)
            {
                return;
            }
            else if (atomSymbol.formalCharge == 1)
            {
                chargeString = "+";
            }
            else if (atomSymbol.formalCharge > 1)
            {
                chargeString = atomSymbol.formalCharge + "+";
            }
            else if (atomSymbol.formalCharge == -1)
            {
                chargeString = "-";
            }
            else if (atomSymbol.formalCharge < -1)
            {
                int absCharge = Math.Abs(atomSymbol.formalCharge);
                chargeString = absCharge + "-";
            }
            else
            {
                return;
            }

            var xCoord = bounds.GetCenterX();
            var yCoord = bounds.GetCenterY();
            if (atomSymbol.alignment == 1)
            { // RIGHT
                this.graphics.DrawText(
                    chargeString,
                    (float)xCoord + offset, (float)bounds.Top,
                    atomSymbol.color,
                    textFormat);
            }
            else if (atomSymbol.alignment == -1)
            { // LEFT
                this.graphics.DrawText(
                    chargeString,
                    (float)xCoord - offset, (float)bounds.Top,
                    atomSymbol.color,
                    textFormat);
            }
            else if (atomSymbol.alignment == 2)
            { // TOP
                this.graphics.DrawText(
                    chargeString,
                    (float)xCoord, (float)yCoord - offset,
                    atomSymbol.color,
                    textFormat);
            }
            else if (atomSymbol.alignment == -2)
            { // BOT
                this.graphics.DrawText(
                    chargeString,
                    (float)xCoord, (float)yCoord + offset,
                    atomSymbol.color,
                    textFormat);
            }
        }

        private void Visit(RectangleElement rectangle)
        {
            //this.graphics.SetColor(rectangle.color);
            var width = ScaleX(rectangle.width);
            var height = ScaleY(rectangle.height);
            var p = Transform(rectangle.coord);
            var rect = new Rect(p.X, p.Y, width, height);
            if (rectangle.filled)
            {
                this.graphics.FillRectangle(rect, rectangle.color);
            }
            else
            {
                this.graphics.DrawRectangle(rect, rectangle.color, 1);
            }
        }

        private void Visit(Elements.PathElement path)
        {
            for (int i = 1; i < path.points.Count; i++)
            {
                var point1 = path.points[i - 1];
                var point2 = path.points[i];
                var lineStart = this.TransformPoint(point1).ToSystem();
                var lineEnd = this.TransformPoint(point2).ToSystem();
                this.graphics.DrawLine(lineStart, lineEnd, path.color, 1);
            }
        }

        private void Visit(GeneralPath path)
        {
            var g = new CanvasPathBuilder(graphics.Device);
            g.SetFilledRegionDetermination(path.winding == FillRule.EvenOdd ? CanvasFilledRegionDetermination.Alternate : CanvasFilledRegionDetermination.Winding);

            System.Numerics.Vector2 pos;
            bool started = false;
            foreach (var element in path.elements)
            {
                var pp = element.Points;
                switch (element.Type)
                {
                    case PathType.MoveTo:
                        if (started)
                            g.EndFigure(CanvasFigureLoop.Open);
                        pos = Transform(pp[0]).ToSystem();
                        g.BeginFigure(pos);
                        started = true;
                        break;
                    case PathType.LineTo:
                        pos = Transform(pp[0]).ToSystem();
                        g.AddLine(pos);
                        break;
                    case PathType.QuadTo:
                        {
                            var p1 = Transform(pp[0]).ToSystem();
                            var p2 = Transform(pp[1]).ToSystem();
                            g.AddQuadraticBezier(p1, p2);
                            pos = p1;
                        }
                        break;
                    case PathType.CubicTo:
                        {
                            var p1 = Transform(pp[0]).ToSystem();
                            var p2 = Transform(pp[1]).ToSystem();
                            var p3 = Transform(pp[2]).ToSystem();
                            g.AddCubicBezier(p1, p2, p3);
                            pos = p1;
                        }
                        break;
                    case PathType.Close:
                    default:
                        g.EndFigure(CanvasFigureLoop.Closed);
                        started = false;
                        break;
                }
            }

            var geometry = CanvasGeometry.CreatePath(g);

            if (path.fill)
                this.graphics.FillGeometry(
                    geometry, path.color);
            else
                this.graphics.DrawGeometry(
                    geometry, path.color, (float)path.stroke);
        }


        private void Visit(ArrowElement line)
        {
            double scale = rendererModel.GetV<double>(typeof(Scale));

            Pen pen = null;
            {
                int w = (int)(line.width * scale);
                if (StrokeMap.ContainsKey(w))
                {
                    pen = StrokeMap[w];
                }
                else
                {
                    pen = new Pen(line.color, w);
                    StrokeMap[w] = pen;
                }
            }

            var a = this.TransformPoint(line.start);
            var b = this.TransformPoint(line.end);
            graphics.DrawLine(a.ToSystem(), b.ToSystem(), pen.Color, (float)pen.Width, pen.Style);
            double aW = rendererModel.GetV<double>(typeof(ArrowHeadWidth)) / scale;
            if (line.direction)
            {
                var c = this.TransformPoint(new Vector2(line.start.X - aW, line.start.Y - aW));
                var d = this.TransformPoint(new Vector2(line.start.X - aW, line.start.Y + aW));
                graphics.DrawLine(a.ToSystem(), c.ToSystem(), pen.Color, (float)pen.Width, pen.Style);
                graphics.DrawLine(a.ToSystem(), d.ToSystem(), pen.Color, (float)pen.Width, pen.Style);
            }
            else
            {
                var c = this.TransformPoint(new Vector2(line.end.X + aW, line.end.Y - aW));
                var d = this.TransformPoint(new Vector2(line.end.X + aW, line.end.Y + aW));
                graphics.DrawLine(b.ToSystem(), c.ToSystem(), pen.Color, (float)pen.Width, pen.Style);
                graphics.DrawLine(b.ToSystem(), d.ToSystem(), pen.Color, (float)pen.Width, pen.Style);
            }
        }

        private void Visit(TextGroupElement textGroup)
        {
            var point = GetTextBasePoint(textGroup.text, textGroup.coord, fontManager.CureentTypeface, fontManager.Size);
            var textBounds = GetTextBounds(textGroup.text, textGroup.coord, fontManager.CureentTypeface, fontManager.Size);
            this.graphics.FillRectangle(textBounds, GetBackgroundColor());
            this.graphics.DrawText(
                textGroup.text,
                (float)point.X, (float)point.Y,
                textGroup.color,
                textFormat
                );

            var coord = new Vector2(textBounds.GetCenterX(), textBounds.GetCenterY());
            var coord1 = new Vector2(textBounds.Left, textBounds.Top);
            var coord2 = new Vector2(point.X + textBounds.Width, textBounds.Bottom);

            var vo = coord2 - coord1;
            var o = new Size(vo.X, vo.Y);
            foreach (var child in textGroup.children)
            {
                Vector2 p;

                switch (child.position.Ordinal)
                {
                    case TextGroupElement.Position.O.NE:
                        p = new Vector2(coord2.X, coord1.Y);
                        break;
                    case TextGroupElement.Position.O.N:
                        p = new Vector2(coord1.X, coord1.Y);
                        break;
                    case TextGroupElement.Position.O.NW:
                        p = new Vector2(coord1.X - o.Width, coord1.Y);
                        break;
                    case TextGroupElement.Position.O.W:
                        p = new Vector2(coord1.X - o.Width, coord.Y);
                        break;
                    case TextGroupElement.Position.O.SW:
                        p = new Vector2(coord1.X - o.Width, coord1.Y + o.Height);
                        break;
                    case TextGroupElement.Position.O.S:
                        p = new Vector2(coord1.X, coord2.Y + o.Height);
                        break;
                    case TextGroupElement.Position.O.SE:
                        p = new Vector2(coord2.X, coord2.Y + o.Height);
                        break;
                    case TextGroupElement.Position.O.E:
                        p = new Vector2(coord2.X, coord.Y);
                        break;
                    default:
                        p = new Vector2(coord.X, coord.Y);
                        break;
                }

                this.graphics.DrawText(
                    child.text,
                    p.ToSystem(),
                    textGroup.color,
                    textFormat);

                if (child.subscript != null)
                {
                    var childBounds = GetTextBounds(child.text, p, fontManager.CureentTypeface, fontManager.Size);
                    var scx = p.X + (childBounds.Width * 0.75);
                    var scy = p.Y + (childBounds.Height / 3);
                    textFormat.FontSize = (float)fontManager.Size - 2;
                    this.graphics.DrawText(
                        child.subscript,
                        p.ToSystem(),
                        textGroup.color,
                        textFormat);
                    textFormat.FontSize = (float)fontManager.Size;
                }
            }
        }

        /// <inheritdoc/>
        public override void Visit(IRenderingElement element)
        {
            if (element is ElementGroup)
                Visit((ElementGroup)element);
            else if (element is WedgeLineElement)
                Visit((WedgeLineElement)element);
            else if (element is LineElement)
                Visit((LineElement)element);
            else if (element is OvalElement)
                Visit((OvalElement)element);
            else if (element is TextGroupElement)
                Visit((TextGroupElement)element);
            else if (element is AtomSymbolElement)
                Visit((AtomSymbolElement)element);
            else if (element is TextElement)
                Visit((TextElement)element);
            else if (element is RectangleElement)
                Visit((RectangleElement)element);
            else if (element is Renderers.Elements.PathElement)
                Visit((Renderers.Elements.PathElement)element);
            else if (element is GeneralPath)
                Visit((GeneralPath)element);
            else if (element is ArrowElement)
                Visit((ArrowElement)element);
            else if (element is Bounds)
            {
                Visit(((Bounds)element).Root);
            }
            else if (element is MarkedElement)
            {
                Visit(((MarkedElement)element).Element());
            }
            else
                Console.Error.WriteLine("Visitor method for " + element.GetType().FullName + " is not implemented");
        }

        /// <summary>
        /// The font manager must be set by any renderer that uses this class!
        /// This manager is needed to keep track of fonts of the right size.
        /// </summary>
        /// <param name="fontManager">the <see cref="IFontManager"/> to be used</param>
        public override void SetFontManager(IFontManager fontManager)
        {
            this.fontManager = (WPFFontManager)fontManager;
            textFormat = new CanvasTextFormat
            {
                FontSize = (float)this.fontManager.Size,
                FontFamily = this.fontManager.CureentTypeface.FontFamily.Source,
                FontStretch = this.fontManager.CureentTypeface.Stretch,
                FontStyle = this.fontManager.CureentTypeface.Style,
                FontWeight = this.fontManager.CureentTypeface.Weight,
            };
        }

        /// <inheritdoc/>
        public override void SetRendererModel(RendererModel rendererModel)
        {
            this.rendererModel = rendererModel;
            if (rendererModel.HasParameter(typeof(UseAntiAliasing)))
            {
                if (rendererModel.GetV<bool>(typeof(UseAntiAliasing)))
                {
                    // just ignore it.
                    //graphics..SetRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON);
                    // g.SetStroke(new Media.Pen((int)rendererModel.GetBondWidth()));
                }
            }
        }
    }
}
