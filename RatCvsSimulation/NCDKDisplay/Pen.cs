using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace NCDK
{
    public class Pen
    {
        public CanvasStrokeStyle Style { get; set; }
        public double Width { get; set; }
        public Color Color { get; set; }

        public Pen() : this(Colors.Black, 1)
        { }

        public Pen(Color color, double width)
        {
            Style = new CanvasStrokeStyle();
            Color = color;
            Width = width;
        }
    }
}
