//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static Ratmain.Global;

//namespace Ratmain
//{
//    public class TMainForm : TForm
//    {
//        public TMainMenu MainMenu1;
//        public TMenuItem StandardDrugs;
//        public TMenuItem mndrugAL;
//        public TMenuItem mnOptions;
//        public TTimer Timer;
//        public TMenuItem mnAdrenaline;
//        public TMenuItem mnNoradrenaline;
//        public TMenuItem mnIsoprenaline;
//        public TMenuItem mnAcetylcholine;
//        public TMenuItem mnPropranolol;
//        public TMenuItem mnAtropine;
//        public TMenuItem mnFile;
//        public TMenuItem mnNewRat;
//        public TMenuItem mnLoadRat;
//        public TMenuItem mnSaveRat;
//        public TMenuItem mnPrint;
//        public TMenuItem PrintSetup1;
//        public TMenuItem mnExit;
//        public TMenuItem mnHelp;
//        public TMenuItem mnContents;
//        public TMenuItem mnAbout;
//        public TSaveDialog SaveDialog;
//        public TOpenDialog OpenDialog;
//        public TPrinterSetupDialog PrinterSetupDialog;
//        public TMenuItem mnDrugA;
//        public TMenuItem mnDrugB;
//        public TMenuItem mnDrugC;
//        public TMenuItem mnDrugD;
//        public TMenuItem mnDrugE;
//        public TMenuItem mnDrugF;
//        public TMenuItem mnDrugG;
//        public TMenuItem mnDrugH;
//        public TMenuItem mndrugI;
//        public TMenuItem mndrugJ;
//        public TMenuItem mnLNOARG;
//        public TMenuItem mn8SPT;
//        public TMenuItem Phentolamine1;
//        public TMenuItem Verapamil1;
//        public TMenuItem mnGlyceryltrinitrate;
//        public TMenuItem N1;
//        public TMenuItem Cromakalim1;
//        public TMenuItem mnAdenosine1;
//        public TMenuItem NerveStimulation1;
//        public TMenuItem mnStimCardiac;
//        public TMenuItem mnStimAdrenal;
//        public TMenuItem mnAngiotensin2;
//        public TMenuItem mnCaptopril;
//        public TMenuItem mnLosartan;
//        public TMenuItem mnRenal;
//        public TMenuItem mnDigoxin;
//        public TMenuItem mnMilrinone;
//        public TMenuItem mnPhenylephrine;
//        public TMenuItem mnPrazozin;
//        public TMenuItem mnAtenolol;
//        public TMenuItem N2;
//        public TMenuItem mndisplay;
//        public TMenuItem mnGlibenclamide;
//        public TMenuItem mnStimBloodVessels;
//        public TMenuItem mnStimVagus;
//        public TPageControl Page;
//        public TTabSheet ChartTab;
//        public TTabSheet PictureTab;
//        public TPaintBox pbDisplay;
//        public TGroupBox ControlGrp;
//        public TScrollBar sbRecord;
//        public TButton bStart;
//        public TButton bStop;
//        public TEdit edCursor;
//        public TGroupBox GroupBox1;
//        public TRadioButton rbNormalRat;
//        public TRadioButton rbPithedRat;
//        public TImage ExpImage;



//        public void FormResize(TObject Sender)
//        {
//            int MinHeight, MinWidth;

//            MinWidth = ControlGrp.Width + 20;
//            MinHeight = ControlGrp.Height + 100;

//            if (ClientHeight < MinHeight)
//                ClientHeight = Minheight;

//            if (ClientWidth < MinWidth)
//                ClientWidth = MinWidth;

//            Page.Width = ClientWidth - Page.Left - 5;
//            Page.Height = ClientHeight - Page.Top - 5;
//            ControlGrp.Top = ChartTab.ClientHeight - ControlGrp.Height - 10;
//            pbDisplay.Height = ControlGrp.Top - pbDisplay.Top - 10;
//            pbDisplay.Width = ChartTab.CLientWidth - pbDisplay.Left - 10;
//            ExpImage.Width = Max(Round(PictureTab.ClientWidth * 0.9), 465);
//            ExpImage.Height = Max(Round(ExpImage.Width * 329.0 / 465.0), 329);
//            ExpImage.Left = (PictureTab.ClientWidth - ExpImage.Width) / 2;
//            ExpImage.Top = (PictureTab.ClientHeight - ExpImage.Height) / 2;
//            Application.ProcessMessages;
//            pbDisplay.Invalidate;
//        }


//        public void sbRecordChange(TObject Sender)
//        {
//            State = Replay;
//        }

//        public void mnPrintClick(TObject Sender)
//        {
//            PrintTraces;
//        }

//        public void mnExitClick(TObject Sender)
//        {
//            Close;
//        }

//        public void mnContentsClick(TObject Sender)
//        {
//            PChar pWinDir;
//            string WindowsDir;

//            GetMem(pWinDir, 512);
//            GetWindowsDirectoryW(pWinDir, 256);
//            FileName = pWinDir + "\\hh.exe";
//            ShellExecute(Handle, PChar("open"), PChar(FileName), PChar(Application.HelpFile), null, SW_SHOWNORMAL);
//            FreeMem(pWinDir);
//        }

