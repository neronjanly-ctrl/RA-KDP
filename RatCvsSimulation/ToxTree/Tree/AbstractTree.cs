using System;
using Color = java.awt.Color;
using Dimension = java.awt.Dimension;
using Font = java.awt.Font;
using Graphics2D = java.awt.Graphics2D;
using RenderingHints = java.awt.RenderingHints;
using BufferedImage = java.awt.image.BufferedImage;
using PropertyChangeListener = java.beans.PropertyChangeListener;
using PropertyChangeSupport = java.beans.PropertyChangeSupport;
using DataOutputStream = java.io.DataOutputStream;
using FileOutputStream = java.io.FileOutputStream;
using IOException = java.io.IOException;
using ObjectInputStream = java.io.ObjectInputStream;
using OutputStream = java.io.OutputStream;
using ArrayList = java.util.ArrayList;
using Collection = java.util.Collection;
using List = java.util.List;
using Locale = java.util.Locale;
using Observable = java.util.Observable;
using Observer = java.util.Observer;
using ResourceBundle = java.util.ResourceBundle;
using Level = java.util.logging.Level;
using Logger = java.util.logging.Logger;
using JComponent = javax.swing.JComponent;
using AmbitException = net.idea.modbcum.i.exceptions.AmbitException;
using IProcessor = net.idea.modbcum.i.processors.IProcessor;
using IAtomContainer = org.openscience.cdk.interfaces.IAtomContainer;
using IAtomContainerSet = org.openscience.cdk.interfaces.IAtomContainerSet;
using IChemObjectSelection = org.openscience.cdk.renderer.selection.IChemObjectSelection;
using MultiSelection = org.openscience.cdk.renderer.selection.MultiSelection;
using Document = org.w3c.dom.Document;
using Element = org.w3c.dom.Element;
using IDecisionCategories = toxTree.core.IDecisionCategories;
using IDecisionCategory = toxTree.core.IDecisionCategory;
using IDecisionMethod = toxTree.core.IDecisionMethod;
using IDecisionMethodEditor = toxTree.core.IDecisionMethodEditor;
using IDecisionResult = toxTree.core.IDecisionResult;
using IDecisionRule = toxTree.core.IDecisionRule;
using IDecisionRuleList = toxTree.core.IDecisionRuleList;
using IProcessRule = toxTree.core.IProcessRule;
using XMLSerializable = toxTree.core.XMLSerializable;
using DecisionMethodException = toxTree.exceptions.DecisionMethodException;
using DecisionResultException = toxTree.exceptions.DecisionResultException;
using MolAnalyseException = toxTree.exceptions.MolAnalyseException;
using XMLDecisionMethodException = toxTree.exceptions.XMLDecisionMethodException;
using MolAnalyser = toxTree.query.MolAnalyser;
using EditorFactory = toxTree.ui.EditorFactory;
using CategoriesRenderer = toxTree.ui.tree.categories.CategoriesRenderer;
using CompoundImageTools = ambit2.rendering.CompoundImageTools;

namespace toxTree.tree
{
    public abstract class AbstractTree : Observable, IDecisionMethod, Observer, XMLSerializable
    {
        private const long serialVersionUID = -3786430005546416893L;

        [NonSerialized]
        protected static Logger logger = Logger.getLogger(typeof(AbstractTree).getName());

        protected Dimension imageSize = new Dimension(150, 150);

        protected bool web = false;

        public Dimension getImageSize()
        {
            return imageSize;
        }

        public void setImageSize(Dimension imageSize)
        {
            this.imageSize = imageSize;
        }

        [NonSerialized]
        protected PropertyChangeSupport changes = null;

        protected IDecisionRuleList rules;

        protected IDecisionCategories categories = null;

        protected bool falseIfRuleNotImplemented = true;

        protected int treeRoot = 1;

        protected String name = "";

        protected String explanation = "";

        [NonSerialized]
        protected bool modified = false;

        protected int priority = Integer.MAX_VALUE;

        public AbstractTree()
            : this(new CategoriesList(), new RulesList())
        {
        }

        public AbstractTree(IDecisionCategories categories)
            : this(categories, new RulesList())
        {
        }

        public AbstractTree(IDecisionCategories categories, IDecisionRuleList rules)
            : base()
        {
            setRules(rules);
            setCategories(categories);
            setTitle(getClass().getName());
            setExplanation(getTitle());
            setChanged();
            notifyObservers();
        }

        protected abstract IDecisionRuleList initRules();

        protected void setRules(String[] customRules)
        {
            rules.setRules(customRules);
            setChanged();
            notifyObservers();
            if (changes != null)
            {
                changes.firePropertyChange("Rules", rules, null);
            }
        }

        protected abstract void setTransitions(int[][] customTransitions);

        public String getTitle()
        {
            return name;
        }

