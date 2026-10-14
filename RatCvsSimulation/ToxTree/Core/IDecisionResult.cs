using System;
//using Property = ambit2.@base.data.Property;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using NCDK;
using System.ComponentModel;
using toxTree.data;
using toxTree.tree;

namespace toxTree.core
{
    public interface IDecisionResult : IComparable, ISerializable, IProgressStatus, IDecisionMethodPrority
    {
        String toString();

        StringBuilder explain(bool verbose);

        bool classify(IAtomContainer mol);

        IDecisionCategories getAssignedCategories();

        IDecisionCategory getCategory();

        IDecisionCategory getCategory(int index);

        void setCategory(IDecisionCategory classID);

        IAtomContainer getMolecule(int index);

        void setMolecule(IAtomContainer molecule);

        void setSilent(bool silent);

        IDecisionRule getRule(int index);

        RuleResult getRuleResult(int index);

        void addRuleResult(IDecisionRule rule, bool value, IAtomContainer molecule);

        int getRuleResultsCount();

        void setDecisionMethod(IDecisionMethod method);

        IDecisionMethod getDecisionMethod();

        void clear();

        void addPropertyChangeListener(PropertyChangedEventHandler l);

        void removePropertyChangeListener(PropertyChangedEventHandler l);

        PropertyChangedEventHandler[] getPropertyChangeListeners();

        void assignResult(IAtomContainer mol);

        String[] getResultPropertyNames();

        //List<Property> getResultProperties();

        List<CategoryFilter> getFilters();

        void setNotify(bool value);

        void hilightAlert(IDecisionRule rule);

        void hilightAlert(RuleResult ruleresult);

        void setWeb(Boolean web);
    }
}
