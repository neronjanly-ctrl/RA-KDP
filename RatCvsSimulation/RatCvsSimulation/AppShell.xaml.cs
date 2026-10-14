using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml.Automation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using RatCvsSimulation.Controls;
using RatCvsSimulation.Views;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.Graphics.Display;

namespace RatCvsSimulation
{
    /// <summary>
    /// The "chrome" layer of the app that provides top-level navigation with
    /// proper keyboarding navigation.
    /// 此外壳模块，仅处理菜单导航（支持键盘/鼠标/触摸混合操作）、后退逻辑，不处理具体业务逻辑。
    /// </summary>
    public sealed partial class AppShell : Page
    {
        private bool isPaddingAdded = false;

        // Declare the top level nav items
        private readonly List<ActionMenuItem> navlist;
        public static AppShell Current = null;

        /// <summary>
        /// Initializes a new instance of the AppShell, sets the static 'Current' reference,
        /// adds callbacks for Back requests and changes in the SplitView's DisplayMode, and
        /// provide the nav menu list with the data to display.
        /// </summary>
        public AppShell()
        {
            this.InitializeComponent();

            navlist = new List<ActionMenuItem>(
                new[]
                {
                    //new ActionMenuItem()
                    //{
                    //    Symbol = Symbol.Home,
                    //    Label = "Home",
                    //    Command = GoHome,
                    //},
                    new ActionMenuItem()
                    {
                        SymbolAsChar = '\xE109',
                        Label = "New experiment",
                        Command = NewRat,
                    },
                    new ActionMenuItem()
                    {
                        Symbol = Symbol.OpenFile,
                        Label = "Load experiment...",
                        Command = LoadRat,
                    },
                    new ActionMenuItem()
                    {
                        Symbol = Symbol.Save,
                        Label = "Save experiment...",
                        Command = SaveRat,
                    },
                    new ActionMenuItem()
                    {
                        SymbolAsChar = '\xE2F6',
                        Label = "Print",
                        Command = Print,
                    },
                    new ActionMenuItem()
                    {
                        Symbol = Symbol.Repair,
                        Label = "Print setup",
                        Command = PrintSetup,
                    },
                });

            this.Loaded += (sender, args) =>
            {
                Current = this;

                this.CheckTogglePaneButtonSizeChanged();

                var titleBar2 = CoreApplication.GetCurrentView().TitleBar;
                titleBar2.IsVisibleChanged += TitleBar_IsVisibleChanged;
            };

            this.RootSplitView.RegisterPropertyChangedCallback(
                SplitView.DisplayModeProperty,
                (s, a) =>
                {
                    // Ensure that we update the reported size of the TogglePaneButton when the SplitView's
                    // DisplayMode changes.
                    this.CheckTogglePaneButtonSizeChanged();
                });

            SystemNavigationManager.GetForCurrentView().BackRequested += SystemNavigationManager_BackRequested;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;


            NavMenuList.ItemsSource = navlist;
        }

        public Frame AppFrame { get { return this.frame; } }

        /// <summary>
        /// Invoked when window title bar visibility changes, such as after loading or in tablet mode
        /// Ensures correct padding at window top, between title bar and app content
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void TitleBar_IsVisibleChanged(Windows.ApplicationModel.Core.CoreApplicationViewTitleBar sender, object args)
        {
            if (!this.isPaddingAdded && sender.IsVisible)
            {
                //add extra padding between window title bar and app content
                double extraPadding = (Double)App.Current.Resources["DesktopWindowTopPadding"];
                this.isPaddingAdded = true;

                Thickness margin = NavMenuList.Margin;
                NavMenuList.Margin = new Thickness(margin.Left, margin.Top + extraPadding, margin.Right, margin.Bottom);
                margin = frame.Margin;
                frame.Margin = new Thickness(margin.Left, margin.Top + extraPadding, margin.Right, margin.Bottom);
                margin = TogglePaneButton.Margin;
                TogglePaneButton.Margin = new Thickness(margin.Left, margin.Top + extraPadding, margin.Right, margin.Bottom);
            }
        }

        /// <summary>
        /// Default keyboard focus movement for any unhandled keyboarding
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppShell_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            FocusNavigationDirection direction = FocusNavigationDirection.None;
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Left:
                case Windows.System.VirtualKey.GamepadDPadLeft:
                case Windows.System.VirtualKey.GamepadLeftThumbstickLeft:
                case Windows.System.VirtualKey.NavigationLeft:
                    direction = FocusNavigationDirection.Left;
                    break;
                case Windows.System.VirtualKey.Right:
                case Windows.System.VirtualKey.GamepadDPadRight:
                case Windows.System.VirtualKey.GamepadLeftThumbstickRight:
                case Windows.System.VirtualKey.NavigationRight:
                    direction = FocusNavigationDirection.Right;
                    break;

