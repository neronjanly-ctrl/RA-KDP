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

using Windows.Foundation;
using System.Globalization;
using System;
using System.Text;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using NCDK.Renderers.Fonts;
using NCDK.Renderers.Elements;
using NCDK.Numerics;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas;

namespace NCDK.Renderers.Generators.Standards
{
    /// <summary>
    /// Immutable outline of text. The outline is maintained as a Java 2D shape
    /// instance and can be transformed. As an immutable instance, transforming the
    /// outline creates a new instance.
    /// </summary>
    // @author John May
    sealed class TextOutline
    {
        public readonly ICanvasResourceCreator resourceCreator;

        /// <summary>
        /// The original text.
        /// </summary>
        private readonly string text;

        /// <summary>
        /// The original glyphs.
        /// </summary>
        private readonly CanvasTextLayout glyphs;

        /// <summary>
        /// The outline of the text (untransformed).
        /// </summary>
        private readonly CanvasGeometry outline;

        /// <summary>
        /// Transform applied to outline.
        /// </summary>
        private readonly Matrix3x2 transform;

        /// <summary>
        /// Create an outline of text in provided font.
        /// </summary>
        /// <param name="text">the text to create an outline of</param>
        /// <param name="font">the font style, size, and shape that defines the outline</param>
        public TextOutline(ICanvasResourceCreator resourceCreator, string text, Typeface font, double emSize)
            : this(
                resourceCreator,
                text,
                new CanvasTextLayout(
                    resourceCreator,
                    text,
                    new CanvasTextFormat
                    {
                        FontFamily = font.FontFamily.Source,
                        FontSize = (float)emSize,
                        FontStretch = font.Stretch,
                        FontStyle = font.Style,
                        FontWeight = font.Weight,
                    },
                    float.PositiveInfinity,
                    float.PositiveInfinity))
        { }

        /// <summary>
        /// Create an outline of text and the glyphs for that text.
        /// </summary>
        /// <param name="text">the text to create an outline of</param>
        /// <param name="glyphs">the glyphs for the provided outlined</param>
        public TextOutline(ICanvasResourceCreator resourceCreator, string text, CanvasTextLayout glyphs)
            : this(resourceCreator, text, glyphs, CanvasGeometry.CreateText(glyphs), Matrix3x2.Identity)
        { }

        /// <summary>
        /// Internal constructor, requires all attributes.
        /// </summary>
        /// <param name="text">the text</param>
        /// <param name="glyphs">glyphs of the text</param>
        /// <param name="outline">the outline of the glyphs</param>
        /// <param name="transform">the transform</param>
        private TextOutline(ICanvasResourceCreator resourceCreator, string text, CanvasTextLayout glyphs, CanvasGeometry outline, Matrix3x2 transform)
        {
            this.resourceCreator = resourceCreator;
            this.text = text;
            this.glyphs = glyphs;
            this.outline = outline;
            this.transform = transform;
        }

        /// <summary>
        /// The text which the outline displays.
        /// <returns>the text</returns>
        /// </summary>
        public string Text => text;

        /// <summary>
        /// Access the transformed outline of the text.
        /// </summary>
        /// <returns>transformed outline</returns>
        public CanvasGeometry GetOutline()
        {
            var c = outline.Transform(transform.ToSystem());
            return c;
        }

        /// <summary>
        /// Access the transformed bounds of the outline text.
        /// </summary>
        /// <returns>transformed bounds</returns>
        public Rect GetBounds() => TransformedBounds(outline);

        /// <summary>
        /// Access the transformed logical bounds of the outline text.
        /// </summary>
        /// <returns>logical bounds</returns>
        public Rect GetLogicalBounds()
        {
            return TransformedBounds(CanvasGeometry.CreateRectangle(resourceCreator, glyphs.LayoutBounds));
        }