//        public void mnAboutClick(TObject Sender)
//        {
//            AboutFrm.ShowModal;
//        }

//        public void FormActivate(TObject Sender)
//        {
//            REsize;
//            AboutFrm.ShowModal;
//        }

//        public void FormCloseQuery(TObject Sender, ref bool CanClose)
//        {
//            if (MessageDlg("Exit Program! Are you Sure? ", mtConfirmation, mbYes, mbNo, 0) == mrYes)
//                CanClose = True;
//            else
//                CanClose = False;
//        }

//        public void PrintSetup1Click(TObject Sender)
//        {
//            PrinterSetupDialog.Execute;
//        }


//        public void mnHelpClick(TObject Sender)
//        {
//        }

// // Print hard copy of traces on screen
//        internal void PrintTraces()
//        {
//            int LineSpace, ChanHeight, n, CTop, ch, i, j;
//            int TopMargin, BottomMargin, LeftMargin, RightMargin;
//            float x, xStep, y;
//            int xPix, yPix, yPixTop, yPixBottom;
//            TChannel[] PrChan = new TChannel[ChannelLimit + 1];
//            string Lab;

//            SetPageFrm.ShowModal;

//            if (SetPageFrm.ModalResult == mrOK)
//            {
//                screen.cursor = crHourGlass;
//                Printer.BeginDoc;
//                TopMargin = PrinterCMToPixels('V', ExtractFloat(SetPageFrm.edTopMargin.text, 5.0));
//                BottomMargin = PrinterCMToPixels('V', ExtractFloat(SetPageFrm.edBottomMargin.text, 5.0));
//                LeftMargin = PrinterCMToPixels('H', ExtractFloat(SetPageFrm.edLeftMargin.text, 5.0));
//                RightMargin = PrinterCMToPixels('H', ExtractFloat(SetPageFrm.edRightMargin.text, 5.0));
// // Set printer typeface and size
//                Printer.canvas.font.name = "Arial";
//                Printer.canvas.font.Size = 10;
//                LineSpace = printer.canvas.textheight('X');
// // Print user name at top of page
//                Printer.canvas.font.Size = 10;
//                LineSpace = printer.canvas.TextHeight('X');
//                xPix = PrinterCMToPixels('H', 2.5);
//                yPix = PrinterCMToPixels('V', 1.0);
//                Printer.Canvas.TextOut(xPix, yPix, "User: " + AboutFrm.edName.text);
//                yPix = yPix + LineSpace;
//                Printer.Canvas.TextOut(xPix, yPix, "File: " + FileName);
//                yPix = yPix + LineSpace;

//                if (rbPithedrat.Checked)
//                    Printer.Canvas.TextOut(xPix, yPix, "Experiment : Pithed");
//                else
//                    Printer.Canvas.TextOut(xPix, yPix, "Experiment : Normal");

// // Set up printer page scaling
//                n = 0;

//                for (ch = 0; ch <= ChMarker - 1; ch++)
//                    if (Channel[ch].InUse)
//                        Inc(n);

//                if (n < 1)
//                    n = 1;

// // Set position on page and scaling factor for each channel
//                ChanHeight = (Printer.PageHeight - TopMargin - BottomMargin) / n;
//                CTop = TopMargin;

//                for (ch = 0; ch <= ChMarker - 1; ch++)
//                {
//                    PrChan[ch] = Channel[ch];

//                    if (PrChan[ch].InUse)
//                    {
//                        PrChan[ch].Left = LeftMargin;
//                        PrChan[ch].Right = Printer.PageWidth - RightMargin;
//                        PrChan[ch].Top = CTop;
//                        PrChan[ch].Bottom = PrChan[ch].Top + ChanHeight - PrinterCMToPixels('V', 0.5);
//                        PrChan[ch].xScale = (PrChan[ch].Right - PrChan[ch].Left) / (PrChan[ch].xMax - PrChan[ch].xMin);
//                        PrChan[ch].yScale = (PrChan[ch].Bottom - PrChan[ch].Top) / (PrChan[ch].yMax - PrChan[ch].yMin);
//                        CTop = CTop + ChanHeight;
// // Copy channel settings to Marker channel to ensure that
// //                 there is a correct set of X axis scaling factors in it
// //            (any channel will do) 30/10/97
//                        PrChan[chMarker] = PrChan[ch];
//                    }
//                }

// // Display Min./Max. range and channel name
//                Printer.canvas.font.Size = 10;

