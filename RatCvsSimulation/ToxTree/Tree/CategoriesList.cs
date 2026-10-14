using System;
using List = java.util.List;
using Collection = java.util.Collection;
using Iterator = java.util.Iterator;
using IList = java.util.IList;
using ListIterator = java.util.ListIterator;
using Observable = java.util.Observable;
using Observer = java.util.Observer;
using Document = org.w3c.dom.Document;
using Element = org.w3c.dom.Element;
using IDecisionCategories = toxTree.core.IDecisionCategories;
using IDecisionCategory = toxTree.core.IDecisionCategory;
using Introspection = toxTree.core.Introspection;
using XMLSerializable = toxTree.core.XMLSerializable;
using XMLDecisionMethodException = toxTree.exceptions.XMLDecisionMethodException;
using System.Collections.Generic;

namespace toxTree.tree
{
    public class CategoriesList : IObservable<>, IDecisionCategories, IObserver, XMLSerializable
    {
        protected List<IDecisionCategory> storage;

        protected IDecisionCategory selected = null;

        protected bool multilabel = false;

        private const long serialVersionUID = 1984731709034516593L;

        public CategoriesList()
            : base()
        {
            storage = new List<IDecisionCategory>();
            selected = null;
        }

        public CategoriesList(IDecisionCategory[] classes)
            : this(classes, false)
        {
        }

        public CategoriesList(IDecisionCategory[] classes, bool multilabel)
            : this()
        {
            setMultilabel(multilabel);
            for (int i = 0; i < classes.length; i++)
                addCategory(classes[i]);
        }

        public CategoriesList(String[] classes)
            : this(classes, false)
        {
        }

        public CategoriesList(String[] classes, bool multilabel)
            : this()
        {
            setMultilabel(multilabel);
            for (int i = 0; i < classes.length; i++)
                try
                {
                    addCategory((IDecisionCategory)Introspection.loadCreateObject(classes[i]));
                }
                catch (Exception x)
                {
                    x.printStackTrace();
                }
        }

        public IDecisionCategory getCategory(IDecisionCategory key)
        {
            try
            {
                int index = storage.IndexOf(key);
                if (index == -1)
                    return null;
                else
                    return (IDecisionCategory)storage[index];
            }
            catch (Exception x)
            {
                return null;
            }
        }

        public void addCategory(IDecisionCategory category)
        {
            if (getCategory(category) != null)
                return;
            if (category.getID() == -1)
                category.setID(storage.size() + 1);
            storage.add(category);
            if (category is Observable)
                ((Observable)category).addObserver(this);
            setChanged();
            notifyObservers();
        }

        public int size()
        {
            return storage.size();
        }

        public void clear()
        {
            for (int i = 0; i < storage.size(); i++)
                if (storage.get(i) is Observable)
                    ((Observable)storage.get(i)).deleteObserver(this);
            storage.clear();
            setChanged();
            notifyObservers();
        }

        public override bool equals(Object o)
        {
            if (o is CategoriesList)
            {
                CategoriesList cl = (CategoriesList)o;
                if (o == null)
                    return false;
                if (cl.size() != size())
                    return false;
                else
                    return storage.containsAll(cl.storage);
            }
            else
                return false;
        }

        public IDecisionCategory getSelected()
        {
            return selected;
        }

        public void setSelected(IDecisionCategory category)
        {
            if (!multilabel)
                for (int i = 0; i < storage.size(); i++)
                    storage.get(i).setSelected(false);
            if (category != null)
            {
                selected = getCategory(category);
                selected.setSelected(true);
            }
            else
                selected = null;
            setChanged();
            notifyObservers();
        }

        public void add(int arg0, IDecisionCategory arg1)
        {
            storage.add(arg0, arg1);
            setChanged();
            notifyObservers();
        }

        public bool add(IDecisionCategory arg0)
        {
            addCategory((IDecisionCategory)arg0);
            return true;
        }

