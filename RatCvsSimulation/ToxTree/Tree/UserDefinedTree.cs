using System;
using PropertyChangeListener = java.beans.PropertyChangeListener;
using IOException = java.io.IOException;
using ObjectInputStream = java.io.ObjectInputStream;
using Iterator = java.util.Iterator;
using List = java.util.List;
using Observable = java.util.Observable;
using Level = java.util.logging.Level;
using JComponent = javax.swing.JComponent;
using JOptionPane = javax.swing.JOptionPane;
using AmbitException = net.idea.modbcum.i.exceptions.AmbitException;
using CDKException = org.openscience.cdk.exception.CDKException;
using IAtomContainer = org.openscience.cdk.interfaces.IAtomContainer;
using IAtomContainerSet = org.openscience.cdk.interfaces.IAtomContainerSet;
using DescriptorSpecification = org.openscience.cdk.qsar.DescriptorSpecification;
using DescriptorValue = org.openscience.cdk.qsar.DescriptorValue;
using IMolecularDescriptor = org.openscience.cdk.qsar.IMolecularDescriptor;
using IDescriptorResult = org.openscience.cdk.qsar.result.IDescriptorResult;
using IDecisionCategories = toxTree.core.IDecisionCategories;
using IDecisionCategory = toxTree.core.IDecisionCategory;
using IDecisionInteractive = toxTree.core.IDecisionInteractive;
using IDecisionMethod = toxTree.core.IDecisionMethod;
using IDecisionResult = toxTree.core.IDecisionResult;
using IDecisionRule = toxTree.core.IDecisionRule;
using IDecisionRuleList = toxTree.core.IDecisionRuleList;
using IProcessRule = toxTree.core.IProcessRule;
using Introspection = toxTree.core.Introspection;
using DRuleException = toxTree.exceptions.DRuleException;
using DRuleNotImplemented = toxTree.exceptions.DRuleNotImplemented;
using DecisionMethodException = toxTree.exceptions.DecisionMethodException;
using DecisionMethodIOException = toxTree.exceptions.DecisionMethodIOException;
using DecisionResultException = toxTree.exceptions.DecisionResultException;
using MolFlags = toxTree.query.MolFlags;
using Property = ambit2.@base.data.Property;
using ArrayResult = ambit2.core.data.ArrayResult;
using IObject2Properties = ambit2.core.data.IObject2Properties;
using StringDescriptorResultType = ambit2.core.data.StringDescriptorResultType;

namespace toxTree.tree
{
    public class UserDefinedTree : AbstractTree, IDecisionInteractive, IMolecularDescriptor, IObject2Properties<UserDefinedTree>
    {
        protected bool editable = true;

        protected IDecisionNodesFactory nodesFactory;

        protected UserOptions options = UserOptions.YEStoALL;

        public UserOptions getOptions()
        {
            return options;
        }

        public void setOptions(UserOptions options)
        {
            this.options = options;
        }

        public PropertyChangeListener getListener()
        {
            return listener;
        }

        public void setListener(PropertyChangeListener listener)
        {
            this.listener = listener;
        }

        [NonSerialized]
        protected PropertyChangeListener listener;

        private const long serialVersionUID = -1399565169354017486L;

        public UserDefinedTree(IDecisionNodesFactory nodesFactory)
            : base(new CategoriesList(), nodesFactory.createNodesList())
        {
            setNodesFactory(new DecisionNodesFactory());
            treeRoot = 0;
            setExplanation(retrieveExplanation(getClass().getName()));
        }

        public UserDefinedTree()
            : this(new DecisionNodesFactory())
        {
        }

        public UserDefinedTree(IDecisionCategories classes, IDecisionRuleList rules)
            : this()
        {
            this.categories = classes;
            this.rules = rules;
            if (rules is Observable)
                ((Observable)rules).addObserver(this);
            if (categories is Observable)
                ((Observable)categories).addObserver(this);
        }

        public UserDefinedTree(IDecisionCategories classes, String[] customRules, int[][] customTransitions, IDecisionNodesFactory nodesFactory)
            : base(classes)
        {
            setNodesFactory(nodesFactory);
            treeRoot = 0;
            this.rules = nodesFactory.createNodesList(getCategories(), customRules, customTransitions);
            if (rules is Observable)
                ((Observable)rules).addObserver(this);
            if (categories is Observable)
                ((Observable)categories).addObserver(this);
            setExplanation(retrieveExplanation(getClass().getName()));
        }