//                for (ch = 0; ch <= chMarker - 1; ch++)
//                    if (PrChan[ch].InUse)
//                    {
// // Maximum value
//                        PrChan[ch].Left = PrChan[ch].Left + printer.Canvas.TextWidth('X') * 4;
//                        PrChan[ch].xScale = (PrChan[ch].Right - PrChan[ch].Left) / (PrChan[ch].xMax - PrChan[ch].xMin);
//                        Lab = format("%d", Trunc(PrChan[ch].yMax));
//                        printer.canvas.TextOut(PrChan[ch].Left - printer.Canvas.TextWidth(Lab) - printer.Canvas.TextWidth('X') / 2, PrChan[ch].Top, Lab);
// // Minimum value
//                        Lab = '0';
//                        printer.canvas.TextOut(PrChan[ch].Left - printer.Canvas.TextWidth(Lab) - printer.Canvas.TextWidth('X') / 2, PrChan[ch].Bottom - printer.Canvas.TextHeight('X'), Lab);
// // PrChan name
//                        Lab = PrChan[ch].ADCName;
//                        printer.Canvas.TextOut(PrChan[ch].Left - printer.Canvas.TextWidth(Lab) - printer.Canvas.TextWidth('X') / 2, PrChan[ch].Top + printer.Canvas.TextHeight('X'), Lab);
//                    }

// // Draw grid
//                for (ch = 0; ch <= chMarker - 1; ch++)
//                    if (PrChan[ch].InUse)
//                    {
// // Grid line style
//                        Printer.canvas.pen.style = psDot;
// // Horizontal grid
//                        y = 0.0;

//                        while (y <= PrChan[ch].yMax)
//                        {
//                            yPix = PrChan[ch].bottom - Trunc((y - PrChan[ch].yMin) * PrChan[ch].yScale);
//                            Printer.canvas.Polyline(Point(PrChan[ch].Left, yPix), Point(PrChan[ch].Right, yPix));
//                            y = y + Channel[ch].yCal;
//                        }

// // Vertical grid lines
//                        x = (PrChan[ch].xMax - PrChan[ch].xMin) / 10.0;
//                        yPixTop = PrChan[ch].bottom - Trunc((PrChan[ch].yMax - PrChan[ch].yMin) * PrChan[ch].yScale);
//                        yPixBottom = PrChan[ch].bottom - Trunc(-PrChan[ch].yMin * PrChan[ch].yScale);
//                        xStep = x;

//                        while (x < PrChan[ch].xMax)
//                        {
//                            xPix = Trunc((x - PrChan[ch].xMin) * PrChan[ch].xScale) + PrChan[ch].Left;
//                            Printer.canvas.Polyline(Point(xPix, yPixTop), Point(xPix, yPixBottom));
//                            x = x + xStep;
//                        }
//                    }

//                Printer.canvas.pen.style = psSolid;

// // Draw channels traces
//                for (ch = 0; ch <= chMarker - 1; ch++)
//                    if (PrChan[ch].InUse)
//                    {
//                        for (i = 0; i <= BufPointer; i++)
//                        {
//                            xPix = Trunc((i - PrChan[ch].xMin) * PrChan[ch].xScale) + PrChan[ch].Left;
//                            j = i * NumChannels + ch;
//                            yPix = PrChan[ch].Bottom - Trunc(Buf[j] * PrChan[ch].yScale);

//                            if (i == 0)
//                                Printer.canvas.MoveTo(xPix, yPix);
//                            else
//                                Printer.canvas.LineTo(xPix, yPix);
//                        }
//                    }

// // Display markers
//                Printer.canvas.font.height = PrinterPointsToPixels(8);

//                for (ch = 0; ch <= chMarker - 1; ch++)
//                    if (PrChan[ch].InUse)
//                        PrChan[chMarker] = PrChan[ch];

//                for (i = 0; i <= Bufpointer; i++)
//                {
//                    j = i * NumChannels;

//                    if (Buf[j + chMarker] != None)
//                    {
// // Calculate point of drug application on trace
//                        xPix = Trunc((i - PrChan[chMarker].xMin) * PrChan[chMarker].xScale) + PrChan[chMarker].Left;
//                        yPix = Printer.PageHeight - BottomMargin;
// // Draw tic indicating point of drug application
//                        Printer.Canvas.MoveTo(xPix, yPix);
//                        Printer.Canvas.LineTo(xPix, yPix + Printer.Canvas.TextHeight('X'));
//                        xPix = xPix + Printer.Canvas.TextWidth('X') / 4;

// // Display drug/dose information
//                        if (Buf[j + ChMarkerValue] != 0.0)
//                            Printer.canvas.TextOut(xPix, yPix, Drugs[Trunc(Buf[j + ChMarker])].ShortName + format(" %.4g %s", Buf[j + ChMarkerValue], MarkerUnits[Trunc(Buf[j + ChMarkerUnits])]));
//                        else
//                            Printer.canvas.TextOut(xPix, yPix, Drugs[Trunc(Buf[j + ChMarker])].ShortName);
//                    }
//                }

//                Printer.canvas.font.Size = 8;
//                LineSpace = printer.canvas.TextHeight('X');
//                Printer.Canvas.TextOut(PrinterCMToPixels('H', 2.5), Printer.PageHeight - PrinterCMToPixels('V', 1.0), ProgName + ' ' + DateTimeToStr(Now));
//                Printer.EndDoc;
//                screen.cursor = crDefault;
//            }
//        }


//        public PChar WinDir;
//    }

//}
