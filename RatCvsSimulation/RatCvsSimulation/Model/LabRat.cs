using RatCvsSimulation.Core;
using System;
using System.Linq;

namespace RatCvsSimulation.Model
{
    public class LabRat
    {
        public bool Dead { get; private set; } = false;
        public readonly Drug[] Drugs = new Drug[51];

        #region Internal status
        // TODO: check how this status affects stimulation
        float Alpha2AdR = 0f; // Proportion of Alpha2 adrenoceptors activated

        // cache variables
        int iAlpha1AdrNerves;
        int iBeta1AdrNerves;
        int iAngiotensin1; // Angiotensin I entry with Drugs array
        int iAngiotensin2ACE; // ACE-produced Angiotensin II entry with Drugs array
        int iRenalNerve;
        int iVagusNerve;

        int Dying = 0;
        float Fibrillation = 0.0f;
        //BP BP;
        #endregion

        protected Element Ven;
        protected Element Art;
        protected Element Heart;
        protected Element Con;

        public int MeanHR { get; set; }

        public LabRat()
        {
            CardiogramData.Drugs = Drugs;
        }

        #region Initialization
        public void Init()
        {
            InitDrugs();
            InitElements();
        }

        void InitDrugs()
        {
            const float FastOn = 0.006f;
            const float FastOff = 0.0035f;
            const float SlowOn = 0.002f;
            const float SlowOff = 5E-4f;

            // Initialise drug doses and potencies
            for (int i = 0; i < Drugs.Length; i++)
                Drugs[i] = null;

            int iDrug = 0;

            // *** STANDARD DRUGS ***************************************************
            // Adrenaline - Alpha11,beta1,beta2 adrenoceptor agonist
            Drugs[iDrug] = new Drug("Adrenaline", "Adr", "CNC[C@H](O)C1=CC=C(O)C(O)=C1")
            {
                OnRate = FastOn,
                RemovalRate = FastOff,
                MinDose = 1E-4f,
                MaxDose = 1E-1f
            };
            SetAgonistPotency(Drugs[iDrug].Alpha1AdR, 3.5e-3f);
            SetAgonistPotency(Drugs[iDrug].Beta1AdR, 3.5E-3f);
            SetAgonistPotency(Drugs[iDrug].Beta2AdR, 5E-3f);

            iDrug++;
            Drugs[iDrug] = new Drug("Noradrenaline", "Nor")
            {
                OnRate = FastOn,
                RemovalRate = FastOff,
                MinDose = 1E-3f,
                MaxDose = 1.0f
            };
            SetAgonistPotency(Drugs[iDrug].Alpha1AdR, 1E-2f);
            SetAgonistPotency(Drugs[iDrug].Beta1AdR, 1.5E-2f);
            SetAgonistPotency(Drugs[iDrug].Beta2AdR, 1.5E-1f);

            iDrug++;
            Drugs[iDrug] = new Drug("Isoprenaline", "Iso", "Oc1ccc(cc1O)C(O)CNC(C)C")
            {
                OnRate = FastOn,
                RemovalRate = FastOff,
                MinDose = 1E-4f,
                MaxDose = 1E-1f
            };
            SetAgonistPotency(Drugs[iDrug].Beta1AdR, 1E-3f);
            SetAgonistPotency(Drugs[iDrug].Beta2AdR, 7E-4f);

            iDrug++;
            Drugs[iDrug] = new Drug("Phenylephrine", "Phe", "O[C@H](c1cc(O)ccc1)CNC")
            {
                OnRate = FastOn,
                RemovalRate = FastOff,
                MinDose = 1E-3f,
                MaxDose = 1.0f
            };
            SetAgonistPotency(Drugs[iDrug].Alpha1AdR, 1E-2f);

            iDrug++;
            Drugs[iDrug] = new Drug("Phentolamine", "Phe") // Alpha Adr.
            {
                OnRate = FastOn, // antagonist
                RemovalRate = SlowOff,
                MinDose = 1E-2f,
                MaxDose = 10.0f
            };
            SetAntagonistPotency(Drugs[iDrug].Alpha1AdR, 1E-1f);
            SetAgonistPotency(Drugs[iDrug].Alpha2AdR, 1E-1f);

            iDrug++;
            Drugs[iDrug] = new Drug("Prazosin", "Pra") // Alpha-1 Adr.
            {
                OnRate = FastOn, // antagonist
                RemovalRate = SlowOff,
                MinDose = 0.01f,
                MaxDose = 10.0f
            };
            SetAntagonistPotency(Drugs[iDrug].Alpha1AdR, 0.1f);

            iDrug++;
            Drugs[iDrug] = new Drug("Propranolol", "Pro") // Beta Adr.
            {
                OnRate = FastOn, // antagonist
                RemovalRate = SlowOff, // Potency modified 4/7/00
                MinDose = 0.01f,
                MaxDose = 100.0f
            };
            SetAntagonistPotency(Drugs[iDrug].Beta1AdR, 0.5f);
            SetAntagonistPotency(Drugs[iDrug].Beta2AdR, 0.5f);

            iDrug++;
            Drugs[iDrug] = new Drug("Atenolol", "Ate") // Beta-1 Adr.
            {
                OnRate = FastOn, // antagonist
                RemovalRate = SlowOff,
                MinDose = 1.0f,
                MaxDose = 1000.0f
            };
            SetAntagonistPotency(Drugs[iDrug].Beta1AdR, 10.0f);

            iDrug++;
            Drugs[iDrug] = new Drug("Acetylcholine", "Ach")
            {
                OnRate = FastOn,
                RemovalRate = FastOff * 2.0f,
                MinDose = 1E-3f,
                MaxDose = 1.0f
            };
            SetAgonistPotency(Drugs[iDrug].MusChR, 5E-3f);
            SetAgonistPotency(Drugs[iDrug].NicChR, 0.15f);
            SetAgonistPotency(Drugs[iDrug].Alpha1AdR, 0.3f);
            SetAgonistPotency(Drugs[iDrug].Beta1AdR, 0.15f);

            iDrug++;
            Drugs[iDrug] = new Drug("Atropine", "Atr")
            {
                OnRate = FastOn,
                RemovalRate = SlowOff,
                MinDose = 0.01f,
                MaxDose = 10.0f
            };
            SetAntagonistPotency(Drugs[iDrug].MusChR, 0.1f); // Potency reduced to 0.1 4/7/00
            SetAntagonistPotency(Drugs[iDrug].VagChR, 0.1f);

            iDrug++;
            Drugs[iDrug] = new Drug("Adenosine", "Ade")
            {
                OnRate = FastOn,
                RemovalRate = FastOff,
                MinDose = 0.1f,
                MaxDose = 100.0f
            };
            SetAgonistPotency(Drugs[iDrug].AdenR, 1.0f);

            iDrug++;
            Drugs[iDrug] = new Drug("8-s-p-theophylline", "8spt")
            {
                OnRate = FastOn,
                RemovalRate = SlowOff,
                MinDose = 0.1f,
                MaxDose = 100.0f
            };
            SetAntagonistPotency(Drugs[iDrug].AdenR, 5.0f);

            iDrug++;
            Drugs[iDrug] = new Drug("Verapamil", "Ver")
            {
                OnRate = SlowOn,
                RemovalRate = SlowOff,
                MinDose = 0.1f,
                MaxDose = 100.0f
            };
            SetAgonistPotency(Drugs[iDrug].HMCaChannel, 0.75f);
            SetAgonistPotency(Drugs[iDrug].SMCaChannel, 0.75f);

            iDrug++;
            Drugs[iDrug] = new Drug("L-NOARG", "LNO")
            {
                OnRate = FastOn,
                RemovalRate = SlowOff,
                MinDose = 1.0f,
                MaxDose = 1000.0f
            };
            SetAgonistPotency(Drugs[iDrug].NOS, 10.0f);

            iDrug++;
            Drugs[iDrug] = new Drug("Cromakalim", "Cro") // K channel opener
            {
                OnRate = SlowOn, // causes vasodilation
                RemovalRate = SlowOff,
                MinDose = 0.1f,
                MaxDose = 100.0f
            };
            SetAgonistPotency(Drugs[iDrug].KChannel, 0.3f);

            iDrug++;
            Drugs[iDrug] = new Drug("Glibenclamide", "Gli") // K Channel blocker
            {
                OnRate = SlowOn,
                RemovalRate = SlowOff,
                MinDose = 0.1f,
                MaxDose = 100.0f
            };
            SetAntagonistPotency(Drugs[iDrug].KChannel, 1.0f);

            iDrug++;
            Drugs[iDrug] = new Drug("Angiotensin I", "An1")
            {
                OnRate = FastOn,
                RemovalRate = FastOff,
                MinDose = 1E-4f,
                MaxDose = 1E-2f
            };
            SetAgonistPotency(Drugs[iDrug].Angt1R, 1E-4f);
            iAngiotensin1 = iDrug;

            iDrug++;
            Drugs[iDrug] = new Drug("Angiotensin II", "An2")
            {
                OnRate = FastOn,
                RemovalRate = FastOff,
                MinDose = 1E-4f,
                MaxDose = 1E-2f
            };
            SetAgonistPotency(Drugs[iDrug].Angt2R, 1E-4f);

            iDrug++;
            Drugs[iDrug] = new Drug("AngiotensinIIACE", "An2")
            {
                OnRate = FastOn,
                RemovalRate = FastOff,
                MinDose = 1E-4f,
                MaxDose = 1E-2f
            };
            SetAgonistPotency(Drugs[iDrug].Angt2R, 1E-4f);
            iAngiotensin2ACE = iDrug;

            iDrug++;
            Drugs[iDrug] = new Drug("Captopril", "Cap") // ACE inhibitor
            {
                OnRate = SlowOn,
                RemovalRate = SlowOff,
                MinDose = 1.0f,
                MaxDose = 1000.0f
            };
            SetAgonistPotency(Drugs[iDrug].ACE, 5.0f);
            //SetAgonistPotency(Drugs[iDrug].BradR, 5.0 );

            iDrug++;
            Drugs[iDrug] = new Drug("Losartan", "Los") // Blocks angiotensin II
            {
                OnRate = FastOn,
                RemovalRate = SlowOff,
                MinDose = 1.0f,
                MaxDose = 1000.0f
            };
            SetAntagonistPotency(Drugs[iDrug].Angt2R, 1.0f); // Potency reduced to 1 4/7/00)

            iDrug++;
            Drugs[iDrug] = new Drug("Digoxin", "Dig") // Digoxin
            {
                OnRate = SlowOn * 0.25f,
                RemovalRate = SlowOff,
                MinDose = 0.1f,
                MaxDose = 100.0f
            };
            SetAgonistPotency(Drugs[iDrug].DigR, 0.5f);

            iDrug++;
            Drugs[iDrug] = new Drug("Glyceryl trinitrate", "GTN") // Glyceryl trinitrate
            {
                OnRate = FastOn,
                RemovalRate = FastOff,
                MinDose = 1.0f,
                MaxDose = 1000.0f
            };
            SetAgonistPotency(Drugs[iDrug].NOX, 10.0f);

            iDrug++;
            Drugs[iDrug] = new Drug("Milrinone", "Mil") // Phosphodiesterase inh.
            {
                OnRate = SlowOn,
                RemovalRate = SlowOff,
                MinDose = 1.0f,
                MaxDose = 1000.0f
            };
            SetAgonistPotency(Drugs[iDrug].PDE, 10.0f);

            // ***UNKNOWN DRUGS * ****************************************************

            iDrug++;
            Drugs[iDrug] = new Drug("Drug A", "A") // Acetylcholine
            {
                OnRate = FastOn,
                RemovalRate = FastOff,
                MinDose = 1E-3f,
                MaxDose = 1.0f
            };
            SetAgonistPotency(Drugs[iDrug].MusChR, 5E-3f);
            SetAgonistPotency(Drugs[iDrug].NicChR, 0.1f);
            SetAgonistPotency(Drugs[iDrug].Alpha1AdR, 0.3f);
            SetAgonistPotency(Drugs[iDrug].Beta1AdR, 0.15f);

            iDrug++;
            Drugs[iDrug] = new Drug("Drug B", "B") // Adenosine
            {
                OnRate = FastOn,
                RemovalRate = FastOff,
                MinDose = 0.1f,
                MaxDose = 1000.0f
            };
            SetAgonistPotency(Drugs[iDrug].AdenR, 1.0f);

            iDrug++;
            Drugs[iDrug] = new Drug("Drug C", "C") // Isoprenaline
            {
                OnRate = FastOn,
                RemovalRate = SlowOff,
                MinDose = 1E-4f,
                MaxDose = 1E-1f
            };
            SetAgonistPotency(Drugs[iDrug].Beta1AdR, 1E-3f);
            SetAgonistPotency(Drugs[iDrug].Beta2AdR, 7E-4f);

            iDrug++;
            Drugs[iDrug] = new Drug("Drug D", "D") // Noradrenaline
            {
                OnRate = FastOn,
                RemovalRate = FastOff,
                MinDose = 1E-3f,
                MaxDose = 1.0f
            };
            SetAgonistPotency(Drugs[iDrug].Alpha1AdR, 1E-2f);
            SetAgonistPotency(Drugs[iDrug].Beta1AdR, 1.5E-2f);
            SetAgonistPotency(Drugs[iDrug].Beta2AdR, 1.5E-1f);

            iDrug++;
            Drugs[iDrug] = new Drug("Drug E", "E") // Sodium nitroprusside
            {
                OnRate = FastOn,
                RemovalRate = FastOff,
                MinDose = 1.0f,
                MaxDose = 1000.0f
            };
            SetAgonistPotency(Drugs[iDrug].NOX, 15.0f);

            iDrug++;
            Drugs[iDrug] = new Drug("Drug F", "F") // Cromokalim
            {
                OnRate = SlowOn,
                RemovalRate = SlowOff,
                MinDose = 1.0f,
                MaxDose = 1000.0f
            };
            SetAgonistPotency(Drugs[iDrug].KChannel, 0.3f);

            iDrug++;
            Drugs[iDrug] = new Drug("Drug G", "G") // Milrinone
            {
                OnRate = SlowOn,
                RemovalRate = SlowOff,
                MinDose = 1.0f,
                MaxDose = 1000.0f
            };
            SetAgonistPotency(Drugs[iDrug].PDE, 10.0f);

            iDrug++;
            Drugs[iDrug] = new Drug("Drug H", "H") // Propanalol
            {
                OnRate = FastOn,
                RemovalRate = SlowOff,
                MinDose = 1.0f,
                MaxDose = 1000.0f
            };
            SetAntagonistPotency(Drugs[iDrug].Beta1AdR, 10.0f);
            SetAntagonistPotency(Drugs[iDrug].Beta2AdR, 10.0f);

            iDrug++;
            Drugs[iDrug] = new Drug("Drug I", "I") // Verapamil
            {
                OnRate = SlowOn,
                RemovalRate = SlowOff,
                MinDose = 0.1f,
                MaxDose = 100.0f
            };
            SetAgonistPotency(Drugs[iDrug].HMCaChannel, 0.75f);
            SetAgonistPotency(Drugs[iDrug].SMCaChannel, 0.75f);

            iDrug++;
            Drugs[iDrug] = new Drug("Drug J", "J") // L-NOARG (NOS-inhibitor)
            {
                OnRate = FastOn,
                RemovalRate = SlowOff,
                MinDose = 1.0f,
                MaxDose = 1000.0f
            };
            SetAgonistPotency(Drugs[iDrug].NOS, 10.0f);

            iDrug++;
            Drugs[iDrug] = new Drug("Drug K", "K") // Atropine
            {
                OnRate = FastOn,
                RemovalRate = SlowOff,
                MinDose = 0.1f,
                MaxDose = 100.0f
            };
            SetAntagonistPotency(Drugs[iDrug].MusChR, 0.1f); // Potency reduced to 0.1 4/7/00
            SetAntagonistPotency(Drugs[iDrug].VagChR, 0.1f);

            // *** NERVE STIMULATION **************************************************
            // Cardiac nerve stimulation - beta1 adrenoceptor agonist
            iDrug++;
            Drugs[iDrug] = new Drug("Cardiac", "Symp(H)")
            {
                OnRate = FastOn * 4.0f,
                RemovalRate = FastOff * 4.0f
            };
            SetAgonistPotency(Drugs[iDrug].Beta1AdR, 1.5E-2f);

            // Adrenal nerve stimulation - Alpha1,beta1,beta2 adrenoceptor agonist
            iDrug++;
            Drugs[iDrug] = new Drug("AdrenNerve", "Symp(AD)")
            {
                OnRate = FastOn * 2.0f,
                RemovalRate = FastOff
            };
            SetAgonistPotency(Drugs[iDrug].Alpha1AdR, 5E-3f);
            SetAgonistPotency(Drugs[iDrug].Beta1AdR, 3.55E-3f);
            SetAgonistPotency(Drugs[iDrug].Beta2AdR, 5E-3f);

            // Blood vessel symp. nerve stimulation - noradrenaline
            iDrug++;
            Drugs[iDrug] = new Drug("SympNerve", "Symp(-AD)")
            {
                OnRate = FastOn * 7.0f,
                RemovalRate = FastOff * 4.0f
            };
            SetAgonistPotency(Drugs[iDrug].Alpha1AdR, 1E-2f);
            SetAgonistPotency(Drugs[iDrug].Beta1AdR, 1.5E-2f);
            SetAgonistPotency(Drugs[iDrug].Beta2AdR, 1.5E-1f);

            // Vagus nerve stimulation - cholinergic agonist
            iDrug++;
            Drugs[iDrug] = new Drug("Vagus", "Vag. Nv.")
            {
                OnRate = FastOn * 4.0f,
                RemovalRate = FastOn * 4.0f
            };
            SetAgonistPotency(Drugs[iDrug].VagChR, 5E-3f);
            iVagusNerve = iDrug;

            // Renal nerve stimulation - Release angiotensin I - vasoconstrictor
            iDrug++;
            Drugs[iDrug] = new Drug("Renal", "Ren. Nv.")
            {
                OnRate = FastOn,
                RemovalRate = FastOff
            };
            SetAgonistPotency(Drugs[iDrug].Angt1R, 2E-4f);
            iRenalNerve = iDrug;

            // Vascular nerve stimulation acting on alpha-1 adrenoceptors
            iDrug++;
            Drugs[iDrug] = new Drug("Alpha1", "???")
            {
                OnRate = FastOn,
                RemovalRate = FastOff * 2.0f
            };
            SetAgonistPotency(Drugs[iDrug].Alpha1AdR, 1E-2f);
            iAlpha1AdrNerves = iDrug;

            // Nerve stimulation acting on beta-1 adrenoceptors in heart
            iDrug++;
            Drugs[iDrug] = new Drug("Beta1", "???")
            {
                OnRate = FastOn,
                RemovalRate = FastOff * 2.0f
            };
            SetAgonistPotency(Drugs[iDrug].Beta1AdR, 1E-2f);
            iBeta1AdrNerves = iDrug;
        }

