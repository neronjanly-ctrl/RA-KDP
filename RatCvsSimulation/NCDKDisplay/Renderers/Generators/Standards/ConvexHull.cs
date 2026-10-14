/*
 * Copyright (c) 2014 European Bioinformatics Institute (EMBL-EBI)
 *                    John May <jwmay@users.sf.net>
 *
 * Contact: cdk-devel@lists.sourceforge.net
 *
 * This program is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation; either version 2.1 of the License, or (at
 * your option) any later version. All we ask is that proper credit is given
 * for our work, which includes - but is not limited to - adding the above
 * copyright notice to the beginning of your source code files, and to any
 * copyright notice that you may distribute with programs based on this work.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public
 * License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA.
 */
using NCDK.Common.Collections;
using NCDK.Common.Mathematics;
using NCDK.Renderers.Elements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas.Geometry;
using NCDK.Numerics;
using Microsoft.Graphics.Canvas;

namespace NCDK.Renderers.Generators.Standards
{
    /// <summary>
    /// Immutable convex hull that is the smallest set of convex points that surround a shape.
    /// </summary>
    /// <example>
    /// <code>
    /// ConvexHull hull = ConvexHull.OfShape(shape);
    ///
    /// // the hull can be transformed
    /// hull = hull.Transform(new AffineTransform());
    ///
    /// // given a line, a point on the hull can be found that intersects the line
    /// Point2D point == hull.Intersect(new Line2D.Double(...), 0);
    /// }
    /// </code>
    /// </example>
    // @author John May
    internal sealed class ConvexHull
    {
        /// <summary>The convex hull.</summary>
        private readonly CanvasGeometry hull;

        /// <summary>
        /// Internal constructor, the hull is an argument.
        /// </summary>
        /// <param name="hull">the convex hull</param>
        private ConvexHull(CanvasGeometry hull)
        {
            this.hull = hull;
        }

        /// <summary>
        /// Calculate the convex hull of a shape.
        /// </summary>
        /// <param name="shape">a Java 2D shape</param>
        /// <returns>the convex hull</returns>
        public static ConvexHull OfShape(ICanvasResourceCreator resourceCreator, CanvasGeometry shape)
        {
            return OfShapes(resourceCreator, new ReadOnlyCollection<CanvasGeometry>(new[] { shape }));
        }

        /// <summary>
        /// Calculate the convex hull of multiple shapes.
        /// </summary>
        /// <param name="shapes">Java 2D shapes</param>
        /// <returns>the convex hull</returns>
        public static ConvexHull OfShapes(ICanvasResourceCreator resourceCreator, IEnumerable<CanvasGeometry> shapes)
        {
            var combined = CanvasGeometry.CreateGroup(resourceCreator, shapes.ToArray());
            return new ConvexHull(ShapeOf(resourceCreator, GrahamScan(PointsOf(combined))));
        }

        /// <summary>
        /// The outline of the hull as a Java 2D shape.
        /// </summary>
        /// <returns>outline of the hull</returns>
        public CanvasGeometry Outline()
        {
            return hull;
        }

        /// <summary>
        /// Apply the provided transformation to the convex hull.
        /// </summary>
        /// <param name="transform">a transform</param>
        /// <returns>a new transformed hull</returns>
        public ConvexHull Transform(Matrix3x2 transform)
        {
            var g = hull.Transform(transform.ToSystem());
            return new ConvexHull(g);
        }

        /// <summary>
        /// Convert a list of points to a shape.
        /// </summary>
        /// <param name="points">list of points</param>
        /// <returns>a shape</returns>
        public static CanvasGeometry ShapeOf(ICanvasResourceCreator resourceCreator, IList<System.Numerics.Vector2> points)
        {
            var g = CanvasGeometry.CreatePolygon(resourceCreator, points.ToArray());
            return g;
        }

        class PathReader : ICanvasPathReceiver
        {
            public List<System.Numerics.Vector2> Points { get; } = new List<System.Numerics.Vector2>();
            List<System.Numerics.Vector2> _newPoints = new List<System.Numerics.Vector2>();

            public void BeginFigure(System.Numerics.Vector2 startPoint, CanvasFigureFill figureFill)
            {
                _newPoints.Clear();
                _newPoints.Add(startPoint);
            }

            public void AddArc(System.Numerics.Vector2 endPoint, float radiusX, float radiusY, float rotationAngle, CanvasSweepDirection sweepDirection, CanvasArcSize arcSize)
            {
                _newPoints.Add(endPoint);
            }

