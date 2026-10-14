namespace RatCvsSimulation.Model
{
    public class Element
    {
        public float Conductance;
        public float Elastance;
        public float Pressure;
        public float MaxPressure;
        public float MinPressure;
        public float MeanPressure;
        public float PressureLo;
        public float PressureHi;
        public float TimeLo;
        public float TimeHi;
        public float Volume;
        public float Volume0;
        public float Flow;
        public float Rate;
        public float TimeSystole;
        public float NextBeat;
        public float Time;
        public float Force;

        public void CalculatePressureRange(float time)
        {
            if (Pressure <= PressureLo)
            {
                PressureLo = Pressure;
                TimeLo = time;
            }

            if (Pressure >= PressureHi)
            {
                PressureHi = Pressure;
                TimeHi = time;
            }
        }

        public void InitialisePressureRange()
        {
            PressureHi = -1E38f;
            PressureLo = 1E38f;
        }

        public float GetLeadingPressure()
        {
            float result;

            result = TimeLo <= TimeHi ? PressureLo : PressureHi;

            return result;
        }

        public float GetTrailingPressure()
        {
            float result;

            result = TimeHi >= TimeLo ? PressureHi : PressureLo;

            return result;
        }
    }
}