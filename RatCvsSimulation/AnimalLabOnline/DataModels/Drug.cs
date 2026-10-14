using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AnimalLabOnline.DataModels
{
    public class Drug
    {
        /****************************
         *        Primary key
         ****************************/
        public Guid Id { get; set; }

        /****************************
         *      Data properties
         ****************************/
        [Required]
        public string Name { get; set; }
        [Required]
        public string Smiles { get; set; }
        [Required]
        public string Depict2D { get; set; }
        [Required]
        public byte[] Fingerprint { get; set; }

        /****************************
         *   Navigation properties
         ****************************/
        public IList<ExpDrug> ExpDrugs { get; set; }
    }
}
