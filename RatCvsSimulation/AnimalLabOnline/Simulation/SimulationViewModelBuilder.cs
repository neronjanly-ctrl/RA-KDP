using AnimalLabOnline.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimalLabOnline.Simulation
{
    public class SimulationViewModelBuilder
    {
        private readonly Simulation _simulation;

        public SimulationViewModelBuilder(Simulation simulation)
        {
            _simulation = simulation;
        }

        public SimulationViewModel Build(int width, int height)
        {
            var canvasOrgans = _simulation.GetCanvasOrgans(width, height);

            return new SimulationViewModel
            {
                OrganNames = Simulation.OrganNames,
                Selectors = Simulation.Selectors,
                CanvasOrgans = canvasOrgans,
                OrganConnections = _simulation.GetOrganConnections(canvasOrgans),
                Width = width,
                Height = height,
            };
        }
    }

}