        /// <summary>
        /// Access the bounds of a shape that have been transformed.
        /// </summary>
        /// <param name="shape">any shape</param>
        /// <returns>the bounds of the shape transformed</returns>
        private Rect TransformedBounds(CanvasGeometry shape)
        {
            var rectangle2D = shape.ComputeBounds();

            var minPoint = new Vector2(rectangle2D.Left, rectangle2D.Top);
            var maxPoint = new Vector2(rectangle2D.Right, rectangle2D.Bottom);

            minPoint = Vector2.Transform(minPoint, transform);
            maxPoint = Vector2.Transform(maxPoint, transform);

            // may be flipped by transformation
            double minX = Math.Min(minPoint.X, maxPoint.X);
            double maxX = Math.Max(minPoint.X, maxPoint.X);
            double minY = Math.Min(minPoint.Y, maxPoint.Y);
            double maxY = Math.Max(minPoint.Y, maxPoint.Y);

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// Access the transformed center of the whole outline.
        /// </summary>
        /// <returns>center of outline</returns>
        public Vector2 GetCenter()
        {
            var bounds = GetBounds();
            return new Vector2((bounds.Left + bounds.Right) / 2, (bounds.Top + bounds.Bottom) / 2);
        }

        /// <summary>
        /// Access the transformed center of the first glyph outline.
        /// </summary>
        /// <returns>center of first glyph outline</returns>
        public Vector2 GetFirstGlyphCenter()
        {
            return GetGlyphCenter(0);
        }

        /// <summary>
        /// Access the transformed center of the last glyph outline.
        /// </summary>
        /// <returns>center of last glyph outline</returns>
        public Vector2 GetLastGlyphCenter()
        {
            return GetGlyphCenter(text.Length - 1);
        }

        /// <summary>
        /// Determines the transformed centre of a specified glyph.
        /// </summary>
        /// <param name="index">glyph index</param>
        /// <returns>center point</returns>
        private Vector2 GetGlyphCenter(int index)
        {
            if (text.Length == 1) return GetCenter();
            if (index == 0)
            {
                var bounds = GetBounds();
                var p = new Vector2(bounds.Left, (bounds.Top + bounds.Bottom) / 2);
                return Vector2.Transform(p, transform);
            }
            var o1 = new TextOutline(resourceCreator, text.Substring(0, index - 1), glyphs, outline, transform);
            var o2 = new TextOutline(resourceCreator, text.Substring(0, index), glyphs, outline, transform);
            var b1 = o1.GetBounds();
            var b2 = o2.GetBounds();
            return new Vector2((b1.Right + b2.Right) / 2, (b1.Bottom + b2.Bottom) / 2);
        }

        /// <summary>
        /// Add a transformation to the outline.
        /// </summary>
        /// <param name="nextTransform">new transformation</param>
        /// <returns>new text outline</returns>
        public TextOutline Transform(Matrix3x2 nextTransform)
        {
            return new TextOutline(resourceCreator, text, glyphs, outline, nextTransform * transform);
        }

        /// <summary>
        /// Convenience function to resize the outline and maintain the existing
        /// center point.
        /// </summary>
        /// <param name="scaleX">scale x-axis</param>
        /// <param name="scaleY">scale y-axis</param>
        /// <returns>resized outline</returns>
        public TextOutline Resize(double scaleX, double scaleY)
        {
            var center = GetCenter();

            var t = Matrix3x2.CreateTranslation(-center);
            t = Matrix3x2.CreateScale(scaleX, scaleY) * t;
            t = Matrix3x2.CreateTranslation(center) * t;
            return Transform(t);
        }

        /// <summary>
        /// Convenience function to translate the outline.
        /// </summary>
        /// <param name="translateX">x-axis translation</param>
        /// <param name="translateY">y-axis translation</param>
        /// <returns>translated outline</returns>
        public TextOutline Translate(double translateX, double translateY)
        {
            var m = Matrix3x2.CreateTranslation(translateX, translateY);
            return Transform(m);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var bounds = GetBounds();
            var sb = new StringBuilder(25);
            sb.Append(text);
            sb.Append(" [x=").Append(FormatDouble(bounds.X));
            sb.Append(", y=").Append(FormatDouble(bounds.Y));
            sb.Append(", w=").Append(FormatDouble(bounds.Width));
            sb.Append(", h=").Append(FormatDouble(bounds.Height));
            sb.Append(']');
            return sb.ToString();
        }

        /// <summary>
        /// Format a double - displayed as two decimal places.
        /// </summary>
        /// <param name="value">number value</param>
        /// <returns>string of formatted double</returns>
        public static string FormatDouble(double value)
        {
            return value.ToString("F2");
        }
    }
}
