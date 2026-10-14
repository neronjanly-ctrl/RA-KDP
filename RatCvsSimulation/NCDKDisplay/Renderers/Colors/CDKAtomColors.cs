/* Copyright (C) 1997-2007  The Chemistry Development Kit (CDK) project
 *
 * Contact: cdk-devel@lists.sourceforge.net
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public License
 * as published by the Free Software Foundation; either version 2.1
 * of the License, or (at your option) any later version.
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
using System;
using Windows.UI;

namespace NCDK.Renderers.Colors
{
    /// <summary>
    /// Gives a short table of atom colors for 3D display.
    /// </summary>
    // @cdk.module render
    // @cdk.githash
    [Obsolete(nameof(JmolColors) + " provides more comprehensive color pallet for 3D")]
    public class CDKAtomColors : IAtomColorer
    {
        private readonly static Color HYDROGEN = Windows.UI.Colors.White;
        private readonly static Color CARBON = Windows.UI.Colors.Black;
        private readonly static Color NITROGEN = Windows.UI.Colors.Blue;
        private readonly static Color OXYGEN = Windows.UI.Colors.Red;
        private readonly static Color PHOSPHORUS = Windows.UI.Colors.Green;
        private readonly static Color SULPHUR = Windows.UI.Colors.Yellow;
        private readonly static Color CHLORINE = Windows.UI.Colors.Magenta;

        private readonly static Color DEFAULT = Windows.UI.Colors.DarkGray;

        /// <summary>
        /// Returns the CDK scheme color for the given atom's element.
        /// </summary>
        /// <param name="atom">IAtom to get a color for</param>
        /// <returns>the atom's color according to this coloring scheme.</returns>
        public Color GetAtomColor(IAtom atom)
        {
            return GetAtomColor(atom, DEFAULT);
        }

        /// <summary>
        /// Returns the CDK scheme color for the given atom's element, or
        /// defaults to the given color if no color is defined.
        /// </summary>
        /// <param name="atom">IAtom to get a color for</param>
        /// <param name="defaultColor">Color returned if this scheme does not define a color for the passed IAtom</param>
        /// <returns>the atom's color according to this coloring scheme.</returns>
        public Color GetAtomColor(IAtom atom, Color defaultColor)
        {
            Color color = defaultColor;
            if (atom.AtomicNumber == null) return defaultColor;
            int atomnumber = atom.AtomicNumber.Value;
            switch (atomnumber)
            {
                case 1:
                    color = HYDROGEN;
                    break;
                case 6:
                    color = CARBON;
                    break;
                case 7:
                    color = NITROGEN;
                    break;
                case 8:
                    color = OXYGEN;
                    break;
                case 15:
                    color = PHOSPHORUS;
                    break;
                case 16:
                    color = SULPHUR;
                    break;
                case 17:
                    color = CHLORINE;
                    break;
            }
            return color;
        }
    }
}
