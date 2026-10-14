using Microsoft.Graphics.Canvas;
using NCDK;
using NCDK.Default;
using NCDK.IO;
using NCDK.Layout;
using NCDK.Numerics;
using NCDK.Renderers;
using NCDK.Renderers.Fonts;
using NCDK.Renderers.Generators;
using NCDK.Renderers.Visitors;
using NCDK.Smiles;
using RatCvsSimulation.Core;
using RatCvsSimulation.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RatCvsSimulation.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GridMainPage : Page
    {
        public AnimalArguments AnimalArgs { get; set; }

        public static int PrinterPointsToPixels(int PointSize)
        {
            int Result;

            float PixelsPerInch;

            // Get height and width of page (in mm) and calculate
            // the size of a pixel(in cm)
            PixelsPerInch = 0;// GetDeviceCaps(printer.handle, LOGPIXELSX);
            Result = Trunc(PointSize * PixelsPerInch / 72.0f);

            return Result;
        }

        /// <summary>
        /// Convert from cm (on printer page) to pixels
        /// </summary>
        /// <param name="Axis"></param>
        /// <param name="cm"></param>
        /// <returns></returns>
        public static int PrinterCmToPixels(string Axis, float cm)
        {
            //float PixelWidth, PixelHeight;

            // Get height and width of page (in mm) and calculate
            // the size of a pixel(in cm)
            if (Axis.ToUpper() == "H")
            {
                // Printer pixel width (mm)
                //PixelWidth = GetDeviceCaps(printer.handle, HORZSIZE);
                return 0;// Trunc(10.0f * cm * printer.pagewidth / PixelWidth);
            }
            else
            {
                // Printer pixel height (mm)
                //PixelHeight = GetDeviceCaps(printer.handle, VERTSIZE);
                return 0;// Trunc(printer.pageheight * 10.0f * cm / PixelHeight);
            }
        }

        static int Trunc(float a)
        {
            return (int)Math.Floor(a);
        }

        MainPageViewModel _viewModel = new MainPageViewModel();
        LabRat _rat = new LabRat();
        Ticker _ticker;

        DispatcherTimer _timer;
        string _progName;

        public GridMainPage()
        {
            this.InitializeComponent();
            CatButton_Click(null, null);
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            DisplayChart.DataSource = _viewModel.Doc;
        }

        async Task Close()
        {
            _timer.Stop();
            await _viewModel.Doc.Cleanup();

            Application.Current.Exit();
        }

        public async void TimerTimer()
        {
            switch (_viewModel.State)
            {
                case State.Recording:
                    _rat.Tick(_ticker, /*rbPithedRat.IsChecked ?? false*/false);

                    await _viewModel.Doc.PushStrip(
                        _rat.GetChannelValues(true)
                    );

                    await _viewModel.Doc.PushStrip(
                        _rat.GetChannelValues(false)
                    );

                    _ticker.Time += _ticker.Step;
                    DisplayChart.Invalidate();

                    if (_rat.Dead)
                    {
                        //TODO: display alert like below
                        //internal void DeadRatDisplayDraw(Size canvasSize, CanvasDrawingSession session)
                        //{
                        //    var fontColor = Colors.Red;
                        //    session.DrawText("Your rat has just died!!!",
                        //        (float)canvasSize.Width / 4, (float)canvasSize.Height / 2,
                        //        fontColor, new CanvasTextFormat { FontSize = 32 });
                        //}
                    }
                    break;

                case State.Replay:
                    //await _viewModel.Doc.LoadPage(sbRecord.Position);
                    DisplayChart.Invalidate();
                    _viewModel.State = State.Idle;
                    break;
            }
        }

        /// <summary>
        /// Set up initial conditions for simulation
        /// 15/10/97 Better temporary file allocation
        /// </summary>
        /// <returns></returns>
        internal async Task InitializeSimulation()
        {
            try
            {
                _viewModel.Doc.ClearPage();
                await _viewModel.Doc.NewFile();
            }
            catch (Exception ex)
            {
                var messageDlg = new MessageDialog(ex.Message);
                await messageDlg.ShowAsync();
                await Close();
                return;
            }

            ApplicationView.GetForCurrentView().Title = _progName;

            // Initialise drug doses and potencies
            _rat.Init();

            _ticker = new Ticker()
            {
                Step = 0.125E-3f,
                Time = 0.0f,
                Next = -1.0f
            };

            DisplayChart.Invalidate();
            _viewModel.State = State.Idle;
            UpdateButtonStates(true);
            //sbRecord.IsEnabled = false;
        }

        private void UpdateButtonStates(bool start)
        {
            //bStart.IsEnabled = start;
            //bStop.IsEnabled = !start;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DisplayChart.Invalidate();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var av = ApplicationView.GetForCurrentView();
            av.Title = AnimalArgs.Name;
            _progName = av.Title;
            //Application.HelpFile = ExtractFilePath(ParamStr(0)) + "ratcvs.chm";
            await InitializeSimulation();
            ShowMenu(0);

            _timer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, 0, 16)
            };
            _timer.Tick += (s, e2) => TimerTimer();
        }


        private void bStart_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.State = State.Recording;

            UpdateButtonStates(false);
            //sbRecord.IsEnabled = false;
            //mnFile.IsEnabled = false;

            _viewModel.Doc.ClearPage();

            _timer.Start();

            if (IsMaximized(AnimalSelector))
            {
                ToggleMaximized(AnimalSelector, 1, 0);
                ToggleMaximized(DisplayChart, 1, 1);
            }
        }

        private void bStop_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.State = State.Replay;

            //sbRecord.Max = MaxInt(nBufs - 1, 1);
            //sbRecord.Min = 0;
            UpdateButtonStates(true);
            //mnFile.IsEnabled = true;

            _timer.Stop();
        }

        public async Task NewRat()
        {
            bStop_Click(null, null);
            await _viewModel.Doc.Cleanup();
            await InitializeSimulation();
        }

        /// <summary>
        /// Load an experiment from a .labrat storage file
        /// </summary>
        public async Task LoadRat()
        {
            var filePicker = new FileOpenPicker();
            var file = await filePicker.PickSingleFileAsync();

            if (file != null)
            {
                //Caption = ProgName + " : " + file.Name;

                await _viewModel.Doc.OpenFile(file);

                //sbRecord.Max = nBufs - 1;
                //sbRecord.Min = 0;

                UpdateButtonStates(true);

                //if (Buf.HasData())
                //    sbRecord.IsEnabled = true;

                _viewModel.State = State.Replay;
            }
        }

        /// <summary>
        /// Save an experiment to a .labrat storage file
        /// </summary>
        public async Task SaveRat()
        {
            var filePicker = new FileSavePicker();
            var file = await filePicker.PickSaveFileAsync();

            if (file != null)
            {
                //Caption = ProgName + " : " + file.Name;

                await _viewModel.Doc.SaveFile(file);
            }
        }

        public async Task Settings()
        {
            var channels = DisplayChart.ChannelDefinitions;

            var dialog = new SettingsDialog()
            {
                ABPColor = channels.ABP.Color,
                ABPInUse = channels.ABP.InUse,
                LVPColor = channels.LVP.Color,
                LVPInUse = channels.LVP.InUse,
                CVPColor = channels.CVP.Color,
                CVPInUse = channels.CVP.InUse,
                HCFColor = channels.HCF.Color,
                HCFInUse = channels.HCF.InUse,
                HRColor = channels.HR.Color,
                HRInUse = channels.HR.InUse
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                channels.ABP.Color = dialog.ABPColor;
                channels.ABP.InUse = dialog.ABPInUse;
                channels.LVP.Color = dialog.LVPColor;
                channels.LVP.InUse = dialog.LVPInUse;
                channels.CVP.Color = dialog.CVPColor;
                channels.CVP.InUse = dialog.CVPInUse;
                channels.HCF.Color = dialog.HCFColor;
                channels.HCF.InUse = dialog.HCFInUse;
                channels.HR.Color = dialog.HRColor;
                channels.HR.InUse = dialog.HRInUse;
                DisplayChart.Invalidate();
            }
        }

        int currentMenu = -1;
        public void ShowMenu(int type)
        {
            if (currentMenu == type)
                return;

            currentMenu = type;

            //if (type == -1)
            //{
            //    LeftMenuColumn.MinWidth = 0;
            //    return;
            //}

            //LeftMenuColumn.MinWidth = 160;

            var query = _rat.Drugs.Where(o => o != null && o.Name != "Alpha1" && o.Name != "Beta1");

            //if (type == 1)
            //{
            //    query = query.Where(o => o.Name.StartsWith("Drug"));
            //}
            //else if (type == 2)
            //{
            //    query = query.Where(o => o.ShortName.StartsWith("Symp") || o.ShortName.EndsWith("Nv."));
            //}
            //else
            //{
            //    query = query.Where(o => !o.Name.StartsWith("Drug") && !o.ShortName.StartsWith("Symp") && !o.ShortName.EndsWith("Nv."));
            //}

            DrugMenu.ItemsSource = query;
        }

        public void ShowVisualization(bool show)
        {
            //VisualizationControl.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void DrugMenu_ItemClick(object sender, ItemClickEventArgs e)
        {
            var drug = e.ClickedItem as Drug;
            int ix;
            float value = 0f;
            UnitType unit = UnitType.None;

            // TODO: remodel Drug class to have subtypes and call virtual Stim method directly here
            if (drug.Name == "Cardiac")
            {
                ix = _rat.StimCardiac(drug);
            }
            else if (drug.Name == "AdrenNerve")
            {
                ix = _rat.StimAdrenal(drug);
            }
            else if (drug.Name == "Renal")
            {
                ix = _rat.StimRenalNerve(drug);
            }
            else if (drug.Name == "Vagus")
            {
                ix = _rat.StimVagusNerve(drug);
            }
            else if (drug.Name == "SympNerve")
            {
                ix = _rat.StimBloodVessels(drug);
            }
            else
            {
                var dialog = new AddDrugDialog
                {
                    Title = drug.Name,
                    MinDose = drug.MinDose,
                    MaxDose = drug.MaxDose,
                };

                // Let user set dose
                // user canceled the dialog
                if (await dialog.ShowAsync() != ContentDialogResult.Primary)
                    return;

                ix = _rat.InjectDrug(drug, dialog.SelectedDose, dialog.Scale);
                value = dialog.SelectedDose * dialog.Scale;
                unit = dialog.Scale != 1.0 ? UnitType.ug : UnitType.mg;
            }

            _viewModel.Doc.PushMarker(ix, value, unit);
        }

        private void DisplayChart_PointerMoved(UserControl sender, ChannelPointerMovedEventArgs args)
        {
            //if (args.IsOverChannel)
            //{
            //    edCursor.Text = $"T= {args.X:0.0f} s {args.ChannelName}= {args.Y:0.0f} {args.ChannelUnit}";
            //}
            //else
            //{
            //    edCursor.Text = " ";
            //}
        }

        private void UpdateAnimalArguments()
        {
            DisplayChart.ChannelDefinitions.HR.YMax = AnimalArgs.MaxHR;
            DisplayChart.ChannelDefinitions.HR.YCal = AnimalArgs.MaxHR / 8;

            //rbNormalRat.Content += ' ' + AnimalArgs.Name.ToLower();
            //rbPithedRat.Content += ' ' + AnimalArgs.Name.ToLower();

            _rat.MeanHR = AnimalArgs.MeanHR;
        }

        private void CatButton_Click(object sender, RoutedEventArgs e)
        {
            AnimalArgs = new AnimalArguments
            {
                PulseDelay = 32,
                Name = "Cat",
                MaxHR = 400,
                MeanHR = (170 + 120) / 2,
            };
            UpdateAnimalArguments();
            if (sender != null)
                bStart_Click(sender, e);
        }

        private void RabbitButton_Click(object sender, RoutedEventArgs e)
        {
            AnimalArgs = new AnimalArguments
            {
                PulseDelay = 40,
                Name = "Rabbit",
                MaxHR = 400,
                MeanHR = (150 + 120) / 2,
            };
            UpdateAnimalArguments();
            if (sender != null)
                bStart_Click(sender, e);
        }

        private void RatButton_Click(object sender, RoutedEventArgs e)
        {
            AnimalArgs = new AnimalArguments
            {
                PulseDelay = 16,
                Name = "Rat",
                MaxHR = 800,
                MeanHR = (310 + 240) / 2,
            };
            UpdateAnimalArguments();
            if (sender != null)
                bStart_Click(sender, e);
        }

        private void DogButton_Click(object sender, RoutedEventArgs e)
        {
            AnimalArgs = new AnimalArguments
            {
                PulseDelay = 24,
                Name = "Dog",
                MaxHR = 200,
                MeanHR = (120 + 70) / 2,
            };
            UpdateAnimalArguments();
            if (sender != null)
                bStart_Click(sender, e);
        }

        bool IsMaximized(FrameworkElement element)
        {
            return Grid.GetColumnSpan(element) == 2;
        }

        void ToggleMaximized(FrameworkElement element, int row, int col)
        {
            if (IsMaximized(element))
            {
                Canvas.SetZIndex(element, 1);
                Grid.SetRow(element, row);
                Grid.SetRowSpan(element, 1);
                Grid.SetColumn(element, col);
                Grid.SetColumnSpan(element, 1);
            }
            else
            {
                Canvas.SetZIndex(element, 100);
                Grid.SetRow(element, 1);
                Grid.SetRowSpan(element, 2);
                Grid.SetColumn(element, 0);
                Grid.SetColumnSpan(element, 2);
            }
        }

        private void AnimalSelector_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ToggleMaximized(AnimalSelector, 1, 0);
        }

        private void Image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ToggleMaximized(AnimalImage, 2, 0);
        }

        private void DisplayChart_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ToggleMaximized(DisplayChart, 1, 1);
        }

        private void DrugMenu_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ToggleMaximized(DrugMenu, 2, 1);
        }
    }
}