        void InitElements()
        {
            Heart = new Element()
            {
                Elastance = 1.0f / 30.0f,
                Volume0 = 5.0f,
                Volume = 10.0f,
                Conductance = 2500.0f,
                NextBeat = 0.0f,
                Rate = 400.0f
            };

            Art = new Element()
            {
                Conductance = 0.05f,
                Elastance = 1.0f / 0.8f,
                Volume0 = 100.0f,
                Volume = 150.0f
            };

            Ven = new Element()
            {
                Conductance = 5000.0f,
                Elastance = 1.0f / 30.0f,
                Volume0 = 300.0f,
                Volume = 600.0f
            };

            Con = new Element();
        }
        #endregion

        #region Operations which modify model states
        /// <summary>
        /// Calculate next step of simulated BP and HR
        /// </summary>
        public void Tick(Ticker t, bool pithedRat)
        {
            float x;
            int i;
            float Num = 0, Denom = 0;
            float PotencyShift, TMid, YMax, YMin;

            float MusChR = 0f; // Proportion of muscarinic cholinoceptors activated
            float VagChR = 0f; // Proportion of muscarinic cholinoceptors activated
            float NicChR = 0f; // Proportion of nicotinic cholinoceptors activated
            float Alpha1AdR = 0f; // Proportion of Alpha1 adrenoceptors activated
            float Beta1AdR = 0f; // Proportion of beta-1 adrenoceptors activated
            float Beta2AdR = 0f; // Proportion of beta-2 adrenoceptors activated
            float AdenR = 0f; // Proportion of adenosine receptors activated
            float HMCaChannels = 0f; // Proportion of heart muscle Ca channels blocked
            float SMCaChannels = 0f; // Proportion of smooth muscle Ca channels blocked
            float KChannels = 0f; // K channel mediated vasodilation
            float NOX = 0f; // Nitric oxide mediated vasodilation
            float NOS = 0f; // Nitric oxide synthase
            float Angt1R = 0f; // Angiotensin I receptor
            float Angt2R = 0f; // Angiotensin II receptor

            float ACE = 0f; // Angiotensin converting enzyme activity
            float BradR = 0f; // Bradykinin recptor
            float DigR = 0f; // Digoxin activity on cardiac force
            float PDE = 0f; // Degree of phosphidesterase inhibition
            //float Baroreceptors = 0f;

            if (t.Time >= t.Next)
            {
                // Ensure resting levels of angiotensin in circulation
                Drugs[iRenalNerve].Dose = Math.Max(Drugs[iRenalNerve].Dose, Drugs[iRenalNerve].Angt1R.Potency * 0.25f);

                // Nicotinic (ganglion) cholinoceptor activation
                for (i = 0; i < Drugs.Length; i++)
                    if (Drugs[i] != null)
                        NicChR = ReceptorActivation(i, Drugs[i].Dose, Drugs[i].NicChR, ref Num, ref Denom, 1.0f);

                // Effects of sympathetic nerve and vagal baroreceptor feedback in normal UNPITHED rat
                if (!pithedRat)
                {
                    // Basal sympathetic nerve stimulation of blood vessels
                    Drugs[iAlpha1AdrNerves].Dose = Drugs[iAlpha1AdrNerves].Alpha1AdR.Potency * 0.4f;
                    Drugs[iBeta1AdrNerves].Dose = Drugs[iBeta1AdrNerves].Beta1AdR.Potency * 0.2f;
                    // ** Baroreceptor feedback loops (in normal rat only) **
                    Drugs[iVagusNerve].Dose = Math.Max(Drugs[iVagusNerve].Dose,
                        Drugs[iVagusNerve].VagChR.Potency * 5.0f / (1.0f + (float)Math.Exp(-Art.MeanPressure - 130.0f / 8.0f)));
                }

                // Update drugs in circulation
                for (i = 0; i < Drugs.Length; i++)
                    if (Drugs[i] != null)
                    {
                        Drugs[i].Dose = Drugs[i].Dose + Drugs[i].OnRate * (Drugs[i].DoseInjected - Drugs[i].Dose) - Drugs[i].Dose * Drugs[i].RemovalRate;
                        Drugs[i].Dose = Math.Max(Drugs[i].Dose, 0.0f);
                        Drugs[i].DoseInjected = Math.Max(Drugs[i].DoseInjected - Drugs[i].DoseInjected * Drugs[i].RemovalRate, 0.0f);
                    }

                // Muscarinic cholinoceptor activation
                for (i = 0; i < Drugs.Length; i++)
                    if (Drugs[i] != null)
                        MusChR = ReceptorActivation(i, Drugs[i].Dose, Drugs[i].MusChR, ref Num, ref Denom, 1.0f);

                // Vagally stimulated muscarinic cholinoceptor activation
                for (i = 0; i < Drugs.Length; i++)
                    if (Drugs[i] != null)
                        VagChR = ReceptorActivation(i, Drugs[i].Dose, Drugs[i].VagChR, ref Num, ref Denom, 1.0f);

                // Phosphodiesterase inhibition
                for (i = 0; i < Drugs.Length; i++)
                    if (Drugs[i] != null)
                        PDE = ReceptorActivation(i, Drugs[i].Dose, Drugs[i].PDE, ref Num, ref Denom, 1.0f);

                // Alpha1-adrenoceptor activation
                for (i = 0; i < Drugs.Length; i++)
                    if (Drugs[i] != null)
                        Alpha1AdR = ReceptorActivation(i, Drugs[i].Dose, Drugs[i].Alpha1AdR, ref Num, ref Denom, 1.0f);

                // Alpha2-adrenoceptor activation
                for (i = 0; i < Drugs.Length; i++)
                    if (Drugs[i] != null)
                        Alpha2AdR = ReceptorActivation(i, Drugs[i].Dose, Drugs[i].Alpha2AdR, ref Num, ref Denom, 1.0f);

                // Beta1-adrenoceptor activation
                PotencyShift = 1.0f - PDE * 0.5f;

                for (i = 0; i < Drugs.Length; i++)
                    if (Drugs[i] != null)
                        Beta1AdR = ReceptorActivation(i, Drugs[i].Dose, Drugs[i].Beta1AdR, ref Num, ref Denom, PotencyShift);

                // Beta2-adrenoceptor activation
                PotencyShift = 1.0f - PDE * 0.5f;

                for (i = 0; i < Drugs.Length; i++)
                    if (Drugs[i] != null)
                        Beta2AdR = ReceptorActivation(i, Drugs[i].Dose, Drugs[i].Beta2AdR, ref Num, ref Denom, PotencyShift);

                // Heart muscle calcium channel activation
                for (i = 0; i < Drugs.Length; i++)
                    if (Drugs[i] != null)
                        HMCaChannels = ReceptorActivation(i, Drugs[i].Dose, Drugs[i].HMCaChannel, ref Num, ref Denom, 1.0f);

                // Digoxin effect on heart muscle
                for (i = 0; i < Drugs.Length; i++)
                    if (Drugs[i] != null)
                        DigR = ReceptorActivation(i, Drugs[i].Dose, Drugs[i].DigR, ref Num, ref Denom, 1.0f);

                // Smooth muscle calcium channel activation
                for (i = 0; i < Drugs.Length; i++)
                    if (Drugs[i] != null)
                        SMCaChannels = ReceptorActivation(i, Drugs[i].Dose, Drugs[i].SMCaChannel, ref Num, ref Denom, 1.0f);

                // Adenosine receptor activation
                for (i = 0; i < Drugs.Length; i++)
                    if (Drugs[i] != null)
                        AdenR = ReceptorActivation(i, Drugs[i].Dose, Drugs[i].AdenR, ref Num, ref Denom, 1.0f);

                // K channel activation
                for (i = 0; i < Drugs.Length; i++)
                    if (Drugs[i] != null)
                        KChannels = ReceptorActivation(i, Drugs[i].Dose, Drugs[i].KChannel, ref Num, ref Denom, 1.0f);

                // Nitric oxide synthase
                for (i = 0; i < Drugs.Length; i++)
                    if (Drugs[i] != null)
                        NOS = ReceptorActivation(i, Drugs[i].Dose, Drugs[i].NOS, ref Num, ref Denom, 1.0f);

                // Nitric oxide activation
                // Note. Intrinsic generation of nitric oxide means that a certain
                // degree of vasodilation exists in the absence of drugs
                for (i = 0; i < Drugs.Length; i++)
                    if (Drugs[i] != null)
                        NOX = ReceptorActivation(i, Drugs[i].Dose, Drugs[i].NOX, ref Num, ref Denom, 1.0f);

                // Angiotensin I activation
                for (i = 0; i < Drugs.Length; i++)
                    if (Drugs[i] != null)
                        Angt1R = ReceptorActivation(i, Drugs[i].Dose, Drugs[i].Angt1R, ref Num, ref Denom, 1.0f);

                // Angiotensin converting enzyme activity
                for (i = 0; i < Drugs.Length; i++)
                    if (Drugs[i] != null)
                        ACE = ReceptorActivation(i, Drugs[i].Dose, Drugs[i].ACE, ref Num, ref Denom, 1.0f);

                // Angiotensin II produce from Angiotensin I by angiotensin converting enzyme
                Drugs[iAngiotensin2ACE].Dose = Drugs[iAngiotensin2ACE].Angt2R.Potency * 5.0f * (1.0f - ACE) * Angt1R / (1.0f + Angt1R);

                // Angiotensin II activation
                for (i = 0; i < Drugs.Length; i++)
                    if (Drugs[i] != null)
                        Angt2R = ReceptorActivation(i, Drugs[i].Dose, Drugs[i].Angt2R, ref Num, ref Denom, 1.0f);

                // Bradykinin receptor activation
                for (i = 0; i < Drugs.Length; i++)
                    if (Drugs[i] != null)
                        BradR = ReceptorActivation(i, Drugs[i].Dose, Drugs[i].BradR, ref Num, ref Denom, 1.0f);

                t.Next = t.Time + 0.005f;
            }

            Art.Conductance = 5.0f * (1.0f
                + 2.5f * MusChR * (1.0f - NOS) // Muscarinic receptors VD
                + 2.25f * Beta2AdR // Beta-2 adrenoceptor VD
                + 2.5f * AdenR // Adenosine receptors VD
                + 2.0f * NOX // Nitric oxide VD
                + 2.0f * PDE // Phosphodiesterase inhibition VD
                + 0.95f * SMCaChannels // Ca channel block VD
                + 0.95f * KChannels // K channel opening VD
                + BradR // Bradykinin VD
                - 0.6f * NOS // Inhib. of NO synthase VC
                - 0.9f * Alpha1AdR // Alpha-1 adrenoceptor VD
                                   /* - 0.9*NicChR */ // Nicotinic VC
                - 0.9f * Angt2R // Angiotension II VC
                );
            Art.Conductance = Math.Max(Art.Conductance, 0.25f);

            // Arterial elastance increased by vasoconstrictors
            // Results in increased pulse pressure
            Art.Elastance = 1.0f / 0.8f * (1.0f + 0.2f * Alpha1AdR);

            // Venous elastance : decreased by certain vasodilators
            // Decreased elastance = increased venous capacity
            Ven.Elastance = 1.0f / 30.0f * (1.0f - 0.7f * Math.Min(1.0f, NOX + SMCaChannels - Alpha1AdR));

            Heart.InitialisePressureRange();
            Art.InitialisePressureRange();
            Ven.InitialisePressureRange();
            Con.InitialisePressureRange();
            YMax = -1E38f;
            YMin = 1E38f;

            for (i = 1; i <= 80; i++)
            {
                // Set heart rate and force for next beat
                if (t.Time >= Heart.NextBeat)
                {
                    Art.MeanPressure = (Art.MaxPressure - Art.MinPressure) * 0.33f + Art.MinPressure;
                    Art.MinPressure = 300.0f;
                    Art.MaxPressure = 0.0f;

                    // Heart rate
                    Heart.Rate = 1.0f + 350.0f * (1.0f - 0.7f * VagChR - MusChR - AdenR + Beta1AdR - HMCaChannels + 1.0f * PDE);
                    Heart.Rate = Math.Max(Heart.Rate, 1.0f);

                    // Contractile force
                    Heart.Force = 8.0f * (1.0f - 0.95f * MusChR - 0.95f * AdenR - 0.6f * KChannels + 1.5f * Beta1AdR + DigR + PDE) * (1.0f - HMCaChannels) * 1.0f / (1.0f + Heart.Volume0 / Heart.Volume);

                    // Duration of contraction (systole)
                    Heart.Time = 12.0f / 400.0f;

                    if (Heart.Time >= 12.0f / Heart.Rate)
                        Heart.Time = 12.0f / Heart.Rate;

                    Heart.TimeSystole = Heart.Time;

                    // Time for next beat
                    Heart.NextBeat = t.Time + 60.0f / Heart.Rate * 100 / MeanHR;
                }

                if (Heart.Time > 0.0f)
                {
                    // Compute cardiac contractile force
                    TMid = t.Step * (float)Math.Round(Heart.TimeSystole * 0.5f / t.Step);
                    x = (Heart.Time - TMid) / (TMid * 0.33f);
                    //Heart.Elastance = (Math.Sin(3.141 * (Heart.Time / Heart.TSystole))
                    //* Heart.Force)
                    //+ Ven.Elastance * 4.0;
                    Heart.Elastance = (float)Math.Exp(-x * x) * Heart.Force + Ven.Elastance * 4.0f;
                    Heart.Time = Heart.Time - t.Step;
                }

                if (DigR > 0.95f)
                {
                    Heart.Elastance = Heart.Force * AddNoise(0.5f) * 0.25f;
                    Fibrillation = Fibrillation + t.Step;
                    Heart.Rate = 300.0f * AddNoise(0.5f);
                    Heart.Pressure = 50.0f * AddNoise(1.0f);
                }
                else
                    Heart.Pressure = Pressure(Heart);

                Con.Pressure = Heart.Elastance;
                YMax = Math.Max(YMax, Heart.Elastance);
                YMin = Math.Min(YMin, Heart.Elastance);

                // Arterial valve
                if (Heart.Pressure >= Art.Pressure && Fibrillation <= 0.0f)
                {
                    Heart.Flow = (Heart.Pressure - Art.Pressure) * Heart.Conductance;
                    Art.Volume = Art.Volume + Heart.Flow * t.Step;
                    Heart.Volume = Heart.Volume - Heart.Flow * t.Step;
                }

                Art.Pressure = Pressure(Art);

                // Find Systolic/Diastolic pressures
                Art.MaxPressure = Math.Max(Art.MaxPressure, Art.Pressure);
                Art.MinPressure = Math.Min(Art.MinPressure, Art.Pressure);
                Art.Flow = (Art.Pressure - Ven.Pressure) * Art.Conductance;
                Ven.Volume = Ven.Volume + Art.Flow * t.Step;
                Art.Volume = Art.Volume - Art.Flow * t.Step;
                Ven.Pressure = Pressure(Ven);

                // Venous system -> heart valve
                if (Ven.Pressure >= Heart.Pressure && Fibrillation <= 0.0f)
                {
                    Ven.Flow = (Ven.Pressure - Heart.Pressure) * Ven.Conductance;
                    Heart.Volume = Heart.Volume + Ven.Flow * t.Step;
                    Ven.Volume = Ven.Volume - Ven.Flow * t.Step;
                }

                t.Time = t.Time + t.Step;
                Heart.CalculatePressureRange(t.Time);
                Art.CalculatePressureRange(t.Time);
                Ven.CalculatePressureRange(t.Time);
                Con.CalculatePressureRange(t.Time);
            }

            // The rat dies if the B.P. falls too low for too long
            if (!Dead)
            {
                if (Art.Pressure < 20.0f)
                    Dying++;
                else
                    Dying = 0;

                if (Dying == 500)
                {
                    Dead = true;
                }
            }
            else
            {
            }
        }

