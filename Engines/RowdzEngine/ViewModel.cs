using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexLight.Engine.Rowdz
{
    public struct Range<T>
    {
        public T Min;
        public T Max;
    }

    public class ViewModel
    {
        public const double SensitivityMin = 0.0;
        public const double SensitivityMax = 1.0;

        public const double DecayRateMin = 0.0;
        public const double DecayRateMax = 1.0;

        public double Sensitivity { get; set; }
        public double DecayRate { get; set; }

        public ViewModel()
        {
            Sensitivity = 1.0;
            DecayRate = 0.1;
        }
    }
}
