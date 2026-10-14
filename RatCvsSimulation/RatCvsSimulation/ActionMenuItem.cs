using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace RatCvsSimulation
{
    /// <summary>
    /// Data to represent an item in the nav menu.
    /// </summary>
    public class ActionMenuItem
    {
        public string Label { get; set; }
        public Symbol Symbol { set { _symbolchar = (char)value; } }
        char _symbolchar;
        public char SymbolAsChar
        {
            get { return _symbolchar; }
            set { _symbolchar = value; }
        }

        public Action<object> Command { get; set; }
        public object Arguments { get; set; }
    }
}