        public int InjectDrug(Drug drug, float dose, float scale)
        {
            // Find drug to be added
            int ix = FindDrug(drug);
            //if (ix == -1) MessageDlg(DrugName + " not available.", mtWarning, mbOK, 0);

            if (ix >= 0)
            {
                // TODO: no scale here??
                Drugs[ix].DoseInjected = dose * AddNoise(0.1f);
            }

            return ix;
        }

        /// <summary>
        /// Stimulation cardiac sympathetic nerves
        /// </summary>
        /// <param name="drug"></param>
        /// <returns></returns>
        public int StimCardiac(Drug drug)
        {
            int ix;

            ix = FindDrug(drug);
            //if (ix == -1) MessageDlg(DrugName + " not available.", mtWarning, mbOK, 0);

            Drugs[ix].DoseInjected = 5.0f * Drugs[ix].Beta1AdR.Potency * (0.5f + Alpha2AdR);

            return ix;
        }

        /// <summary>
        /// Stimulation sympathetic nerves to blood vessels
        /// </summary>
        /// <param name="drug"></param>
        public int StimBloodVessels(Drug drug)
        {
            int ix;

            ix = FindDrug(drug);
            //if (ix == -1) MessageDlg(DrugName + " not available.", mtWarning, mbOK, 0);
            Drugs[ix].DoseInjected = 4.0f * Drugs[ix].Beta1AdR.Potency * (0.5f + Alpha2AdR);

            return ix;
        }

