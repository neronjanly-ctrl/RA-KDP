using System;
using IAtomContainer = org.openscience.cdk.interfaces.IAtomContainer;
using IDecisionRule = toxTree.core.IDecisionRule;
using DecisionResultException = toxTree.exceptions.DecisionResultException;
using ProgressStatus = toxTree.tree.ProgressStatus;
using TreeResult = toxTree.tree.TreeResult;

namespace toxTree.tree.cramer
{
    public class CramerTreeResult : TreeResult
    {
        private const long serialVersionUID = -7883635712321700678L;

        public CramerTreeResult()
            : base()
        {
        }

        public override void addRuleResult(IDecisionRule rule, bool value, IAtomContainer molecule)
        {
            base.addRuleResult(rule, value, molecule);
            if (rule.getNum() > 33)
                setSilent(true);
        }

        public override void assignResult(IAtomContainer mol)
        {
            if (mol == null)
                return;
            if (getCategory() != null)
                mol.setProperty(getResultPropertyNames()[0], getCategory().toString());
            mol.setProperty(getClass().getName(), explain(false).toString());
            firePropertyChangeEvent(ProgressStatus._pRuleResult, null, status);
        }

        public override String[] getResultPropertyNames()
        {
            return new String[]
            {
                "Cramer rules",
            };
        }
    }
}
