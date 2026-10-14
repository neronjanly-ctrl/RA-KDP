using RatCvsSimulation.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatCvsSimulation.Core
{
    public class MainPageViewModel
    {
        public State State { get; set; }
        public Document Doc = new Document();
    }
}
