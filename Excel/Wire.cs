using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Wiring
{
    public class Wire
    {
        public string NameOfCabinet { get; set; } = "";
        public string Number { get; set; } = "";
        public string DtSource { get; set; } = "";
        public string WireEndTerminationSource { get; set; } = "";
        public string DtTarget { get; set; } = "";
        public string WireEndDimensionSource { get; set; } = "";
        public string WireEndDimensionTarget { get; set; } = "";
        public string WireEndTerminationTarget { get; set; } = "";
        public string Colour { get; set; } = "";
        public double CrossSection { get; set; } = 0.0;
        public string Type { get; set; } = "";
        public double Lenght { get; set; } = 0.0;
        public double? Progress { get; set; } = 0;
        public DateTime Start { get; set; } = DateTime.Now;
        public DateTime DateOfFinish { get; set; } = DateTime.Now;
        public string? MadeBy { get; set; } = "";

        public bool IsConfirmed { get; set; } = false;
        public int? WireStatus { get; set; } = 0;
        public double Seconds { get; set; } = 0;
        public string? Addnotations { get; set; }
        public double TimeForExecuting { get; set; }
        public bool Overtime { get; set; }
        public double SecondsSource { get; set; }
        public double SecondsDestination { get; set; }
        public string? ReasonDT { get; set; }
        public double HandlingTime { get; set; }
        public bool Skipped { get; set; }
        public override string ToString()
        {
            return this.Number + ", " + this.DtSource + "";
        }


    }
}
