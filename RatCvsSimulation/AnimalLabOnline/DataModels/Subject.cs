using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AnimalLabOnline.DataModels
{
    public class Subject
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
        [Required]
        public string Desc { get; set; }

        /****************************
         *   Navigation properties
         ****************************/
        public IList<Experiment> Experiments { get; set; }
    }
}
