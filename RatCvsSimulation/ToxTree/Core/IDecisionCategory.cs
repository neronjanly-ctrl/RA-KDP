using System;
using System.Runtime.Serialization;
//using ICategory = ambit2.@base.interfaces.ICategory;

namespace toxTree.core
{
    public interface IDecisionCategory : ISerializable, IComparable<IDecisionCategory>//, ICategory
    {
        int getID();

        void setID(int id);

        String getName();

        void setName(String name);

        void setExplanation(String name);

        String getExplanation();

        String getThreshold();

        void setThreshold(String threshold);

        //IDecisionCategoryEditor getEditor();

        bool isSelected();

        void setSelected(bool value);
    }
}
