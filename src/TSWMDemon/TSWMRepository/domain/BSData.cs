using System;
using System.Linq;

namespace TSWMRepository.domain
{
    public abstract class BSData
    {
        public static readonly string SENSOR_KISLER = "0E";
        public static readonly string SENSOR_LOOP = "2E";

        public int PackageNumber { get; set; }
        public TimeSpan TimeStampFrom  { get; set; }
        public string DeviceMAC { get; set; }
        public string SensorType { get; }
        public int TimeFromStart { get; set; }
        public int Flag { get; set; }

        public int DataBlockCount { get; set; } // ?? excess
        public int[] DataBlocks { get; set; }

        public int Checksum { get; set; }

        public BSData(string sensorType)
        {
            if (SENSOR_KISLER.Equals(sensorType))
                SensorType = SENSOR_KISLER;
            else if (SENSOR_LOOP.Equals(sensorType))
                SensorType = SENSOR_LOOP;
            else
                throw new NotSupportedException("Sensor not supported: " + sensorType);
        }

        /// <summary>
        /// Checksum validtion impl required
        /// Dependens on Data fields
        /// </summary>
        /// <returns></returns>
        public abstract bool IsValidData();
    }

    public class BSKislerData : BSData
    {
        public int SensorSleepValue { get; set; }
        public int SensorMaxValue { get; set; }
        public int SensorAxesValue { get; set; }
        public int NoiseLevel { get; set; }
        public int Decimation { get; set; }        
        public int ImpulseAmplitude { get; set; }
        public int ImpulseWidth { get; set; }
        public int ImpulseSquare { get; set; }
        public int RoadTemperature { get; set; }
        public int Vibration { get; set; }

        public BSKislerData() : base(BSData.SENSOR_LOOP)
        {
        }

        /// <summary>
        /// Checksum validation
        /// </summary>
        /// <returns>True if fields data sum equals BSData.Checksum</returns>
        public override bool IsValidData()
        {
            return Checksum == PackageNumber + TimeFromStart + (TimeStampFrom.Milliseconds / 10) +
                               Flag + DataBlockCount + DataBlocks.Sum() + Checksum +
                               SensorSleepValue + SensorMaxValue + SensorAxesValue + NoiseLevel + Decimation + 
                               ImpulseAmplitude + ImpulseWidth + ImpulseSquare + RoadTemperature + Vibration;
        }
    }

    public class BSLoopData : BSData
    {
        public int SensorSleepValue { get; set; }
        public int SensorMaxValue { get; set; }
        public int NoiseLevel { get; set; }
        public int DescreteFrequency { get; set; }
        public int LocalMaxCount { get; set; }
        public int LocalMax1Time { get; set; }
        public int LocalMax1Amplitude { get; set; }
        public int LocalMax2Time { get; set; }
        public int LocalMax2Amplitude { get; set; }
        public int LocalMax3Time { get; set; }
        public int LocalMax3Amplitude { get; set; }

        public BSLoopData() : base(BSData.SENSOR_KISLER)
        {
        }

        /// <summary>
        /// Checksum validation
        /// </summary>
        /// <returns>True if fields data sum equals BSData.Checksum</returns>
        public override bool IsValidData()
        {
            return Checksum == PackageNumber + TimeFromStart + (TimeStampFrom.Milliseconds / 10) +
                               Flag + DataBlockCount + DataBlocks.Sum() + Checksum +
                               SensorSleepValue + SensorMaxValue + NoiseLevel + DescreteFrequency +
                               LocalMaxCount + LocalMax1Time + LocalMax1Amplitude + LocalMax2Time +
                               LocalMax2Amplitude + LocalMax3Time + LocalMax3Amplitude;
        }
    }

}
