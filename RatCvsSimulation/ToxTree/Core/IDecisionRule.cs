using System;
using NCDK.FaulonSignatures.Chemistry;
using NCDK;
using NCDK.Renderers.Selection;
using System.ComponentModel;
using System.Runtime.Serialization;
using net.idea.modbcum.i.processors;

namespace toxTree.core
{
    public interface IDecisionRule : ISerializable, ICloneable
    {
        String getID();

        void setID(String id);

        int getNum();

        void setNum(int no);

        String getTitle();

        void setTitle(String name);

        String getExplanation();

        void setExplanation(String message);

        Molecule getExampleMolecule(bool ruleResult);

        void setExampleMolecule(IAtomContainer mol, bool ruleResult);

        void clearFlags(IAtomContainer mol);

        bool verifyRule(IAtomContainer mol);

        String toString();

        void addPropertyChangeListener(PropertyChangedEventHandler l);

        void removePropertyChangeListener(PropertyChangedEventHandler l);

        bool isImplemented();

        void hideResiduesID(bool hide);

        bool isResidueIDHidden();

        //IDecisionRuleEditor getEditor();

        Object clone();

        bool isEditable();

        void setEditable(bool value);

        IProcessor<IAtomContainer, IChemObjectSelection> getSelector();
    }
}
