using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace toxTree.core
{
    public interface IDecisionCategories : ISerializable, IList<IDecisionCategory>
    {
        IDecisionCategory getCategory(IDecisionCategory key);

        void addCategory(IDecisionCategory category);

        bool isMultilabel();

        void setMultilabel(bool multilabel);

        void setSelected(IDecisionCategory category);

        void selectAll(bool selected);

        int size();

        void clear();
    }
}
