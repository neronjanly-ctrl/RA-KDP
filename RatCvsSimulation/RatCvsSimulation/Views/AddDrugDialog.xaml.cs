using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class AddDrugDialog : ContentDialog
    {
        public float MinDose;
        public float MaxDose;
        public float SelectedDose;
        public string Units;
        public float Scale;

        public object Global { get; private set; }

        public AddDrugDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var s = cbDose.SelectedItem.ToString();
            SelectedDose = float.Parse(s.Substring(0, s.Length - 6)) / Scale;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            float Dose;

            //Left = MainForm.Left + 50;
            //Top = MainForm.Top + 50;

            if (MaxDose <= 1.0)
            {
                Scale = 1000.0f;
                Units = "ug/kg";
            }
            else
            {
                Scale = 1.0f;
                Units = "mg/kg";
            }

            Dose = MinDose;
            cbDose.Items.Clear();

            while (Dose <= MaxDose)
            {
                if (Dose <= MaxDose)
                    cbDose.Items.Add(string.Format(" {0:0.00} {1} ", Dose * Scale, Units));

                if (2.0 * Dose <= MaxDose)
                    cbDose.Items.Add(string.Format(" {0:0.0} {1} ", 2.0 * Dose * Scale, Units));

                if (5.0 * Dose <= MaxDose)
                    cbDose.Items.Add(string.Format(" {0:0.0} {1} ", 5.0 * Dose * Scale, Units));

                Dose = Dose * 10.0f;
            }

            cbDose.SelectedIndex = 0;
        }
    }
}
