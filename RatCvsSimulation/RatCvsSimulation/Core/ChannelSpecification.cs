using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatCvsSimulation.Core
{
    public class ChannelSpecification<TChannel> : IEnumerable<TChannel>
    {
        /// <summary>
        /// 动脉血压(Arterial Blood Pressure)
        /// </summary>
        public TChannel ABP { get; set; }
        /// <summary>
        /// 左心室内压(Left Ventricular Pressure)
        /// </summary>
        public TChannel LVP { get; set; }
        /// <summary>
        /// 中心静脉压(Central Venous Pressure)
        /// </summary>
        public TChannel CVP { get; set; }
        /// <summary>
        /// 心脏收缩力(Heart Contractile Force)
        /// </summary>
        public TChannel HCF { get; set; }
        /// <summary>
        /// 心率(Heart Rate)
        /// </summary>
        public TChannel HR { get; set; }

        public TChannel First => ABP;

        public IEnumerator<TChannel> GetEnumerator()
        {
            yield return ABP;
            yield return LVP;
            yield return CVP;
            yield return HCF;
            yield return HR;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void SetData(params TChannel[] data)
        {
            ABP = data[0];
            LVP = data[1];
            CVP = data[2];
            HCF = data[3];
            HR = data[4];
        }

        public TChannel[] GetData()
        {
            return this.ToArray();
        }
    }
}
