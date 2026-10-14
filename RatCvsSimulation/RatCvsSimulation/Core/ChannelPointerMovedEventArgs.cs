using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatCvsSimulation.Core
{
    public class ChannelPointerMovedEventArgs
    {
        public ChannelPointerMovedEventArgs()
        {
        }

        public ChannelPointerMovedEventArgs(ChannelDefinition channel, float x, float y)
        {
            IsOverChannel = true;
            Channel = channel;
            X = x;
            Y = y;
        }

        public bool IsOverChannel { get; }
        public ChannelDefinition Channel { get; }
        public float X { get; }
        public float Y { get; }
        public string ChannelName => Channel.ADCName;
        public string ChannelUnit => Channel.ADCUnits;
    }
}
