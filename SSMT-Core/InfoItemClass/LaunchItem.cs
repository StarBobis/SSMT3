using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT
{
    public class LaunchItem
    {
        public string LaunchExePath { get; set; } = "";
        public string LaunchArgs { get; set; } = "";

        public LaunchItem(string LaunchExePath,string LaunchArgs)
        {
            this.LaunchExePath = LaunchExePath;
            this.LaunchArgs = LaunchArgs;
        }
    }
}
