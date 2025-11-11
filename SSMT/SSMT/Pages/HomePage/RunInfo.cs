using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT.Pages.HomePage
{
    public class RunInfo
    {

        public string RunPath { get; set; } = ""; //启动路径
        public string RunWithArguments { get; set; } = ""; //启动参数
        public string RunLocation { get; set; } = ""; //在哪个程序中启动

        public bool UseShell { get; set; } = true; //是否以Shell方式调用启动

    }

}
