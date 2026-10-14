using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AnimalLabOnline.DataModels
{
    public class ExpDrug
    {
        /****************************
         *        Primary key
         ****************************/
        public Guid Id { get; set; }

        /****************************
         *      Data properties
         ****************************/
        public int Index { get; set; } // 1-based index

        /****************************
         *   Navigation properties
         ****************************/
        [Required]
        public Experiment Experiment { get; set; }
        [Required]
        public Drug Drug { get; set; }
        public IList<Protocol> Protocols { get; set; }
    }
}