        /// <summary>
        /// Stimulation adrenal sympathetic nerves
        /// </summary>
        /// <param name="drug"></param>
        public int StimAdrenal(Drug drug)
        {
            int ix;

            ix = FindDrug(drug);
            //if (ix == -1) MessageDlg(DrugName + " not available.", mtWarning, mbOK, 0);
            Drugs[ix].DoseInjected = 2.0f * Drugs[ix].Beta1AdR.Potency;
            return ix;
        }

        /// <summary>
        /// Stimulation renal nerves
        /// </summary>
        /// <param name="drug"></param>
        public int StimRenalNerve(Drug drug)
        {
            int ix;

            ix = FindDrug(drug);
            //if (ix == -1) MessageDlg(DrugName + " not available.", mtWarning, mbOK, 0);
            Drugs[ix].DoseInjected = 5.0f * Drugs[ix].Angt1R.Potency;
            return ix;
        }

        /// <summary>
        /// Stimulation vagal nerves
        /// </summary>
        /// <param name="drug"></param>
        public int StimVagusNerve(Drug drug)
        {
            int ix;

            ix = FindDrug(drug);
            //if (ix == -1) MessageDlg(DrugName + " not available.", mtWarning, mbOK, 0);
            Drugs[ix].DoseInjected = 10.0f * Drugs[ix].VagChR.Potency;
            return ix;
        }