        protected override IDecisionRuleList initRules()
        {
            treeRoot = 0;
            rules = nodesFactory.createNodesList();
            return rules;
        }

        protected override void setTransitions(int[][] customTransitions)
        {
            DefaultCategory key = new DefaultCategory("", 1);
            if (rules is DecisionNodesList)
                for (int i = 0; i < customTransitions.length; i++)
                {
                    DecisionNode node = ((DecisionNodesList)rules).getNode(i);
                    if (customTransitions[i][0] != 0)
                        node.setBranch(false, ((DecisionNodesList)rules).getNode(customTransitions[i][0] - 1));
                    if (customTransitions[i][1] != 0)
                        node.setBranch(true, ((DecisionNodesList)rules).getNode(customTransitions[i][1] - 1));
                    if (customTransitions[i][2] != 0)
                    {
                        key.setID(customTransitions[i][2]);
                        node.setCategory(false, categories.getCategory(key));
                    }
                    if (customTransitions[i][3] != 0)
                    {
                        key.setID(customTransitions[i][3]);
                        node.setCategory(true, categories.getCategory(key));
                    }
                }
        }

        protected void visitRules(DecisionNode rule)
        {
            if (rule == null)
                return;
            if (rule.isVisited())
                return;
            rule.setVisited(true);
            DecisionNode node = (DecisionNode)rule.getBranch(true);
            if (node != null)
                visitRules(node);
            node = (DecisionNode)rule.getBranch(false);
            if (node != null)
                visitRules(node);
        }

        public IDecisionRuleList hasUnreachableRules()
        {
            UnvisitedRules p = new UnvisitedRules(nodesFactory.createNodesList());
            try
            {
                walkRules(getTopRule(), p);
                return p.getUnvisited();
            }
            catch (Exception x)
            {
                return null;
            }
        }

        protected void visitCategories(DecisionNode rule, IDecisionCategories usedCategories)
        {
            bool[] b = new bool[] { false, true };
            if (rule == null)
                return;
            if (rule.isVisited())
                return;
            rule.setVisited(true);
            for (int i = 0; i < b.length; i++)
            {
                IDecisionCategory c = rule.getCategory(b[i]);
                if (c != null)
                    usedCategories.addCategory(c);
            }
            DecisionNode node = (DecisionNode)rule.getBranch(true);
            if (node != null)
                visitCategories(node, usedCategories);
            node = (DecisionNode)rule.getBranch(false);
            if (node != null)
                visitCategories(node, usedCategories);
        }

        public IDecisionCategories hasUnusedCategories()
        {
            DecisionNodesList nodes = (DecisionNodesList)rules;
            for (int i = 0; i < nodes.size(); i++)
                nodes.getNode(i).setVisited(false);
            IDecisionCategories usedCategories = new CategoriesList();
            visitCategories(nodes.getNode(treeRoot), usedCategories);
            if (getCategories().size() == usedCategories.size())
                return null;
            IDecisionCategories unusedCategories = new CategoriesList();
            Iterator<IDecisionCategory> iterator = getCategories().iterator();
            while (iterator.hasNext())
            {
                IDecisionCategory c = iterator.next();
                if (usedCategories.getCategory(c) == null)
                    unusedCategories.addCategory(c);
            }
            return unusedCategories;
        }

