using System;
using File = java.io.File;
using Level = java.util.logging.Level;
using IAtomContainer = org.openscience.cdk.interfaces.IAtomContainer;
using IDecisionCategories = toxTree.core.IDecisionCategories;
using IDecisionCategory = toxTree.core.IDecisionCategory;
using IDecisionMethodsList = toxTree.core.IDecisionMethodsList;
using IDecisionResult = toxTree.core.IDecisionResult;
using DecisionResultException = toxTree.exceptions.DecisionResultException;
using DecisionResultsList = toxTree.tree.DecisionResultsList;
using ConfusionMatrix = toxTree.tree.stats.ConfusionMatrix;

namespace toxtree.data
{
    public class DecisionMethodData : DataContainer
    {
        public DecisionMethodData(File inputFile)
            : base(inputFile)
        {
        }

        protected void setResult(IDecisionResult treeResult, IAtomContainer molecule)
        {
            if (!enabled)
                return;
            try
            {
                treeResult.assignResult(molecule);
                modified = true;
                notifyObservers();
            }
            catch (Exception x)
            {
                logger.log(Level.SEVERE, x.getMessage(), x);
            }
        }

        public void classify(IDecisionResult treeResult)
        {
            if (!enabled)
                return;
            IAtomContainer ac = null;
            try
            {
                treeResult.getDecisionMethod().setParameters((IAtomContainer)containers.getMolecule());
                ac = (IAtomContainer)containers.getMolecule().clone();
            }
            catch (Exception x)
            {
                logger.log(Level.SEVERE, x.getMessage(), x);
                ac = containers.getMolecule();
            }
            logger.log(Level.FINER, "Start classify");
            treeResult.classify(ac);
            logger.log(Level.FINER, "End classify");
            logger.log(Level.FINEST, treeResult.toString());
            setResult(treeResult, getMolecule());
            logger.log(Level.FINER, "Result set");
            ac = null;
        }

        public ConfusionMatrix classifyAll(IDecisionResult treeResult)
        {
            if (!enabled)
                return null;
            IAtomContainer ac = null;
            double n = containers.getMoleculesCount();
            treeResult.setNotify(false);
            ConfusionMatrix<Comparable, IDecisionCategory> cmatrix = new ConfusionMatrix<Comparable, IDecisionCategory>();
            cmatrix.setExpectedTitle("All");
            cmatrix.setPredictedTitle(treeResult.getDecisionMethod().getTitle());
            for (int record = 0; record < n; record++)
            {
                try
                {
                    IAtomContainer c = (IAtomContainer)containers.getAtomContainer(record);
                    treeResult.getDecisionMethod().setParameters(c);
                    ac = (IAtomContainer)c.clone();
                    treeResult.classify(ac);
                    ac = null;
                    setResult(treeResult, containers.getAtomContainer(record));
                    IDecisionCategories categories = treeResult.getAssignedCategories();
                    foreach (IDecisionCategory category in categories)
                    {
                        cmatrix.addEntry("All", category);
                    }
                }
                catch (Exception x)
                {
                    logger.log(Level.WARNING, String.format("Error when processing record %d", record), x);
                    x.printStackTrace();
                }
                ac = null;
            }
            treeResult.setNotify(true);
            first();
            modified = true;
            return cmatrix;
        }

        public ConfusionMatrix classifyAll(IDecisionMethodsList methods)
        {
            ConfusionMatrix matrix = null;
            IAtomContainer ac = null;
            setEnabled(false);
            containers.setReading();
            setChanged();
            notifyObservers();
            for (int record = 0; record < containers.getMoleculesCount(); record++)
            {
                try
                {
                    for (int i = 0; i < methods.size(); i++)
                    {
                        IDecisionResult treeResult = ((DecisionResultsList)methods).getResult(i);
                        treeResult.getDecisionMethod().setParameters((IAtomContainer)containers.getAtomContainer(record));
                        treeResult.setNotify(false);
                    }
                    ac = (IAtomContainer)containers.getAtomContainer(record).clone();
                    for (int i = 0; i < methods.size(); i++)
                    {
                        IDecisionResult treeResult = ((DecisionResultsList)methods).getResult(i);
                        try
                        {
                            treeResult.classify(ac);
                            setResult(treeResult, containers.getAtomContainer(record));
                        }
                        catch (DecisionResultException x)
                        {
                            logger.log(Level.SEVERE, x.getMessage(), x);
                            x.printStackTrace();
                            treeResult.clear();
                            containers.setDone(false);
                        }
                    }
                    ac = null;
                }
                catch (Exception x)
                {
                    logger.log(Level.SEVERE, String.format("Error when processing record %d", record), x);
                    x.printStackTrace();
                }
                ac = null;
            }
            for (int i = 0; i < methods.size(); i++)
            {
                IDecisionResult treeResult = ((DecisionResultsList)methods).getResult(i);
                treeResult.setNotify(true);
            }
            setEnabled(true);
            first();
            modified = true;
            containers.setDone(true);
            setChanged();
            notifyObservers();
            return matrix;
        }

        public void filterAll(IDecisionResult treeResult)
        {
            if (!enabled)
                return;
            IAtomContainer ac = null;
            for (int record = 0; record < containers.getMoleculesCount(); record++)
            {
                try
                {
                    ac = (IAtomContainer)containers.getAtomContainer(record).clone();
                    treeResult.classify(ac);
                    ac = null;
                    setResult(treeResult, containers.getAtomContainer(record));
                }
                catch (Exception x)
                {
                    logger.log(Level.WARNING, String.format("Error when processing record %d", record), x);
                    x.printStackTrace();
                }
                ac = null;
            }
            first();
            modified = true;
        }
    }
}
