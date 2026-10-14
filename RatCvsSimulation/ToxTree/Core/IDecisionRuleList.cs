using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace toxTree.core
{
    public interface IDecisionRuleList : IList<IDecisionRule>, ISerializable, ICloneable
    {
        void addRule(IDecisionRule rule);

        IDecisionRule getRule(int index);

        void setRules(String[] customRules);
    }
}
