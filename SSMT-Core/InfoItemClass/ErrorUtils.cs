using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSMT_Core;

namespace SSMT
{
    public class ErrorUtils
    {
        public static string CantFindDataType { get; set; } = "";


        public static void ShowCantFindDataTypeError(string DrawIB)
        {
            LOG.Error("无法找到当前提取IndexBuffer Hash值:   " + DrawIB + "   对应Buffer数据的数据类型\n" + 
                "可能的解决方案如下，请先自己排查一下:" + 
                "1.结合运行日志信息，到数据类型管理页面添加此数据类型\n" + 
                "2.联系NicoMico，发送给他Dump下来的FrameAnalysis文件夹和提取使用的DrawIB来添加此数据类型支持并更新SSMT版本\n" + 
                "3.可能游戏中未关闭[角色动态高精度]图形设置项，关闭后重新F8 Dump并提取测试\n" + 
                "4.可能当前提取逻辑与当前游戏渲染逻辑并未适配，检查提取逻辑是否设置正确\n" + 
                "5.可能提取的是打了Mod的模型，SSMT并不支持提取Mod的模型，请使用SSMT的一键逆向插件来提取Mod中的模型。\n" + 
                "6.可能是提取用的Hash是VB(Vertex Buffer Hash)而不是IB(Index Buffer Hash)，IB是通过小键盘7和8切换，小键盘9复制得到的，IB是SSMT的通用用法，输入VB一般是使用过类似GIMI这种古董脚本按照经验在SSMT中使用VB导致的，SSMT只使用IB。\n" + 
                "如果实在无法解决，请联系开发者NicoMico获取技术支持。");
        }
    }
}