            public void AddCubicBezier(System.Numerics.Vector2 controlPoint1, System.Numerics.Vector2 controlPoint2, System.Numerics.Vector2 endPoint)
            {
                _newPoints.Add(controlPoint1);
                _newPoints.Add(controlPoint2);
                _newPoints.Add(endPoint);
            }

            public void AddLine(System.Numerics.Vector2 endPoint)
            {
                _newPoints.Add(endPoint);
            }

            public void AddQuadraticBezier(System.Numerics.Vector2 controlPoint, System.Numerics.Vector2 endPoint)
            {
                _newPoints.Add(controlPoint);
                _newPoints.Add(endPoint);
            }

            public void SetFilledRegionDetermination(CanvasFilledRegionDetermination filledRegionDetermination)
            {
            }

            public void SetSegmentOptions(CanvasFigureSegmentOptions figureSegmentOptions)
            {
            }

            public void EndFigure(CanvasFigureLoop figureLoop)
            {
                if (_newPoints.Count > 2 && _newPoints.Last().Equals(_newPoints.First()))
                {
                    _newPoints.RemoveAt(_newPoints.Count - 1);
                }
                Points.AddRange(_newPoints);
            }
        }

        /// <summary>
        /// Convert a Java 2D shape to a list of points.
        /// </summary>
        /// <param name="shape">a shape</param>
        /// <returns>list of point</returns>
        public static IList<System.Numerics.Vector2> PointsOf(CanvasGeometry shape)
        {
            var pr = new PathReader();
            shape.SendPathTo(pr);

            return pr.Points;
        }

        /// <summary>
        /// The Graham Scan algorithm determines the points belonging to the convex hull in O(n lg n).
        /// </summary>
        /// <param name="points">set of points</param>
        /// <returns>points in the convex hull</returns>
        /// <seealso href="http://en.wikipedia.org/wiki/Graham_scan">Graham scan, Wikipedia</seealso>
        public static IList<System.Numerics.Vector2> GrahamScan(IList<System.Numerics.Vector2> points)
        {
            if (points.Count <= 3) return new List<System.Numerics.Vector2>(points);

            var ps = new List<System.Numerics.Vector2>(points);
            ps.Sort(new CompareYThenX());
            ps.Sort(new PolarComparator(ps[0]));

            var hull = new ArrayDeque<System.Numerics.Vector2>();

            hull.Push(ps[0]);
            hull.Push(ps[1]);
            hull.Push(ps[2]);

            for (int i = 3; i < points.Count; i++)
            {
                var top = hull.Pop();
                while (hull.Any() && !IsLeftTurn(hull.Peek(), top, ps[i]))
                {
                    top = hull.Pop();
                }
                hull.Push(top);
                hull.Push(ps[i]);
            }

            return new List<System.Numerics.Vector2>(hull);
        }

        /// <summary>
        /// Determine the minimum intersection of a line and the outline of a shape (specified as a list of points).
        /// </summary>
        /// <param name="outline">the outline of a shape</param>
        /// <param name="point1">start of the line</param>
        /// <param name="point2">end of the line</param>
        /// <returns>the intersection</returns>
        private Vector2 Intersect(IList<Vector2> outline, Vector2 point1, Vector2 point2)
        {
            var previousPoint = outline[outline.Count - 1];
            foreach (var point in outline)
            {
                if (Vectors.LinesIntersect(
                    point1.X, point1.Y,
                    point2.X, point2.Y,
                    point.X, point.Y,
                    previousPoint.X, previousPoint.Y))
                {
                    return LineLineIntersect(
                        point.X, point.Y,
                        previousPoint.X, previousPoint.Y,
                        point1.X, point1.Y,
                        point2.X, point2.Y);
                }
                previousPoint = point;
            }
            return point1;
        }

        /// <summary>
        /// Determine the intersect of a line (between <paramref name="a"/> and <paramref name="b"/>) with the hull.
        /// </summary>
        /// <param name="a">first point of the line</param>
        /// <param name="b">second points of the line</param>
        /// <returns>intersection, or null if not found</returns>
        public Vector2 Intersect(Vector2 a, Vector2 b)
        {
            return Intersect(PointsOf(hull).Select(o => o.ToNCDK()).ToList(), new Vector2(a.X, a.Y), new Vector2(b.X, b.Y));
        }

