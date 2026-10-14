using CommonTools;

namespace ChemicalFunctions
{
    /// <summary>
    /// Except the first 3 enums
    /// See https://en.wikipedia.org/wiki/Chemical_file_format#The_Chemical_MIME_Project
    /// </summary>
    public enum ChemicalFormat
    {
        [Mime("chemical/x-pdb", "RCSB Protein Data Bank format", "pdb")]
        [Match(@"^\.?pdb$", CaseSensitive = false, UseRegex = true)]
        ProteinDataBank,

        [Mime("chemical/x-pdbqt", "AutoDock PDB format", "pdbqt")]
        [Match(@"^\.?pdbqt$", CaseSensitive = false, UseRegex = true)]
        PDBQT,

        [Mime("chemical/x-mol2", "Tripos MOL2 molecule model file", "mol2")]
        [Match(@"^\.?mol2$", CaseSensitive = false, UseRegex = true)]
        TriposMoleculeModel,

        [Mime("chemical/x-alchemy", "Alchemy Format", "alc")]
        [Match(@"^\.?alc$", CaseSensitive = false, UseRegex = true)]
        Alchemy,

        [Mime("chemical/x-cache-csf", "CAChe MolStruct CSF", "csf")]
        [Match(@"^\.?csf$", CaseSensitive = false, UseRegex = true)]
        CACheMol_Struct_CSF,

        [Mime("chemical/x-cactvs-binary", "CACTVS format", "cbin", "cascii", "ctab")]
        [Match(@"^\.?(?:cbin|cascii|ctab)$", CaseSensitive = false, UseRegex = true)]
        CACTVS,

        [Mime("chemical/x-cdx", "ChemDraw eXchange file", "cdx")]
        [Match(@"^\.?cdx$", CaseSensitive = false, UseRegex = true)]
        ChemDrawExchangeFile,

        [Mime("chemical/x-cerius", "MSI Cerius II format", "cer")]
        [Match(@"^\.?cer$", CaseSensitive = false, UseRegex = true)]
        MSI_Cerius_II,

        [Mime("chemical/x-chem3d", "Chem3D Format", "c3d")]
        [Match(@"^\.?c3d$", CaseSensitive = false, UseRegex = true)]
        Chem3D,

        [Mime("chemical/x-chemdraw", "ChemDraw file", "chm")]
        [Match(@"^\.?chm$", CaseSensitive = false, UseRegex = true)]
        ChemDrawFile,

        [Mime("chemical/x-cif", "Crystallographic Information File, Crystallographic Information Framework", "cif")]
        [Match(@"^\.?cif$", CaseSensitive = false, UseRegex = true)]
        Crystallographic,

        [Mime("chemical/x-cmdf", "CrystalMaker Data format", "cmdf")]
        [Match(@"^\.?cmdf$", CaseSensitive = false, UseRegex = true)]
        CrystalMakerData,

        [Mime("chemical/x-cml", "Chemical Markup Language", "cml")]
        [Match(@"^\.?cml$", CaseSensitive = false, UseRegex = true)]
        ChemicalMarkupLanguage,

        [Mime("chemical/x-compass", "Compass program of the Takahashi", "cpa")]
        [Match(@"^\.?cpa$", CaseSensitive = false, UseRegex = true)]
        CompassProgramTakahashi,

        [Mime("chemical/x-crossfire", "Crossfire file", "bsd")]
        [Match(@"^\.?bsd$", CaseSensitive = false, UseRegex = true)]
        CrossfireFile,

        [Mime("chemical/x-csml", "Chemical Style Markup Language", "csm", "csml")]
        [Match(@"^\.?csml?$", CaseSensitive = false, UseRegex = true)]
        ChemicalStyleMarkupLanguage,

        [Mime("chemical/x-ctx", "Gasteiger group CTX file format", "ctx")]
        [Match(@"^\.?ctx$", CaseSensitive = false, UseRegex = true)]
        GasteigerGroupCTX,

        [Mime("chemical/x-cxf", "Chemical eXchange Format", "cxf", "cef")]
        [Match(@"^\.?c[xe]f$", CaseSensitive = false, UseRegex = true)]
        ChemicalExchange,

        [Mime("chemical/x-embl-dl-nucleotide", "EMBL Nucleotide Format", "emb", "embl")]
        [Match(@"^\.?embl?$", CaseSensitive = false, UseRegex = true)]
        EMBLNucleotide,

        [Mime("chemical/x-galactic-spc", "SPC format for spectral and chromatographic data", "spc")]
        [Match(@"^\.?spc$", CaseSensitive = false, UseRegex = true)]
        SPC,

