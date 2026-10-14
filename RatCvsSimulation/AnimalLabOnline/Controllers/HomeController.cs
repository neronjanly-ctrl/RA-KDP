using AnimalLabOnline.DataModels;
using AnimalLabOnline.ViewModels;
using CommonTools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Linq;

namespace AnimalLabOnline.Controllers
{
    public class HomeController : Controller
    {
        private readonly Simulation.Simulation _simulation;
        private readonly ExperimentsContext _ctx;

        public HomeController(Simulation.Simulation simulation, ExperimentsContext ctx)
        {
            _simulation = simulation;
            _ctx = ctx;
        }

        [HttpGet("/new")]
        public IActionResult NewIndex()
        {
            var model = new BenchViewModel
            {
                Subjects = _ctx.Subjects
                    .OrderBy(o => o.Index)
                    .ToArray()
            };

            return View(model);
        }

        [HttpGet("/")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/subject")]
        public IActionResult Subject()
        {
            var model = new BenchViewModel
            {
                Subjects = _ctx.Subjects
                    .OrderBy(o => o.Index)
                    .ToArray()
            };

            return View(model);
        }

        [HttpGet("/treatment/{subjectId}")]
        public IActionResult Treatment(Guid subjectId)
        {
            var model = new BenchViewModel
            {
                SelectedSubject = _ctx.Subjects
                    .Include(o => o.Experiments)
                    .ThenInclude(o => o.ExpGroups)
                    .ThenInclude(o => o.Protocols)
                    .ThenInclude(o => o.ExpDrug)
                    .ThenInclude(o => o.Drug)
                    .SingleOrDefault(o => o.Id == subjectId),
                Drugs = _ctx.Protocols
                    .Where(o => o.ExpGroup.Experiment.Subject.Id == subjectId && o.Treatment == TreatmentType.DrugTherapy)
                    .Select(o => o.ExpDrug.Drug)
                    .Distinct()
                    .ToArray()
            };

            return View(model);
        }

        [HttpGet("/models/{subjectId}")]
        [HttpGet("/models/{subjectId}/{drugId}")]
        public IActionResult Models(Guid subjectId, Guid drugId, string smiles)
        {
            if (drugId == default && !string.IsNullOrEmpty(smiles))
            {
                byte[] fp2 = smiles.ComputeFingerprint("FP2");
                var (drug, score) = _ctx.SearchForSimilarDrug(subjectId, smiles, fp2);
                if (drug != null)
                    drugId = drug.Id;
            }

            if (drugId == default)
                return NotFound();

            var model = new BenchViewModel
            {
                SelectedSubject = _ctx.Subjects
                    .SingleOrDefault(o => o.Id == subjectId),
                SelectedDrug = _ctx.Drugs
                    .SingleOrDefault(o => o.Id == drugId),
                Experiments = _ctx.Experiments
                    .Include(o => o.ExpGroups)
                    .ThenInclude(o => o.Protocols)
                    .ThenInclude(o => o.ExpDrug)
                    .ThenInclude(o => o.Drug)
                    .Where(o => o.Subject.Id == subjectId)
                    .Where(o => o.ExpDrugs.Any(p => p.Drug.Id == drugId))
                    .ToArray()
            };

            return View(model);
        }

        [HttpGet("/simulation/{subjectId}")]
        [HttpGet("/simulation/{subjectId}/{drugId}")]
        public IActionResult Simulation(Guid subjectId, Guid drugId, string smiles)
        {
            if (drugId == default && !string.IsNullOrEmpty(smiles))
            {
                byte[] fp2 = smiles.ComputeFingerprint("FP2");
                var (drug, score) = _ctx.SearchForSimilarDrug(subjectId, smiles, fp2);
                if (drug != null)
                    drugId = drug.Id;
            }

            if (drugId == default)
                return NotFound();

            var model = new BenchViewModel
            {
                SelectedSubject = _ctx.Subjects
                    .SingleOrDefault(o => o.Id == subjectId),
                SelectedDrug = _ctx.Drugs
                    .SingleOrDefault(o => o.Id == drugId),
                Simulation = _simulation
                    .GetSimulationViewModelBuilder()
                    .Build(600, 800)
            };

            model.Simulation.GroupNames = _ctx.Experiments
                .Where(o => o.Subject.Id == subjectId)
                .Where(o => o.ExpDrugs.Any(p => p.Drug.Id == drugId))
                .Select(o => o.ExpGroups)
                .FirstOrDefault()
                .OrderBy(p => p.Index)
                .Select(p => p.NameFull)
                .ToArray(); // in case multiple experiments test the same drug

            var protocols = _ctx.Experiments
                .Where(o => o.Subject.Id == subjectId)
                .Where(o => o.ExpDrugs.Any(p => p.Drug.Id == drugId))
                .Include(o => o.ExpGroups)
                .ThenInclude(o => o.Protocols)
                .ThenInclude(o => o.ExpDrug)
                .ThenInclude(o => o.Drug)
                .Select(o => o.ExpGroups.Select(p => p.Protocols))
                .AsEnumerable()
                .Select(o => o.FirstOrDefault(p => p.Any(q => q.Treatment == TreatmentType.DrugTherapy) && p.Any(q => q.Treatment == TreatmentType.DrugInducedModeling)))
                .FirstOrDefault(o => o != null); // in case multiple experiments test the same drug

            if (protocols != null)
            {
                var drug = protocols.FirstOrDefault(o => o.Treatment == TreatmentType.DrugTherapy);
                var inducer = protocols.FirstOrDefault(o => o.Treatment == TreatmentType.DrugInducedModeling);

                model.Simulation.DrugDose = drug.Dose == null ? "given dose" : $"{drug.Dose} {drug.DoseUnitString}";
                model.Simulation.DrugAdmin = drug.AdminMethod == AdminMethod.Unknown ? "administration method" : $"{drug.AdminStringFull} ({drug.AdminString})";
                model.Simulation.InducerName = inducer == null ? "inducer" : inducer.DrugNameString;
                model.Simulation.InducerDose = inducer?.Dose == null ? "given dose per" : $"{inducer.Dose} {inducer.DoseUnitString}";
                model.Simulation.InducerAdmin = string.IsNullOrWhiteSpace(inducer?.AdminStringFull) ? "administration method" : $"{inducer.AdminStringFull} ({inducer.AdminString})";
            }
            else
            {
                model.Simulation.DrugDose = "given dose";
                model.Simulation.DrugAdmin = "administration method";
                model.Simulation.InducerName = "inducer";
                model.Simulation.InducerDose = "given dose per";
                model.Simulation.InducerAdmin = "administration method";
            }

            return View(model);
        }

        [HttpGet("/results/{subjectId}")]
        [HttpGet("/results/{subjectId}/{drugId}")]
        public IActionResult Results(Guid subjectId, Guid drugId, string smiles)
        {
            if (drugId == default && !string.IsNullOrEmpty(smiles))
            {
                byte[] fp2 = smiles.ComputeFingerprint("FP2");
                var (drug, score) = _ctx.SearchForSimilarDrug(subjectId, smiles, fp2);
                if (drug != null)
                    drugId = drug.Id;
            }

            if (drugId == default)
                return NotFound();

            var model = new BenchViewModel
            {
                SelectedSubject = _ctx.Subjects
                    .SingleOrDefault(o => o.Id == subjectId),
                SelectedDrug = _ctx.Drugs
                    .SingleOrDefault(o => o.Id == drugId),
                Experiments = _ctx.Experiments
                    .Include(o => o.ExpDrugs)
                    .Include(o => o.ExpGroups)
                    .ThenInclude(o => o.Protocols)
                    .ThenInclude(o => o.ExpDrug)
                    .ThenInclude(o => o.Drug)
                    .Include(o => o.ExpIndicators)
                    .Where(o => o.Subject.Id == subjectId)
                    .Where(o => o.ExpDrugs.Any(p => p.Drug.Id == drugId))
                    .ToArray()
            };

            return View(model);
        }

        [HttpGet("/error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
