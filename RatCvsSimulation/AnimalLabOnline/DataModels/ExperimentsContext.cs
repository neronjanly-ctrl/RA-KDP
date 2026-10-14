using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace AnimalLabOnline.DataModels
{
    public class ExperimentsContext : DbContext
    {
        public ExperimentsContext(DbContextOptions<ExperimentsContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Experiment>()
                .Property(o => o.PValues)
                .HasConversion(
                    o => JsonConvert.SerializeObject(o),
                    o => JsonConvert.DeserializeObject<(int, double)[,][]>(o)
                );

            modelBuilder
                .Entity<Experiment>()
                .Property(o => o.Results)
                .HasConversion(
                    o => JsonConvert.SerializeObject(o),
                    o => JsonConvert.DeserializeObject<double?[,,]>(o)
                );
        }

        /*              Subject
        *                 1|
        *                  |*
        *              Experiment      Drug
        *            1/   1|   1\     1/
        *            /*    |*    \*   /*
        *  ExpIndicator ExpGroup ExpDrug
        *                  1\    1/
        *                    \*  /*
        *                   Protocol
        */

        // Root entities
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Drug> Drugs { get; set; }

        // Level 1 entities
        public DbSet<Experiment> Experiments { get; set; }

        // Level 2 entities
        public DbSet<ExpDrug> ExpDrugs { get; set; }
        public DbSet<ExpGroup> ExpGroups { get; set; }
        public DbSet<ExpIndicator> ExpIndicators { get; set; }

        // Level 3 entities
        public DbSet<Protocol> Protocols { get; set; }

        private static int[] BitsSet { get; } =
            Enumerable.Range(0, 256)
                .Select(o => Enumerable.Range(0, 8).Sum(p => (o >> p) & 1))
                .ToArray();

        public float? ComputeTanimotoScore(byte[] fp1, byte[] fp2)
        {
            if (fp1.Length != fp2.Length)
                return null;

            int intersect = 0, union = 0;
            for (int i = 0; i < fp1.Length; i++)
            {
                intersect += BitsSet[fp1[i] & fp2[i]];
                union += BitsSet[fp1[i] | fp2[i]];
            }

            return intersect / (float)union;
        }

        public (Drug, float) SearchForSimilarDrug(Guid subjectId, string smiles, byte[] fp2)
        {
            var ret = Subjects
                .Where(o => o.Id == subjectId)
                .SelectMany(o => o.Experiments.SelectMany(p => p.ExpDrugs))
                .Where(o => o.Drug.Fingerprint != null)
                .Select(o => o.Drug)
                .Distinct()
                .AsEnumerable()
                .Select(o => new
                {
                    data = o,
                    score = ComputeTanimotoScore(o.Fingerprint, fp2)!.Value,
                })
                .OrderByDescending(o => o.score)
                .FirstOrDefault();
            return (ret.data, ret.score);
        }
    }
}
