using System;
using PropertyChangeListener = java.beans.PropertyChangeListener;
using PropertyChangeSupport = java.beans.PropertyChangeSupport;
using Observable = java.util.Observable;
using DescriptorSpecification = org.openscience.cdk.qsar.DescriptorSpecification;
using IDecisionResult = toxTree.core.IDecisionResult;
using DecisionMethodException = toxTree.exceptions.DecisionMethodException;
using DecisionResultException = toxTree.exceptions.DecisionResultException;
using CategoriesList = toxTree.tree.CategoriesList;
using DecisionNodesList = toxTree.tree.DecisionNodesList;
using UserDefinedTree = toxTree.tree.UserDefinedTree;

namespace toxTree.tree.cramer
{
    public class CramerRules : UserDefinedTree
    {
        private const long serialVersionUID = 2709354449315583323L;

        protected bool residuesIDVisible;

        [NonSerialized]
        public readonly static String[] c_rules = { "toxTree.tree.cramer.RuleNormalBodyConstituent", "toxTree.tree.cramer.RuleToxicFunctionalGroups", "toxTree.tree.cramer.RuleHasOtherThanC_H_O_N_S2", "toxTree.tree.cramer.RuleHasOnlySaltSulphonateSulphate", "toxTree.tree.cramer.RuleSimplyBranchedAliphaticHydrocarbon", "toxTree.tree.cramer.RuleSomeBenzeneDerivatives", "toxTree.tree.cramer.RuleIsHeterocyclic", "toxTree.tree.cramer.RuleLactoneOrCyclicDiester", "toxTree.tree.cramer.RuleLactonesFusedOrUnsaturated", "toxTree.tree.cramer.Rule3MemberedHeterocycle", "toxTree.tree.cramer.RuleHeterocyclicComplexSubstituents", "toxTree.tree.cramer.RuleHeteroaromatic", "toxTree.tree.cramer.RuleRingWithSubstituents", "toxTree.tree.cramer.RuleManyAromaticRings14", "toxTree.tree.cramer.RuleReadilyHydrolysedMononuclear", "toxTree.tree.cramer.RuleIsCommonTerpene", "toxTree.tree.cramer.RuleReadilyHydrolysedToCommonterpene", "toxTree.tree.cramer.RuleKetoneAlcoholEtc", "toxTree.tree.cramer.RuleIsOpenChain", "toxTree.tree.cramer.RuleAliphaticWithSomeFuncGroups", "toxTree.tree.cramer.Rule3FuncGroups", "toxTree.tree.cramer.RuleCommonComponentOfFood", "toxTree.tree.cramer.RuleIsAromatic", "toxTree.tree.cramer.RuleMonocarbocyclic", "toxTree.tree.cramer.RuleCyclopropaneEtc", "toxTree.tree.cramer.RuleMonocycloalkanoneEtc", "toxTree.tree.cramer.RuleRingsWithSubstituents", "toxTree.tree.cramer.RuleManyAromaticRings28", "toxTree.tree.cramer.RuleReadilyHydrolysed29", "toxTree.tree.cramer.RuleRingComplexSubstituents30", "toxTree.tree.cramer.RuleAcyclicAcetalEsterOfQ30", "toxTree.tree.cramer.Rule32", "toxTree.tree.cramer.RuleSufficientSulphonateGroups", "toxTree.tree.rules.RuleOpenChain", "toxTree.tree.rules.RuleHeterocyclic", "toxTree.tree.rules.RuleAromatic", "toxTree.tree.rules.RuleHeterocyclic", "toxTree.tree.rules.RuleCommonTerpene", "toxTree.tree.rules.RuleAromatic" };