        public void setTitle(String value)
        {
            this.name = value;
        }

        public void addDecisionRule(IDecisionRule rule)
        {
            throw new DecisionMethodException("Not allowed!");
        }

        public void setDecisionRule(IDecisionRule rule)
        {
            throw new DecisionMethodException("Not allowed!");
        }

        public StringBuffer explainRules(IDecisionResult result, bool verbose)
        {
            try
            {
                StringBuffer b = result.explain(verbose);
                return b;
            }
            catch (DecisionResultException x)
            {
                throw new DecisionMethodException(x);
            }
        }

        protected bool verifyResidues(IAtomContainerSet mols, IDecisionResult result, IDecisionRule rule)
        {
            logger.finer("Start processing residues\t" + mols.getAtomContainerCount());
            bool r = true;
            for (int i = 0; i < mols.getAtomContainerCount(); i++)
            {
                logger.finer("Residue\t" + Integer.toString(i + 1));
                try
                {
                    MolAnalyser.analyse(mols.getAtomContainer(i));
                    r = r && verifyRules(mols.getAtomContainer(i), result, rule);
                }
                catch (Exception x)
                {
                    throw new DecisionMethodException(x);
                }
            }
            logger.finer("Done processing residues.");
            return r;
        }

        public bool verifyRules(IAtomContainer mol, IDecisionResult result)
        {
            return verifyRules(mol, result, getTopRule());
        }

        protected abstract bool verifyRules(IAtomContainer mol, IDecisionResult result, IDecisionRule rule);

        public bool classify(IAtomContainer mol, IDecisionResult result)
        {
            result.clear();
            result.setDecisionMethod(this);
            result.setEstimating();
            try
            {
                MolAnalyser.analyse(mol);
            }
            catch (MolAnalyseException x)
            {
                if (logger.isLoggable(Level.FINE))
                    logger.log(Level.SEVERE, x.getMessage(), x);
                else
                    logger.log(Level.SEVERE, x.getMessage());
                throw new DecisionMethodException(x);
            }
            categories.selectAll(false);
            bool r = verifyRules(mol, result);
            return r;
        }

        public abstract void setParameters(IAtomContainer mol);

        public JComponent optionsPanel(IAtomContainer atomContainer)
        {
            return EditorFactory.getInstance().optionsPanel(this, atomContainer);
        }

        public int getNumberOfRules()
        {
            if (rules != null)
                return rules.size();
            else
                return 0;
        }

        public IDecisionRule getRule(int id)
        {
            if (rules == null)
                return null;
            return rules.getRule(id);
        }

        public IDecisionRule getRule(String name)
        {
            if (rules == null)
                return null;
            for (int i = 0; i < rules.size(); i++)
                if (getRule(i).getTitle().equals(name))
                    return getRule(i);
            return null;
        }

        public void printToStream(OutputStream os)
        {
            DataOutputStream @out = new DataOutputStream(os);
            for (int i = 0; i < rules.size(); i++)
            {
                @out.writeChars(rules.getRule(i).toString());
                @out.writeBytes("\n");
            }
        }

        public void printResults(String filename)
        {
            FileOutputStream o = new FileOutputStream(filename);
            printToStream(o);
            o.close();
        }

        public bool isFalseIfRuleNotImplemented()
        {
            return falseIfRuleNotImplemented;
        }

        public void setFalseIfRuleNotImplemented(bool falseIfRuleNotImplemented)
        {
            this.falseIfRuleNotImplemented = falseIfRuleNotImplemented;
            setChanged();
            notifyObservers();
        }

        public void addPropertyChangeListener(PropertyChangeListener l)
        {
            if (changes == null)
                changes = new PropertyChangeSupport(this);
            changes.addPropertyChangeListener(l);
        }

        public void removePropertyChangeListener(PropertyChangeListener l)
        {
            if (changes != null)
                changes.removePropertyChangeListener(l);
        }

        public override bool equals(Object obj)
        {
            IDecisionMethod m = (IDecisionMethod)obj;
            if (getTitle().equals(m.getTitle()) && (getNumberOfClasses() == m.getNumberOfClasses()) && (getNumberOfRules() == m.getNumberOfRules()))
            {
                if (!categories.equals(m.getCategories()))
                    return false;
                int nr = getNumberOfRules();
                if (nr != m.getNumberOfRules())
                    return false;
                for (int i = 0; i < nr; i++)
                    if (!getRule(i).equals(m.getRule(i)))
                        return false;
                return true;
            }
            else
                return false;
        }

        public IDecisionCategories getCategories()
        {
            return categories;
        }

        public override String toString()
        {
            return getTitle();
        }

        public IDecisionResult createDecisionResult()
        {
            IDecisionResult result = new TreeResult();
            result.setDecisionMethod(this);
            return result;
        }