                case Windows.System.VirtualKey.Up:
                case Windows.System.VirtualKey.GamepadDPadUp:
                case Windows.System.VirtualKey.GamepadLeftThumbstickUp:
                case Windows.System.VirtualKey.NavigationUp:
                    direction = FocusNavigationDirection.Up;
                    break;

                case Windows.System.VirtualKey.Down:
                case Windows.System.VirtualKey.GamepadDPadDown:
                case Windows.System.VirtualKey.GamepadLeftThumbstickDown:
                case Windows.System.VirtualKey.NavigationDown:
                    direction = FocusNavigationDirection.Down;
                    break;
            }

            if (direction != FocusNavigationDirection.None)
            {
                var control = FocusManager.FindNextFocusableElement(direction) as Control;
                if (control != null)
                {
                    control.Focus(FocusState.Programmatic);
                    e.Handled = true;
                }
            }
        }

        #region BackRequested Handlers

        private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            bool handled = e.Handled;
            this.BackRequested(ref handled);
            e.Handled = handled;
        }

        private void BackRequested(ref bool handled)
        {
            // Get a hold of the current frame so that we can inspect the app back stack.

            if (this.AppFrame == null)
                return;

            // Check to see if this is the top-most page on the app back stack.
            if (this.AppFrame.CanGoBack && !handled)
            {
                // If not, set the event to handled and go back to the previous page in the app.
                handled = true;
                this.AppFrame.GoBack();
            }
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Navigate to the Page for the selected <paramref name="listViewItem"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="listViewItem"></param>
        private void NavMenuList_ItemInvoked(object sender, ListViewItem listViewItem)
        {
            var item = (ActionMenuItem)((NavMenuListView)sender).ItemFromContainer(listViewItem);

            item?.Command?.Invoke(item.Arguments);
        }

        /// <summary>
        /// Ensures the nav menu reflects reality when navigation is triggered outside of
        /// the nav menu buttons.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNavigatingToPage(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                //var item = (from p in this.navlist where p.Command == e.SourcePageType select p).SingleOrDefault();
                //if (item == null && this.AppFrame.BackStackDepth > 0)
                //{
                //    // In cases where a page drills into sub-pages then we'll highlight the most recent
                //    // navigation menu item that appears in the BackStack
                //    foreach (var entry in this.AppFrame.BackStack.Reverse())
                //    {
                //        item = (from p in this.navlist where p.Command == entry.SourcePageType select p).SingleOrDefault();
                //        if (item != null)
                //            break;
                //    }
                //}

                //var container = (ListViewItem)NavMenuList.ContainerFromItem(item);

                //// While updating the selection state of the item prevent it from taking keyboard focus.  If a
                //// user is invoking the back button via the keyboard causing the selected nav menu item to change
                //// then focus will remain on the back button.
                //if (container != null) container.IsTabStop = false;
                //NavMenuList.SetSelectedItem(container);
                //if (container != null) container.IsTabStop = true;
            }
        }

        private void OnNavigatedToPage(object sender, NavigationEventArgs e)
        {
            // After a successful navigation set keyboard focus to the loaded page
            if (e.Content is Page && e.Content != null)
            {
                var control = (Page)e.Content;
                control.Loaded += Page_Loaded;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ((Page)sender).Focus(FocusState.Programmatic);
            ((Page)sender).Loaded -= Page_Loaded;
        }

        #endregion

        public Rect TogglePaneButtonRect
        {
            get;
            private set;
        }

        /// <summary>
        /// An event to notify listeners when the hamburger button may occlude other content in the app.
        /// The custom "PageHeader" user control is using this.
        /// </summary>
        public event TypedEventHandler<AppShell, Rect> TogglePaneButtonRectChanged;

        /// <summary>
        /// Public method to allow pages to open SplitView's pane.
        /// Used for custom app shortcuts like navigating left from page's left-most item
        /// </summary>
        public void OpenNavePane()
        {
            TogglePaneButton.IsChecked = true;
            AdjustPane(true);
        }

        void AdjustPane(bool open)
        {
            var dividers = new[] { NavPaneDivider, NavPaneDivider2, NavPaneDivider3 };

            foreach (var divider in dividers)
            {
                var marginH = open ? 16 : 4;
                var marginV = 6;
                divider.Margin = new Thickness(marginH, marginV, marginH, marginV);
                divider.Width = open ? double.NaN : 40.0;
                divider.HorizontalAlignment = open ? HorizontalAlignment.Stretch : HorizontalAlignment.Left;
            }

            Grid.SetColumn(SettingsNavPaneButton, open ? 2 : 0);
        }

        /// <summary>
        /// Hides divider when nav pane is closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void RootSplitView_PaneClosed(SplitView sender, object args)
        {
            AdjustPane(false);
        }

        /// <summary>
        /// Callback when the SplitView's Pane is toggled closed.  When the Pane is not visible
        /// then the floating hamburger may be occluding other content in the app unless it is aware.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TogglePaneButton_Unchecked(object sender, RoutedEventArgs e)
        {
            this.CheckTogglePaneButtonSizeChanged();
        }

        /// <summary>
        /// Callback when the SplitView's Pane is toggled opened.
        /// Restores divider's visibility and ensures that margins around the floating hamburger are correctly set.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TogglePaneButton_Checked(object sender, RoutedEventArgs e)
        {
            AdjustPane(true);
            this.CheckTogglePaneButtonSizeChanged();
        }

        /// <summary>
        /// Check for the conditions where the navigation pane does not occupy the space under the floating
        /// hamburger button and trigger the event.
        /// </summary>
        private void CheckTogglePaneButtonSizeChanged()
        {
            if (this.RootSplitView.DisplayMode == SplitViewDisplayMode.Inline ||
                this.RootSplitView.DisplayMode == SplitViewDisplayMode.Overlay)
            {
                var transform = this.TogglePaneButton.TransformToVisual(this);
                var rect = transform.TransformBounds(new Rect(0, 0, this.TogglePaneButton.ActualWidth, this.TogglePaneButton.ActualHeight));
                this.TogglePaneButtonRect = rect;
            }
            else
            {
                this.TogglePaneButtonRect = new Rect();
            }

            var handler = this.TogglePaneButtonRectChanged;
            if (handler != null)
            {
                // handler(this, this.TogglePaneButtonRect);
                handler.DynamicInvoke(this, this.TogglePaneButtonRect);
            }
        }

        /// <summary>
        /// Enable accessibility on each nav menu item by setting the AutomationProperties.Name on each container
        /// using the associated Label of each item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ActionMenuItemContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (!args.InRecycleQueue && args.Item != null && args.Item is ActionMenuItem)
            {
                //args.ItemContainer.SetValue(AutomationProperties.NameProperty, ((ActionMenuItem)args.Item).Label);
            }
            else
            {
                args.ItemContainer.ClearValue(AutomationProperties.NameProperty);
            }
        }

        public MainPage MainPage => this.AppFrame.Content as MainPage;

        private async void NewRat(object argument)
        {
            var dialog = new MessageDialog("New Experiment! Are you sure?", "Confirm");

            dialog.Commands.Add(new UICommand("Yes", async o => await MainPage.NewRat()));
            dialog.Commands.Add(new UICommand("No"));

            // Set the command that will be invoked by default
            dialog.DefaultCommandIndex = 0;

            // Set the command to be invoked when escape is pressed
            dialog.CancelCommandIndex = 1;

            await dialog.ShowAsync();
        }

        private async void LoadRat(object argument)
        {
            //int TempHandle;
            //int i, nBufs, FileLength, nw;

            //if (OpenDialog.execute)
            //{
            //    if (FileExists(OpenDialog.FileName))
            //    {
            //        Caption = ProgName + " : " + OpenDialog.FileName;
            //        FileName = OpenDialog.FileName;

            //        MainPage.LoadRat(FileName);
            //    }
            //}
        }

        private async void SaveRat(object argument)
        {
            //int TempHandle;
            //int i, nw;

            //if (SaveDialog.execute)
            //{
            //    TempHandle = FileCreate(SaveDialog.FileName);
            //    FileName = SaveDialog.FileName;
            //    MainPage.SaveRat(FileName);
            //    Caption = ProgName + " : " + SaveDialog.FileName;
            //}
        }

        private async void Print(object argument)
        {
            await new MessageDialog("print").ShowAsync();
        }

        private async void PrintSetup(object argument)
        {
            await new MessageDialog("print setup").ShowAsync();
        }

        private void GoHome(object argument)
        {
            frame.Navigate(typeof(MainPage));
        }

        private async void SettingsNavPaneButton_Click(object sender, RoutedEventArgs e)
        {
            await MainPage.Settings();
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var listview = e.OriginalSource as ListView;
            MainPage.ShowMenu(listview.Items.IndexOf(e.ClickedItem));
        }

        private void RatVisToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            MainPage.ShowVisualization(true);
        }

        private void RatVisToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            MainPage.ShowVisualization(false);
        }
    }
}
