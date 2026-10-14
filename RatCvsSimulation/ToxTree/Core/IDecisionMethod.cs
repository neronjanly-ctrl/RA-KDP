using System;
//using IStructureDiagramHighlights = ambit2.core.data.IStructureDiagramHighlights;
using System.Text;
using System.ComponentModel;
using NCDK;
using System.Runtime.Serialization;

namespace toxTree.core
{
    public interface IDecisionMethod : ISerializable, ICloneable, IDecisionMethodPrority//, IStructureDiagramHighlights
    {
        String getTitle();

        void setTitle(String value);

        String getExplanation();

        void setExplanation(String value);

        IDecisionRule getRule(int id);

        IDecisionRule getRule(String name);

        IDecisionRule getTopRule();

        IDecisionRule getBranch(IDecisionRule rule, bool answer);

        IDecisionCategory getCategory(IDecisionRule rule, bool answer);

        IDecisionCategories getCategories();

        IDecisionRuleList getRules();

        void setDecisionRule(IDecisionRule rule);

        void addDecisionRule(IDecisionRule rule);

        StringBuilder explainRules(IDecisionResult result, bool verbose);

        bool verifyRules(IAtomContainer mol, IDecisionResult result);

        bool classify(IAtomContainer mol, IDecisionResult result);

        void walkRules(IDecisionRule rule, IProcessRule processor);

        IDecisionRuleList hasUnreachableRules();

        IDecisionCategories hasUnusedCategories();

        int getNumberOfRules();

        int getNumberOfClasses();

        void addPropertyChangeListener(PropertyChangedEventHandler l);

        void removePropertyChangeListener(PropertyChangedEventHandler l);

        IDecisionResult createDecisionResult();

        bool isEditable();

        void setEditable(bool value);

        bool isModified();

        void setModified(bool value);

        //IDecisionMethodEditor getEditor();

        void setParameters(IAtomContainer mol);

        void setWeb(Boolean web);

        bool isWeb();
    }
}
