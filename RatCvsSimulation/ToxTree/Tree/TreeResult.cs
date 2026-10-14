using System;
using PropertyChangeSupport = java.beans.PropertyChangeSupport;
using Level = java.util.logging.Level;
using Logger = java.util.logging.Logger;
using AmbitException = net.idea.modbcum.i.exceptions.AmbitException;
using IAtomContainer = org.openscience.cdk.interfaces.IAtomContainer;
using IMolecularDescriptor = org.openscience.cdk.qsar.IMolecularDescriptor;
using DMethodNotAssigned = toxTree.exceptions.DMethodNotAssigned;
using ILiteratureEntry = ambit2.@base.data.ILiteratureEntry;
using _type = ambit2.@base.data.ILiteratureEntry._type;
using LiteratureEntry = ambit2.@base.data.LiteratureEntry;
using Property = ambit2.@base.data.Property;
using PropertyAnnotation = ambit2.@base.data.PropertyAnnotation;
using PropertyAnnotations = ambit2.@base.data.PropertyAnnotations;
using System.Collections.Generic;
using toxTree.core;
using toxTree.exceptions;
using System.Text;
using toxTree.data;
using System.ComponentModel;
using System.Diagnostics;

namespace toxTree.tree
{
    public class TreeResult : IDecisionResult
    {
        [NonSerialized]
        protected static Logger logger = Logger.getLogger(nameof(TreeResult));

        private const long serialVersionUID = 87786882555929168L;

        [NonSerialized]
        protected PropertyChangeSupport changes = null;

        protected IDecisionMethod decisionMethod = null;

        protected IDecisionCategory category = null;

        protected IDecisionCategories assignedCategories = null;

        [NonSerialized]
        protected IAtomContainer originalMolecule;

        protected List<RuleResult> ruleResults = null;

        protected ProgressStatus status;

        protected bool notify = true;

        protected bool web = false;

        public bool isNotify()
        {
            lock (this)
            {
                return notify;
            }
        }

        public void setNotify(bool notify)
        {
            lock (this)
            {
                this.notify = notify;
            }
        }

        public TreeResult()
            : base()
        {
            status = new ProgressStatus();
            ruleResults = new List<RuleResult>();
            assignedCategories = new CategoriesList();
        }

        public void clear()
        {
            clearResults();
        }

        protected void clearResults()
        {
            firePropertyChangeEvent(ProgressStatus._pClassID, category, null);
            category = null;
            assignedCategories.clear();
            if (ruleResults != null)
            {
                ruleResults.Clear();
            }
            status.clear();
            firePropertyChangeEvent(ProgressStatus._pRuleResult, null, status);
            firePropertyChangeEvent(ProgressStatus._pRuleResult, null, "");
            firePropertyChangeEvent("panel2d_selected", null, null);
        }

        public bool isError()
        {
            return status.isError();
        }

        public void setError(String message)
        {
            status.setError(message);
            firePropertyChangeEvent(ProgressStatus._pRuleResult, null, message);
        }

        public bool isEstimated()
        {
            return status.isEstimated();
        }

        public bool isEstimating()
        {
            return status.isEstimating();
        }

        public void setEstimating()
        {
            status.setEstimating();
            firePropertyChangeEvent(ProgressStatus._pRuleResult, null, status);
        }

        public void setPercentEstimated(int percent)
        {
            status.setPercentEstimated(percent);
            firePropertyChangeEvent(ProgressStatus._pRuleResult, null, status);
        }

        public void setEstimated()
        {
            status.setEstimated();
            firePropertyChangeEvent(ProgressStatus._pRuleResult, null, status);
        }

        public void setEstimated(bool value)
        {
            status.setEstimated(value);
            firePropertyChangeEvent(ProgressStatus._pRuleResult, null, status);
            ;
        }

        public IDecisionCategory getCategory()
        {
            return category;
        }

        public IDecisionCategories getAssignedCategories()
        {
            return assignedCategories;
        }

        public void setCategory(IDecisionCategory category)
        {
            firePropertyChangeEvent(ProgressStatus._pClassID, this.category, category);
            if (acceptCategory(category))
            {
                logger.finer("Assign category\t" + category);
                this.category = category;
                if (!decisionMethod.getCategories().isMultilabel())
                {
                    assignedCategories.clear();
                }
                addCategory(category);
            }
            ruleResults[ruleResults.Count - 1].setCategory(category);
            firePropertyChangeEvent(ProgressStatus._pRuleResult, null, this);
        }