        [Mime("chemical/x-gamess-input", "GAMESS Input format", "inp", "gam", "gamin")]
        [Match(@"^\.?(?:inp|gam(?:in)?)$", CaseSensitive = false, UseRegex = true)]
        GAMESSInput,

        [Mime("chemical/x-gaussian-checkpoint", "Gaussian Checkpoint Format", "fch", "fchk")]
        [Match(@"^\.?fchk?$", CaseSensitive = false, UseRegex = true)]
        GaussianCheckpoint,

        [Mime("chemical/x-gaussian-cube", "Gaussian Cube (Wavefunction) Format", "cub")]
        [Match(@"^\.?cub$", CaseSensitive = false, UseRegex = true)]
        GaussianCube,

        [Mime("chemical/x-gaussian-input", "Gaussian Input Format", "gau", "gjc", "gjf", "com")]
        [Match(@"^\.?(?:gau|gjc|gjf|com)$", CaseSensitive = false, UseRegex = true)]
        GaussianInput,

        [Mime("chemical/x-gcg8-sequence", "Protein Sequence Format", "gcg")]
        [Match(@"^\.?gcg$", CaseSensitive = false, UseRegex = true)]
        ProteinSequence,

        [Mime("chemical/x-genbank", "ToGenBank Format", "gen")]
        [Match(@"^\.?gen$", CaseSensitive = false, UseRegex = true)]
        ToGenBank,

        [Mime("chemical/x-isostar", "IsoStar Library of Intermolecular Interactions", "istr", "ist")]
        [Match(@"^\.?istr?$", CaseSensitive = false, UseRegex = true)]
        IsoStarLibrary,

        [Mime("chemical/x-jcamp-dx", "JCAMP Spectroscopic Data Exchange Format", "jdx", "dx")]
        [Match(@"^\.?j?dx$", CaseSensitive = false, UseRegex = true)]
        JCAMPSpectroscopicDataExchange,

        [Mime("chemical/x-kinemage", "Kinetic (Protein Structure) Images; Kinemage", "kin")]
        [Match(@"^\.?kin$", CaseSensitive = false, UseRegex = true)]
        KineticImages,

        [Mime("chemical/x-macmolecule", "MacMolecule File Format", "mcm")]
        [Match(@"^\.?mcm$", CaseSensitive = false, UseRegex = true)]
        MacMolecule,

        [Mime("chemical/x-macromodel-input", "MacroModel Molecular Mechanics", "mmd", "mmod")]
        [Match(@"^\.?mmo?d$", CaseSensitive = false, UseRegex = true)]
        MacroModelMolecularMechanics,

        [Mime("chemical/x-mdl-molfile", "MDL Molfile", "mol")]
        [Match(@"^\.?mol$", CaseSensitive = false, UseRegex = true)]
        MDLMolfile,

        [Mime("chemical/x-daylight-smiles", "Simplified molecular input line entry specification", "smiles", "smi")]
        [Match(@"^\.?smi(?:les)?$", CaseSensitive = false, UseRegex = true)]
        SMILES,

        [Mime("chemical/x-mdl-sdfile", "Structure-Data File", "sdf")]
        [Match(@"^\.?sdf$", CaseSensitive = false, UseRegex = true)]
        StructureDataFile,

        [Mime("chemical/x-sketchel", "SketchEl Molecule", "el")]
        [Match(@"^\.?el$", CaseSensitive = false, UseRegex = true)]
        SketchElMolecule,

        [Mime("chemical/x-datasheet", "SketchEl XML DataSheet", "ds")]
        [Match(@"^\.?ds$", CaseSensitive = false, UseRegex = true)]
        SketchElXMLDataSheet,

        [Mime("chemical/x-inchi", "The IUPAC International Chemical Identifier", "inchi")]
        [Match(@"^\.?inchi$", CaseSensitive = false, UseRegex = true)]
        Inchi,

        [Mime("chemical/x-jsdraw", "JSDraw native file format", "jsd", "jsdraw")]
        [Match(@"^\.?jsd(?:raw)?$", CaseSensitive = false, UseRegex = true)]
        JSDrawNative,

        [Mime("chemical/x-helm", "Pistoia Alliance HELM string", "helm", "ihelm")]
        [Match(@"^\.?i?helm$", CaseSensitive = false, UseRegex = true)]
        PistoiaAllianceHELM,

        [Mime("chemical/x-xhelm", "Pistoia Alliance XHELM XML file", "xhelm")]
        [Match(@"^\.?xhelm$", CaseSensitive = false, UseRegex = true)]
        PistoiaAllianceXHELMXML,

    }
}
