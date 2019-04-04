using System;
using System.Windows.Forms;
using System.Threading;

namespace InstrumentDrivers
{
    public class eqmtRSNRPZ
    {
        private rsnrpz myRSnrp { get; set; }
        //public eqmtRSNRPZ(string IOAddress);

        private static double previousMeasLength = 0;
        private static int previousNumAvgs = 0;
        private static bool isInitialized = false;
        private static int number_of_channels = 0;
        /// <summary>
        /// Initializes Power Sensor
        /// Using channels in the NRP driver, different power sensors are treated as channels
        /// Channels are initialized with unique IOAddress identifiers
        /// Different settings for each channel is possible as they work independently
       
        public  void Initialize(string[] IOAddress)
        {
            int chan = 0;
            number_of_channels = IOAddress.Length;

            if (myRSnrp == null)
            {
                try
                {
                    myRSnrp = new rsnrpz();
                    for (int i = 0; i < IOAddress.Length; i++)
                    {
                        chan = i + 1;
                        if(i==0)
                        { myRSnrp.Init(IOAddress[0], true, true); }
                        else
                        {
                            myRSnrp.AddSensor(chan, IOAddress[i], true, true);
                        }
                        //add sensor by assigning channels
                       

                        //Set to cont average measurement and immediate trigger
                        myRSnrp.chan_mode(chan, InstrumentDrivers.rsnrpzConstants.SensorModeContav);
                        myRSnrp.trigger_setSource(chan, InstrumentDrivers.rsnrpzConstants.TriggerSourceImmediate);
                        myRSnrp.chan_setInitContinuousEnabled(chan, false);
                        
                        //enable offsets
                        myRSnrp.corr_setOffsetEnabled(chan, true);

                        //set auto average off
                        myRSnrp.avg_setAutoEnabled(chan, false);

                        
                        //Disable error checking for higher throughput. Enabled by default
                        myRSnrp.errorCheckState(true);
                        
                        
                    }
                    isInitialized = true;

                    
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }


        #region PowerSensor Local Members
        /// <summary>
        /// Resets NRP power sensor
        /// </summary>
        public void Reset()
        {
            myRSnrp.reset();
        }

        /// <summary>
        /// Close the driver session
        /// </summary>
        public void Close()
        {

            myRSnrp.Dispose();

        }
        
        //Set Frequency for individual channels
        public void SetFreq(int chNo, double freqMHz)
        {
            //set frequency
            myRSnrp.chan_setCorrectionFrequency(chNo, freqMHz*1e6);

        }

        //channel list frequency setting
        public void SetFreq(double[] freqlist)
        {
            try
            {
                int chan = 0;
                
                    for (int i = 0; i < freqlist.Length; i++)
                    {
                        chan = i + 1;
                        SetFreq(chan, freqlist[i]);
                    }
               
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            
        }
        public void SetOffset(int chNo, double offset)
        {
            try
            {
                
                myRSnrp.corr_setOffset(chNo, offset);
                
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void SetOffset(double[] offset)
        {
            try
            {
                int chan = 0;
               

                    for (int i = 0; i < offset.Length; i++)
                    {
                        chan = i + 1;
                        SetOffset(chan,offset[i]);
                       
                    }
                
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            

        }

        public void EnableOffset(int chNo, bool status)
        {
            try
            {
                myRSnrp.corr_setOffsetEnabled(chNo, status);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void EnableOffset(bool[] status)
        {
            int chan = 0;
            try
            {
                for (int i = 0; i < status.Length; i++)
                {
                    chan = i + 1;
                    EnableOffset(chan, status[i]);

                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void Init_meas_all_channels()
        {
            bool complete = false;
            
            //init all channels
            myRSnrp.chans_initiate();

            //check for measurement complete
            do
            {
                myRSnrp.chans_isMeasurementComplete(out complete);
                //Thread.Sleep(1);

            } while (!complete);
        }

        public float[] MeasPwr()
        {
            //initialize results array
            float[] results=new float[number_of_channels];
            int chan = 0;
            for (int i = 0;  i < number_of_channels; i++)
            {
                    chan = i + 1;
                    results[i] = MeasPwr(chan);
                
            }

            return results;

        }
        public float MeasPwr(int chNo)
        {
            
            float measValDbm = -2000;
            double measval = -2000;

            try
            {
                
                myRSnrp.meass_fetchMeasurement(chNo, out measval);
                measValDbm = 10f * (float)Math.Log10(1000.0 * Math.Abs(measval));
            }
            catch (Exception e)
            {
                myRSnrp.chan_abort(chNo);
                
            }
            
            if (float.IsNaN(measValDbm) || (measValDbm < -100 || measValDbm > 100))    // need this in case of NAN or -inifinity
            {
                measValDbm = -999;
            }

            return measValDbm;
        }

        public void Zeroing(int chNo)
        {
            bool z_complete = false;
            myRSnrp.chan_zero(chNo);

            //check for completion
            do
            {
                myRSnrp.chan_isZeroComplete(chNo, out z_complete);
                Thread.Sleep(1);
            } while (!z_complete);

        }

        public void Zero_all_channels()
        {
            bool z_complete = false;

            //zero all channel
            myRSnrp.chans_zero();

            //check for completion
            do
            {
                myRSnrp.chans_isZeroingComplete(out z_complete);
                Thread.Sleep(1);
            } while (!z_complete);
            


        }

        public void SetupMeasurement(int ChNo, double measureFreqMHz, double aperture, int numAvgs)
        {
            try
            {
                //using continuous average mode
                
                if (measureFreqMHz > 8000) myRSnrp.chan_setCorrectionFrequency(1, 8000 * 1e6);
                else myRSnrp.chan_setCorrectionFrequency(ChNo, measureFreqMHz * 1e6); // Set corr frequency
                myRSnrp.chan_setContAvAperture(ChNo, aperture);
                myRSnrp.avg_setCount(ChNo, numAvgs);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void SetupMeasurement(double[] measureFreqMHz, double[] aperture, int[] numAvgs)
        {
            try
            {
                int chan = 0;
                for (int i = 0; i < number_of_channels; i++)
                {
                    chan = i + 1;
                    SetupMeasurement(chan, measureFreqMHz[i],aperture[i],numAvgs[i]);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }


        public void check_setup()
        {

            for (int i = 0; i < number_of_channels; i++)
            {
                int chan = i + 1;
                int count = 0;
                double aper = 0;
                myRSnrp.avg_getCount(chan, out count);
                myRSnrp.chan_getContAvAperture(chan, out aper);
                MessageBox.Show("Average Count sensor " + chan.ToString() + " is " + count.ToString()+" Aperture is "+aper.ToString());
            }

        }
        public void SetupBurstMeasurement(double measureFreqMHz, double measLengthS, double triggerLevDbm, int numAvgs)
        {
            try
            {
                myRSnrp.chan_mode(1, InstrumentDrivers.rsnrpzConstants.SensorModeTimeslot);
                myRSnrp.chan_setCorrectionFrequency(1, measureFreqMHz * 1e6); // Set corr frequency
                myRSnrp.trigger_setSource(1, InstrumentDrivers.rsnrpzConstants.TriggerSourceInternal);
                SetupMeasLength(measLengthS);
                double trigLev = Math.Pow(10.0, triggerLevDbm / 10.0) / 1000.0;
                myRSnrp.trigger_setLevel(1, trigLev);
                SetupNumAverages(numAvgs);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        public void SetupMeasLength(double measLengthS)
        {
            try
            {
                if (true | measLengthS != previousMeasLength)
                {
                    myRSnrp.tslot_configureTimeSlot(1, 1, measLengthS);
                    previousMeasLength = measLengthS;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void SetupNumAverages(int numAvgs)
        {
            try
            {

                myRSnrp.avg_configureAvgManual(1, numAvgs);

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void SetupNumAverages(int chNo, int numAvgs)
        {
            try
            {
                
                    myRSnrp.avg_configureAvgManual(chNo, numAvgs);
                 
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        #endregion


    }
}