        protected bool acceptCategory(IDecisionCategory category)
        {
            return ((this.category == null) || (this.category.CompareTo(category) < 0));
        }

        protected void addCategory(IDecisionCategory category)
        {
            assignedCategories.Add(category);
        }

        public void setMolecule(IAtomContainer molecule)
        {
            ((RuleResult)ruleResults[ruleResults.Count - 1]).setMolecule(molecule);
            firePropertyChangeEvent(ProgressStatus._pRuleResult, null, this);
        }

        public void setSilent(bool silent)
        {
            ((RuleResult)ruleResults[ruleResults.Count - 1]).setSilent(silent);
            firePropertyChangeEvent(ProgressStatus._pRuleResult, null, this);
        }

        public void addRuleResult(IDecisionRule rule, bool value, IAtomContainer molecule)
        {
            if (decisionMethod == null)
                throw new DMethodNotAssigned(ProgressStatus._eMethodNotAssigned);
            if (rule == null)
                throw new DecisionResultException("Rule # not assigned!");
            RuleResult ruleResult = new RuleResult(rule, value);
            ruleResult.setWeb(web);
            ruleResults.Add(ruleResult);
            setMolecule(molecule);
        }

        public StringBuilder explain(bool verbose)
        {
            try
            {
                StringBuilder b;
                switch (status.estimated)
                {
                    case ProgressStatus.STATUS.COMPLETED:
                        {
                            b = new StringBuilder();
                            IAtomContainer ac = null;
                            bool first = true;
                            String delimiter = "";
                            for (int i = 0; i < ruleResults.Count; i++)
                            {
                                if ((!verbose) && (i > 0))
                                    delimiter = ",";
                                else
                                    delimiter = "";
                                RuleResult r = ((RuleResult)ruleResults[i]);
                                if (r.isSilent())
                                    continue;
                                IAtomContainer mol = r.getMolecule();
                                if (ac == null)
                                    ac = mol;
                                if ((ac != null) && (ac.getID() != mol.getID()))
                                {
                                    if (!first)
                                        if (!verbose)
                                            b.Append(")");
                                        else
                                            ;
                                    else
                                        first = false;
                                    if (verbose)
                                        b.Append("<br>");
                                    else
                                        b.Append("(");
                                    ac = mol;
                                }
                                else
                                    b.Append(delimiter);
                                b.Append(r.explain(verbose, i));
                                if (verbose)
                                    b.Append("<br>");
                            }
                            if (!first)
                                if (!verbose)
                                    b.Append(")");
                            return b;
                        }
                    default:
                        {
                            b = new StringBuilder();
                            b.Append(status.ToString());
                            return b;
                        }
                }
            }
            catch (ArgumentNullException x)
            {
                throw new DMethodNotAssigned(ProgressStatus._eMethodNotAssigned);
            }
        }

        public override String ToString()
        {
            if (decisionMethod == null)
                return ProgressStatus._eMethodNotAssigned;
            switch (status.estimated)
            {
                case COMPLETED:
                    return decisionMethod.ToString() + "\t Class\t" + category.ToString();
                default:
                    return status.ToString();
            }
        }

        public int CompareTo(Object o)
        {
            TreeResult r = (TreeResult)o;
            return category.CompareTo(r.getCategory());
        }

        public IDecisionMethod getDecisionMethod()
        {
            return decisionMethod;
        }

        public void setDecisionMethod(IDecisionMethod decisionMethod)
        {
            if (decisionMethod == null)
            {
                if (this.decisionMethod != null)
                    clear();
                firePropertyChangeEvent(ProgressStatus._pDecisionMethod, this.decisionMethod, decisionMethod);
                this.decisionMethod = null;
            }
            else
            {
                setWeb(decisionMethod.isWeb());
                if ((this.decisionMethod != null) && (this.decisionMethod.Equals(decisionMethod)))
                    return;
                firePropertyChangeEvent(ProgressStatus._pDecisionMethod, this.decisionMethod, decisionMethod);
                this.decisionMethod = decisionMethod;
                ruleResults.clear();
            }
            firePropertyChangeEvent(ProgressStatus._pRuleResult, null, this);
        }

