using System;
using System.Runtime.Serialization;
using NCDK;
using System.Text;
using toxTree.core;
using NCDK.Renderers.Selection;
using net.idea.modbcum.i.processors;

namespace toxTree.tree
{
    public class RuleResult// : ISerializable
    {
        private const long serialVersionUID = -7844039700306791740L;

        public static readonly String prefix = "http://localhost";

        public static readonly String ruleURL = $"{prefix}/rule/";

        public static readonly String categoryURL = $"{prefix}/category/";

        public static readonly String alertURL = $"{prefix}?alerts=";

        public static readonly String resultURL = $"{prefix}?parameters=";

        protected IDecisionRule rule = null;

        protected bool result = false;

        protected IDecisionCategory category = null;

        [NonSerialized]
        protected IAtomContainer molecule = null;

        protected bool silent = false;

        protected bool web = false;

        public bool isWeb()
        {
            return web;
        }

        public void setWeb(bool web)
        {
            this.web = web;
        }

        public RuleResult()
            : base()
        {
        }

        public RuleResult(IDecisionRule rule, bool result)
            : base()
        {
            this.rule = rule;
            this.result = result;
        }

        public RuleResult(IDecisionRule rule, bool result, IDecisionCategory category)
            : base()
        {
            this.rule = rule;
            this.result = result;
            this.category = category;
        }

        public bool isResult()
        {
            return result;
        }

        public void setResult(bool result)
        {
            this.result = result;
        }

        public IDecisionCategory getCategory()
        {
            lock (this)
            {
                return category;
            }
        }

        public void setCategory(IDecisionCategory category)
        {
            lock (this)
            {
                this.category = category;
            }
        }

        public IDecisionRule getRule()
        {
            lock (this)
            {
                return rule;
            }
        }

        public void setRule(IDecisionRule rule)
        {
            lock (this)
            {
                this.rule = rule;
            }
        }

        public StringBuilder explain(bool verbose)
        {
            return explain(verbose, -1);
        }

        protected static readonly String wwwAlertFormat = "{0}.{1}";

        public StringBuilder explain(bool verbose, int ruleIndex)
        {
            throw new NotImplementedException();
            //StringBuilder b = new StringBuilder();
            //String space = web ? " " : "&nbsp;";
            //if (!silent)
            //    if (verbose)
            //    {
            //        if (web)
            //        {
            //            if (result && (rule.isImplemented()) && (rule.getSelector() != null))
            //                b.Append(string.Format(wwwAlertFormat, rule.getID(), rule.getTitle()));
            //            else
            //                b.Append(rule.ToString());
            //            b.Append(space);
            //            if (web)
            //                b.Append(result ? "Yes" : "No");
            //            else
            //                b.Append(string.Format("<span style=\'color:%s;position:relative;font-weight: bold;\'>%s</span>", result ? "green" : "red", result ? "Yes" : "No"));
            //            b.Append(space);
            //            if (category != null)
            //            {
            //                if (web)
            //                    b.Append(string.Format("Class %s", category.ToString()));
            //                else
            //                    b.Append(string.Format("Class&nbsp;<span style=\'color:black\'>%s</span>", category.ToString()));
            //            }
            //            else
            //                b.Append(space);
            //        }
            //        else
            //        {
            //            b.Append(string.Format("<a href=\"%s%s\" title=\'Show rule\'><img src=\'%s\' border=\'0\' alt=\'Show rule\' title=\'Show rule\'></a>", ruleURL, rule.getTitle(), this.GetType().getResource("/toxTree/ui/tree/images/find.png").ToString()));
            //            b.Append(space);
            //            if (result && (rule.isImplemented()) && (rule.getSelector() != null))
            //                if (ruleIndex < 0)
            //                    b.Append(string.Format("<a href=\"%s%s\"  title=\'Hilight structure alert\'>%s</a>", alertURL, rule.getTitle(), rule.ToString()));
            //                else
            //                    b.Append(string.Format("<a href=\"%s%d\"  title=\'Hilight structure alert\'>%s</a>", resultURL, ruleIndex, rule.ToString()));
            //            else
            //                b.Append(rule.ToString());
            //            b.Append(space);
            //            if (web)
            //                b.Append(string.Format(result ? "Yes" : "No"));
            //            else
            //            {
            //                b.Append(string.Format("<span style=\'color:%s;position:relative;font-weight: bold;\'>%s</span>", result ? "green" : "red", result ? "Yes" : "No"));
            //                b.Append(space);
            //            }
            //            if (category != null)
            //            {
            //                if (web)
            //                    b.Append(string.Format(category.ToString()));
            //                else
            //                    b.Append(string.Format("Class&nbsp;<span style=\'color:orange\'><a href=\"%s%s\">%s</a></span>", categoryURL, category.getID(), category.ToString()));
            //            }
            //            else
            //                b.Append(space);
            //            if ((molecule != null) && (molecule.Id != null))
            //            {
            //                b.Append("\t");
            //                b.Append(molecule.Id);
            //            }
            //            else
            //                b.Append("\t");
            //        }
            //    }
            //    else
            //    {
            //        b.Append(rule.getID());
            //        if (result)
            //            b.Append("Y");
            //        else
            //            b.Append("N");
            //    }
            //return b;
        }

        public IAtomContainer getMolecule()
        {
            lock (this)
            {
                return molecule;
            }
        }

        public void setMolecule(IAtomContainer molecule)
        {
            lock (this)
            {
                this.molecule = molecule;
            }
        }

        public bool isSilent()
        {
            lock (this)
            {
                return silent;
            }
        }

        public void setSilent(bool silent)
        {
            lock (this)
            {
                this.silent = silent;
            }
        }

        public override bool Equals(Object obj)
        {
            try
            {
                RuleResult r = (RuleResult)obj;
                return (result == r.result) && (rule.Equals(r.rule)) && (((category == null) && (r.category == null)) || category.Equals(r.category));
            }
            catch (Exception x)
            {
                return false;
            }
        }

        public override String ToString()
        {
            return explain(true).ToString();
        }

        public IProcessor<IAtomContainer, IChemObjectSelection> getSelector()
        {
            return getRule().getSelector();
        }
    }
}
