using Windows.UI.Xaml;

namespace RatCvsSimulation.Model
{
    public class DrugProperties
    {
        public const float None = -1.0f;

        public float Potency = None;
        public float Efficacy = None;
    }

    public class Drug
    {
        public readonly string Name;
        public readonly string ShortName;
        public readonly string Smiles;
        public readonly Visibility SmilesVisibility;

        public readonly DrugProperties Alpha1AdR = new DrugProperties();
        public readonly DrugProperties Alpha2AdR = new DrugProperties();
        public readonly DrugProperties Beta1AdR = new DrugProperties();
        public readonly DrugProperties Beta2AdR = new DrugProperties();
        public readonly DrugProperties MusChR = new DrugProperties();
        public readonly DrugProperties VagChR = new DrugProperties();
        public readonly DrugProperties NicChR = new DrugProperties();
        public readonly DrugProperties HMCaChannel = new DrugProperties();
        public readonly DrugProperties SMCaChannel = new DrugProperties();
        public readonly DrugProperties AdenR = new DrugProperties();
        public readonly DrugProperties KChannel = new DrugProperties();
        public readonly DrugProperties NOX = new DrugProperties();
        public readonly DrugProperties NOS = new DrugProperties();
        public readonly DrugProperties Angt1R = new DrugProperties();
        public readonly DrugProperties Angt2R = new DrugProperties();
        public readonly DrugProperties BradR = new DrugProperties();
        public readonly DrugProperties ACE = new DrugProperties();
        public readonly DrugProperties DigR = new DrugProperties();
        public readonly DrugProperties PDE = new DrugProperties();

        public float Dose = 0.0f;
        public float DoseInjected = 0.0f;
        public float Conc = 0.0f;

        public float OnRate = 1.0f;
        public float RemovalRate = 0.05f;
        public float MinDose;
        public float MaxDose;

        public Drug(string name, string shortName, string smiles = null)
        {
            Name = name;
            ShortName = shortName;
            Smiles = smiles;
            SmilesVisibility = string.IsNullOrWhiteSpace(smiles) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}