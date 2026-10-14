using AnimalLabOnline.DataModels;
using CommonTools;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;

namespace AnimalLabOnline.Controllers
{
    public class MaintainController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ExperimentsContext _ctx;
        private readonly IConfiguration _config;

        public MaintainController(IWebHostEnvironment env, ExperimentsContext ctx, IConfiguration config)
        {
            _env = env;
            _ctx = ctx;
            _config = config;
        }

        [HttpGet("/rebuild-experiment-db")]
        public IActionResult RebuildExpDatabase()
        {
            // build drugs and experiments with indicators and groups
            var xls = ExcelFileParser.GetRawData(Path.Combine(_env.ContentRootPath, "Workspace\\experimental-data.xlsx"));
            var sheets = xls.ToArray();

            if (sheets.Length < 2)
                throw new Exception($"Excel file contains less than 2 sheets.");

            var drugsSheet = sheets.SingleOrDefault(o => o.name.Equals("drugs", StringComparison.CurrentCultureIgnoreCase));
            var indicatorsSheet = sheets.SingleOrDefault(o => o.name.Equals("indicators", StringComparison.CurrentCultureIgnoreCase));

            if (drugsSheet == default)
                throw new Exception($"Cannot find Drugs sheet.");
            if (indicatorsSheet == default)
                throw new Exception($"Cannot find Indicators sheet.");

            var parser = new ExpDataParser();
            var options = _config.GetSection("OpenBabel").Get<OpenBabelOptions>();
            var drugs = parser.ConvertToDrugs(drugsSheet.data, options, drugsSheet.name);
            var indicators = parser.ConvertToIndicators(indicatorsSheet.data, indicatorsSheet.name);

            var exps = sheets
                .Where(o => o != drugsSheet && o != indicatorsSheet)
                .SelectMany((sheet, i) => parser.ConvertToExperiments(sheet.data, drugs, indicators, sheet.name, i + 1))
                .ToArray();

            // clean up existing data
            _ctx.RemoveRange(_ctx.Protocols);
            _ctx.RemoveRange(_ctx.ExpIndicators);
            _ctx.RemoveRange(_ctx.ExpGroups);
            _ctx.RemoveRange(_ctx.ExpDrugs);
            _ctx.RemoveRange(_ctx.Experiments);
            _ctx.RemoveRange(_ctx.Drugs);
            _ctx.RemoveRange(_ctx.Subjects);

            var subjects = exps
                .Select(o => o.Subject)
                .Distinct()
                .ToArray();

            // insert new data
            _ctx.Experiments.AddRange(exps);

            // commit transaction
            _ctx.SaveChanges();

            var experiments = _ctx.Experiments
                .Include(o => o.Subject)
                .Include(o => o.ExpGroups)
                .Include(o => o.ExpIndicators)
                .Include(o => o.ExpDrugs)
                .AsEnumerable();

            var protocols = _ctx.Protocols
                .AsEnumerable();

            var drugsdb = _ctx.Drugs
                .AsEnumerable();

            var expIndicators = _ctx.ExpIndicators
                .AsEnumerable();

            return Json(new
            {
                Subject_Experiment_Stat = experiments
                    .GroupBy(o => o.Subject)
                    .ToDictionary(o => o.Key.Name, o => o.Count()),
                Experiment_Group_Stat = experiments
                    .GroupBy(o => o.ExpGroups.Count)
                    .ToDictionary(o => $"{o.Key} groups", o => o.Count()),
                Experiment_Indicator_Stat = experiments
                    .GroupBy(o => o.ExpIndicators.Count)
                    .ToDictionary(o => $"{o.Key} indicators", o => o.Count()),
                Experiment_Drug_Stat = experiments
                    .GroupBy(o => o.ExpDrugs.Count)
                    .ToDictionary(o => $"{o.Key} drugs", o => o.Count()),
                Experiment_Categories = experiments
                    .GroupBy(o => o.Tag)
                    .ToDictionary(o => o.Key, o => o.Count()),
                Protocol_Treatment_Stat = protocols
                    .GroupBy(o => o.Treatment)
                    .ToDictionary(o => o.Key, o => o.Count()),
                Drug_Usage_Stat = drugsdb
                    .ToDictionary(o => o.Name, o => o.ExpDrugs.Count()),
                Indicator_Names = expIndicators
                    .Select(o => o.Name)
                    .OrderBy(o => o)
                    .Distinct()
                    .ToArray(),
            });
        }
    }
}