using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace RatCvsSimulation.Extensions
{
    public static class CanvasDrawingSessionExts
    {
        public static Rect MeasureTextRect(this CanvasDrawingSession session, CanvasTextFormat format, string text)
        {
            var layout = new CanvasTextLayout(session, text, format, 0, 0);
            return layout.LayoutBounds;
        }

        public static float MeasureTextWidth(this CanvasDrawingSession session, CanvasTextFormat format, string text)
        {
            return (float)MeasureTextRect(session, format, text).Width;
        }

        public static float MeasureTextHeight(this CanvasDrawingSession session, CanvasTextFormat format, string text)
        {
            return (float)MeasureTextRect(session, format, text).Height;
        }

    }
}
