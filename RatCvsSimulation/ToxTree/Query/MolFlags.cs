using System;
using Logger = java.util.logging.Logger;
using IAtomContainer = org.openscience.cdk.interfaces.IAtomContainer;
using IAtomContainerSet = org.openscience.cdk.interfaces.IAtomContainerSet;
using IRingSet = org.openscience.cdk.interfaces.IRingSet;
using SilentChemObjectBuilder = org.openscience.cdk.silent.SilentChemObjectBuilder;
using MoleculeTools = ambit2.core.data.MoleculeTools;

namespace toxTree.query
{
    public class MolFlags
    {
        [NonSerialized]
        protected static Logger logger = Logger.getLogger(typeof(MolFlags).getName());

        public readonly static String MOLFLAGS = "CRAMERFLAGS";

        public readonly static String PARENT = "PARENT";

        protected bool analysed = false;

        protected bool openChain = false;

        protected bool aliphatic = false;

        protected bool alicyclic = false;

        protected bool acetylenic = false;

        protected bool aromatic = false;

        protected bool heteroaromatic = false;

        protected bool heterocyclic = false;

        protected bool heterocyclic3 = false;

        protected int aromaticRings = 0;

        protected IRingSet ringset = null;

        protected IAtomContainerSet hydrolysisProducts = null;

        protected IAtomContainerSet residues = null;

        public MolFlags()
            : base()
        {
        }

        public void clear()
        {
            openChain = false;
            analysed = false;
            aliphatic = false;
            alicyclic = false;
            acetylenic = false;
            aromatic = false;
            heteroaromatic = false;
            heterocyclic = false;
            heterocyclic3 = false;
            aromaticRings = 0;
        }

        public bool isAcetylenic()
        {
            return acetylenic;
        }

        public void setAcetylenic(bool acetylenic)
        {
            this.acetylenic = acetylenic;
        }

        public bool isAlicyclic()
        {
            return alicyclic;
        }

        public void setAlicyclic(bool alicyclic)
        {
            this.alicyclic = alicyclic;
        }

        public bool isAliphatic()
        {
            return aliphatic;
        }

        public void setAliphatic(bool aliphatic)
        {
            this.aliphatic = aliphatic;
        }

        public bool isAnalysed()
        {
            return analysed;
        }

        public void setAnalysed(bool analysed)
        {
            this.analysed = analysed;
        }

        public bool isAromatic()
        {
            return aromatic;
        }

        public void setAromatic(bool aromatic)
        {
            this.aromatic = aromatic;
        }

        public bool isHeterocyclic()
        {
            return heterocyclic;
        }

        public void setHeterocyclic(bool heterocyclic)
        {
            this.heterocyclic = heterocyclic;
        }

        public IRingSet getRingset()
        {
            return ringset;
        }

        public void setRingset(IRingSet ringset)
        {
            this.ringset = ringset;
        }

        public bool isHeteroaromatic()
        {
            return heteroaromatic;
        }

        public void setHeteroaromatic(bool heteroaromatic)
        {
            this.heteroaromatic = heteroaromatic;
        }

        public bool isHeterocyclic3()
        {
            return heterocyclic3;
        }

        public void setHeterocyclic3(bool heterocyclic3)
        {
            this.heterocyclic3 = heterocyclic3;
        }

        public int getAromaticRings()
        {
            return aromaticRings;
        }

        public void setAromaticRings(int aromaticRings)
        {
            this.aromaticRings = aromaticRings;
        }

        public bool isOpenChain()
        {
            return openChain;
        }

        public void setOpenChain(bool openChain)
        {
            this.openChain = openChain;
        }

        public IAtomContainerSet getHydrolysisProducts()
        {
            return hydrolysisProducts;
        }

        public void setHydrolysisProducts(IAtomContainerSet hydrolysisProducts)
        {
            if (hydrolysisProducts != null)
                logger.fine("Hydrolysis products already exists");
            this.hydrolysisProducts = hydrolysisProducts;
            logger.fine("Hydrolysis products set");
        }

        public IAtomContainerSet getResidues()
        {
            return residues;
        }

        public void setResidues(IAtomContainerSet residues)
        {
            this.residues = residues;
        }

        public void addResidue(IAtomContainer residue)
        {
            if (residues == null)
                this.residues = MoleculeTools.newAtomContainerSet(SilentChemObjectBuilder.getInstance());
            residues.addAtomContainer(residue);
        }

        public override String toString()
        {
            return "";
        }
    }
}
