using RatCvsSimulation.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatCvsSimulation.Core
{
    public class CardiogramData : ChannelSpecification<float>
    {
        /// <summary>
        /// An index indicates a drug id
        /// </summary>
        public int MarkerIndex { get; set; }
        /// <summary>
        /// Dose injected
        /// </summary>
        public float MarkerValue { get; set; }
        /// <summary>
        /// Unit of the injected dose
        /// </summary>
        public UnitType MarkerUnit { get; set; }

        /// <summary>
        /// Check if the data point contains a marker
        /// </summary>
        public bool HasMarker => MarkerIndex != -1;

        // TODO: use IFormatProvider and ICustomFormatter to wrap string with unit
        static string[] MarkerUnits { get; } = new string[] { "mg/kg", "ug/kg", "" };
        // TODO: rearchitect drug accessing method
        public static Drug[] Drugs;

        public string MarkerText =>
            MarkerValue != 0f ?
                $"{Drugs[MarkerIndex].ShortName} {MarkerValue:g3} {MarkerUnits[(int)MarkerUnit]}" :
                Drugs[MarkerIndex].ShortName;

        public void SetData(ChannelSpecification<float> data, Marker marker)
        {
            this.ABP = data.ABP;
            this.LVP = data.LVP;
            this.CVP = data.CVP;
            this.HCF = data.HCF;
            this.HR = data.HR;
            this.MarkerIndex = marker.Index;
            this.MarkerValue = marker.Value;
            this.MarkerUnit = marker.Units;
        }
    }
}