        public int getNumberOfClasses()
        {
            return categories.size();
        }

        public IDecisionRuleList getRules()
        {
            return rules;
        }

        public String getExplanation()
        {
            return explanation;
        }

        public void setExplanation(String explanation)
        {
            this.explanation = explanation;
        }

        public void update(Observable o, Object arg)
        {
            if (o == rules)
            {
                setChanged();
                notifyObservers();
            }
            else if (o is IDecisionCategories)
            {
                setChanged();
                notifyObservers(o);
            }
        }

        public IDecisionRule getTopRule()
        {
            return getRule(0);
        }

        protected override void setChanged()
        {
            lock (this)
            {
                base.setChanged();
            }
        }

        protected override void clearChanged()
        {
            lock (this)
            {
                base.clearChanged();
            }
        }

        public bool isModified()
        {
            return modified;
        }

        public void setModified(bool value)
        {
            modified = value;
        }

        private void readObject(ObjectInputStream @in)
        {
            @in.defaultReadObject();
            if (rules is Observable)
                ((Observable)rules).addObserver(this);
            if (categories is Observable)
                ((Observable)categories).addObserver(this);
            modified = false;
        }

        public IDecisionMethodEditor getEditor()
        {
            return EditorFactory.getInstance().createTreeEditor(this);
        }

        public void fromXML(Element xml)
        {
        }

        public Element toShallowXML(Document document)
        {
            Element m = document.createElement(xmltag_METHOD);
            m.setAttribute(xmltag_CLASS, getClass().getName());
            return m;
        }

        public Element toXML(Document document)
        {
            Element e = document.createElement(XMLSerializable.xmltag_METHOD);
            e.setAttribute(xmltag_NAME, getTitle());
            Element explanation = document.createElement(XMLSerializable.xmltag_EXPLANATION);
            explanation.setTextContent(getExplanation());
            e.appendChild(explanation);
            e.setAttribute(xmltag_TREEROOT, Integer.toString(treeRoot));
            if (rules is XMLSerializable)
                e.appendChild(((XMLSerializable)rules).toXML(document));
            else
                throw new XMLDecisionMethodException(rules.getClass().getName() + " not XML serializable!");
            if (categories is XMLSerializable)
                e.appendChild(((XMLSerializable)categories).toXML(document));
            else
                throw new XMLDecisionMethodException(categories.getClass().getName() + " not XML serializable!");
            return e;
        }

        public void setRules(IDecisionRuleList rules)
        {
            if ((this.rules != null) && (this.rules is Observable))
                ((Observable)rules).deleteObserver(this);
            this.rules = rules;
            if ((rules != null) && (rules is Observable))
                ((Observable)rules).addObserver(this);
        }

        public void setCategories(IDecisionCategories categories)
        {
            if ((this.categories != null) && (this.categories is Observable))
                ((Observable)categories).deleteObserver(this);
            this.categories = categories;
            if ((categories != null) && (categories is Observable))
                ((Observable)categories).addObserver(this);
        }

        public int getPriority()
        {
            lock (this)
            {
                return priority;
            }
        }

        public void setPriority(int priority)
        {
            lock (this)
            {
                this.priority = priority;
            }
        }

        public int testRulesWithSelector()
        {
            int nr = getNumberOfRules();
            int na = 0;
            for (int i = 0; i < nr; i++)
            {
                IDecisionRule rule = rules.getRule(i);
                if (rule.getSelector() == null)
                {
                    na++;
                }
                else
                {
                    IAtomContainer a = null;
                    try
                    {
                        a = rule.getExampleMolecule(true);
                        if (a == null)
                            continue;
                    }
                    catch (Exception x)
                    {
                        continue;
                    }
                    IChemObjectSelection hit = rule.getSelector().process(a);
                }
            }
            return na;
        }

        public void walkRules(IDecisionRule rule, IProcessRule processor)
        {
            ArrayList<Integer> visited = new ArrayList<Integer>();
            processor.init(this);
            walkRules(rule, visited, processor);
            processor.done();
        }

        protected void walkRules(IDecisionRule rule, ArrayList<Integer> visited, IProcessRule processor)
        {
            if (rule == null)
                return;
            else
            {
                if (visited.indexOf(rule.getNum()) == -1)
                {
                    visited.add(rule.getNum());
                    processor.process(this, rule);
                    const bool[] answers = { true, false };
                    for (int i = 0; i < answers.length; i++)
                    {
                        IDecisionRule nextrule = getBranch(rule, answers[i]);
                        walkRules(nextrule, visited, processor);
                    }
                }
                else
                    return;
            }
        }

        public BufferedImage getImage(IAtomContainer mol)
        {
            return getImage(mol, null, 150, 150, false);
        }

