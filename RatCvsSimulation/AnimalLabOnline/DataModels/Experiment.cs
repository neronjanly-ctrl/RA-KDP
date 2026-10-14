using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimalLabOnline.DataModels
{
    public class Experiment
    {
        /****************************
         *        Primary key
         ****************************/
        public Guid Id { get; set; }

        /****************************
         *      Data properties
         ****************************/
        [Required]
        public string Author { get; set; }
        [Required]
        public string Url { get; set; }
        [Required]
        public string ArticleTitle { get; set; }

        /****************************
         *   Converted properties
         ****************************/
        [Required]
        public double?[,,] Results { get; set; }
        [Required]
        public (int target, double pvalue)[,][] PValues { get; set; }

        /****************************
         *   Navigation properties
         ****************************/
        [Required]
        public Subject Subject { get; set; }
        public IList<ExpDrug> ExpDrugs { get; set; }
        public IList<ExpGroup> ExpGroups { get; set; }
        public IList<ExpIndicator> ExpIndicators { get; set; }

        [NotMapped]
        public string Tag { get; set; }
    }
}