        [NonSerialized]
        private const int[][] c_transitions = { { 2, 0, 0, 1 }, { 3, 0, 0, 3 }, { 5, 4, 0, 0 }, { 0, 7, 3, 0 }, { 6, 0, 0, 1 }, { 7, 0, 0, 3 }, { 16, 8, 0, 0 }, { 10, 9, 0, 0 }, { 34, 0, 0, 3 }, { 11, 0, 0, 3 }, { 12, 33, 0, 0 }, { 22, 13, 0, 0 }, { 0, 14, 3, 0 }, { 22, 15, 0, 0 }, { 33, 37, 0, 0 }, { 17, 0, 0, 1 }, { 19, 38, 0, 0 }, { 0, 0, 1, 2 }, { 23, 20, 0, 0 }, { 22, 21, 0, 0 }, { 18, 0, 0, 3 }, { 33, 0, 0, 2 }, { 24, 27, 0, 0 }, { 25, 18, 0, 0 }, { 26, 0, 0, 2 }, { 22, 0, 0, 2 }, { 0, 28, 3, 0 }, { 30, 29, 0, 0 }, { 33, 36, 0, 0 }, { 39, 31, 0, 0 }, { 32, 39, 0, 0 }, { 22, 0, 0, 2 }, { 0, 0, 3, 1 }, { 35, 20, 0, 0 }, { 23, 10, 0, 0 }, { 19, 30, 0, 0 }, { 16, 22, 0, 0 }, { 19, 18, 0, 0 }, { 19, 18, 0, 0 } };

        [NonSerialized]
        private readonly static String[] c_categories = { "toxTree.tree.cramer.CramerClass1", "toxTree.tree.cramer.CramerClass2", "toxTree.tree.cramer.CramerClass3" };

        public CramerRules()
            : base(new CategoriesList(c_categories), null)
        {
            rules = new DecisionNodesList(categories, c_rules, c_transitions);
            if (rules is Observable)
                ((Observable)rules).addObserver(this);
            setTitle("Cramer rules");
            setChanged();
            notifyObservers();
            setPriority(1);
        }

        public DescriptorSpecification getSpecification()
        {
            return new DescriptorSpecification("http://toxtree.sourceforge.net/cramer.html", getTitle(), this.getClass().getName(), "Toxtree plugin");
        }

        public override void addPropertyChangeListener(PropertyChangeListener l)
        {
            if (changes == null)
                changes = new PropertyChangeSupport(this);
            changes.addPropertyChangeListener(l);
            for (int i = 0; i < rules.size(); i++)
                if (rules.getRule(i) != null)
                    rules.getRule(i).addPropertyChangeListener(l);
        }

        public override void removePropertyChangeListener(PropertyChangeListener l)
        {
            if (changes == null)
            {
                changes.removePropertyChangeListener(l);
                for (int i = 0; i < rules.size(); i++)
                    if (rules.getRule(i) != null)
                        rules.getRule(i).removePropertyChangeListener(l);
            }
        }

        public override String toString()
        {
            return getName();
        }

        public String getName()
        {
            return name;
        }

        public void setName(String value)
        {
            name = value;
        }

        public override StringBuffer explainRules(IDecisionResult result, bool verbose)
        {
            try
            {
                StringBuffer b = result.explain(verbose);
                return b;
            }
            catch (DecisionResultException x)
            {
                throw new DecisionMethodException(x);
            }
        }

        public override IDecisionResult createDecisionResult()
        {
            IDecisionResult result = new CramerTreeResult();
            result.setDecisionMethod(this);
            return result;
        }

        public bool isResiduesIDVisible()
        {
            return residuesIDVisible;
        }

        public void setResiduesIDVisible(bool residuesIDVisible)
        {
            this.residuesIDVisible = residuesIDVisible;
            for (int i = 0; i < rules.size(); i++)
            {
                rules.getRule(i).hideResiduesID(!residuesIDVisible);
            }
        }

        public override void setEditable(bool value)
        {
            editable = value;
            for (int i = 0; i < rules.size(); i++)
                rules.getRule(i).setEditable(value);
        }
    }
}
