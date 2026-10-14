using System;

namespace toxTree.core
{
    public interface IProcessRule
    {
        void init(IDecisionMethod method);

        void done();

        Object process(IDecisionMethod method, IDecisionRule rule);
    }
}
