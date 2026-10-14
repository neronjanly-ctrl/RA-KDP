using System;
using Component = java.awt.Component;
using File = java.io.File;
using List = java.util.List;
using Observable = java.util.Observable;
using Level = java.util.logging.Level;
using Logger = java.util.logging.Logger;
using Elements = org.openscience.cdk.config.Elements;
using IAtomContainer = org.openscience.cdk.interfaces.IAtomContainer;
using IMolecule = org.openscience.cdk.interfaces.IMolecule;
using SilentChemObjectBuilder = org.openscience.cdk.silent.SilentChemObjectBuilder;
using IDecisionCategories = toxTree.core.IDecisionCategories;
using IDecisionResult = toxTree.core.IDecisionResult;
using IMoleculesIterator = toxTree.core.IMoleculesIterator;
using CategoryFilter = toxTree.data.CategoryFilter;
using FilterException = toxTree.exceptions.FilterException;
using ToxTreeIOException = toxTree.exceptions.ToxTreeIOException;
using MoleculeTools = ambit2.core.data.MoleculeTools;

namespace toxtree.data
{
    public class DataContainer : Observable
    {
        protected static Logger logger = null;

        protected IMoleculesIterator containers = null;

        protected File processingFile = null;

        protected bool modified = false;

        protected bool enabled = true;

        public DataContainer()
            : this(null)
        {
        }

        public DataContainer(File inputFile)
            : base()
        {
            if (logger == null)
                logger = Logger.getLogger(typeof(DecisionMethodData).getName());
            containers = new FilteredMoleculesIterator();
            if (inputFile != null)
                openFile(inputFile);
            setChanged();
            notifyObservers();
        }

        public IAtomContainer getMolecule()
        {
            return containers.getMolecule();
        }

        public void newMolecule()
        {
            if (!enabled)
                return;
            IMolecule mol = MoleculeTools.newMolecule(SilentChemObjectBuilder.getInstance());
            mol.setProperty("SMILES", "");
            newMolecule(mol);
        }

        public void newMolecule(IAtomContainer molecule)
        {
            if (!enabled)
                return;
            containers.clear();
            addMolecule(molecule);
        }

        public void addMolecule()
        {
            if (!enabled)
                return;
            IMolecule mol = MoleculeTools.newMolecule(SilentChemObjectBuilder.getInstance());
            mol.addAtom(MoleculeTools.newAtom(SilentChemObjectBuilder.getInstance(), Elements.CARBON));
            addMolecule(mol);
        }

        public void addMolecule(IAtomContainer molecule)
        {
            if (!enabled)
                return;
            containers.addMolecule(molecule);
            modified = true;
            containers.last();
            setChanged();
            notifyObservers();
        }

        public void setMolecule(IAtomContainer molecule)
        {
            if (!enabled)
                return;
            containers.setMolecule(molecule);
            modified = true;
            setChanged();
            notifyObservers();
        }

        public void clearFilters()
        {
            if (containers is FilteredMoleculesIterator)
            {
                ((FilteredMoleculesIterator)containers).deleteAllFilters();
                setChanged();
                notifyObservers();
            }
            else
                throw new FilterException("Filtering not supported!");
        }

        public void selectFilter(Component parentComponent)
        {
            if (containers is FilteredMoleculesIterator)
            {
                ((FilteredMoleculesIterator)containers).selectFilter(parentComponent);
                setChanged();
                notifyObservers();
            }
            else
                throw new FilterException("Filtering not supported!");
        }

        public void filter(IDecisionResult treeResult)
        {
            if (treeResult == null)
                throw new FilterException("Filter not defined!");
            IDecisionCategories categories = treeResult.getDecisionMethod().getCategories();
            if (categories == null)
                throw new FilterException("Categories not defined!");
            if (containers is FilteredMoleculesIterator)
            {
                List<CategoryFilter> f = treeResult.getFilters();
                if (f != null)
                {
                    for (int i = 0; i < f.size(); i++)
                        ((FilteredMoleculesIterator)containers).addFilter(f.get(i));
                    setChanged();
                    notifyObservers();
                }
            }
            else
                throw new FilterException("Filtering not supported!");
        }

