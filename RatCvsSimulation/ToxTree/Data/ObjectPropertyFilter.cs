using NCDK;
using System;
using System.Collections;
using System.Diagnostics;
using toxTree.exceptions;

namespace toxTree.data
{
    public class ObjectPropertyFilter : AbstractFilter
    {
        public static String p_tag = "tag";

        public static String p_value = "value";

        public ObjectPropertyFilter()
            : base()
        {
        }

        public ObjectPropertyFilter(Object key, Object value)
            : this()
        {
            if ((key == null) || (key.Equals("")))
                throw new FilterException("The condition \"" + key + "\"=\"" + value + "\" can not be created!");
            setProperty(p_tag, key);
            setProperty(p_value, value);
        }

        public void setProperties(Hashtable properties)
        {
            var i = properties.Keys.GetEnumerator();
            while (i.MoveNext())
            {
                Object key = i.Current;
                setProperty(key.ToString(), properties[key]);
            }
        }

        public override void setProperty(String key, Object value)
        {
            if (p_tag.Equals(key) || p_value.Equals(key))
                base.setProperty(key, value);
        }

        public override bool accept(IChemObject @object)
        {
            Object key = parameters[p_tag];
            return acceptProperty(@object.GetProperty<object>(key));
        }

        public bool acceptProperty(Object value)
        {
            try
            {
                if (value == null)
                    return false;
                Object values = parameters[p_value];
                if (values is IList list)
                    return list.Contains(value);
                else
                    return values.ToString().Equals(value.ToString());
            }
            catch (Exception x)
            {
                Debug.WriteLine(x.StackTrace);
                return false;
            }
        }

        public override String ToString()
        {
            return parameters.ToString();
        }

        public Object getTag()
        {
            return getProperty(p_tag);
        }

        public void setTag(Object property)
        {
            setProperty(p_tag, property);
        }

        public Object getValue()
        {
            return getProperty(p_value);
        }

        public void setValue(Object property)
        {
            setProperty(p_value, property);
        }

        public override bool Equals(object arg0)
        {
            ObjectPropertyFilter pf = (ObjectPropertyFilter)arg0;
            return getTag().Equals(pf.getTag()) && getValue().Equals(pf.getValue());
        }

        //public override JComponent getEditor()
        //{
        //    return new ObjectPropertyFilterEditor("", this);
        //}
    }

    //class ObjectPropertyFilterEditor : AbstractFilterEditor, ActionListener
    //{
    //    private const long serialVersionUID = 4964600959825654563L;

    //    protected JLabel keyLabel;

    //    protected JComboBox keyField;

    //    protected JLabel valueLabel;

    //    protected JComboBox valueField;

    //    public ObjectPropertyFilterEditor(String title, ObjectPropertyFilter filter)
    //        : this(new FlowLayout(), title, filter)
    //    {
    //    }

    //    public ObjectPropertyFilterEditor(LayoutManager layout, String title, ObjectPropertyFilter filter)
    //        : base(layout, title, filter)
    //    {
    //        setLayout(new BoxLayout(this, BoxLayout.PAGE_AXIS));
    //        Object key = filter.getProperty(ObjectPropertyFilter.p_tag);
    //        if (key == null)
    //            filter.setProperty(ObjectPropertyFilter.p_tag, "");
    //        key = "";
    //        keyField = new JComboBox(new String[]
    //        {
    //            key.ToString(),
    //        });
    //        keyField.setSelectedIndex(0);
    //        keyField.addActionListener(this);
    //        keyField.addFocusListener(this);
    //        keyField.setPreferredSize(new Dimension(100, 24));
    //        keyField.setMaximumSize(new Dimension(int.MaxValue, 24));
    //        keyLabel = new JLabel("Parameter");
    //        add(keyLabel);
    //        add(keyField);
    //        Object value = filter.getProperty(ObjectPropertyFilter.p_value);
    //        if (value == null)
    //            value = "";
    //        valueField = new JComboBox(new String[]
    //        {
    //            value.ToString(),
    //        });
    //        valueField.setPreferredSize(new Dimension(100, 24));
    //        valueField.setMaximumSize(new Dimension(int.MaxValue, 24));
    //        valueField.setSelectedIndex(0);
    //        valueField.addFocusListener(this);
    //        valueField.addActionListener(this);
    //        valueLabel = new JLabel("Value");
    //        add(valueLabel);
    //        add(valueField);
    //        setBorder(BorderFactory.createTitledBorder("Define group of compounds"));
    //    }

    //    public override void setEditable(bool editable)
    //    {
    //        base.setEditable(editable);
    //        keyField.setEditable(editable);
    //        valueField.setEditable(editable);
    //    }

    //    protected void update(Object source)
    //    {
    //        if (source == keyField)
    //        {
    //            ((ObjectPropertyFilter)@object).setProperty(ObjectPropertyFilter.p_tag, keyField.getSelectedItem().ToString());
    //        }
    //        else if (source == valueField)
    //        {
    //            ((ObjectPropertyFilter)@object).setProperty(ObjectPropertyFilter.p_value, valueField.getSelectedItem().ToString());
    //        }
    //    }

    //    public override void actionPerformed(ActionEvent e)
    //    {
    //        base.actionPerformed(e);
    //        try
    //        {
    //            update(e.getSource());
    //        }
    //        catch (Exception x)
    //        {
    //            JOptionPane.showMessageDialog(this, x.Message, "Error", JOptionPane.ERROR_MESSAGE);
    //        }
    //    }

    //    public override void focusLost(FocusEvent e)
    //    {
    //        base.focusLost(e);
    //        try
    //        {
    //            update(e.getSource());
    //        }
    //        catch (Exception x)
    //        {
    //            JOptionPane.showMessageDialog(this, x.Message, "Error", JOptionPane.ERROR_MESSAGE);
    //        }
    //    }

    //    public void addTags(Object[] tags)
    //    {
    //        for (int i = 0; i < tags.Length; i++)
    //            keyField.addItem(tags[i]);
    //    }
    //}
}
