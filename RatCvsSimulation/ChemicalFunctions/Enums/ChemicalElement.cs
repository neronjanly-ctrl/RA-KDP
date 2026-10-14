using CommonTools;

namespace ChemicalFunctions
{
    /// <summary>
    /// https://en.wikipedia.org/wiki/List_of_chemical_elements
    /// </summary>
    public enum ChemicalElement
    {
        [Atom(1, "H", "Hydrogen", 1.008)]
        [Match("H", "Hydrogen", CaseSensitive = false)]
        Hydrogen,

        [Atom(2, "He", "Helium", 4.002602)]
        [Match("He", "Helium", CaseSensitive = false)]
        Helium,

        [Atom(3, "Li", "Lithium", 6.94)]
        [Match("Li", "Lithium", CaseSensitive = false)]
        Lithium,

        [Atom(4, "Be", "Beryllium", 9.0121831)]
        [Match("Be", "Beryllium", CaseSensitive = false)]
        Beryllium,

        [Atom(5, "B", "Boron", 10.81)]
        [Match("B", "Boron", CaseSensitive = false)]
        Boron,

        [Atom(6, "C", "Carbon", 12.011)]
        [Match("C", "Carbon", CaseSensitive = false)]
        Carbon,

        [Atom(7, "N", "Nitrogen", 14.007)]
        [Match("N", "Nitrogen", CaseSensitive = false)]
        Nitrogen,

        [Atom(8, "O", "Oxygen", 15.999)]
        [Match("O", "Oxygen", CaseSensitive = false)]
        Oxygen,

        [Atom(9, "F", "Fluorine", 18.99840316)]
        [Match("F", "Fluorine", CaseSensitive = false)]
        Fluorine,

        [Atom(10, "Ne", "Neon", 20.1797)]
        [Match("Ne", "Neon", CaseSensitive = false)]
        Neon,

        [Atom(11, "Na", "Sodium", 22.98976928)]
        [Match("Na", "Sodium", CaseSensitive = false)]
        Sodium,

        [Atom(12, "Mg", "Magnesium", 24.305)]
        [Match("Mg", "Magnesium", CaseSensitive = false)]
        Magnesium,

        [Atom(13, "Al", "Aluminium", 26.9815385)]
        [Match("Al", "Aluminium", CaseSensitive = false)]
        Aluminium,

        [Atom(14, "Si", "Silicon", 28.085)]
        [Match("Si", "Silicon", CaseSensitive = false)]
        Silicon,

        [Atom(15, "P", "Phosphorus", 30.973762)]
        [Match("P", "Phosphorus", CaseSensitive = false)]
        Phosphorus,

        [Atom(16, "S", "Sulfur", 32.06)]
        [Match("S", "Sulfur", CaseSensitive = false)]
        Sulfur,

        [Atom(17, "Cl", "Chlorine", 35.45)]
        [Match("Cl", "Chlorine", CaseSensitive = false)]
        Chlorine,

        [Atom(18, "Ar", "Argon", 39.948)]
        [Match("Ar", "Argon", CaseSensitive = false)]
        Argon,

        [Atom(19, "K", "Potassium", 39.0983)]
        [Match("K", "Potassium", CaseSensitive = false)]
        Potassium,

        [Atom(20, "Ca", "Calcium", 40.078)]
        [Match("Ca", "Calcium", CaseSensitive = false)]
        Calcium,

        [Atom(21, "Sc", "Scandium", 44.955908)]
        [Match("Sc", "Scandium", CaseSensitive = false)]
        Scandium,

        [Atom(22, "Ti", "Titanium", 47.867)]
        [Match("Ti", "Titanium", CaseSensitive = false)]
        Titanium,

        [Atom(23, "V", "Vanadium", 50.9415)]
        [Match("V", "Vanadium", CaseSensitive = false)]
        Vanadium,

        [Atom(24, "Cr", "Chromium", 51.9961)]
        [Match("Cr", "Chromium", CaseSensitive = false)]
        Chromium,

        [Atom(25, "Mn", "Manganese", 54.938044)]
        [Match("Mn", "Manganese", CaseSensitive = false)]
        Manganese,

        [Atom(26, "Fe", "Iron", 55.845)]
        [Match("Fe", "Iron", CaseSensitive = false)]
        Iron,

        [Atom(27, "Co", "Cobalt", 58.933194)]
        [Match("Co", "Cobalt", CaseSensitive = false)]
        Cobalt,

        [Atom(28, "Ni", "Nickel", 58.6934)]
        [Match("Ni", "Nickel", CaseSensitive = false)]
        Nickel,

