/* Copyright (C) 2009  Mark Rijnbeek <mark_rynbeek@users.sf.net>
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
using System.Collections.Generic;
using Windows.UI;

namespace NCDK.Renderers.Colors
{
    /// <summary>
    /// Atom coloring following RasMol/Chime Color scheme
    /// <see href="http://www.umass.edu/microbio/rasmol/rascolor.htm">http://www.umass.edu/microbio/rasmol/rascolor.htm</see>.
    /// </summary>
    // @cdk.module render
    // @cdk.githash
    [Serializable]
    public class RasmolColors : IAtomColorer
    {
        private readonly static Color DEFAULT = Color.FromArgb(0xFF, 255, 20, 147);

        private static IDictionary<string, Color> colorMap;

        /// <summary>
        /// Color map with RasMol/Chime Color RGB Values. Excepted H and C (too
        /// light).
        /// </summary>
        static RasmolColors()
        {
            colorMap = new Dictionary<string, Color>();

            colorMap["C"] = Color.FromArgb(0xFF, 144, 144, 144);
            colorMap["H"] = Color.FromArgb(0xFF, 144, 144, 144);
            colorMap["O"] = Color.FromArgb(0xFF, 240, 0, 0);
            colorMap["N"] = Color.FromArgb(0xFF, 143, 143, 255);
            colorMap["S"] = Color.FromArgb(0xFF, 255, 200, 50);
            colorMap["Cl"] = Color.FromArgb(0xFF, 0, 255, 0);
            colorMap["B"] = Color.FromArgb(0xFF, 0, 255, 0);
            colorMap["P"] = Color.FromArgb(0xFF, 255, 165, 0);
            colorMap["Fe"] = Color.FromArgb(0xFF, 255, 165, 0);
            colorMap["Ba"] = Color.FromArgb(0xFF, 255, 165, 0);
            colorMap["Na"] = Color.FromArgb(0xFF, 0, 0, 255);
            colorMap["Mg"] = Color.FromArgb(0xFF, 34, 139, 34);
            colorMap["Zn"] = Color.FromArgb(0xFF, 165, 42, 42);
            colorMap["Cu"] = Color.FromArgb(0xFF, 165, 42, 42);
            colorMap["Ni"] = Color.FromArgb(0xFF, 165, 42, 42);
            colorMap["Br"] = Color.FromArgb(0xFF, 165, 42, 42);
            colorMap["Ca"] = Color.FromArgb(0xFF, 128, 128, 144);
            colorMap["Mn"] = Color.FromArgb(0xFF, 128, 128, 144);
            colorMap["Al"] = Color.FromArgb(0xFF, 128, 128, 144);
            colorMap["Ti"] = Color.FromArgb(0xFF, 128, 128, 144);
            colorMap["Cr"] = Color.FromArgb(0xFF, 128, 128, 144);
            colorMap["Ag"] = Color.FromArgb(0xFF, 128, 128, 144);
            colorMap["F"] = Color.FromArgb(0xFF, 218, 165, 32);
            colorMap["Si"] = Color.FromArgb(0xFF, 218, 165, 32);
            colorMap["Au"] = Color.FromArgb(0xFF, 218, 165, 32);
            colorMap["I"] = Color.FromArgb(0xFF, 160, 32, 240);
            colorMap["Li"] = Color.FromArgb(0xFF, 178, 34, 34);
            colorMap["He"] = Color.FromArgb(0xFF, 255, 192, 203);
        }

        /// <summary>
        /// Returns the Rasmol color for the given atom's element.
        /// </summary>
        /// <param name="atom">IAtom to get a color for</param>
        /// <returns>the atom's color according to this coloring scheme.</returns>
        public Color GetAtomColor(IAtom atom)
        {
            return GetAtomColor(atom, DEFAULT);
        }

        /// <summary>
        /// Returns the Rasmol color for the given atom's element, or
        /// defaults to the given color if no color is defined.
        /// </summary>
        /// <param name="atom">IAtom to get a color for</param>
        /// <param name="defaultColor">Color returned if this scheme does not define a color for the passed IAtom</param>
        /// <returns>the atom's color according to this coloring scheme.</returns>
        public Color GetAtomColor(IAtom atom, Color defaultColor)
        {
            Color color = defaultColor;
            string symbol = atom.Symbol;
            if (colorMap.ContainsKey(symbol))
            {
                color = colorMap[symbol];
            }
            return color;
        }
    }
}