        public BufferedImage getImage(IAtomContainer mol, String ruleID, int width, int height, bool atomnumbers)
        {
            IDecisionRuleList rules = getRules();
            IProcessor<IAtomContainer, IChemObjectSelection> selector = null;
            for (int i = 0; i < rules.size(); i++)
            {
                IDecisionRule rule = rules.get(i);
                if (ruleID == null)
                {
                    if (selector == null)
                        selector = new TreeSelector();
                    ((TreeSelector)selector).addSelector(rule.getSelector());
                }
                else if (rule.getID().equals(ruleID))
                {
                    selector = rule.getSelector();
                    break;
                }
            }
            CompoundImageTools tools = new CompoundImageTools(new Dimension(width, height));
            return tools.getImage(mol, selector, true, atomnumbers);
        }

        public void setWeb(Boolean web)
        {
            this.web = web;
        }

        public bool isWeb()
        {
            return web;
        }

        public override BufferedImage getLegend(int width, int height)
        {
            BufferedImage buffer = new BufferedImage(width, height, BufferedImage.TYPE_INT_RGB);
            Graphics2D g = buffer.createGraphics();
            g.setColor(Color.white);
            g.fillRect(0, 0, width, height);
            RenderingHints rh = g.getRenderingHints();
            rh.put(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON);
            g.addRenderingHints(rh);
            IDecisionCategories c = getCategories();
            if ((c == null) || (c.size() == 0))
                return buffer;
            int h = height / c.size();
            g.setFont(new Font("Arial", Font.BOLD, h / 4 == 0 ? 12 : h / 4));
            CategoriesRenderer r = new CategoriesRenderer(c);
            for (int i = 0; i < c.size(); i++)
            {
                IDecisionCategory cat = c.get(i);
                g.setBackground(r.getShowColor(i));
                g.setColor(r.getShowColor(i));
                g.fillRect(3, 3 + h * i, h - 6, h - 6);
                g.setColor(Color.black);
                g.drawString(cat.getName(), h + 10, 3 + h / 2 + h * i);
            }
            return buffer;
        }

        protected String retrieveExplanation(String resourceBundle)
        {
            ResourceBundle labels = ResourceBundle.getBundle(resourceBundle, Locale.ENGLISH, getClass().getClassLoader());
            StringBuilder b = new StringBuilder();
            for (int i = 1; i < 100; i++)
            {
                String key = String.format("reference%d", i);
                if (labels.containsKey(key))
                {
                    String @ref = labels.getString(key);
                    if (@ref.startsWith("http"))
                        b.append(String.format("<li><a href=\'%s\'>%s</a>", @ref, @ref));
                    else
                        b.append(String.format("<li>%s", @ref));
                }
                else
                    break;
            }
            String vendor = labels.getString("vendor");
            String vendoruri = labels.getString("vendoruri");
            String uri = labels.getString("uri");
            String traindata = labels.containsKey("trainingdata") ? String.format("<h5>Training data: <a href=\'%s\'>%s</a></h5>", labels.getString("trainingdata"), "Click to retrieve") : "";
            String testdata = labels.containsKey("testdata") ? String.format("<h5>Test data: <a href=\'%s\'>%s</a></h5>", labels.getString("testdata"), "Click to retrieve") : "";
            return String.format("<html><body>" + "<h3>%s</h3><h5>WWW: <a href=\'%s\'>%s</a></h5>" + "<h5>Vendor: <a href=\'%s\'>%s</a></h5>" + "%s%s" + "<hr><h3>References:</h3><ol>%s</ol>" + "</body></html>", labels.getString("explanation"), uri, uri, vendoruri, vendor, traindata, testdata, b.toString());
        }
    }

    class TreeSelector : IProcessor<IAtomContainer, IChemObjectSelection>
    {
        protected List<IProcessor<IAtomContainer, IChemObjectSelection>> selectors = new ArrayList<IProcessor<IAtomContainer, IChemObjectSelection>>();

        private const long serialVersionUID = 7753309179379127408L;

        public void addSelector(IProcessor<IAtomContainer, IChemObjectSelection> selector)
        {
            selectors.add(selector);
        }

        public override long getID()
        {
            return 0;
        }

        public override bool isEnabled()
        {
            return true;
        }

        public override IChemObjectSelection process(IAtomContainer target)
        {
            Collection<IAtomContainer> set = new ArrayList<IAtomContainer>();
            MultiSelection<IAtomContainer> ms = new MultiSelection<IAtomContainer>(set);
            foreach (IProcessor<IAtomContainer, IChemObjectSelection> selector in selectors)
                try
                {
                    set.add(selector.process(target).getConnectedAtomContainer());
                }
                catch (Exception x)
                {
                    throw new AmbitException(x);
                }
            return ms;
        }

        public override void setEnabled(bool value)
        {
        }

        public override void close()
        {
        }

        public override void open()
        {
        }
    }
}