        int FindDrug(Drug drug)
        {
            var query = Drugs.Select((o, i) => new { Id = i, Drug = o })
                .Where(o => o.Drug == drug)
                .Select(o => o.Id);

            if (query.Count() == 0)
                return -1;

            return query.Single();
        }
        #endregion

        #region Helper functions
        private static float ReceptorActivation(int iDrug, float Dose, DrugProperties Drug, ref float Numerator, ref float Denominator, float PotencyShift)
        {
            float Result;

            float x;

            if (iDrug == 0)
            {
                Numerator = 0.0f;
                Denominator = 1.0f;
            }

            if (Drug.Efficacy != DrugProperties.None)
            {
                x = Dose / (Drug.Potency * PotencyShift);
                Denominator = Denominator + x;
                Numerator = Numerator + Drug.Efficacy * x;
            }

            if (Denominator > 0.0)
                Result = Numerator / Denominator;
            else
                Result = 0.0f;

            return Result;
        }

        private static void SetAgonistPotency(DrugProperties drug, float potency)
        {
            if (potency != DrugProperties.None)
            {
                drug.Potency = potency * AddNoise(0.5f);
                drug.Efficacy = 1.0f;
            }
            else
                drug.Efficacy = DrugProperties.None;
        }

        private static void SetAntagonistPotency(DrugProperties drug, float potency)
        {
            if (potency != DrugProperties.None)
            {
                drug.Potency = potency * AddNoise(0.5f);
                drug.Efficacy = 0.0f;
            }
            else
                drug.Efficacy = DrugProperties.None;
        }

