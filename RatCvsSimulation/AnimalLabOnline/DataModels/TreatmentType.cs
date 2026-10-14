using CommonTools;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AnimalLabOnline.DataModels
{
    [DefaultValue(DrugTherapy)]
    public enum TreatmentType
    {
        [Display(Name = "Unknown Treatment", ShortName = "Unknown")]
        Unknown,

        /// <summary>
        /// Treated with Normal Saline (生理盐水)
        /// </summary>
        [Display(Name = "Normal Saline", ShortName = "Normal")]
        [Match("", "normal", "control", "normal control", "saline", CaseSensitive = false)]
        Normal,

        /// <summary>
        /// Treated with Solvent (溶剂)
        /// </summary>
        [Display(Name = "Vehicle ({0})", ShortName = "Vehicle")]
        [Match(
            "vehicle", // 溶剂
            "water", "distilled water", "double distilled water", // 蒸馏水
            "dmso", "dimethylsulfoxide", // 二甲基亚砜
            "20% tween 80", "polysorbate-80", // 聚山梨醇酯-80
            "phosphate buffer saline", // 磷酸盐缓冲液
            CaseSensitive = false
        )]
        Vehicle,

        /// <summary>
        /// Treated with Sham Operation (假手术)
        /// </summary>
        [Display(Name = "Sham Operated", ShortName = "Sham")]
        [Match("sham", "sham operated", "sham operation", CaseSensitive = false)]
        Sham,

        /// <summary>
        /// Treated with Placebo (安慰剂)
        /// </summary>
        [Display(Name = "Placebo Treated", ShortName = "Placebo")]
        [Match("placebo", CaseSensitive = false)]
        Placebo,

        /// <summary>
        /// Treated with Incuder (药物诱导造模)
        /// </summary>
        [Display(Name = "Drug-Induced Modeling ({0})", ShortName = "Inducer")]
        [Match(
            "ISO", "isoprenaline", "isoproterenol", // 异丙基肾上腺素
            "MCT", "medium chain triglycerides", // 中链甘油三脂
            CaseSensitive = false
        )]
        DrugInducedModeling,

        /// <summary>
        /// Treated with Surgery (手术造模)
        /// </summary>
        [Display(Name = "Surgical Modeling ({0})", ShortName = "Surgical Modeling")]
        [Match(
            "MCAo", "Middle Cerebral Artery Occlusion", // 大脑中动脉闭塞
            "PAAC", "Partial Abdominal Aortic Constriction", // 腹主动脉部分缩窄
            "ovx SHR", "SHR", "Spontaneously Hypertensive Rat", // 卵巢切除致自发性高血压大鼠
            "SuHx", "SU5416 and hypoxia", // 血管内皮生长因子受体拮抗剂SU5416与缺氧联用
            "Hx", "hypoxia", // 缺氧
            CaseSensitive = false
        )]
        SurgicalModeling,

        /// <summary>
        /// Treated with Stress (应激造模)
        /// </summary>
        [Display(Name = "Stress Modeling ({0})", ShortName = "Stress Modeling")]
        StressModeling,

        /// <summary>
        /// Genetic Desease (遗传造模)
        /// </summary>
        [Display(Name = "Genetic Modeling ({0})", ShortName = "Genetic Modeling")]
        GeneticModeling,

        /// <summary>
        /// Modeling method not mentioned (造模方式未知)
        /// </summary>
        [Display(Name = "Animal Model ({0})", ShortName = "Animal Model")]
        [Match("MI", "HF")]
        OtherModeling,

        /// <summary>
        /// Treated with drugs (药物疗法)
        /// </summary>
        [Display(Name = "Drug Therapy ({0})", ShortName = "Drug Therapy")]
        DrugTherapy,

        /// <summary>
        /// Treated with cell tissues (细胞疗法)
        /// </summary>
        [Display(Name = "Cellular Therapy", ShortName = "Cellular Therapy")]
        [Match("BM-MSC", CaseSensitive = false)]
        CellularTherapy,
    }
}