        public int gotoRecord(int record)
        {
            if ((record >= 0) && (record < getMoleculesCount()))
            {
                IAtomContainer ac = containers.setCurrentNo(record - 1);
                setChanged();
                notifyObservers(ac);
                return record;
            }
            else
                return getCurrentNo();
        }

        public int lookup(String field, Object value)
        {
            return lookup(field, value, false);
        }

        public int lookup(String field, Object value, bool silent)
        {
            if ((field == null) || (value == null))
                return -1;
            IAtomContainer ac = null;
            for (int record = 0; record < containers.getMoleculesCount(); record++)
            {
                try
                {
                    ac = containers.getAtomContainer(record);
                    if (ac.getProperty(field) == null)
                        continue;
                    String property = ac.getProperty(field).toString().toLowerCase();
                    if (value.equals(property))
                    {
                        if (!silent)
                        {
                            containers.setCurrentNo(record);
                            setChanged();
                            notifyObservers(ac);
                        }
                        return record;
                    }
                }
                catch (Exception x)
                {
                    logger.log(Level.WARNING, "Error when processing record %s %s", new Object[]
                    {
                        record,
                        x,
                    });
                    x.printStackTrace();
                }
                ac = null;
            }
            return -1;
        }

        public void openFile(File input)
        {
            if (!enabled)
                return;
            this.processingFile = input;
            containers.setReading();
            setChanged();
            notifyObservers();
            List molecules = null;
            try
            {
                molecules = containers.openFile(processingFile);
                if (molecules != null)
                    containers.setMolecules(molecules);
                containers.setDone(true);
                modified = false;
            }
            catch (ToxTreeIOException x)
            {
                containers.setDone(false);
                logger.log(Level.SEVERE, x.getMessage(), x);
                String msg = "";
                if ((x.getCause() != null) && (x.getCause().getMessage() != null))
                    msg = x.getCause().getMessage();
                else
                    msg = "Error";
                ToxTreeActions.showMsg(x.getMessage() + "\n" + x.getFilename(), msg);
            }
            setChanged();
            notifyObservers();
        }

        public void saveFile(File output)
        {
            if (!enabled)
                return;
            this.processingFile = output;
            containers.setWriting();
            setChanged();
            notifyObservers();
            try
            {
                containers.saveFile(processingFile);
                modified = false;
                containers.setDone(true);
                ToxTreeActions.showMsg("File saved ", processingFile.getName());
            }
            catch (ToxTreeIOException x)
            {
                logger.log(Level.SEVERE, x.getMessage(), x);
                containers.setDone(false);
                ToxTreeActions.showMsg(x.getMessage(), x.getFilename());
            }
            setChanged();
            notifyObservers();
        }

        public int getMoleculesCount()
        {
            return containers.getMoleculesCount();
        }

        public int getCurrentNo()
        {
            return containers.getCurrentNo();
        }

        public Object first()
        {
            if (!enabled)
                return null;
            Object o = containers.first();
            setChanged();
            notifyObservers();
            return o;
        }

        public Object last()
        {
            if (!enabled)
                return null;
            Object o = containers.last();
            setChanged();
            notifyObservers();
            return o;
        }

        public Object next()
        {
            if (!enabled)
                return null;
            Object o = containers.next();
            setChanged();
            notifyObservers();
            return o;
        }

        public Object prev()
        {
            if (!enabled)
                return null;
            Object o = containers.prev();
            setChanged();
            notifyObservers();
            return o;
        }

        public bool isProcessing()
        {
            return containers.isReading() || containers.isWriting();
        }

        public String getStatus()
        {
            return containers.getStatus();
        }

        public override String toString()
        {
            return containers.toString();
        }

        public bool loadedFromFile()
        {
            return !containers.getFilename().equals("");
        }

        public String getFileName()
        {
            if (modified)
                return containers.getFilename() + '*';
            else
                return containers.getFilename();
        }

        public bool isEnabled()
        {
            return enabled;
        }

        public void setEnabled(bool enabled)
        {
            this.enabled = enabled;
        }
    }
}
