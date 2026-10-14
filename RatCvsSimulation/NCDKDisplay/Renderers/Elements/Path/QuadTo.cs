/*
 * Copyright (C) 2009  Arvid Berg <goglepox@users.sourceforge.net>
 *
 * Contact: cdk-devel@lists.sourceforge.net
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public License
 * as published by the Free Software Foundation; either version 2.1
 * of the License, or (at your option) any later version.
 * All I ask is that proper credit is given for my work, which includes
 * - but is not limited to - adding the above copyright notice to the beginning
 * of your source code files, and to any copyright notice that you may distribute
 * with programs based on this work.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA.
 */
using NCDK.Numerics;

namespace NCDK.Renderers.Elements.Path
{
    /// <summary>
    /// Make a quadratic curve in the path.
    /// </summary>
    // @author Arvid
    // @cdk.module renderbasic
    // @cdk.githash
    public class QuadTo : PathElement
    {
        /// <summary>Coordinates of control point and end point.</summary>
        public Vector2 coord1;
        public Vector2 coord2;

        /// <summary>
        /// Make a quad curve.
        /// </summary>
        /// <param name="cp">control point of the curve</param>
        /// <param name="ep">end point of the curve</param>
        public QuadTo(Vector2 cp, Vector2 ep)
            : base(PathType.QuadTo)
        {
            coord1 = cp;
            coord2 = ep;
        }
        
        /// <summary>
        /// Make a quad curve path element.
        /// </summary>
        /// <param name="coords">[0,1] : control point 1, [2,3] : control point 2, [4,5] end point</param>
        public QuadTo(Vector2[] coords)
            : this(coords[0], coords[1])
        {
        }

        public override Vector2[] Points
        {
            get { return new[] { coord1, coord2 }; }
            set
            {
                coord1 = value[0];
                coord2 = value[1];
            }
        }
    }
}