        [Atom(29, "Cu", "Copper", 63.546)]
        [Match("Cu", "Copper", CaseSensitive = false)]
        Copper,

        [Atom(30, "Zn", "Zinc", 65.38)]
        [Match("Zn", "Zinc", CaseSensitive = false)]
        Zinc,

        [Atom(31, "Ga", "Gallium", 69.723)]
        [Match("Ga", "Gallium", CaseSensitive = false)]
        Gallium,

        [Atom(32, "Ge", "Germanium", 72.63)]
        [Match("Ge", "Germanium", CaseSensitive = false)]
        Germanium,

        [Atom(33, "As", "Arsenic", 74.921595)]
        [Match("As", "Arsenic", CaseSensitive = false)]
        Arsenic,

        [Atom(34, "Se", "Selenium", 78.971)]
        [Match("Se", "Selenium", CaseSensitive = false)]
        Selenium,

        [Atom(35, "Br", "Bromine", 79.904)]
        [Match("Br", "Bromine", CaseSensitive = false)]
        Bromine,

        [Atom(36, "Kr", "Krypton", 83.798)]
        [Match("Kr", "Krypton", CaseSensitive = false)]
        Krypton,

        [Atom(37, "Rb", "Rubidium", 85.4678)]
        [Match("Rb", "Rubidium", CaseSensitive = false)]
        Rubidium,

        [Atom(38, "Sr", "Strontium", 87.62)]
        [Match("Sr", "Strontium", CaseSensitive = false)]
        Strontium,

        [Atom(39, "Y", "Yttrium", 88.90584)]
        [Match("Y", "Yttrium", CaseSensitive = false)]
        Yttrium,

        [Atom(40, "Zr", "Zirconium", 91.224)]
        [Match("Zr", "Zirconium", CaseSensitive = false)]
        Zirconium,

        [Atom(41, "Nb", "Niobium", 92.90637)]
        [Match("Nb", "Niobium", CaseSensitive = false)]
        Niobium,

        [Atom(42, "Mo", "Molybdenum", 95.95)]
        [Match("Mo", "Molybdenum", CaseSensitive = false)]
        Molybdenum,

        [Atom(43, "Tc", "Technetium", 98)]
        [Match("Tc", "Technetium", CaseSensitive = false)]
        Technetium,

        [Atom(44, "Ru", "Ruthenium", 101.07)]
        [Match("Ru", "Ruthenium", CaseSensitive = false)]
        Ruthenium,

        [Atom(45, "Rh", "Rhodium", 102.9055)]
        [Match("Rh", "Rhodium", CaseSensitive = false)]
        Rhodium,

        [Atom(46, "Pd", "Palladium", 106.42)]
        [Match("Pd", "Palladium", CaseSensitive = false)]
        Palladium,

        [Atom(47, "Ag", "Silver", 107.8682)]
        [Match("Ag", "Silver", CaseSensitive = false)]
        Silver,

        [Atom(48, "Cd", "Cadmium", 112.414)]
        [Match("Cd", "Cadmium", CaseSensitive = false)]
        Cadmium,

        [Atom(49, "In", "Indium", 114.818)]
        [Match("In", "Indium", CaseSensitive = false)]
        Indium,

        [Atom(50, "Sn", "Tin", 118.71)]
        [Match("Sn", "Tin", CaseSensitive = false)]
        Tin,

        [Atom(51, "Sb", "Antimony", 121.76)]
        [Match("Sb", "Antimony", CaseSensitive = false)]
        Antimony,

        [Atom(52, "Te", "Tellurium", 127.6)]
        [Match("Te", "Tellurium", CaseSensitive = false)]
        Tellurium,

        [Atom(53, "I", "Iodine", 126.90447)]
        [Match("I", "Iodine", CaseSensitive = false)]
        Iodine,

        [Atom(54, "Xe", "Xenon", 131.293)]
        [Match("Xe", "Xenon", CaseSensitive = false)]
        Xenon,

        [Atom(55, "Cs", "Caesium", 132.905452)]
        [Match("Cs", "Caesium", CaseSensitive = false)]
        Caesium,

        [Atom(56, "Ba", "Barium", 137.327)]
        [Match("Ba", "Barium", CaseSensitive = false)]
        Barium,

        [Atom(57, "La", "Lanthanum", 138.90547)]
        [Match("La", "Lanthanum", CaseSensitive = false)]
        Lanthanum,

        [Atom(58, "Ce", "Cerium", 140.116)]
        [Match("Ce", "Cerium", CaseSensitive = false)]
        Cerium,

