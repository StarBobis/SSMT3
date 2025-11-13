using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT_Core.InfoItemClass
{
    public class BackgroundSuffixItem
    {
        /// <summary>
        /// 背景文件名
        /// </summary>
        public string Suffix { get; set; }
        public bool IsVideo { get; set; } = false;
        public bool IsPicture { get; set; } = false;

    }
}
