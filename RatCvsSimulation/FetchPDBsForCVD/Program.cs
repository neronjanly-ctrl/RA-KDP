using ChemicalFunctions;
using CommonTools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FetchPDBsForCVD
{
    internal class Program
    {
        /// <summary>
        /// PDB entries collected by Lirong Wang published at
        /// https://www.cbligand.org/CVD/retrieve_docking_result.php?dir=wrk66817
        /// </summary>
        private static readonly string[] lirongPdbs =
        {
            "3W39", "1HSA", "3VCL", "1HSB", "2XPG", "2BCK", "1E27", "3UPR",
            "1I4F", "1Q94", "1W72", "3KWW", "1IM9", "1M05", "4JQV", "3LN4",
            "1XR8", "4JQX", "3BZF", "1A1M", "1AQD", "2SEB", "2WBJ", "3TWR",
            "4IAQ", "4IB4", "2XCW", "4H2I", "3U9X", "2WH0", "3KQ0", "3APX",
            "3CWM", "4ACQ", "3KTM", "3QAK", "3DLW", "3B96", "2JIF", "1R4L",
            "2XYD", "3UON", "3D2U", "1QUU", "3MY0", "3MTF", "3IAR", "1D1T",
            "3P0G", "3HG3", "4FTG", "2D58", "4FDC", "3CW3", "1ZUA", "3IM4",
            "4EJN", "2Q7M", "1N5U", "1CW3", "2R24", "4FNZ", "3QKG", "4FYR",
            "2FCW", "3B67", "3N56", "1JDP", "3N57", "4AOH", "4JYO", "4JZC",
            "2WXW", "4D8O", "1AZX", "1AIN", "1XJL", "2XO2", "1M9I", "2C11",
            "1Z6T", "1TH1", "1DE9", "2A01", "2OU1", "3S84", "3KIV", "2HZQ",
            "1B68", "1QUB", "2YG2", "3GD8", "3D9S", "3DOE", "3KRW", "4IE1",
            "4I06", "2G30", "2CLP", "1T2K", "3GHN", "3LJT", "4AQP", "4JBP",
            "3SOC", "2QLU", "3SP7", "2V6Q", "3I1H", "3OV6", "1HYN", "1V83",
            "4DWF", "2IMS", "4BDU", "2ABJ", "4IEH", "1K1A", "3LBZ", "1K1F",
            "1BND", "3V0L", "2VXP", "2H63", "2VM5", "3OZ1", "2UVL", "2QFA",
            "4H10", "3RPG", "1ES7", "2R52", "1BMP", "3G2F", "1REW", "3MDY",
            "1EWF", "4DBN", "3PJ3", "4FDH", "3RPX", "1A4I", "3BXK", "3OXQ",
            "2O72", "1Z93", "3O78", "3N7R", "3POS", "4EYC", "2QFE", "1MF8",
            "1S1D", "3NS7", "1NMS", "3KJQ", "2AR9", "2VAF", "1DGB", "3AI8",
            "1LYA", "1T32", "1NL6", "3H8B", "3D67", "2NSM", "1M54", "1NR2",
            "2HCI", "1DOK", "2X69", "2X6L", "1U4M", "2W96", "1W98", "4GLP",
            "4EN3", "2OSL", "1YJD", "2O3U", "1XIW", "1XIW", "1SY6", "3IOZ",
            "3LKJ", "1UUH", "2JJS", "2QAD", "2J8B", "2JA4", "1FM5", "3KG5",
            "1I8L", "1G8Q", "1I85", "1AKJ", "3EZR", "3G33", "2EUF", "1AXC",
            "1JSU", "1G3N", "3D1M", "1F6W", "1T5C", "4ENZ", "2OBD", "1RTK",
            "2V8E", "2BBO", "3COG", "1HKK", "4FTN", "2CN5", "4B0P", "1DKT",
            "1CKS", "2YHF", "2PFI", "1K0N", "3NR9", "4H10", "3N7O", "1WOJ",
            "3DLJ", "4GMJ", "1CNT", "1Q7D", "2FSE", "2V53", "2HR0", "1LI1",
            "1LI1", "4FXG", "3CU7", "1KNT", "3OJY", "3OJY", "1LF7", "1BNL",
            "3A7E", "3RUK", "3S79", "4I8V", "2HI4", "3QU8", "2NNH", "1R9O",
            "2H6P", "4GQS", "3TBG", "3E6I", "3V8D", "2YVQ", "3OED", "3EHT",
            "3EHU", "1B09", "2Y1Z", "3LCO", "3UEZ", "3CXE", "2D9Q", "1RHG",
            "1J2X", "2F53", "1I85", "3SL9", "1P6A", "3OE6", "2VHF", "3S67",
            "1OJV", "1EG4", "3GU4", "4HGA", "3AU4", "3W1B", "2JAJ", "3ZOS",
            "3DKP", "3FE2", "3R6N", "2YT4", "3N1G", "3EY4", "1RQD", "3S2K",
            "4FQX", "4FQX", "1EG3", "1WT6", "4AWN", "3L2P", "3W5O", "4I0P",
            "4I0P", "3LQZ", "3E0J", "3IKM", "4A5S", "4GG6", "4GG6", "3PDO",
            "3PBL", "1QBJ", "2REY", "3NXY", "2AZE", "3DWB", "1DYT", "1EDN",
            "2Z13", "3GXU", "3LZB", "1IVO", "4KBZ", "1FLE", "4ED5", "1FXL",
            "1B0F", "2XQN", "2PSN", "4A35", "3UCC", "3JTC", "3FXX", "3KUL",
            "3P1I", "2VWU", "1CN4", "1CN4", "2YD0", "3PP0", "3KEX", "2R4B",
            "2A1I", "2IOG", "1NDE", "2H7C", "4AVP", "3SAF", "1EX0", "3KQB",
            "3SOS", "4BDW", "1CZS", "2ZZU", "3HNB", "3LC5", "3FR4", "1HMT",
            "3AKM", "3VG7", "4I4F", "3FZT", "3TJM", "1UZP", "1UCT", "2H2R",
            "1FCG", "2FCB", "3SGJ", "1E4J", "3RJD", "2J3U", "3MPX", "3CU1",
            "2P39", "1FGA", "2FGI", "2PVF", "1RY7", "2FHI", "3RJ3", "3GHG",
            "3GHG", "1DUG", "2RKZ", "1J4R", "3RGH", "2WA7", "3QS9", "2YLE",
            "2XEG", "1FOS", "2UZK", "3QRF", "3S4M", "1FL7", "4AY9", "3HH2",
            "4IE7", "2DQ7", "2BH9", "2VWF", "2FFU", "1SMB", "2C5D", "4HC7",
            "1FB1", "4BB9", "3K23", "3FUB", "4DY0", "3S6N", "1JPL", "1A22",
            "2QKH", "2QKH", "2XWE", "1FL7", "2GLI", "4ERS", "3IOL", "1BJ4",
            "1OQJ", "2OM2", "1P8V", "2F8A", "2R37", "1FQ3", "1MZA", "3HK0",
            "3LDL", "4HJ2", "3IK7", "1XWK", "3GTU", "4IS0", "3QAG", "4PGT",
            "2C3Q", "3TZD", "3SHV", "3U5P", "3UVY", "2W72", "2YRS", "1SI4",
            "1XDT", "1I3E", "3U11", "3MAX", "3IO4", "1PV8", "1JMO", "3H0T",
            "2DB7", "1A6Z", "1ICF", "1YBW", "3SP8", "4H6J", "3I00", "2I32",
            "3AM8", "2D31", "1DQA", "1S8C", "2GYP", "2H8R", "4IQR", "1LV2",
            "1JQE", "1BZY", "3RZE", "3GDQ", "4AWO", "3NMQ", "3ATV", "3I33",
            "3FE1", "3FZF", "3Q9P", "3NUM", "3H1D", "3FGU", "3I28", "1J7V",
            "3LQM", "2VGE", "2DSQ", "1H59", "2OAY", "1P53", "4AYA", "3E4A",
            "3UX9", "1FYH", "3NW6", "2V5P", "3H9Z", "1N7M", "2AGJ", "3O11",
            "1IKN", "3BRT", "4KIK", "1INR", "3HMX", "3D85", "4GS7", "2VXS",
            "2VXT", "2ILA", "3LTQ", "1IRA", "3O4O", "1ILR", "4DOH", "3TGX",
            "3TGX", "1M4R", "3D85", "1Z92", "4GS7", "4GS7", "1QVN", "1IAR",
            "1HIJ", "3QT2", "3QT2", "1P9M", "3L5H", "1ALU", "3DI2", "1ICW",
            "3KMW", "3SE3", "3SE3", "1FG9", "2ARP", "3ETA", "2WS1", "2HI9",
            "3FAY", "3DSH", "1QQG", "3T3M", "1AOX", "3V4P", "3VI3", "3BQN",
            "1M1U", "3IJE", "3VI3", "3K6S", "2VC2", "3V4V", "1KI1", "2VJ2",
            "4I5C", "3UGC", "1NBQ", "1T2K", "3TNU", "4DNC", "4FG9", "3BHH",
            "1S1E", "3NAF", "2NZ0", "4HQA", "3BJ4", "4GOW", "3B6R", "1I0E",
            "1QK1", "4BBQ", "3AVS", "1IM9", "3VH8", "3B6V", "2R3V", "4HNA",
            "2ORV", "1T46", "3NF1", "1SPJ", "2ANW", "1B6E", "4ECC", "3IW4",
            "2I0E", "3BGM", "1YRK", "3TXO", "1XJD", "3U7D", "4D9U", "2OG8",
            "4JNK", "1I0Z", "1N7D", "3OYW", "1HLC", "3T1M", "3V6O", "1CCZ",
            "4FGU", "1QIP", "3E0G", "2Q7N", "3S95", "2PPL", "3SOO", "2R59",
            "4GKX", "3GEF", "3V99", "3LK3", "1J8E", "3S2K", "3A7Q", "2UUH",
            "3CFW", "1ESL", "1G1Q", "3A4O", "1BB5", "4DN5", "3BO8", "1QEW",
            "2WA0", "3KGA", "1ZJK", "1AN2", "1HUP", "4HW2", "2OAX", "4JVR",
            "2VCY", "3MU6", "2WL1", "2O2K", "3LQ8", "1EH8", "2WY3", "3L5T",
            "3I60", "2ZOQ", "4B99", "2YIS", "3NA3", "1Q3A", "4GR0", "2OZR",
            "3C7X", "966C", "1CK7", "1HFS", "2Y6D", "3DPE", "4H1Q", "3ORN",
            "1S9I", "3VN9", "2O2T", "2V5O", "3T1I", "2CBZ", "3THX", "3AAA",
            "2QTL", "1SM3", "3PMI", "3SZR", "1NKP", "4DOM", "3RGK", "4DB1",
            "3ZWH", "3KF9", "2YXM", "2V6H", "3H54", "3FED", "3QF2", "3MTR",
            "1HH8", "3HVL", "3N00", "4A69", "3NOI", "4F14", "3JVZ", "1DMT",
            "3F5N", "2FLU", "2AS5", "3P3Y", "1SVC", "3I0A", "1OJ6", "1SG1",
            "3RWR", "3TMH", "3P0C", "3BDW", "1HYR", "3RKQ", "4E9M", "1M4U",
            "3HR4", "1M9K", "3ETO", "2OO4", "1GG5", "3N00", "3IPQ", "3P0U",
            "3I97", "1GY5", "4AOJ", "3V5Q", "3H95", "2YZ8", "1ZY8", "1LWV",
            "3VLG", "1EVS", "3VTV", "1UVQ", "3CXD", "3ZNO", "2V5F", "4AGQ",
            "3QYM", "2XWC", "3HHM", "3U8I", "1LE6", "3F96", "4AQH", "1BY7",
            "3VW7", "4HHY", "1K78", "3VKX", "3N2Z", "2P4E", "3BIK", "3L8I",
            "3RRQ", "3G4G", "2H44", "3JWQ", "3MJK", "3MJG", "3F8U", "4EF0",
            "1IMV", "2OKN", "4EJX", "3SH4", "3MJG", "2IWI", "3ZIM", "3TT9",
            "3IFQ", "4EY0", "2W2X", "1F9Q", "1FZV", "4A4O", "1BUI", "3HM6",
            "3HCD", "3PHB", "1CQT", "2BQ8", "3KDU", "3PEQ", "1FM9", "1P9O",
            "1YND", "3DAL", "2QPW", "3D24", "3SP6", "4APU", "3HAF", "3NPZ",
            "3F6U", "3H5C", "3E0P", "1D5R", "3O2Y", "4AL1", "2IAG", "3L2J",
            "3FFD", "1ET1", "2SHP", "2CNI", "2QCT", "3PS5", "2YD8", "3QCM",
            "1P4R", "3R0N", "4FQP", "3PBB", "3SUA", "3Q7P", "3OMV", "3O3U",
            "3N7P", "1QBK", "4DM6", "4DLS", "3GFT", "3CON", "3CFS", "1YZM",
            "3UIP", "4ELJ", "1ZY7", "6RLX", "3OWN", "2GJK", "3FMZ", "1FGU",
            "4EKD", "3KUQ", "1KMQ", "2J1L", "1L3K", "2V9Q", "3TWJ", "3B0W",
            "3ZBF", "4JHN", "1LJM", "4GIW", "2GL8", "3HCM", "1KSO", "1K8U",
            "1MR8", "1IRJ", "2WC8", "3O10", "1LI4", "3U1N", "4AVV", "2B3V",
            "1SCF", "4DCK", "4GVC", "1YBO", "2NWG",
        };

        /// <summary>
        /// Protein names collected by Shifan Ma published at
        /// https://www.cbligand.org/CVD/target_list.php
        /// </summary>
        private static readonly string[] cvdProteins =
        {
            "1433S", "1A03", "2B11", "3BHS1", "5HT1A", "5HT1B", "5HT1D", "5HT1F",
            "5HT2A", "5HT2B", "5HT2C", "5HT4R", "5NTD", "A1AG1", "A1AG2", "A4",
            "AA1R", "AA2AR", "AA2BR", "AA3R", "AACT", "AAKG2", "AB17B", "ABCA1",
            "ABCC8", "ABCC9", "ABCCB", "ABCG2", "ABL1", "ACE2", "ACE", "ACH10",
            "ACHA2", "ACHA3", "ACHA4", "ACHA5", "ACHA6", "ACHA7", "ACHA9", "ACHA",
            "ACHB2", "ACHB3", "ACHB4", "ACHB", "ACHD", "ACHE", "ACHG", "ACKR1",
            "ACM1", "ACM2", "ACM3", "ACOD", "ACTB", "ACVL1", "ACVR1", "ADA10",
            "ADA15", "ADA1A", "ADA1B", "ADA1D", "ADA2A", "ADA2B", "ADA2C", "ADA",
            "ADCK4", "ADCY2", "ADCY6", "ADH1B", "ADH1G", "ADH7", "ADML", "ADRB1",
            "ADRB2", "ADRB3", "AGAL", "AGTR1", "AGTR2", "AHR", "AK1BA", "AKT1",
            "AKT2", "AL1B1", "AL5AP", "ALBU", "ALDH2", "ALDR", "ALK", "AMPD1",
            "AMPE", "AMPN", "ANDR", "ANF", "ANGI", "ANKK1", "ANO1", "ANPRA",
            "ANPRB", "ANPRC", "ANT3", "ANXA2", "AOC3", "APAF", "APEX1", "APH1B",
            "APJ", "APOA1", "APOB", "AQP4", "ARBK1", "AREG", "ARGI1", "ARGI2",
            "AT1A1", "AT1A2", "AT1B1", "AT2A2", "ATG7", "ATM", "ATP4A", "ATPB",
            "ATR", "ATS13", "ATS5", "AURKA", "AURKB", "AVR2A", "AVR2B", "B2CL1",
            "B2CL2", "B2L11", "B2LA1", "BAK", "BAX", "BAZ2A", "BCAT1", "BCL2",
            "BCR", "BGAT", "BHMT1", "BHMT2", "BIRC2", "BIRC3", "BIRC5", "BKRB1",
            "BKRB2", "BMP2", "BMP4", "BMPR2", "BMR1A", "BMR1B", "BRAF", "BTK",
            "C11B1", "C11B2", "C1TC", "C5AR1", "CA2D1", "CAC1A", "CAC1B", "CAC1C",
            "CAC1D", "CAC1E", "CAC1G", "CAC1H", "CAC1I", "CAC1S", "CACB1", "CACB2",
            "CADH2", "CAH11", "CAH12", "CAH13", "CAH14", "CAH1", "CAH2", "CAH3",
            "CAH4", "CAH5A", "CAH5B", "CAH7", "CAH9", "CALCA", "CALCR", "CALM",
            "CALRL", "CALX", "CASP1", "CASP3", "CASP8", "CASP9", "CASPA", "CASR",
            "CATB", "CATD", "CATG", "CATK", "CATL1", "CBG", "CBPB2", "CBPN",
            "CCG1", "CCL2", "CCL5", "CCNA1", "CCND1", "CCND2", "CCND3", "CCNE1",
            "CCR1", "CCR2", "CCR4", "CCR5", "CCR6", "CD14", "CD1D", "CD20",
            "CD22", "CD28", "CD2", "CD33", "CD36", "CD38", "CD3D", "CD3E",
            "CD3G", "CD4", "CD52", "CD80", "CD81", "CD86", "CDK1", "CDK2",
            "CDK3", "CDK4", "CDK5", "CDK6", "CDK7", "CDK9", "CDN1A", "CEL",
            "CENPE", "CETP", "CFAB", "CFAH", "CFTR", "CH60", "CHIT1", "CHK1",
            "CHK2", "CHLE", "CLC4C", "CLCA4", "CLK2", "CMA1", "CNR1", "CNR2",
            "CO1A1", "CO1A2", "CO2A1", "CO3A1", "CO3", "CO4A1", "CO4A2", "CO4A3",
            "CO4A4", "CO4A5", "CO4A6", "CO5A1", "CO5A2", "CO5A3", "CO5", "CO6A1",
            "CO6A3", "COBA1", "COBA2", "COIA1", "COMT", "CP17A", "CP19A", "CP1A1",
            "CP1A2", "CP21A", "CP27A", "CP2B6", "CP2C8", "CP2C9", "CP2CI", "CP2CJ",
            "CP2D6", "CP2E1", "CP2J2", "CP3A5", "CP4F2", "CPSM", "CPT1A", "CPT1B",
            "CPT2", "CREB1", "CRFR1", "CRK", "CSF1R", "CSF2R", "CSF3R", "CSK21",
            "CTLA4", "CTNB1", "CX3C1", "CXCR1", "CXCR2", "CXCR3", "CXCR4", "DAPK1",
            "DDAH1", "DDR1", "DHCR7", "DHI1", "DHI2", "DHX36", "DHYS", "DKK1",
            "DMPK", "DOPO", "DPOE1", "DPOG1", "DPOLQ", "DPP4", "DRB3", "DRD1",
            "DRD2", "DRD3", "DUS1", "DVL1", "DVL2", "DVL3", "DYR", "E2F1",
            "EAA1", "EAA2", "ECE1", "EDAR", "EDNRA", "EDNRB", "EGFR", "EGF",
            "EGLN1", "ELAV1", "ELNE", "ENPL", "ENPP1", "ENTP1", "EPHA3", "EPHA8",
            "EPHB3", "EPHB4", "EPOR", "EPO", "ERAP1", "ERBB2", "ERBB3", "ERBB4",
            "ERCC5", "ERR2", "ESR1", "ESR2", "EST1", "ETV1", "F13A", "FA10",
            "FA11", "FA12", "FA5", "FA7", "FA8", "FA9", "FAAH1", "FABP4",
            "FABPH", "FABPI", "FABPL", "FADS1", "FADS2", "FAK1", "FAK2", "FAS",
            "FCER2", "FCG2A", "FCG3B", "FCGR1", "FGF1", "FGF2", "FGFR1", "FGFR2",
            "FGFR3", "FGFR4", "FHIT", "FIBA", "FIBB", "FIBG", "FINC", "FKB1A",
            "FKB1B", "FLT3", "FNTA", "FNTB", "FOLH1", "FOS", "FOXO3", "FPPS",
            "FRDA", "FSHR", "FTO", "FUCO", "FURIN", "FUT3", "FUT4", "FYN",
            "FZD1", "G6PD", "GALR2", "GATA4", "GBRA1", "GBRA4", "GBRA6", "GBRB1",
            "GBRB2", "GCKR", "GCR", "GCYA2", "GCYA3", "GCYB1", "GCYB2", "GDF15",
            "GGT1", "GHRL", "GHR", "GHSR", "GIPR", "GIP", "GLCM", "GLHA",
            "GLI1", "GLI2", "GLRA2", "GLR", "GLUC", "GLYC", "GNAI1", "GNAS2",
            "GPX1", "GRAB", "GRAK", "GRIA1", "GRIA2", "GRIA3", "GRIA4", "GRK4",
            "GRK7", "GRP3", "GRP78", "GSH1", "GSTA1", "GSTA4", "GSTM1", "GSTM3",
            "GSTO1", "GSTO2", "GSTP1", "GSTT1", "GTR1", "GTR4", "GTR9", "GUC2C",
            "GYS1", "H4", "HBA", "HBB", "HCN1", "HCN4", "HDAC3", "HDAC2",
            "HDAC1", "HDAC6", "HDAC9", "HD", "HEM2", "HG2A", "HGF", "HIF1A",
            "HMDH", "HMGB1", "HMOX1", "HNF4A", "HNF4G", "HNMT", "HPRT", "HPSE",
            "HRH1", "HRH2", "HRH3", "HS90A", "HS90B", "HSF1", "HSP71", "HSP72",
            "HSP7C", "HSPB1", "HXK4", "HYEP", "HYES", "I11RA", "IBP1", "IBP2",
            "IBP3", "IBP5", "ICAM1", "ID1", "IDE", "IF4H", "IGF1R", "IGHE",
            "IKBA", "IKKA", "IKKB", "IL12A", "IL12B", "IL1B", "IL1R1", "IL23A",
            "IL2RA", "IL2RB", "IL2RG", "IL2", "IL5", "IL6RA", "IL6RB", "IL6",
            "IL8", "ILK", "INAR1", "INAR2", "INGR1", "INGR2", "INSRR", "INSR",
            "INS", "IRAK1", "ITA2B", "ITA2", "ITA3", "ITA4", "ITA5", "ITA6",
            "ITA9", "ITAL", "ITAM", "ITAV", "ITB1", "ITB2", "ITB3", "ITB5",
            "ITB7", "JAK1", "JAK2", "JUN", "KAT8", "KC1A", "KCC1A", "KCC2B",
            "KCC2G", "KCJ11", "KCMA1", "KCMB1", "KCNA1", "KCNA5", "KCNA7", "KCNB1",
            "KCND2", "KCND3", "KCNE1", "KCNH2", "KCNJ1", "KCNJ2", "KCNJ5", "KCNJ8",
            "KCNK2", "KCNKA", "KCNN3", "KCNQ1", "KCNQ4", "KCRB", "KCRM", "KDM2A",
            "KDM6A", "KGP1", "KIF11", "KIF3C", "KINH", "KITH", "KIT", "KLK1",
            "KLK2", "KLK8", "KLKB1", "KMT2A", "KMT2D", "KPCA", "KPCB", "KPCD1",
            "KPCD2", "KPCD", "KPCL", "KPCT", "KS6A2", "KS6A3", "LAMA1", "LAMA3",
            "LAMA4", "LAMB1", "LARGE", "LATS2", "LCAT", "LCK", "LDHA", "LDHB",
            "LDLR", "LEG1", "LEG2", "LEG3", "LEPR", "LFA3", "LGMN", "LGUL",
            "LIFR", "LIMK1", "LIPC", "LIPE", "LIPL", "LIPS", "LKHA4", "LMNA",
            "LOX12", "LOX15", "LOX5", "LPAR6", "LT4R1", "LT4R2", "LTC4S", "LYAG",
            "LYAM1", "LYAM2", "LYAM3", "LYN", "LYOX", "M3K14", "M3K1", "M3K4",
            "M3K8", "MAG", "MAPK2", "MAX", "MBL2", "MC4R", "MCAT", "MCL1",
            "MCR", "MDM2", "MDR1", "METH", "MET", "MGMT", "MIF", "MINK1",
            "MK03", "MK01", "MK07", "MK11", "MK12", "MK13", "MK14", "MK",
            "MMP10", "MMP11", "MMP12", "MMP13", "MMP14", "MMP1", "MMP2", "MMP3",
            "MMP7", "MMP8", "MMP9", "MP2K1", "MP2K2", "MP2K3", "MP2K6", "MPRI",
            "MRP1", "MRP5", "MRP6", "MSRE", "MTOR", "MTP", "MYC", "MYD88",
            "MYG", "MYH9", "MYLK2", "MYLK", "MYT1", "NAC1", "NAGAB", "NALD2",
            "NALP3", "NCEH1", "NCOA1", "NCOR1", "NCOR2", "NDUAA", "NDUAD", "NDUB2",
            "NDUV2", "NEC1", "NEK8", "NEP", "NEUR3", "NF2L2", "NFAC1", "NFKB1",
            "NGF", "NISCH", "NMDE2", "NOD1", "NOD2", "NOS1", "NOS2", "NOS3",
            "NOTC1", "NOX1", "NOX3", "NOX4", "NOX5", "NQO1", "NR0B1", "NR1D1",
            "NR1H3", "NR2C2", "NRAM1", "NRP1", "NSD2", "NTR1", "NTRK1", "NTRK3",
            "NU1M", "NU2M", "NU5M", "NUAK1", "OPRM", "OXDA", "P2RX4", "P2RX5",
            "P2RX6", "P2RX7", "P2RY1", "P2RY2", "P2Y11", "P2Y12", "P4HA1", "P53",
            "P85A", "PA21B", "PA2G3", "PA2G5", "PA2GA", "PA2GF", "PA2GX", "PAFA",
            "PAG15", "PAI1", "PAR1", "PARP1", "PCD15", "PCKGC", "PCNA", "PCP",
            "PCSK5", "PCSK9", "PD2R", "PDE1A", "PDE1B", "PDE1C", "PDE2A", "PDE3A",
            "PDE3B", "PDE4A", "PDE4B", "PDE4C", "PDE4D", "PDE5A", "PDE6C", "PDGFA",
            "PDGFB", "PDIA6", "PDPK1", "PE2R2", "PE2R3", "PEPD", "PERF", "PERM",
            "PF2R", "PFKAM", "PGFRA", "PGFRB", "PGH1", "PGH2", "PI2R", "PIM2",
            "PK3CA", "PK3CD", "PK3CG", "PKD1", "PKR1", "PLCA", "PLCB1", "PLCG1",
            "PLCG2", "PLD1", "PLGF", "PLIN1", "PLK1", "PLK2", "PLMN", "PLPL9",
            "PLTP", "PNMT", "PNPH", "PON1", "PP2BB", "PPARA", "PPARD", "PPARG",
            "PPBI", "PPBT", "PPIA", "PRGC1", "PRGR", "PRIO", "PRKDC", "PROC",
            "PRSS8", "PRTN3", "PSA1", "PSA2", "PSA3", "PSA4", "PSA5", "PSA6",
            "PSA7L", "PSA7", "PSB10", "PSB1", "PSB2", "PSB3", "PSB4", "PSB5",
            "PSB6", "PSB7", "PSB8", "PSB9", "PSN1", "PTAFR", "PTEN", "PTGES",
            "PTGIS", "PTH1R", "PTN11", "PTN1", "PTN22", "PTN6", "PTPRA", "PTPRC",
            "PTPRF", "PTPRG", "PUR9", "PYR1", "QPCT", "RABP1", "RAC1", "RAF1",
            "RAGE", "RAMP1", "RARB", "RASH", "RASK", "RASN", "RB", "RENI",
            "RET4", "RET", "RFA1", "RHOA", "RIPK2", "RIR1", "RIR2B", "RIR2",
            "ROA1", "ROCK1", "ROCK2", "RORG", "ROS1", "RUNX1", "RXRG", "RYR1",
            "S100B", "S12A1", "S12A3", "S19A1", "S22A2", "S22A5", "S22AB", "S23A2",
            "S27A1", "S27A4", "SAHH", "SAMP", "SAT1", "SC5AB", "SC6A2", "SC6A4",
            "SC6A6", "SCF", "SCN1A", "SCN2A", "SCN3A", "SCN5A", "SCN7A", "SCN8A",
            "SCN9A", "SCNAA", "SCNNA", "SCNNB", "SCNNG", "SCRB1", "SNP25", "SRC",
            "SSR1", "SSR2", "ST2A1", "TA2R", "TBA1A", "TBA1B", "TBA1C", "TBA3C",
            "TBA3E", "TBA4A", "TBB1", "TBB2A", "TBB2B", "TBB3", "TBB4A", "TBB4B",
            "TBB5", "TBB6", "TBB8", "THA", "THB", "THRB", "TN13B", "TNF11",
            "TNFA", "TNR5", "TRXR1", "TYK2", "ULA1", "UPAR", "UR2R", "V1AR",
            "V2R", "VEGFA", "VGFR1", "VGFR2", "VGFR3", "VKGC", "VKOR1", "XDH",
        };

        private static readonly string[] _proteinsWithLirongPdb = new[]
        {
            "1433S", "1A03", "2B11", "5HT1B", "5HT2B", "5NTD", "A1AG1", "A1AG2",
            "A4", "AA2AR", "AACT", "ACE", "ACE2", "ACM2", "ACTB", "ACVL1",
            "ACVR1", "ADA", "ADH7", "ADRB2", "AGAL", "AK1BA", "AKT1", "AL5AP",
            "ALBU", "ALDH2", "ALDR", "ALK", "AMPN", "ANDR", "ANF", "ANGI",
            "ANPRC", "ANT3", "ANXA2", "AOC3", "APAF", "APEX1", "AQP4", "ARBK1",
            "ARGI1", "ARGI2", "ATS13", "ATS5", "AURKA", "AVR2A", "AVR2B", "B2CL1",
            "B2L11", "B2LA1", "BAK", "BAX", "BCAT1", "BCL2", "BCR", "BGAT",
            "BIRC2", "BIRC3", "BIRC5", "BMP2", "BMPR2", "BMR1A", "BMR1B", "BRAF",
            "BTK", "C11B2", "C1TC", "CAC1A", "CAC1C", "CAH3", "CALRL", "CASP1",
            "CASP3", "CASP8", "CASP9", "CATB", "CATD", "CATG", "CATK", "CATL1",
            "CBPB2", "CBPN", "CCL2", "CCL5", "CCND1", "CCND3", "CCNE1", "CD14",
            "CD1D", "CD20", "CD28", "CD38", "CD3D", "CD3E", "CD3G", "CD4",
            "CD80", "CD81", "CD86", "CDK2", "CDK4", "CDK6", "CDN1A", "CEL",
            "CENPE", "CETP", "CFAB", "CFAH", "CFTR", "CHIT1", "CHK1", "CHK2",
            "CHLE", "CLK2", "CMA1", "CO1A1", "CO2A1", "CO3", "CO3A1", "CO4A1",
            "CO4A2", "CO5", "CO6A3", "COIA1", "COMT", "CP17A", "CP19A", "CP1A1",
            "CP1A2", "CP2B6", "CP2C8", "CP2C9", "CP2CI", "CP2CJ", "CP2D6", "CP2E1",
            "CPSM", "CRFR1", "CSF1R", "CSF3R", "CTLA4", "CTNB1", "CXCR4", "DAPK1",
            "DDAH1", "DDR1", "DHI1", "DHYS", "DKK1", "DMPK", "DPOG1", "DPP4",
            "DRD3", "DVL2", "DYR", "E2F1", "ECE1", "EGF", "EGFR", "EGLN1",
            "ELAV1", "ELNE", "EPHA3", "EPHA8", "EPHB3", "EPHB4", "EPO", "EPOR",
            "ERAP1", "ERBB2", "ERBB3", "ERBB4", "ESR1", "ESR2", "EST1", "ETV1",
            "F13A", "FA10", "FA11", "FA12", "FA5", "FA7", "FA8", "FA9",
            "FABP4", "FABPH", "FABPI", "FABPL", "FAK1", "FAK2", "FAS", "FCER2",
            "FCG2A", "FCG3B", "FCGR1", "FGF1", "FGF2", "FGFR1", "FGFR2", "FGFR3",
            "FHIT", "FIBA", "FIBB", "FIBG", "FINC", "FKB1A", "FLT3", "FOLH1",
            "FOS", "FOXO3", "FRDA", "FSHR", "FTO", "FYN", "G6PD", "GCKR",
            "GCR", "GHR", "GIP", "GIPR", "GLCM", "GLHA", "GLI1", "GLR",
            "GLUC", "GLYC", "GNAI1", "GPX1", "GRAB", "GRAK", "GRP78", "GSTA1",
            "GSTA4", "GSTM1", "GSTM3", "GSTO1", "GSTO2", "GSTP1", "GSTT1", "H4",
            "HBA", "HBB", "HCN4", "HD", "HDAC2", "HDAC3", "HEM2", "HG2A",
            "HGF", "HIF1A", "HMDH", "HMOX1", "HNF4A", "HNF4G", "HNMT", "HPRT",
            "HRH1", "HS90A", "HS90B", "HSP72", "HSP7C", "HSPB1", "HXK4", "HYES",
            "IBP1", "IBP5", "ICAM1", "IDE", "IGF1R", "IGHE", "IKBA", "IKKA",
            "IKKB", "IL12A", "IL12B", "IL1B", "IL1R1", "IL2", "IL23A", "IL2RA",
            "IL2RB", "IL2RG", "IL5", "IL6", "IL6RA", "IL6RB", "IL8", "ILK",
            "INAR1", "INAR2", "INGR1", "INS", "INSR", "ITA2", "ITA2B", "ITA4",
            "ITA5", "ITAL", "ITAM", "ITAV", "ITB1", "ITB2", "ITB3", "ITB7",
            "JAK1", "JAK2", "JUN", "KAT8", "KCC1A", "KCC2B", "KCMA1", "KCND3",
            "KCNH2", "KCNQ1", "KCNQ4", "KCRB", "KCRM", "KDM2A", "KDM6A", "KIF3C",
            "KINH", "KIT", "KITH", "KLK1", "KLKB1", "KPCA", "KPCB", "KPCD",
            "KPCD2", "KPCL", "KPCT", "KS6A3", "LCK", "LDHA", "LDHB", "LDLR",
            "LEG1", "LEG2", "LEG3", "LEPR", "LFA3", "LGMN", "LGUL", "LIFR",
            "LIMK1", "LKHA4", "LMNA", "LOX5", "LTC4S", "LYAM1", "LYAM2", "LYAM3",
            "LYN", "M3K14", "MAPK2", "MAX", "MBL2", "MCL1", "MCR", "MDM2",
            "MET", "METH", "MGMT", "MIF", "MK01", "MK03", "MK07", "MK14",
            "MMP1", "MMP10", "MMP12", "MMP13", "MMP14", "MMP2", "MMP3", "MMP7",
            "MMP8", "MMP9", "MP2K1", "MP2K2", "MP2K6", "MPRI", "MRP1", "MYC",
            "MYD88", "MYG", "MYH9", "MYLK2", "NAGAB", "NALD2", "NALP3", "NCOA1",
            "NCOR1", "NCOR2", "NEP", "NF2L2", "NFKB1", "NGF", "NISCH", "NOD1",
            "NOS2", "NOS3", "NOTC1", "NQO1", "NR1D1", "NR1H3", "NR2C2", "NRP1",
            "NTRK1", "NTRK3", "OXDA", "P4HA1", "P53", "P85A", "PA2GA", "PA2GX",
            "PAFA", "PAI1", "PAR1", "PARP1", "PCNA", "PCP", "PCSK9", "PDE4D",
            "PDE5A", "PDE6C", "PDGFA", "PDGFB", "PDIA6", "PEPD", "PERM", "PGFRB",
            "PIM2", "PK3CA", "PLCG1", "PLCG2", "PLGF", "PLK1", "PLMN", "PNMT",
            "PNPH", "PPARA", "PPARD", "PPARG", "PPIA", "PRGC1", "PRGR", "PRIO",
            "PROC", "PRSS8", "PTEN", "PTGES", "PTGIS", "PTH1R", "PTN1", "PTN11",
            "PTN22", "PTN6", "PTPRF", "PTPRG", "PUR9", "QPCT", "RAC1", "RAF1",
            "RAGE", "RAMP1", "RARB", "RASH", "RASK", "RASN", "RB", "RENI",
            "RET4", "RFA1", "RHOA", "ROA1", "ROCK1", "RORG", "ROS1", "RUNX1",
            "RXRG", "S100B", "SAHH", "SAMP", "SAT1", "SCF", "SCN5A", "THRB",
        };

        private static readonly string[] _uniprotIdsForExperimentData = new[]
        {
            "D2YW37", "O00303", "O00305", "O00459", "O00469", "O14832", "O14920", "O15111",
            "O15399", "O15440", "O43246", "O43272", "O60488", "O60502", "O60568", "O60669",
            "O60706", "O60840", "O75116", "O75311", "O75469", "O75936", "O76074", "O94768",
            "O94956", "O95298", "O95342", "O95477", "O95932", "P00338", "P00441", "P00533",
            "P00846", "P00915", "P00966", "P01137", "P02763", "P02768", "P03372", "P03886",
            "P04035", "P04278", "P04293", "P04424", "P04637", "P04798", "P05067", "P05106",
            "P05129", "P05164", "P05177", "P05771", "P06276", "P06576", "P07437", "P07550",
            "P07900", "P08183", "P08235", "P08588", "P08631", "P08648", "P08684", "P08908",
            "P08913", "P09038", "P09172", "P09211", "P09601", "P09848", "P09874", "P09917",
            "P0A7V8", "P0A7X3", "P0DP23", "P10275", "P10415", "P10478", "P10632", "P10635",
            "P11021", "P11166", "P11168", "P11309", "P11439", "P11473", "P11511", "P11678",
            "P11712", "P13569", "P13674", "P13945", "P14174", "P14867", "P15692", "P16050",
            "P16083", "P16152", "P16581", "P16662", "P16860", "P17252", "P17302", "P17405",
            "P17612", "P17676", "P18089", "P18545", "P18825", "P18858", "P19021", "P19320",
            "P19652", "P19793", "P20813", "P20815", "P21397", "P22309", "P22310", "P22314",
            "P22894", "P23219", "P23415", "P23416", "P23458", "P23743", "P24300", "P24530",
            "P24557", "P25100", "P25101", "P25705", "P25963", "P26358", "P27338", "P27487",
            "P27797", "P27986", "P28221", "P28222", "P28223", "P28335", "P28472", "P28482",
            "P28702", "P29474", "P30049", "P30825", "P30926", "P31040", "P31641", "P31645",
            "P31749", "P32297", "P33261", "P33402", "P33527", "P35228", "P35348", "P35354",
            "P35368", "P35372", "P35499", "P35503", "P35869", "P36542", "P37059", "P37231",
            "P37610", "P37840", "P39900", "P41143", "P41145", "P41595", "P41743", "P42261",
            "P42330", "P42336", "P42345", "P42357", "P43119", "P43681", "P46059", "P46597",
            "P47989", "P48039", "P48050", "P48147", "P48443", "P48544", "P48637", "P48736",
            "P49286", "P49327", "P49916", "P50416", "P51512", "P51584", "P51812", "P53350",
            "P53985", "P54284", "P54289", "P54577", "P54652", "P54965", "P56181", "P60709",
            "P61024", "P61769", "P61925", "P62508", "P62714", "P62942", "P67775", "P67870",
            "P68366", "P68400", "P78540", "P98170", "P98194", "Q00653", "Q00975", "Q01668",
            "Q01959", "Q02156", "Q02641", "Q02809", "Q03154", "Q03181", "Q04759", "Q04828",
            "Q05513", "Q05586", "Q05655", "Q06278", "Q07869", "Q07973", "Q08289", "Q09428",
            "Q09470", "Q10588", "Q12809", "Q12879", "Q13131", "Q13224", "Q13464", "Q13698",
            "Q13936", "Q13956", "Q14032", "Q14457", "Q14524", "Q14654", "Q14994", "Q15125",
            "Q15393", "Q15596", "Q15822", "Q15842", "Q15858", "Q16665", "Q16678", "Q32P28",
            "Q4U2R8", "Q4ZHU6", "Q54873", "Q55813", "Q56222", "Q5G940", "Q6N063", "Q6NS38",
            "Q6NUS8", "Q6NVY1", "Q6TME4", "Q7Z2H8", "Q86T13", "Q86XF0", "Q89ZI2", "Q8IVL5",
            "Q8IVL6", "Q8IZS8", "Q8N543", "Q8TCG2", "Q8TCU5", "Q8WY07", "Q92569", "Q92731",
            "Q92753", "Q92887", "Q92945", "Q96A70", "Q96KS0", "Q96Q83", "Q99527", "Q9AIU0",
            "Q9BY66", "Q9GZT9", "Q9H3N8", "Q9H6Z9", "Q9HCR9", "Q9NPD5", "Q9NPH5", "Q9NVH6",
            "Q9NXA8", "Q9NXG6", "Q9UBS5", "Q9UGN5", "Q9UHI7", "Q9UKK3", "Q9UKQ2", "Q9UM07",
            "Q9UNA0", "Q9UNQ0", "Q9UPY5", "Q9Y230", "Q9Y478", "Q9Y5N1", "Q9Y5S8", "Q9Y5Y9",
            "Q9Y694", "Q9Y6F1", "Q9Y6L6",
        };

        private static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug))
                .BuildServiceProvider();

            Logger.Current = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();

            var sw = Stopwatch.StartNew();
            string outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "CVD_Proteins_for_Experiment_Data");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var lirongPdbs = new ConcurrentDictionary<string, ConcurrentBag<string>>(Program.lirongPdbs.Distinct().Select(o => new KeyValuePair<string, ConcurrentBag<string>>(o, new ConcurrentBag<string>())));
            var noPdbProteins = new ConcurrentBag<string>();

            Logger.Current.LogInformation($"Total PDBs from Lirong's is {lirongPdbs.Count}");

            var bag = new ConcurrentBag<string[]>();
            var headers = new[] { "PDB Name", "Method", "Resolution", "Chain", "Positions", "Download Url", "Protein Url", "View PDB Url" };

            Parallel.ForEach(_uniprotIdsForExperimentData, uniprotId =>
            {
                var plbag = new Dictionary<string, object>()
                    .SetUniProtId(uniprotId)
                    .FetchUniProtHtml()
                    .ParseUniProtStructures()
                    .ParseProteinName();

                string proteinName = plbag.GetProteinName();
                string proteinUri = plbag.GetUniProtUri();
                var pdbs = plbag.GetUniProtPdbList();

                if (proteinName == null)
                {
                    Logger.Current.LogError($"Not human protein for uniprot id {uniprotId}");
                    return;
                }

                Logger.Current.LogInformation($"Found protein name {proteinName} for uniprot id {uniprotId}");
                if (pdbs.Length == 0)
                {
                    noPdbProteins.Add(proteinName);
                    return;
                }

                string proteinOutput = Path.Combine(outputDir, proteinName);
                if (!Directory.Exists(proteinOutput))
                    Directory.CreateDirectory(proteinOutput);

                Parallel.ForEach(pdbs, pdb =>
                {
                    if (!lirongPdbs.ContainsKey(pdb.PdbEntry))
                        return;
                    lirongPdbs[pdb.PdbEntry].Add(proteinName);

                    var b = new Dictionary<string, object>()
                        .SetPdbEntry(pdb.PdbEntry)
                        .DownloadPdbOrCifAndSplit(proteinOutput, proteinName, null);

                    string[] infoLines = new[] {
                        pdb.PdbEntry, pdb.Method, pdb.Resolution, pdb.Chain, pdb.Positions, b.GetPdbOrCifDownloadUri(), proteinUri, b.GetRcsbDownloadUri()
                    };
                    bag.Add(infoLines);
                    File.WriteAllLines(Path.Combine(proteinOutput, $"{pdb.PdbEntry}.txt"), infoLines);
                });

                if (Directory.GetFiles(proteinOutput).IsEmpty())
                    Directory.Delete(proteinOutput);

                Logger.Current.LogInformation($"Done with all {pdbs.Length} PDBs for {proteinName}");
            });

            File.WriteAllLines(Path.Combine(outputDir, "CVD_Proteins.csv"), CsvHelper.FormatCsvRows(bag, headers));

            Logger.Current.LogInformation($"CVD proteins that have no PDBs:");
            foreach (string item in noPdbProteins)
                Logger.Current.LogInformation(item);

            Logger.Current.LogInformation($"Lirong's PDBs that are Not found in CVD proteins:");
            foreach (var item in lirongPdbs.Where(o => o.Value.Count == 0))
                Logger.Current.LogInformation(item.Key);

            Logger.Current.LogInformation($"Lirong's PDBs by multiple CVD proteins:");
            foreach (var item in lirongPdbs.Where(o => o.Value.Count > 1))
                Logger.Current.LogInformation(item.Key + ": " + string.Join(",", item.Value));

            Logger.Current.LogInformation($"Time elapsed: {sw.Elapsed}");
        }
    }
}
