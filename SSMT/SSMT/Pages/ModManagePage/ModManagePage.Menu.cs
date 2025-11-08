using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT
{
    public partial class ModManagePage
    {
        private void Menu_OpenModsFolder_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(GlobalConfig.Path_ModsFolder))
            {
                SSMTCommandHelper.ShellOpenFolder(GlobalConfig.Path_ModsFolder);
            }
            else
            {
                _ = SSMTMessageHelper.Show("您当前3Dmigoto目录下并不存在Mods文件夹");
            }
        }


        private void Menu_OpenCategoryRepoFolder_Click(object sender, RoutedEventArgs e)
        {
            MakeSureModRepoExists();
            if (Directory.Exists(GlobalConfig.Path_ModsFolder))
            {
                SSMTCommandHelper.ShellOpenFolder(GlobalConfig.Path_ModsFolder);
            }
        }



    }
}
