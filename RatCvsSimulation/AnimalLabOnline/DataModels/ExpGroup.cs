using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AnimalLabOnline.DataModels
{
    public class ExpGroup
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
        public string Speciality { get; set; }

        /****************************
         *   Navigation properties
         ****************************/
        [Required]
        public Experiment Experiment { get; set; }
        public IList<Protocol> Protocols { get; set; }

        /****************************
         *   Computed properties
         ****************************/
        public string NameFull => string.IsNullOrEmpty(Speciality) ? Name : $"{Name} ({Speciality})";
    }
}
