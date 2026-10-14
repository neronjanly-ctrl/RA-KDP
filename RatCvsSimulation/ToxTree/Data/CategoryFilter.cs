using System;
using toxTree.core;

namespace toxTree.data
{
    public class CategoryFilter : ObjectPropertyFilter
    {
        public CategoryFilter(Object property, IDecisionCategory category)
            : base(property, category.ToString())
        {
        }

        public CategoryFilter(Object property, String category)
            : base(property, category)
        {
        }

        public override String ToString()
        {
            return getTag() + " = " + getValue().ToString();
        }
    }
}
