namespace Broadcom.Tests
{
    public interface ITest
    {
        void Initialize();
        void Configure();
        void Initiate();
        void WaitUntilAcquisitionComplete();
        void WaitUntilMeasurementComplete();
        void Abort();
    }
}
