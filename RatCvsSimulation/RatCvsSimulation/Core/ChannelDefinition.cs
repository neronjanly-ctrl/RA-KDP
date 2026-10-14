using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace RatCvsSimulation.Core
{
    public class ChannelDefinition
    {
        public float XMin;
        public float XMax;
        public float YMin;
        public float YMax;
        public float YCal;
        //public float TimeZero;
        public string ADCUnits;
        public string ADCName;
        //public int ZeroIndex;
        //public float CursorTime;
        //public float CursorValue;
        public Color Color = Colors.Blue;
        //public float Value;
        public bool InUse = true;

        /// <summary>
        /// Callback function to get value of a specific channel on a cardiogram data
        /// </summary>
        public Func<CardiogramData, float> GetValue;
    }
}
