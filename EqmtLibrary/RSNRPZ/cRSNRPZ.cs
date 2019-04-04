using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstrumentDrivers;

namespace EqmtLibrary.RSNRPZ
{
    public class cRSNRPZ
    {
        private static int testSite;
        
        #region Constructor
        eqmtRSNRPZ NRP_sensors = new eqmtRSNRPZ();
        //Constructor
        public cRSNRPZ(string[] _address)
        {
            testSite = _address.Length;
            // Initialize hardware
            NRP_sensors.Initialize(_address);
            NRP_sensors.Zero_all_channels();
        }

        public void Close()
        {
            NRP_sensors.Close();
        }

        public void setupMeasurement()
        {
            NRP_sensors.SetupMeasurement(1,2,3,4);
        }

        public double[] MeasureAll()
        {
            double[] dbl_measure;
            NRP_sensors.Init_meas_all_channels();
            float[] measured = NRP_sensors.MeasPwr();

            //convert float to double - to be use in myDUT_WS
            dbl_measure = new double[measured.Length];
            for (int i = 0; i < measured.Length; i++)
            {
                dbl_measure[i] = Math.Round(measured[i], 3);       
            }
            return dbl_measure;
        }

        public void setup_measurement(double[] freq, double[] aperture, int[] average_count)
        {
            for (int i = 0; i < testSite; i++)
            {
                NRP_sensors.SetupMeasurement(i + 1, freq[i], aperture[i], average_count[i]);
            }
        }

        public void set_offsets(double[] offsetdB)
        {

            for (int i = 0; i < testSite; i++)
            {
                NRP_sensors.SetOffset(i+1, offsetdB[i]);
            }
        }

        #endregion
    }
}
