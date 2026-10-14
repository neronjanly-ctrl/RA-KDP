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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RatCvsSimulation.Controls
{
    public sealed partial class DepictionImage : UserControl
    {
        private static IChemObjectBuilder builder = ChemObjectBuilder.Instance;
        private static SmilesParser parser = new SmilesParser(builder);

        string _smiles = null;
        public string Smiles
        {
            get => _smiles;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    this.mol = null;
                    return;
                }

                _smiles = value;
                IAtomContainer mol = null;
                try
                {
                    mol = parser.ParseSmiles(Smiles);
                }
                catch (Exception)
                {
                }
                if (mol == null)
                {
                    return;
                }

                mol = Layout(sdg, mol);

                this.mol = mol;
            }
        }

        public RendererModel Model { get; private set; }
        private IGenerator<IAtomContainer> molgen;
        private WPFFontManager fontManager = new WPFFontManager();
        private IAtomContainer mol = null;
        private StructureDiagramGenerator sdg = new StructureDiagramGenerator();

        private static FormatFactory formatFactory = new FormatFactory();
        private static ReaderFactory readerFactory = new ReaderFactory();

        class MolWindowListener : NCDK.Events.ICDKChangeListener
        {
            DepictionImage parent;
            public MolWindowListener(DepictionImage parent)
            {
                this.parent = parent;
            }

            public void StateChanged(ChemObjectChangeEventArgs evt)
            {
                parent.canvas.Invalidate();
            }
        }

        public DepictionImage()
        {
            this.InitializeComponent();

            sdg.BondLength = 40;
            sdg.UseIdentityTemplates = false;
            Model = new RendererModel();
            molgen = new BasicGenerator();
            Model.RegisterParameters(molgen);
            Model.Listeners.Add(new MolWindowListener(this));
        }

        public static IAtomContainer Layout(StructureDiagramGenerator structureGenerator, IAtomContainer mol)
        {
            structureGenerator.Molecule = mol;
            structureGenerator.GenerateCoordinates();
            mol = structureGenerator.Molecule;

            return mol;
        }

        private void DrawRendeingrElement(CanvasDrawingSession g2d, NCDK.Renderers.Elements.IRenderingElement rendeingrElement, Rect bounds)
        {
            var v = new WPFDrawVisitor(g2d);
            var t = Matrix3x2.CreateTranslation((canvas.ActualWidth - bounds.Width) / 2 - bounds.Left, (canvas.ActualHeight - bounds.Height) / 2 - bounds.Top);
            v.SetTransform(t);
            v.SetFontManager(fontManager);
            v.SetRendererModel(Model);
            v.Visit(rendeingrElement);
        }

        private void canvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            if (mol == null)
                return;

            var rendeingrElement = molgen.Generate(mol, Model);
            var bounds = BoundsCalculator.CalculateBounds(mol);
            DrawRendeingrElement(args.DrawingSession, rendeingrElement, bounds);
        }
    }
}
