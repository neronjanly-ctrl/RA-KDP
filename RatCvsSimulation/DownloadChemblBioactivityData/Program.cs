using ChemicalFunctions;
using CommonTools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DownloadChemblBioactivityData
{
    internal class Program
    {
        private static void Download(IEnumerable<string> proteins, string location)
        {
            if (!location.Contains('{') && !Directory.Exists(location))
            {
                Logger.Current.LogDebug($"Created directory {location}");
                Directory.CreateDirectory(location);
            }

            Logger.Current.LogInformation($"Starting...");

            int n = 0;
            foreach (string protein in proteins)
            {
                string outputDir = string.Format(location, protein);
                if (!Directory.Exists(outputDir))
                {
                    Logger.Current.LogDebug($"Created directory {outputDir}");
                    Directory.CreateDirectory(outputDir);
                }

                string filepath = Path.Combine(outputDir, $"{protein}_Bioactivities.txt");
                if (File.Exists(filepath))
                {
                    Logger.Current.LogDebug($"Skipped for protein {protein}.");
                    continue;
                }

                var chembl = File.ReadAllLines(Path.Combine(outputDir, "Properties.txt")).Single(o => o.StartsWith("ChemblId="))["ChemblId=".Length..];

                if (string.IsNullOrEmpty(chembl))
                    continue;

                var plbag = new Dictionary<string, object>()
                    .SetChemblId(chembl)
                    .FetchChemblActivityData()
                    .Result
                    .SaveChemblBioactivityData(filepath);
                n++;
            }

            Logger.Current.LogInformation($"Done for all {n} proteins.");
        }

        private static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug))
                .BuildServiceProvider();

            Logger.Current = serviceProvider.GetService<ILoggerFactory>().CreateLogger<Program>();

            var sw = Stopwatch.StartNew();

            //string workdir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Git", "GenericComputationPlatform", "DockingApiService", "Workspace", "Receptors");
            string workdir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "receptors");
            var proteins = Directory.GetDirectories(workdir).Select(o => Path.GetFileName(o));
            Download(proteins, Path.Combine(workdir, "{0}"));

            Logger.Current.LogInformation($"Time elapsed: {sw.Elapsed}");
            Console.ReadKey();
        }
    }
}
