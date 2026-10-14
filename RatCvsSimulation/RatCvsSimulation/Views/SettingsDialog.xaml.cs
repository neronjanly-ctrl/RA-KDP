using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace RatCvsSimulation.Views
{
    public sealed partial class SettingsDialog : ContentDialog
    {
        //public TColorDialog ColorDialog1;
        public Color ABPColor { get { return (btnABP.Background as SolidColorBrush).Color; } set { btnABP.Background = new SolidColorBrush(value); } }
        public Color LVPColor { get { return (btnLVP.Background as SolidColorBrush).Color; } set { btnLVP.Background = new SolidColorBrush(value); } }
        public Color CVPColor { get { return (btnCVP.Background as SolidColorBrush).Color; } set { btnCVP.Background = new SolidColorBrush(value); } }
        public Color HCFColor { get { return (btnHCF.Background as SolidColorBrush).Color; } set { btnHCF.Background = new SolidColorBrush(value); } }
        public Color HRColor { get { return (btnHR.Background as SolidColorBrush).Color; } set { btnHR.Background = new SolidColorBrush(value); } }
        public bool ABPInUse { get { return ckABPShow.IsChecked ?? false; } set { ckABPShow.IsChecked = value; } }
        public bool LVPInUse { get { return ckLVPShow.IsChecked ?? false; } set { ckLVPShow.IsChecked = value; } }
        public bool CVPInUse { get { return ckCVPShow.IsChecked ?? false; } set { ckCVPShow.IsChecked = value; } }
        public bool HCFInUse { get { return ckHCFShow.IsChecked ?? false; } set { ckHCFShow.IsChecked = value; } }
        public bool HRInUse { get { return ckHRShow.IsChecked ?? false; } set { ckHRShow.IsChecked = value; } }

        public SettingsDialog()
        {
            this.InitializeComponent();
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            //if (ColorDialog1.execute)
            //    TShape(Sender).brush.color = ColorDialog1.color;
        }
    }
}
