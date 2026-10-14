using System;
using toxTree.core;

namespace toxTree.tree
{
    public class ProgressStatus : IProgressStatus//, ISerializable
    {
        private const long serialVersionUID = 5579521841811010110L;

        public enum STATUS
        {
            NOTESTIMATED,
            INPROGRESS,
            COMPLETED,
            UNKNOWN,
            ERROR,
        }

        [NonSerialized]
        public static readonly String _eMethodNotAssigned = "Decision method not assigned!";

        [NonSerialized]
        public static readonly String _eResultNotEstimated = "Decision method has not been applied yet!";

        [NonSerialized]
        public static readonly String _eResultIsEstimating = "Decision method is being applied now ...!";

        [NonSerialized]
        public static readonly String _eUnknownState = "Unknown state !\t";

        [NonSerialized]
        public static readonly String _eError = "ERROR: \t";

        [NonSerialized]
        public static readonly String _pClassID = "ClassID";

        [NonSerialized]
        public static readonly String _pDecisionMethod = "DecisionMethod";

        [NonSerialized]
        public static readonly String _pRuleResult = "RuleResult";

        protected STATUS estimated;

        protected String errMessage = "";

        protected int percent = 0;

        public int getPercentEstimated()
        {
            lock (this)
            {
                return percent;
            }
        }

        public void setPercentEstimated(int percent)
        {
            lock (this)
            {
                this.percent = percent;
            }
        }

        public ProgressStatus()
            : base()
        {
            estimated = STATUS.NOTESTIMATED;
        }

        public void clear()
        {
            estimated = STATUS.NOTESTIMATED;
        }

        public bool isEstimated()
        {
            return (estimated == STATUS.COMPLETED);
        }

        public bool isEstimating()
        {
            return estimated == STATUS.INPROGRESS;
        }

        public bool isError()
        {
            return estimated == STATUS.ERROR;
        }

        public void setEstimated()
        {
            estimated = STATUS.COMPLETED;
        }

        public void setEstimated(bool value)
        {
            if (value)
                estimated = STATUS.COMPLETED;
            else
                estimated = STATUS.NOTESTIMATED;
        }

        public void setEstimating()
        {
            estimated = STATUS.INPROGRESS;
        }

        public void setError(String message)
        {
            estimated = STATUS.ERROR;
            this.errMessage = message;
        }

        public String getErrMessage()
        {
            return errMessage;
        }

        public override String ToString()
        {
            switch (estimated)
            {
                case STATUS.NOTESTIMATED:
                    return _eResultNotEstimated;
                case STATUS.INPROGRESS:
                    return _eResultIsEstimating;
                case STATUS.COMPLETED:
                    return "Completed ";
                case STATUS.ERROR:
                    return "Error\t" + getErrMessage();
                default:
                    return _eUnknownState;
            }
        }
    }
}
