using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT_Core.InfoClass
{

    /// <summary>
    /// 启动一个程序需要的大部分参数
    /// </summary>
    public class RunInfo
    {

        public string RunPath { get; set; } = ""; //启动路径
        public string RunWithArguments { get; set; } = ""; //启动参数
        public string RunLocation { get; set; } = ""; //在哪个程序中启动

        public bool UseShell { get; set; } = true; //是否以Shell方式调用启动

        public string Verb { get; set; } = "runas";

    }

}