        public bool classify(IAtomContainer mol)
        {
            try
            {
                firePropertyChangeEvent(ProgressStatus._pRuleResult, null, decisionMethod.getTitle());
                setOriginalMolecule(mol);
                bool r = decisionMethod.classify(mol, this);
                setEstimated(true);
                firePropertyChangeEvent(ProgressStatus._pRuleResult, null, "Completed.");
                return r;
            }
            catch (DecisionMethodException x)
            {
                if (logger.isLoggable(Level.FINE))
                    logger.log(Level.SEVERE, x.Message, x);
                else
                    logger.log(Level.SEVERE, x.Message);
                setOriginalMolecule(null);
                setEstimated(false);
                firePropertyChangeEvent(ProgressStatus._pRuleResult, null, x.Message);
                throw new DecisionResultException(x);
            }
            catch (ArgumentNullException x)
            {
                if (logger.isLoggable(Level.FINE))
                    logger.log(Level.SEVERE, x.Message, x);
                else
                    logger.log(Level.SEVERE, x.Message);
                setOriginalMolecule(null);
                setEstimated(false);
                firePropertyChangeEvent(ProgressStatus._pRuleResult, null, x.Message);
                if (decisionMethod == null)
                    throw new DMethodNotAssigned(x);
                else
                    throw new DecisionResultException(x);
            }
            catch (Exception x)
            {
                setEstimated(false);
                firePropertyChangeEvent(ProgressStatus._pRuleResult, null, x.Message);
                throw new DecisionResultException(x);
            }
        }

        public void addPropertyChangeListener(PropertyChangedEventHandler l)
        {
            if (changes == null)
                changes = new PropertyChangeSupport(this);
            changes.AddPropertyChangeListener(l);
        }

        public void removePropertyChangeListener(PropertyChangedEventHandler l)
        {
            if (changes != null)
                changes.removePropertyChangeListener(l);
        }

        public PropertyChangedEventHandler[] getPropertyChangeListeners()
        {
            return changes.getPropertyChangeListeners();
        }

        protected void fireChangeEvent(PropertyChangedEvent e)
        {
            if (notify && (changes != null))
                changes.firePropertyChange(e);
        }

        protected void firePropertyChangeEvent(String propertyName, int oldValue, int newValue)
        {
            if (notify && (changes != null))
                changes.firePropertyChange(propertyName, oldValue, newValue);
        }

        protected void firePropertyChangeEvent(String propertyName, bool oldValue, bool newValue)
        {
            if (notify && (changes != null))
                changes.firePropertyChange(propertyName, oldValue, newValue);
        }

        protected void firePropertyChangeEvent(String propertyName, Object oldValue, Object newValue)
        {
            if (notify && (changes != null))
                changes.firePropertyChange(propertyName, oldValue, newValue);
        }

        public override bool Equals(Object obj)
        {
            IDecisionResult r = (IDecisionResult)obj;
            if ((decisionMethod == null) && (r.getDecisionMethod() == null))
                return true;
            if ((decisionMethod == null) || (r.getDecisionMethod() == null))
                return false;
            if ((category.Equals(r.getCategory())) && (decisionMethod.Equals(r.getDecisionMethod())))
            {
                int nr = getRuleResultsCount();
                if (nr != r.getRuleResultsCount())
                    return false;
                try
                {
                    for (int i = 0; i < nr; i++)
                        if (!getRuleResult(i).Equals(r.getRuleResult(i)))
                        {
                            return false;
                        }
                    return true;
                }
                catch (DecisionResultException x)
                {
                    Debug.WriteLine(x.StackTrace);
                    return false;
                }
            }
            else
                return false;
        }

        public int getRuleResultsCount()
        {
            return ruleResults.Count;
        }

        public IAtomContainer getMolecule(int index)
        {
            try
            {
                return ((RuleResult)ruleResults[index]).getMolecule();
            }
            catch (Exception x)
            {
                throw new DecisionResultException(x);
            }
        }

        public IDecisionCategory getCategory(int index)
        {
            try
            {
                return ((RuleResult)ruleResults[index]).getCategory();
            }
            catch (Exception x)
            {
                throw new DecisionResultException(x);
            }
        }

        public RuleResult getRuleResult(int index)
        {
            try
            {
                return (RuleResult)ruleResults[index];
            }
            catch (Exception x)
            {
                throw new DecisionResultException(x);
            }
        }

        public IDecisionRule getRule(int index)
        {
            try
            {
                return ((RuleResult)ruleResults[index]).getRule();
            }
            catch (Exception x)
            {
                throw new DecisionResultException(x);
            }
        }

