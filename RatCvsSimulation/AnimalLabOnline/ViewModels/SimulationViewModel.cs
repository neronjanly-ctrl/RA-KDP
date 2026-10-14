using System.Collections.Generic;

namespace AnimalLabOnline.ViewModels
{
    public class SimulationViewModel
    {
        public string[] OrganNames { get; set; }
        public IReadOnlyDictionary<string, int[]> Selectors { get; set; }
        public double[][] CanvasOrgans { get; set; }
        public int[][] OrganConnections { get; set; }
        public string[] GroupNames { get; set; }
        public string DrugDose { get; set; }
        public string DrugAdmin { get; set; }
        public string InducerName { get; set; }
        public string InducerDose { get; set; }
        public string InducerAdmin { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
