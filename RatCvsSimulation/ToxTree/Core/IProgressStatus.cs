using System;

namespace toxTree.core
{
    public interface IProgressStatus
    {
        bool isEstimated();

        bool isEstimating();

        bool isError();

        void setEstimated(bool value);

        void setPercentEstimated(int percent);

        void setEstimated();

        void setEstimating();

        void setError(String message);
    }
}
