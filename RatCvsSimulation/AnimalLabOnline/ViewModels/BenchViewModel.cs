using AnimalLabOnline.DataModels;
using System.Collections.Generic;

namespace AnimalLabOnline.ViewModels
{
    public class BenchViewModel
    {
        /// <summary>
        /// For Subject section
        /// </summary>
        public IList<Subject> Subjects { get; set; }

        /// <summary>
        /// Subject selected by Subject section
        /// </summary>
        public Subject SelectedSubject { get; set; }

        /// <summary>
        /// For Treatment section
        /// </summary>
        public IList<Drug> Drugs { get; set; }

        /// <summary>
        /// Drug selected by Treatment section
        /// </summary>
        public Drug SelectedDrug { get; set; }

        /// <summary>
        /// For Models and Results sections
        /// </summary>
        public IList<Experiment> Experiments { get; set; }

        /// <summary>
        /// For Simulation section
        /// </summary>
        public SimulationViewModel Simulation { get; set; }
    }
}
