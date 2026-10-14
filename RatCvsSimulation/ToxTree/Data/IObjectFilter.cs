using NCDK;
using System;
using System.Collections;
//using JComponent = javax.swing.JComponent;

namespace toxTree.data
{
    public interface IObjectFilter : IComparable
    {
        void preprocess(IChemObject @object);

        bool accept(IChemObject @object);

        void setProperty(String key, Object value);

        Object getProperty(String key);

        Object removeProperty(String key);

        Hashtable Properties { get; set; }

        //JComponent getEditor();
    }
}