        protected override bool verifyRules(IAtomContainer mol, IDecisionResult result, IDecisionRule topRule)
        {
            if (mol == null)
                return false;
            int classID = 0;
            bool answer = false;
            IDecisionRule rule = topRule;
            if (rule == null)
                throw new DecisionMethodException("Top rule not defined! Possible reason: Empty decision tree!");
            IAtomContainerSet molsToAnalyze = null;
            IAtomContainer molToAnalyze = mol;
            IDecisionCategory category_on_error = null;
            while (classID == 0)
            {
                category_on_error = null;
                try
                {
                    if (molsToAnalyze == null)
                    {
                        rule.clearFlags(molToAnalyze);
                        answer = rule.verifyRule(molToAnalyze);
                    }
                    else
                    {
                        return verifyResidues(molsToAnalyze, result, rule);
                    }
                }
                catch (DRuleNotImplemented x)
                {
                    if (falseIfRuleNotImplemented)
                    {
                        logger.log(Level.WARNING, x.getMessage(), x);
                        answer = false;
                    }
                    else
                        throw new DecisionMethodException(x);
                }
                catch (DRuleException x)
                {
                    logger.log(Level.FINE, x.getMessage(), x);
                    category_on_error = x.getCategory2assign();
                    answer = x.isAnswer();
                }
                try
                {
                    result.addRuleResult(rule, answer, mol);
                    MolFlags mf = (MolFlags)molToAnalyze.getProperty(MolFlags.MOLFLAGS);
                    if (mf == null)
                        throw new DecisionMethodException(AbstractRule.ERR_STRUCTURENOTPREPROCESSED);
                    molsToAnalyze = mf.getResidues();
                    mf.setResidues(null);
                }
                catch (DecisionResultException x)
                {
                    throw new DecisionMethodException(x);
                }
                IDecisionCategory nextCategory = null;
                if (category_on_error != null)
                    nextCategory = category_on_error;
                else
                    nextCategory = getCategory(rule, answer);
                if (nextCategory != null)
                    result.setCategory(nextCategory);
                IDecisionRule nextRule = getBranch(rule, answer);
                if (nextRule == null)
                {
                    return true;
                }
                else
                    rule = nextRule;
            }
            return false;
        }

        public IDecisionRule getBranch(IDecisionRule rule, bool answer)
        {
            if (rule is DecisionNode)
                return ((DecisionNode)rule).getBranch(answer);
            else
                return null;
        }

        public IDecisionRule getBranch(int ruleId, bool answer)
        {
            DecisionNodesList nodes = (DecisionNodesList)rules;
            return (nodes.getNode(ruleId)).getBranch(answer);
        }

        public IDecisionCategory getCategory(IDecisionRule rule, bool answer)
        {
            if (rule is DecisionNode)
                return ((DecisionNode)rule).getCategory(answer);
            else
                return null;
        }

        public IDecisionCategory getCategory(int ruleId, bool answer)
        {
            DecisionNodesList nodes = (DecisionNodesList)rules;
            return (nodes.getNode(ruleId)).getCategory(answer);
        }

        public bool isEditable()
        {
            return editable;
        }

        public void setEditable(bool value)
        {
            editable = value;
        }

        public override IDecisionRule getRule(int id)
        {
            DecisionNodesList nodes = (DecisionNodesList)rules;
            return nodes.getNode(id);
        }

        public override Object clone()
        {
            throw new CloneNotSupportedException();
        }

        public override String toString()
        {
            return getTitle();
        }

        public override void setParameters(IAtomContainer mol)
        {
            if (getInteractive())
            {
                JComponent c = optionsPanel(mol);
                if (c != null)
                    JOptionPane.showMessageDialog(null, c, getTitle(), JOptionPane.PLAIN_MESSAGE);
            }
        }

        private void readObject(ObjectInputStream @in)
        {
            Thread.currentThread().setContextClassLoader(Introspection.getLoader());
            @in.defaultReadObject();
            if (rules is Observable)
                ((Observable)rules).addObserver(this);
            categories.clear();
            for (int i = 0; i < rules.size(); i++)
            {
                IDecisionCategory c = getCategory(rules.get(i), true);
                if ((c != null) && (categories.indexOf(c) == -1))
                    categories.add(c);
                c = getCategory(rules.get(i), false);
                if ((c != null) && (categories.indexOf(c) == -1))
                    categories.add(c);
            }
            if (categories is Observable)
                ((Observable)categories).addObserver(this);
            modified = false;
        }

        public IDecisionNodesFactory getNodesFactory()
        {
            return nodesFactory;
        }

        public void setNodesFactory(IDecisionNodesFactory nodesFactory)
        {
            this.nodesFactory = nodesFactory;
        }

        public bool getInteractive()
        {
            return options.isInteractive();
        }

        public void setInteractive(bool value)
        {
            setOptions(options.setInteractive(value));
        }

        protected ArrayResult createArrayResult(int length)
        {
            return new ArrayResult<String>(new String[length]);
        }

        protected void setArrayValue(ArrayResult result, int index, IAtomContainer mol, String propertyName)
        {
            result.set(index, mol.getProperty(propertyName).toString());
        }

