using System;
using System.ComponentModel.DataAnnotations;

namespace AnimalLabOnline.DataModels
{
    public class ExpIndicator
    {
        /****************************
         *        Primary key
         ****************************/
        public Guid Id { get; set; }

        /****************************
         *      Data properties
         ****************************/
        public int Index { get; set; } // 1-based index
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string Speciality { get; set; }
        public string Unit { get; set; }
        public string UnitFull { get; set; }
        public bool OnFigure { get; set; }

        /****************************
         *   Navigation properties
         ****************************/
        [Required]
        public Experiment Experiment { get; set; }

        /****************************
         *   Computed properties
         ****************************/
        public string NameFull => string.IsNullOrEmpty(Speciality) ? Name : $"{Name} ({Speciality})";
    }
}
