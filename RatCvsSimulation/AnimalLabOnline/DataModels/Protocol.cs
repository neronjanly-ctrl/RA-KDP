using AnimalLabOnline.EnumHelpers;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace AnimalLabOnline.DataModels
{
    public class Protocol
    {
        /****************************
         *        Primary key
         ****************************/
        public Guid Id { get; set; }

        /****************************
         *      Data properties
         ****************************/
        public TreatmentType Treatment { get; set; }
        public string TreatmentName { get; set; }
        public string Speciality { get; set; }
        public AdminMethod AdminMethod { get; set; }
        public float? Dose { get; set; }
        public DoseUnit DoseUnit { get; set; }

        /****************************
         *   Navigation properties
         ****************************/
        [Required]
        public ExpGroup ExpGroup { get; set; }
        public ExpDrug ExpDrug { get; set; }

        /****************************
         *   Computed properties
         ****************************/
        public string DrugNameString => ExpDrug?.Drug?.Name ?? TreatmentName;
        public string TreatmentStringFull => string.Format(Treatment.GetName(), TreatmentName);
        public string TreatmentString => string.Format(Treatment.GetShortName(), TreatmentName);
        public string AdminStringFull => AdminMethod.GetName();
        public string AdminString => AdminMethod.GetShortName();
        public string DoseUnitStringFull => DoseUnit.GetName();
        public string DoseUnitString => DoseUnit.GetShortName();

        public string DrugJson => JsonConvert.SerializeObject(
            new
            {
                treatment = TreatmentString,
                treatment_full = TreatmentStringFull,
                drug = DrugNameString,
                admin = AdminString,
                admin_full = AdminStringFull,
                dose = Dose,
                unit = DoseUnitString,
                unit_full = DoseUnitStringFull,
            });
    }
}