        public DescriptorValue calculate(IAtomContainer mol)
        {
            String[] descriptorNames = null;
            IDecisionResult result = createDecisionResult();
            try
            {
                result.classify(mol);
                result.assignResult(mol);
                String[] d = result.getResultPropertyNames();
                descriptorNames = new String[d.length + 1];
                ArrayResult value = createArrayResult(descriptorNames.length);
                for (int i = 0; i < d.length; i++)
                    try
                    {
                        setArrayValue(value, i, mol, d[i]);
                        descriptorNames[i] = d[i];
                    }
                    catch (Exception x)
                    {
                        value.set(i, null);
                    }
                StringBuffer b = result.explain(true);
                value.set(descriptorNames.length - 1, b.toString());
                descriptorNames[descriptorNames.length - 1] = String.format("%s#explanation", getTitle());
                return new DescriptorValue(getSpecification(), getParameterNames(), getParameters(), value, descriptorNames);
            }
            catch (DecisionResultException x)
            {
                descriptorNames = result.getResultPropertyNames();
                return new DescriptorValue(getSpecification(), getParameterNames(), getParameters(), null, descriptorNames, x);
            }
        }

        public String[] getDescriptorNames()
        {
            IDecisionResult result = createDecisionResult();
            return result.getResultPropertyNames();
        }

        public IDescriptorResult getDescriptorResultType()
        {
            return new StringDescriptorResultType("");
        }

        public String[] getParameterNames()
        {
            return null;
        }

        public Object[] getParameters()
        {
            return null;
        }

        public Object getParameterType(String arg0)
        {
            return null;
        }

        public void setParameters(Object[] arg0)
        {
        }

        public DescriptorSpecification getSpecification()
        {
            return new DescriptorSpecification("http://toxtree.sourceforge.net", getTitle(), this.getClass().getName(), "Toxtree plugin");
        }

        public void removeListener()
        {
            this.listener = null;
            IDecisionRuleList rules = getRules();
            for (int i = 0; i < rules.size(); i++)
            {
                IDecisionRule rule = rules.get(i);
                if (rule is DecisionNode)
                    rule = ((DecisionNode)rule).getRule();
                if (rule is IDecisionInteractive)
                    ((IDecisionInteractive)rule).removeListener();
            }
        }

        public override List<Property> process(UserDefinedTree target)
        {
            IDecisionResult result = createDecisionResult();
            return result.getResultProperties();
        }

        public override void setEnabled(bool value)
        {
        }

        public override bool isEnabled()
        {
            return true;
        }

        public override long getID()
        {
            return 0;
        }

        public override void close()
        {
        }

        public override void open()
        {
        }
    }

    class UnvisitedRules : IProcessRule
    {
        protected DecisionNodesList unvisited;

        protected IDecisionMethod tree;

        public UnvisitedRules(DecisionNodesList unvisited)
        {
            setUnvisited(unvisited);
        }

        public void init(IDecisionMethod method)
        {
            unvisited.clear();
            setTree(method);
        }

        public Object process(IDecisionMethod method, IDecisionRule rule)
        {
            if (rule is DecisionNode)
            {
                ((DecisionNode)rule).setVisited(true);
            }
            return null;
        }

        public void done()
        {
            if (tree == null)
                return;
            DecisionNodesList nodes = (DecisionNodesList)tree.getRules();
            for (int i = 0; i < nodes.size(); i++)
            {
                IDecisionRule rule = nodes.getNode(i);
                if (!nodes.getNode(i).isVisited())
                {
                    if (unvisited == null)
                        unvisited = new DecisionNodesList();
                    unvisited.addNode(nodes.getNode(i));
                }
            }
        }

        public DecisionNodesList getUnvisited()
        {
            return unvisited;
        }

        public void setUnvisited(DecisionNodesList unvisited)
        {
            this.unvisited = unvisited;
        }

        public IDecisionMethod getTree()
        {
            return tree;
        }

        public void setTree(IDecisionMethod tree)
        {
            this.tree = tree;
            DecisionNodesList nodes = (DecisionNodesList)tree.getRules();
            for (int i = 0; i < nodes.size(); i++)
                nodes.getNode(i).setVisited(false);
        }
    }
}