        /// <summary>
        /// Create a random scaling factor 1 +/- Proportion
        /// </summary>
        /// <param name="proportion"></param>
        /// <returns></returns>
        private static float AddNoise(float proportion)
        {
            var rand = new Random().NextDouble();
            var result = 1.0f + (2.0f * rand - 1.0f) * proportion;

            return (float)result;
        }

        private static float Pressure(Element element)
        {
            float p = (element.Volume - element.Volume0) * element.Elastance;

            if (p < 0.0f)
                p = 0.0f;

            return p;
        }

        ///// <summary>
        ///// Update concentration of drug in circulation
        ///// </summary>
        ///// <param name="drug"></param>
        //private static void UpdateCirculatingDrug(ref Drug drug)
        //{
        //    drug.Dose = drug.Dose + drug.OnRate * (drug.DoseInjected - drug.Dose) - drug.Dose * drug.RemovalRate;
        //    drug.Dose = MaxFlt(drug.Dose, 0.0f);
        //    drug.DoseInjected = MaxFlt(drug.DoseInjected - drug.DoseInjected * drug.RemovalRate, 0.0f);
        //}
        #endregion

        #region Accessor
        public ChannelSpecification<float> GetChannelValues(bool addNoise)
        {
            var cs = new ChannelSpecification<float>();

            var hr = Heart.Rate * MeanHR / 200;

            cs.SetData(
                Art.GetLeadingPressure(),
                Heart.GetLeadingPressure(),
                Ven.GetLeadingPressure(),
                Con.GetLeadingPressure(),
                addNoise ? hr * AddNoise(0.05f) : hr
            );

            return cs;
        }
        #endregion
    }
}
