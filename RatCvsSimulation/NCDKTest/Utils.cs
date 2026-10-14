using NCDK;
using NCDK.Renderers.Fonts;
using NCDK.Renderers.Generators;
using NCDKTest.Renderers;
using System.Collections.Generic;

namespace NCDKTest
{
    public class Utils
    {
        public static AtomContainerRenderer BuildStdRenderer()
        {
            List<IGenerator<IAtomContainer>> generators =
                new List<IGenerator<IAtomContainer>>();
            //generators.Add(new BasicBondGenerator());
            generators.Add(new RingGenerator());
            //generators.Add(new BasicAtomGenerator());
            generators.Add(new ExtendedAtomGenerator());
            generators.Add(new BasicSceneGenerator());
            AtomContainerRenderer atr = new AtomContainerRenderer(generators, new WPFFontManager());
            atr.GetRenderer2DModel().SetFitToScreen(true);
            //atr.GetRenderer2DModel().SetZoomFactor(1);
            atr.GetRenderer2DModel().SetShowImplicitHydrogens(true);
            atr.GetRenderer2DModel().SetShowAromaticity(true);
            atr.GetRenderer2DModel().SetBondWidth(3.5);
            atr.GetRenderer2DModel().SetBondDistance(5);
            atr.GetRenderer2DModel().SetFontManager(FontStyles.Bold);

            return atr;
        }
    }
}
