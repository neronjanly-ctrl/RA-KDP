using System;
using System.Collections.Generic;

namespace GenericComputationPlatform.ViewModels;

public static class RaTargetCategories
{
    public static IReadOnlyList<RaTargetCategoryDefinition> All { get; } = new List<RaTargetCategoryDefinition>
    {
        new()
        {
            Key = "immune-cytokine-signaling",
            Name = "Immune & Cytokine Signaling",
            Description = "Core inflammatory amplification, innate/adaptive immune signaling, cytokine response, and NF-kB/JAK-STAT pathway regulation in RA.",
            Symbols = CreateSymbols(
                "AHR_HUMAN","BTK_HUMAN","CASP1_HUMAN","CCR5_HUMAN","CXCR1_HUMAN","CXCR4_HUMAN",
                "E2AK2_HUMAN","GCR_HUMAN","IKBA_HUMAN","IKKA_HUMAN","IKKB_HUMAN","IL2_HUMAN",
                "IL2RB_HUMAN","IL6_HUMAN","JAK1_HUMAN","JAK2_HUMAN","NFKB1_HUMAN","NOS2_HUMAN",
                "RELB_HUMAN","STAT3_HUMAN","SYK_HUMAN","TF65_HUMAN","TLR7_HUMAN","TNFA_HUMAN","TYK2_HUMAN")
        },
        new()
        {
            Key = "stromal-signaling-proliferation",
            Name = "Synoviocyte / Stromal Signaling & Proliferation",
            Description = "Synoviocyte activation, stromal survival programs, kinase signaling, and proliferation-associated control.",
            Symbols = CreateSymbols(
                "AKT1_HUMAN","FOXO3_HUMAN","HCK_HUMAN","KI20A_HUMAN","MK01_HUMAN","MK14_HUMAN",
                "PLK1_HUMAN","PTEN_HUMAN","PTN1_HUMAN","RAF1_HUMAN","SRC_HUMAN")
        },
        new()
        {
            Key = "tissue-remodeling-angiogenesis",
            Name = "Tissue Remodeling, Invasion & Angiogenesis",
            Description = "Synovial invasion, extracellular matrix degradation, cytoskeleton remodeling, endothelial activation, and angiogenic support.",
            Symbols = CreateSymbols(
                "FAK1_HUMAN","FAK2_HUMAN","HGF_HUMAN","MET_HUMAN","MMP2_HUMAN","MMP8_HUMAN",
                "MMP9_HUMAN","NOS3_HUMAN","RAC1_HUMAN","RHOA_HUMAN","VGFR2_HUMAN","HMR1_HUMAN","TNAP3_HUMAN")
        },
        new()
        {
            Key = "autoimmunity-susceptibility",
            Name = "Autoimmunity & RA Susceptibility",
            Description = "RA-associated autoimmune predisposition, antigen presentation, and citrullination-related disease specificity.",
            Symbols = CreateSymbols(
                "DRB1_HUMAN","PADI4_HUMAN","PTN22_HUMAN")
        },
        new()
        {
            Key = "metabolic-drug-response",
            Name = "Metabolic, Lipid Mediator & Drug-Response Support",
            Description = "Metabolic adaptation, eicosanoid signaling, transport, and drug-response related supportive regulation.",
            Symbols = CreateSymbols(
                "A1AG1_HUMAN","A1AG2_HUMAN","ABCG2_HUMAN","PD2R2_HUMAN","PE2R4_HUMAN","PGH1_HUMAN",
                "PGH2_HUMAN","PPARA_HUMAN","PPARG_HUMAN","PPIA_HUMAN","S19A1_HUMAN","XCT_HUMAN")
        }
    };

    private static IReadOnlySet<string> CreateSymbols(params string[] symbols)
    {
        return new HashSet<string>(symbols, StringComparer.OrdinalIgnoreCase);
    }
}