        [Atom(59, "Pr", "Praseodymium", 140.90766)]
        [Match("Pr", "Praseodymium", CaseSensitive = false)]
        Praseodymium,

        [Atom(60, "Nd", "Neodymium", 144.242)]
        [Match("Nd", "Neodymium", CaseSensitive = false)]
        Neodymium,

        [Atom(61, "Pm", "Promethium", 145)]
        [Match("Pm", "Promethium", CaseSensitive = false)]
        Promethium,

        [Atom(62, "Sm", "Samarium", 150.36)]
        [Match("Sm", "Samarium", CaseSensitive = false)]
        Samarium,

        [Atom(63, "Eu", "Europium", 151.964)]
        [Match("Eu", "Europium", CaseSensitive = false)]
        Europium,

        [Atom(64, "Gd", "Gadolinium", 157.25)]
        [Match("Gd", "Gadolinium", CaseSensitive = false)]
        Gadolinium,

        [Atom(65, "Tb", "Terbium", 158.92535)]
        [Match("Tb", "Terbium", CaseSensitive = false)]
        Terbium,

        [Atom(66, "Dy", "Dysprosium", 162.5)]
        [Match("Dy", "Dysprosium", CaseSensitive = false)]
        Dysprosium,

        [Atom(67, "Ho", "Holmium", 164.93033)]
        [Match("Ho", "Holmium", CaseSensitive = false)]
        Holmium,

        [Atom(68, "Er", "Erbium", 167.259)]
        [Match("Er", "Erbium", CaseSensitive = false)]
        Erbium,

        [Atom(69, "Tm", "Thulium", 168.93422)]
        [Match("Tm", "Thulium", CaseSensitive = false)]
        Thulium,

        [Atom(70, "Yb", "Ytterbium", 173.045)]
        [Match("Yb", "Ytterbium", CaseSensitive = false)]
        Ytterbium,

        [Atom(71, "Lu", "Lutetium", 174.9668)]
        [Match("Lu", "Lutetium", CaseSensitive = false)]
        Lutetium,

        [Atom(72, "Hf", "Hafnium", 178.49)]
        [Match("Hf", "Hafnium", CaseSensitive = false)]
        Hafnium,

        [Atom(73, "Ta", "Tantalum", 180.94788)]
        [Match("Ta", "Tantalum", CaseSensitive = false)]
        Tantalum,

        [Atom(74, "W", "Tungsten", 183.84)]
        [Match("W", "Tungsten", CaseSensitive = false)]
        Tungsten,

        [Atom(75, "Re", "Rhenium", 186.207)]
        [Match("Re", "Rhenium", CaseSensitive = false)]
        Rhenium,

        [Atom(76, "Os", "Osmium", 190.23)]
        [Match("Os", "Osmium", CaseSensitive = false)]
        Osmium,

        [Atom(77, "Ir", "Iridium", 192.217)]
        [Match("Ir", "Iridium", CaseSensitive = false)]
        Iridium,

        [Atom(78, "Pt", "Platinum", 195.084)]
        [Match("Pt", "Platinum", CaseSensitive = false)]
        Platinum,

        [Atom(79, "Au", "Gold", 196.966569)]
        [Match("Au", "Gold", CaseSensitive = false)]
        Gold,

        [Atom(80, "Hg", "Mercury", 200.592)]
        [Match("Hg", "Mercury", CaseSensitive = false)]
        Mercury,

        [Atom(81, "Tl", "Thallium", 204.38)]
        [Match("Tl", "Thallium", CaseSensitive = false)]
        Thallium,

        [Atom(82, "Pb", "Lead", 207.2)]
        [Match("Pb", "Lead", CaseSensitive = false)]
        Lead,

        [Atom(83, "Bi", "Bismuth", 208.9804)]
        [Match("Bi", "Bismuth", CaseSensitive = false)]
        Bismuth,

        [Atom(84, "Po", "Polonium", 209)]
        [Match("Po", "Polonium", CaseSensitive = false)]
        Polonium,

        [Atom(85, "At", "Astatine", 210)]
        [Match("At", "Astatine", CaseSensitive = false)]
        Astatine,

        [Atom(86, "Rn", "Radon", 222)]
        [Match("Rn", "Radon", CaseSensitive = false)]
        Radon,

        [Atom(87, "Fr", "Francium", 223)]
        [Match("Fr", "Francium", CaseSensitive = false)]
        Francium,

        [Atom(88, "Ra", "Radium", 226)]
        [Match("Ra", "Radium", CaseSensitive = false)]
        Radium,

