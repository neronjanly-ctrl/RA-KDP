using System;
using NCDK;
using System.Collections;

namespace toxTree.data
{
    public abstract class AbstractFilter : IObjectFilter
    {
        protected Hashtable parameters;

        public AbstractFilter()
            : base()
        {
            parameters = new Hashtable();
        }

        public void preprocess(IChemObject @object)
        {
        }

        public virtual void setProperty(String key, Object value)
        {
            parameters[key] = value;
        }

        public Object getProperty(String key)
        {
            return parameters[key];
        }

        public Object removeProperty(String key)
        {
            var o = parameters[key];
            parameters.Remove(key);
            return o;
        }

        public Hashtable Properties
        {
            get => parameters;
            set
            {
                parameters.Clear();
                parameters = value;
            }
        }

        //public JComponent getEditor()
        //{
        //    return new AbstractFilterEditor(ToString(), this);
        //}

        public override int GetHashCode()
        {
            return parameters.GetHashCode();
        }

        public override bool Equals(Object obj)
        {
            if (obj is AbstractFilter)
            {
                return parameters.Equals(((AbstractFilter)obj).parameters);
            }
            else
                return false;
        }

        public int CompareTo(Object obj)
        {
            if (obj is AbstractFilter)
            {
                return parameters.ToString().CompareTo(((AbstractFilter)obj).parameters.ToString());
            }
            else
                return -1;
        }

        public abstract bool accept(IChemObject @object);
    }

    //class AbstractFilterEditor : JPanel, ActionListener, FocusListener
    //{
    //    private const long serialVersionUID = -5047904516589635601L;

    //    protected String title;

    //    protected Object @object;

    //    protected bool editable;

    //    public AbstractFilterEditor(LayoutManager layout, String title, IObjectFilter @object)
    //        : base(layout)
    //    {
    //        this.@object = @object;
    //        this.title = title;
    //        setName(@object.ToString());
    //    }

    //    public AbstractFilterEditor(String title, IObjectFilter @object)
    //        : this(new BorderLayout(), title, @object)
    //    {
    //    }

    //    public AbstractFilterEditor()
    //        : this("", null)
    //    {
    //    }

    //    public void actionPerformed(ActionEvent e)
    //    {
    //    }

    //    public bool isEditable()
    //    {
    //        return editable;
    //    }

    //    public void setEditable(bool editable)
    //    {
    //        this.editable = editable;
    //    }

    //    public void focusGained(FocusEvent e)
    //    {
    //    }

    //    public void focusLost(FocusEvent e)
    //    {
    //    }
    //}
}
