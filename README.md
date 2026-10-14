# RA-KDP

**RA-KDP (Rheumatoid Arthritis Knowledge and Docking Platform)** is a publicly accessible, RA-focused computational platform for compound-oriented multi-target analysis.

The platform integrates a curated rheumatoid arthritis target panel with structure-based docking, ligand-based chemical-reference similarity analysis, network-style visualization, auxiliary analytics, and a retrieval-augmented AI assistant for RA-related information retrieval.

## Overview

RA-KDP was developed to support compound-oriented exploration within a curated rheumatoid arthritis target space.

The current release includes:

- **64 curated human RA-associated targets**
- **109 prepared receptor–pocket models**
- **User-selectable target scope**
  - complete 64-target panel
  - validated non-empty user-defined target subset
- **HTDocking**, currently implemented using JDock 2.2.3c
- **TargetHunter** ligand-based chemical-reference similarity analysis
- **Spider Plot** visualization of retained compound–target relationships
- **Auxiliary Analytics** for target-level summaries and result prioritization
- **Retrieval-augmented AI assistant** for RA-related knowledge retrieval and question answering

The validated target list selected for each job defines the computational scope of both the structural HTDocking and chemical-reference TargetHunter branches.

## Repository structure

```text
RA-KDP/
├── GenericComputationPlatform/
├── MasterProtein/
├── RatCvsSimulation/
└── .gitignore