        [Atom(89, "Ac", "Actinium", 227)]
        [Match("Ac", "Actinium", CaseSensitive = false)]
        Actinium,

        [Atom(90, "Th", "Thorium", 232.0377)]
        [Match("Th", "Thorium", CaseSensitive = false)]
        Thorium,

        [Atom(91, "Pa", "Protactinium", 231.03588)]
        [Match("Pa", "Protactinium", CaseSensitive = false)]
        Protactinium,

        [Atom(92, "U", "Uranium", 238.02891)]
        [Match("U", "Uranium", CaseSensitive = false)]
        Uranium,

        [Atom(93, "Np", "Neptunium", 237)]
        [Match("Np", "Neptunium", CaseSensitive = false)]
        Neptunium,

        [Atom(94, "Pu", "Plutonium", 244)]
        [Match("Pu", "Plutonium", CaseSensitive = false)]
        Plutonium,

        [Atom(95, "Am", "Americium", 243)]
        [Match("Am", "Americium", CaseSensitive = false)]
        Americium,

        [Atom(96, "Cm", "Curium", 247)]
        [Match("Cm", "Curium", CaseSensitive = false)]
        Curium,

        [Atom(97, "Bk", "Berkelium", 247)]
        [Match("Bk", "Berkelium", CaseSensitive = false)]
        Berkelium,

        [Atom(98, "Cf", "Californium", 251)]
        [Match("Cf", "Californium", CaseSensitive = false)]
        Californium,

        [Atom(99, "Es", "Einsteinium", 252)]
        [Match("Es", "Einsteinium", CaseSensitive = false)]
        Einsteinium,

        [Atom(100, "Fm", "Fermium", 257)]
        [Match("Fm", "Fermium", CaseSensitive = false)]
        Fermium,

        [Atom(101, "Md", "Mendelevium", 258)]
        [Match("Md", "Mendelevium", CaseSensitive = false)]
        Mendelevium,

        [Atom(102, "No", "Nobelium", 259)]
        [Match("No", "Nobelium", CaseSensitive = false)]
        Nobelium,

        [Atom(103, "Lr", "Lawrencium", 266)]
        [Match("Lr", "Lawrencium", CaseSensitive = false)]
        Lawrencium,

        [Atom(104, "Rf", "Rutherfordium", 267)]
        [Match("Rf", "Rutherfordium", CaseSensitive = false)]
        Rutherfordium,

        [Atom(105, "Db", "Dubnium", 268)]
        [Match("Db", "Dubnium", CaseSensitive = false)]
        Dubnium,

        [Atom(106, "Sg", "Seaborgium", 269)]
        [Match("Sg", "Seaborgium", CaseSensitive = false)]
        Seaborgium,

        [Atom(107, "Bh", "Bohrium", 270)]
        [Match("Bh", "Bohrium", CaseSensitive = false)]
        Bohrium,

        [Atom(108, "Hs", "Hassium", 277)]
        [Match("Hs", "Hassium", CaseSensitive = false)]
        Hassium,

        [Atom(109, "Mt", "Meitnerium", 278)]
        [Match("Mt", "Meitnerium", CaseSensitive = false)]
        Meitnerium,

        [Atom(110, "Ds", "Darmstadtium", 281)]
        [Match("Ds", "Darmstadtium", CaseSensitive = false)]
        Darmstadtium,

        [Atom(111, "Rg", "Roentgenium", 282)]
        [Match("Rg", "Roentgenium", CaseSensitive = false)]
        Roentgenium,

        [Atom(112, "Cn", "Copernicium", 285)]
        [Match("Cn", "Copernicium", CaseSensitive = false)]
        Copernicium,

        [Atom(113, "Nh", "Nihonium", 286)]
        [Match("Nh", "Nihonium", CaseSensitive = false)]
        Nihonium,

        [Atom(114, "Fl", "Flerovium", 289)]
        [Match("Fl", "Flerovium", CaseSensitive = false)]
        Flerovium,

        [Atom(115, "Mc", "Moscovium", 290)]
        [Match("Mc", "Moscovium", CaseSensitive = false)]
        Moscovium,

        [Atom(116, "Lv", "Livermorium", 293)]
        [Match("Lv", "Livermorium", CaseSensitive = false)]
        Livermorium,

        [Atom(117, "Ts", "Tennessine", 294)]
        [Match("Ts", "Tennessine", CaseSensitive = false)]
        Tennessine,

        [Atom(118, "Og", "Oganesson", 294)]
        [Match("Og", "Oganesson", CaseSensitive = false)]
        Oganesson,
    }
}
