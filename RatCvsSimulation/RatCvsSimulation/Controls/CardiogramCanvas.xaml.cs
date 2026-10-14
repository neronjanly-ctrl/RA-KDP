using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using RatCvsSimulation.Core;
using RatCvsSimulation.Extensions;
using System.Threading.Tasks;
using RatCvsSimulation.Views;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace RatCvsSimulation.Controls
{
    public sealed partial class CardiogramCanvas : UserControl
    {
        private const uint XMax = Document.PageLength - 1;

        public ChannelDefinitionCollection ChannelDefinitions = new ChannelDefinitionCollection
        {
            // Blood pressure
            ABP = new Channel
            {
                ADCName = "ABP",
                ADCUnits = "mmHg",
                XMin = 0.0f,
                XMax = XMax,
                YMin = 0.0f,
                YMax = 200.0f,
                YCal = 20.0f,
                GetValue = data => data.ABP,
                Color = Colors.Purple,
            },

            // Left ventricular pressure
            LVP = new Channel
            {
                ADCName = "LVP",
                ADCUnits = "mmHg",
                XMin = 0.0f,
                XMax = XMax,
                YMin = 0.0f,
                YMax = 200.0f,
                YCal = 20.0f,
                GetValue = data => data.LVP,
                Color = Colors.Navy,
            },

            // Venous Pressure
            CVP = new Channel
            {
                ADCName = "VBP",
                ADCUnits = "mmHg",
                XMin = 0.0f,
                XMax = XMax,
                YMin = 0.0f,
                YMax = 20.0f,
                YCal = 2.0f,
                GetValue = data => data.CVP,
                Color = Colors.DarkGreen,
            },

            // Cardiac contractile force (dLVP/dt)/LVP
            HCF = new Channel
            {
                ADCName = "HF",
                ADCUnits = " ",
                XMin = 0.0f,
                XMax = XMax,
                YMin = 0.0f,
                YMax = 20.0f,
                YCal = 2.0f,
                GetValue = data => data.HCF,
                Color = Colors.Maroon,
            },

            // Heart rate
            HR = new Channel
            {
                ADCName = "HR",
                ADCUnits = "BPM",
                XMin = 0.0f,
                XMax = XMax,
                YMin = -10.0f,
                YMax = 800.0f,
                YCal = 100.0f,
                GetValue = data => data.HR,
                Color = Colors.DarkOliveGreen,
            },
        };

        public IEnumerable<CardiogramData> DataSource { get; set; }

        //ArrayByChannelId<Point> OldPos { get; } = new ArrayByChannelId<Point>(Document.StripLength);

        public CardiogramCanvas()
        {
            this.InitializeComponent();
        }

        public void Invalidate()
        {
            DisplayCanvas.Invalidate();
        }

        private void DisplayCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var canvasSize = sender.Size;
            var session = args.DrawingSession;

            // styling settings
            var format = new CanvasTextFormat { FontSize = 12, WordWrapping = CanvasWordWrapping.NoWrap };
            Color penColor = Colors.Black, fontColor = Colors.Black;
            float penWidth = 2;
            var textRect = session.MeasureTextRect(format, "X");

            // Determine number of ChannelDefinitions in use
            var n = ChannelDefinitions.Count(o => o.InUse);

            if (n <= 0)
                n = 1;

            var height = (int)(canvasSize.Height - 2 * textRect.Height) / n;

            // Define display area for each channel
            int cTop = (int)textRect.Height;

            // TODO: move to separated initialization method and call only upon resize, load and channel redefine
            foreach (var channel in ChannelDefinitions)
            {
                if (!channel.InUse) continue;

                channel.Right = (int)canvasSize.Width - 5;
                channel.Top = cTop;
                channel.Bottom = channel.Top + height - (int)textRect.Height / 2;
                channel.XMin = ChannelDefinitions.First.XMin;
                channel.XMax = ChannelDefinitions.First.XMax;
                channel.XScale = (channel.Right - channel.Left) / (channel.XMax - channel.XMin);
                channel.YScale = (channel.Bottom - channel.Top) / (channel.YMax - channel.YMin);
                cTop = cTop + height;
            }

            // Display Min./ Max.range and channel name
            foreach (var channel in ChannelDefinitions)
            {
                if (!channel.InUse) continue;

                // Maximum value
                channel.Left = (int)(textRect.Width * 6);
                var lab = ((int)channel.YMax).ToString();
                session.DrawText(lab,
                    channel.Left - (int)session.MeasureTextWidth(format, lab) - (int)textRect.Width / 2,
                    channel.Top,
                    fontColor, format);

                // Minimum value
                lab = "0";
                session.DrawText(lab,
                    channel.Left - (int)session.MeasureTextWidth(format, lab) - (int)textRect.Width / 2,
                    channel.Bottom - (float)textRect.Height,
                    fontColor, format);

                // Channel name
                lab = channel.ADCName;
                fontColor = channel.Color;
                session.DrawText(lab,
                    channel.Left - (int)session.MeasureTextWidth(format, lab) - (int)textRect.Width / 2,
                    channel.Top + (float)textRect.Height,
                    fontColor, format);
                fontColor = Colors.Black;
            }

            // Draw grid
            // Grid colour
            penColor = Colors.Gray;

            foreach (var channel in ChannelDefinitions)
            {
                if (!channel.InUse) continue;

                // Horizontal grid
                var y = 0.0f;
                penWidth = 2;

                while (y <= channel.YMax)
                {
                    int yPix = channel.Bottom - (int)((y - channel.YMin) * channel.YScale);
                    session.DrawLine(channel.Left, yPix, channel.Right, yPix, penColor, penWidth);
                    y = y + channel.YCal;
                    penWidth = 1;
                }

                // Vertical grid lines
                var x = (ChannelDefinitions.First.XMax - ChannelDefinitions.First.XMin) / 10.0f;
                var yPixTop = channel.Bottom - (int)((channel.YMax - channel.YMin) * channel.YScale);
                var yPixBottom = channel.Bottom - (int)(-channel.YMin * channel.YScale);
                var xStep = x;

                while (x < ChannelDefinitions.First.XMax)
                {
                    int xPix = (int)((x - ChannelDefinitions.First.XMin) * ChannelDefinitions.First.XScale) + ChannelDefinitions.First.Left;
                    session.DrawLine(xPix, yPixTop, xPix, yPixBottom, penColor, penWidth);
                    x = x + xStep;
                }
            }

            //session.pen.style = psSolid;

            // Draw channels
            foreach (var channel in ChannelDefinitions)
            {
                if (!channel.InUse) continue;

                penColor = channel.Color;
                int xPix0 = 0, yPix0 = 0;

                int xPos = 0;
                foreach (var item in DataSource)
                {
                    int xPix = (int)((xPos / 2 * 2 - ChannelDefinitions.First.XMin) * ChannelDefinitions.First.XScale) + ChannelDefinitions.First.Left;
                    int yPix = channel.Bottom - (int)((channel.GetValue(item) - channel.YMin) * channel.YScale);

                    if (xPos != 0)
                        session.DrawLine(xPix0, yPix0, xPix, yPix, penColor);

                    xPix0 = xPix;
                    yPix0 = yPix;
                    xPos++;
                }

                // remember the last positions for each channel for incremental drawing
                // not used for drawing from draft
                //if (BufPointer > 0)
                //    OldPos[ch] = new Point(xPix, yPix);
                //else
                //    OldPos[ch] = new Point(0, 0);
            }

            // Display markers
            // TODO: let the markers be opaque
            int i = 0;
            foreach (var item in DataSource)
            {
                if (item.HasMarker)
                {
                    int xPix = (int)((i - ChannelDefinitions.First.XMin) * ChannelDefinitions.First.XScale) + ChannelDefinitions.First.Left;
                    int yPix = (int)(canvasSize.Height - textRect.Height);

                    // Draw tic indicating point of drug application
                    session.DrawLine(xPix, yPix, xPix, yPix + (float)textRect.Height, penColor, penWidth);
                    session.DrawText(item.MarkerText, xPix + 2, yPix, fontColor, format);
                }
                i++;
            }
        }

        // draw only the last two points for each channel (with markers)
        //internal void RunningDisplayDraw(Size canvasSize, CanvasDrawingSession session)
        //{
        //    var format = new CanvasTextFormat { FontSize = 12, WordWrapping = CanvasWordWrapping.NoWrap };
        //    float textWidth = session.MeasureTextWidth(format, "X"), textHeight = session.MeasureTextHeight(format, "X");
        //    float xPix, yPix;
        //    float penWidth = 2;
        //    Color penColor;
        //    uint BufPointer = _model.Buf.PageLength;

        //    // Plot results on chart traces
        //    xPix = (int)((BufPointer - 1 - ChannelDefinitions.First.XMin) * ChannelDefinitions.First.XScale) + ChannelDefinitions.First.Left;

        //    for (ChannelId ch = ChannelId._Begin; ch < ChannelId._EndChannels; ch++)
        //        if (ChannelDefinitions[ch].InUse)
        //        {
        //            yPix = ChannelDefinitions[ch].Bottom - (int)((Buf[BufPointer - 2, ch] - ChannelDefinitions[ch].YMin) * ChannelDefinitions[ch].YScale);
        //            penColor = ChannelDefinitions[ch].Color;

        //            // If no information about previous position ... used current
        //            if (_model.OldPos[ch].X == 0)
        //                _model.OldPos[ch] = new Point(xPix, yPix);

        //            session.DrawLine((float)_model.OldPos[ch].X, (float)_model.OldPos[ch].Y, xPix, yPix, penColor);
        //            var yPix0 = yPix;

        //            yPix = ChannelDefinitions[ch].Bottom - (int)((Buf[BufPointer - 1, ch] - ChannelDefinitions[ch].YMin) * ChannelDefinitions[ch].YScale);
        //            session.DrawLine(xPix, yPix0, xPix, yPix, penColor);
        //            _model.OldPos[ch] = new Point(xPix, yPix);
        //        }

        //    // Display marker at bottom of screen

        //    if (Buf[BufPointer - 2, ChannelId.Marker] != None)
        //    {
        //        yPix = (int)(canvasSize.Height - textHeight);
        //        // Draw tic indicating point of drug application
        //        session.DrawLine(xPix, yPix, xPix, yPix + textHeight, penColor);

        //        if (Buf[BufPointer - 2, ChannelId.MarkerValue] != 0.0f)
        //            session.DrawText(Drugs[(int)Buf[BufPointer - 2, ChannelId.Marker]].ShortName
        //                + string.Format(" {0:g3} {1}", Buf[BufPointer - 2, ChannelId.MarkerValue], MarkerUnits[(int)(Buf[BufPointer - 2, ChannelId.MarkerUnits])]),
        //                xPix + 2, yPix, Colors.Black);
        //        else
        //            session.DrawText(Drugs[(int)Buf[BufPointer - 2, ChannelId.Marker]].ShortName, xPix + 2, yPix, Colors.Black);
        //    }
        //}

        public new event TypedEventHandler<UserControl, ChannelPointerMovedEventArgs> PointerMoved;

        private void DisplayCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (PointerMoved == null)
                return;

            float X = (float)e.GetCurrentPoint(DisplayCanvas).Position.X;
            float Y = (float)e.GetCurrentPoint(DisplayCanvas).Position.Y;

            Channel chCursor = null;

            float XValue = (X - ChannelDefinitions.First.Left) / ChannelDefinitions.First.XScale + ChannelDefinitions.First.XMin;
            float YValue;

            foreach (var cd in ChannelDefinitions)
            {
                if (!cd.InUse) continue;

                YValue = (cd.Bottom - Y) / cd.YScale + cd.YMin;

                if (cd.YMin <= YValue && YValue <= cd.YMax)
                    chCursor = cd;
            }

            if (chCursor != null && Y <= chCursor.Bottom && Y >= chCursor.Top)
            {
                YValue = (chCursor.Bottom - Y) / chCursor.YScale + chCursor.YMin;
                PointerMoved?.Invoke(this, new ChannelPointerMovedEventArgs(chCursor, XValue, YValue));
            }
            else
            {
                PointerMoved?.Invoke(this, new ChannelPointerMovedEventArgs());
            }
        }
    }
}
