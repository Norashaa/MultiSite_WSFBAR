namespace NationalInstruments.SystemsEngineering.Hardware
{
    public abstract class Instrument
    {
        public string IOAddress { get; private set; }

        public Instrument(string IOAddress)
        {
            this.IOAddress = IOAddress;
        }
        
        public abstract void Initialize();
        public abstract void Reset();
        public abstract void Close();

        public override bool Equals(object obj)
        {
            if (obj is Instrument)
                return string.Equals(obj.ToString(), IOAddress);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return IOAddress;
        }
    }
}