        /// <summary>
        /// Calculate the intersection of two lines.
        /// </summary>
        /// <param name="lineA1">start of the line</param>
        /// <param name="lineA2">end of the line</param>
        /// <param name="lineB1">start of another line</param>
        /// <param name="lineB2">end of another line</param>
        /// <returns>the point where the two lines intersect (or null)</returns>
        public static Vector2 LineLineIntersect(Vector2 lineA1, Vector2 lineA2, Vector2 lineB1, Vector2 lineB2)
        {
            return LineLineIntersect(
                lineA1.X, lineA1.Y,
                lineA2.X, lineA2.Y,
                lineB1.X, lineB1.Y,
                lineB2.X, lineB2.Y);
        }

        /// <summary>
        /// Calculate the intersection of two lines described by the points (x1,y1 -> x2,y2) and (x3,y3 -> x4,y4).
        /// </summary>
        /// <seealso href="http://en.wikipedia.org/wiki/Line–line_intersection">Line-line intersection, Wikipedia</seealso> 
        /// <param name="x1">first x coordinate of line 1</param>
        /// <param name="y1">first y coordinate of line 1</param>
        /// <param name="x2">second x coordinate of line 1</param>
        /// <param name="y2">second y coordinate of line 1</param>
        /// <param name="x3">first x coordinate of line 2</param>
        /// <param name="y3">first y coordinate of line 2</param>
        /// <param name="x4">first x coordinate of line 2</param>
        /// <param name="y4">first y coordinate of line 2</param>
        /// <returns>the point where the two lines intersect (or null)</returns>
        public static Vector2 LineLineIntersect(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
        {
            double x = ((x2 - x1) * (x3 * y4 - x4 * y3) - (x4 - x3) * (x1 * y2 - x2 * y1)) / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4));
            double y = ((y3 - y4) * (x1 * y2 - x2 * y1) - (y1 - y2) * (x3 * y4 - x4 * y3)) / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4));

            return new Vector2((float)x, (float)y);
        }

        /// <summary>
        /// Sort points counter clockwise (polar order) around a reference point.
        /// </summary>
        public sealed class PolarComparator : IComparer<System.Numerics.Vector2>
        {
            private System.Numerics.Vector2 reference;

            public PolarComparator(System.Numerics.Vector2 reference)
            {
                this.reference = reference;
            }

            /// <inheritdoc/>
            public int Compare(System.Numerics.Vector2 a, System.Numerics.Vector2 b)
            {
                double deltaX1 = a.X - reference.X;
                double deltaY1 = a.Y - reference.Y;
                double deltaX2 = b.X - reference.X;
                double deltaY2 = b.Y - reference.Y;

                if (deltaY1 >= 0 && deltaY2 < 0)
                    return -1;
                else if (deltaY2 >= 0 && deltaY1 < 0)
                    return +1;
                else if (deltaY1 == 0 && deltaY2 == 0)
                { // corner case
                    if (deltaX1 >= 0 && deltaX2 < 0)
                        return -1;
                    else if (deltaX2 >= 0 && deltaX1 < 0)
                        return +1;
                    else
                        return 0;
                }
                else
                    return -Winding(reference, a, b); // both above or below
            }
        }

        /// <summary>
        /// Compares points by the y coordinate and then the x if the y's are equal.
        /// </summary>
        public sealed class CompareYThenX : IComparer<System.Numerics.Vector2>
        {
            /// <inheritdoc/>
            public int Compare(System.Numerics.Vector2 a, System.Numerics.Vector2 b)
            {
                if (a.Y < b.Y) return -1;
                if (a.Y > b.Y) return +1;
                if (a.X < b.X) return -1;
                if (a.X > b.X) return +1;
                return 0;
            }
        }

        /// <summary>
        /// Determine if the three points make a left turn.
        /// </summary>
        /// <param name="a">first point</param>
        /// <param name="b">second point</param>
        /// <param name="c">third point</param>
        /// <returns>whether the points make a left turn</returns>
        private static bool IsLeftTurn(System.Numerics.Vector2 a, System.Numerics.Vector2 b, System.Numerics.Vector2 c)
        {
            return Winding(a, b, c) > 0;
        }

        /// <summary>
        /// Determine the winding of three points. The winding is the sign of the space - the parity.
        /// </summary>
        /// <param name="a">first point</param>
        /// <param name="b">second point</param>
        /// <param name="c">third point</param>
        /// <returns>winding, -1=cw, 0=straight, +1=ccw</returns>
        private static int Winding(System.Numerics.Vector2 a, System.Numerics.Vector2 b, System.Numerics.Vector2 c)
        {
            return (int)Math.Sign((b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X));
        }
    }
}
