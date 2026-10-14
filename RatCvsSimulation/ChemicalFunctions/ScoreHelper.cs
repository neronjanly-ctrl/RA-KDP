using System;

namespace ChemicalFunctions
{
    public static class ScoreHelper
    {
        public static float ToSybylScore(this float score)
        {
            // ΔGactual= RTlnKi  (R:gas constant R, 8.314 J/mol*K; T:absolute temperature, 273.15+37=310.15K, since the T for the binding was measured at 37 °C), comparing with the predicted ΔGpredicted (y-axis).
            return (float)-Math.Log10(Math.Exp(score * 4184 / 8.314 / 310.15));
        }

        public static float ToVinaScore(this float score)
        {
            return (float)(Math.Log(Math.Pow(10, -score)) * 310.15 * 8.314 / 4184);
        }
    }
}
