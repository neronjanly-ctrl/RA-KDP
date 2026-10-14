using RatCvsSimulation.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace RatCvsSimulation.Extensions
{
    public static class CardiogramDataExts
    {
        public static void WriteCardiogramData(this DataWriter writer, CardiogramData data)
        {
            foreach (var f in data.GetData())
            {
                writer.WriteSingle(f);
            }
            writer.WriteInt32(data.MarkerIndex);
            writer.WriteSingle(data.MarkerValue);
            writer.WriteByte((byte)data.MarkerUnit);
        }

        public static CardiogramData ReadCardiogramData(this DataReader reader)
        {
            var cd = new CardiogramData();
            var data = new float[5];

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = reader.ReadSingle();
            }

            cd.SetData(data);

            cd.MarkerIndex = reader.ReadInt32();
            cd.MarkerValue = reader.ReadSingle();
            cd.MarkerUnit = (UnitType)reader.ReadByte();

            return cd;
        }
    }
}
