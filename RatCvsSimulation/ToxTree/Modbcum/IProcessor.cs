using System;
using System.Runtime.Serialization;

namespace net.idea.modbcum.i.processors
{
    public interface IProcessor<Target, Result> : ISerializable
    {
        Result process(Target target);

        bool isEnabled();

        void setEnabled(bool value);

        long getID();

        void close();

        void open();
    }
}