        public void assignResult(IAtomContainer mol)
        {
            if (mol == null)
                return;
            if (getCategory() != null)
                mol.setProperty(getResultPropertyNames()[0], getCategory());
            else
                mol.removeProperty(getResultPropertyNames()[0]);
            String paths = getResultPropertyNames()[0] + "#explanation";
            if (getDecisionMethod().getRules().Count > 1)
                mol.setProperty(paths, explain(false).ToString());
            else
                mol.removeProperty(paths);
            firePropertyChangeEvent(ProgressStatus._pRuleResult, null, status);
        }

        public String[] getResultPropertyNames()
        {
            if (getDecisionMethod() == null)
                return new String[]
                {
                    "Unassigned method",
                };
            else
                return new String[]
                {
                    getDecisionMethod().getTitle(),
                };
        }

        protected ILiteratureEntry getReference()
        {
            if (getDecisionMethod() is IMolecularDescriptor)
            {
                IMolecularDescriptor tree = (IMolecularDescriptor)getDecisionMethod();
                return LiteratureEntry.getInstance(tree.getSpecification().getImplementationTitle(), tree.getSpecification().getSpecificationReference());
            }
            else
                return LiteratureEntry.getInstance("Toxtree", "http://toxtree.sf.net");
        }

        public override List<Property> getResultProperties()
        {
            if (getDecisionMethod() == null)
                throw new AmbitException("Unassigned method");
            ILiteratureEntry le = getReference();
            le.setType(_type.Algorithm);
            Property property = new Property(getDecisionMethod().getTitle(), le);
            property.setLabel(le.getURL());
            property.setClazz(typeof(String));
            property.setOrder(1);
            property.setEnabled(true);
            property.setNominal(true);
            PropertyAnnotations pa = new PropertyAnnotations();
            property.setAnnotations(pa);
            foreach (IDecisionCategory category in getDecisionMethod().getCategories())
            {
                PropertyAnnotation a = new PropertyAnnotation();
                a.setType("^^" + category.getCategoryType().name());
                a.setPredicate("acceptValue");
                a.setObject(category.getName());
                pa.Add(a);
            }
            List<Property> p = new List<Property>();
            p.Add(property);
            property = new Property(string.Format("%s#explanation", getDecisionMethod().getTitle()), le);
            property.setLabel(le.getURL());
            property.setClazz(typeof(String));
            property.setOrder(2);
            property.setEnabled(true);
            property.setNominal(false);
            p.Add(property);
            return p;
        }

        public List<RuleResult> getRuleResults()
        {
            return ruleResults;
        }

        public void setRuleResults(List<RuleResult> ruleResults)
        {
            this.ruleResults = ruleResults;
        }

        public int getPriority()
        {
            IDecisionMethod m = getDecisionMethod();
            if (m == null)
                return int.MaxValue;
            else
                return m.getPriority();
        }

        public void setPriority(int priority)
        {
            IDecisionMethod m = getDecisionMethod();
            if (m == null)
                return;
            else
                m.setPriority(priority);
        }

        public List<CategoryFilter> getFilters()
        {
            List<CategoryFilter> l = new List<CategoryFilter>();
            IDecisionCategories c = getDecisionMethod().getCategories();
            for (int i = 0; i < c.size(); i++)
                try
                {
                    l.Add(new CategoryFilter(getResultPropertyNames()[0], c[i]));
                }
                catch (Exception x)
                {
                    logger.log(Level.SEVERE, x.Message, x);
                }
            return l;
        }

        public IAtomContainer getOriginalMolecule()
        {
            lock (this)
            {
                return originalMolecule;
            }
        }

        public void setOriginalMolecule(IAtomContainer originalMolecule)
        {
            lock (this)
            {
                this.originalMolecule = originalMolecule;
            }
        }

        public void hilightAlert(RuleResult ruleResult)
        {
            lock (this)
            {
                firePropertyChangeEvent("panel2d_molecule", null, ruleResult == null ? originalMolecule : ruleResult.getMolecule() == null ? originalMolecule : ruleResult.getMolecule());
                firePropertyChangeEvent("panel2d_selected", null, ruleResult == null ? null : ruleResult.getSelector());
            }
        }

        public void hilightAlert(IDecisionRule rule)
        {
            lock (this)
            {
                firePropertyChangeEvent("panel2d_molecule", null, originalMolecule);
                firePropertyChangeEvent("panel2d_selected", null, rule == null ? null : rule.getSelector());
            }
        }

        public void setWeb(Boolean web)
        {
            this.web = web;
        }
    }
}
