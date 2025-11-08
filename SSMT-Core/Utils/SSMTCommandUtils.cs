using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT_Core.Utils
{
    public class SSMTCommandUtils
    {


        public static async void ConvertTexture(string SourceTextureFilePath, string TextureFormatString, string TargetOutputDirectory)
        {
            SourceTextureFilePath = SourceTextureFilePath.Replace("\\", "/");
            TargetOutputDirectory = TargetOutputDirectory.Replace("\\", "/");

            string channels = " -f rgba ";
            if (TextureFormatString == "jpg")
            {

                if (!SourceTextureFilePath.Contains("BC5_UNORM"))
                {
                    channels = " ";
                }
            }


            string arugmentsstr = " \"" + SourceTextureFilePath + "\" -ft \"" + TextureFormatString + "\" " + channels + " -o \"" + TargetOutputDirectory + "\"";
            string texconv_filepath = PathManager.Path_TexconvExe;
            if (!File.Exists(texconv_filepath))
            {
                throw new Exception("Current execute path didn't exsits: " + texconv_filepath);
            }

            //https://github.com/microsoft/DirectXTex/wiki/Texconv
            Process process = new Process();
            process.StartInfo.FileName = texconv_filepath;
            process.StartInfo.Arguments = arugmentsstr;
            process.StartInfo.UseShellExecute = false;  // 不使用操作系统的shell启动程序
            process.StartInfo.RedirectStandardOutput = true;  // 重定向标准输出
            process.StartInfo.RedirectStandardError = true;   // 重定向标准错误输出
            process.StartInfo.CreateNoWindow = true;  // 不创建新窗口
            process.Start();
            process.WaitForExit();
        }

        public static void ShellOpenFolder(string FolderPath)
        {

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = FolderPath,
                UseShellExecute = true, // 允许操作系统决定如何打开文件夹
                WorkingDirectory = FolderPath // 设置工作路径为要打开的文件夹路径
            };

            Process.Start(startInfo);

        }

    }
}