        public bool addAll(Collection arg0)
        {
            bool b = storage.addAll(arg0);
            setChanged();
            notifyObservers();
            return b;
        }

        public bool addAll(int arg0, Collection arg1)
        {
            bool b = addAll(arg0, arg1);
            setChanged();
            notifyObservers();
            return b;
        }

        public bool contains(Object arg0)
        {
            return storage.contains(arg0);
        }

        public bool containsAll(Collection arg0)
        {
            return storage.containsAll(arg0);
        }

        public IDecisionCategory get(int arg0)
        {
            return storage.get(arg0);
        }

        public IDecisionCategory remove(int arg0)
        {
            IDecisionCategory o = storage.remove(arg0);
            if (o is Observable)
                ((Observable)o).deleteObserver(this);
            setChanged();
            notifyObservers();
            return o;
        }

        public bool remove(Object arg0)
        {
            bool o = storage.remove(arg0);
            if (arg0 is Observable)
                ((Observable)arg0).deleteObserver(this);
            setChanged();
            notifyObservers();
            return o;
        }

        public bool removeAll(Collection arg0)
        {
            bool o = storage.removeAll(arg0);
            setChanged();
            notifyObservers();
            return o;
        }

        public bool retainAll(Collection arg0)
        {
            bool o = storage.retainAll(arg0);
            setChanged();
            notifyObservers();
            return o;
        }

        public Object[] toArray()
        {
            return storage.toArray();
        }

        public Iterator<IDecisionCategory> iterator()
        {
            return storage.iterator();
        }

        public IDecisionCategory set(int arg0, IDecisionCategory arg1)
        {
            IDecisionCategory o = storage.set(arg0, arg1);
            setChanged();
            notifyObservers();
            return o;
        }

        public int indexOf(Object arg0)
        {
            return storage.indexOf(arg0);
        }

        public IList<IDecisionCategory> subList(int arg0, int arg1)
        {
            return storage.subList(arg0, arg1);
        }

        public ListIterator<IDecisionCategory> listIterator()
        {
            return storage.listIterator();
        }

        public ListIterator<IDecisionCategory> listIterator(int arg0)
        {
            return storage.listIterator(arg0);
        }

        public int lastIndexOf(Object arg0)
        {
            return storage.lastIndexOf(arg0);
        }

        public bool isEmpty()
        {
            return storage.isEmpty();
        }

        public void fromXML(Element xml)
        {
        }

        public Element toShallowXML(Document document)
        {
            Element m = document.createElement(xmltag_CATEGORIES);
            m.setAttribute(xmltag_CLASS, getClass().getName());
            return m;
        }

        public Element toXML(Document document)
        {
            Element e = document.createElement(XMLSerializable.xmltag_CATEGORIES);
            for (int i = 0; i < size(); i++)
            {
                IDecisionCategory category = (IDecisionCategory)storage.get(i);
                if (category is XMLSerializable)
                    e.appendChild(((XMLSerializable)category).toXML(document));
                else
                    throw new XMLDecisionMethodException(category.getClass().getName() + " not XML serializable!");
            }
            return e;
        }

        public void update(Observable arg0, Object arg1)
        {
            if (arg0 is IDecisionCategory)
            {
                setChanged();
                notifyObservers(arg0);
            }
        }

        public List getStorage()
        {
            return storage;
        }

        public void setStorage(List<IDecisionCategory> storage)
        {
            this.storage = storage;
        }

        public T[] toArray<T>(T[] a)
        {
            return null;
        }

        public override String toString()
        {
            StringBuffer b = new StringBuffer();
            for (int i = 0; i < size(); i++)
            {
                b.append(get(i).toString());
                b.append("\t");
            }
            return b.toString();
        }

        public bool isMultilabel()
        {
            return multilabel;
        }

        public void setMultilabel(bool multilabel)
        {
            this.multilabel = multilabel;
        }

        public void selectAll(bool selected)
        {
            for (int i = 0; i < storage.size(); i++)
                storage.get(i).setSelected(selected);
        }
    }
}
