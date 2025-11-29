using Microsoft.UI.Xaml;
using SSMT_Core;
using SSMT_Core.InfoItemClass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT
{
    public partial class HomePage
    {

        /// <summary>
        /// 逻辑名称是固定的，需要手动管理
        /// </summary>
        private void InitializeLogicNameList()
        {
            IsLoading = true;

            ComboBox_LogicName.Items.Clear();

            ComboBox_LogicName.Items.Add(LogicName.GIMI);
            ComboBox_LogicName.Items.Add(LogicName.HIMI);
            ComboBox_LogicName.Items.Add(LogicName.SRMI);
            ComboBox_LogicName.Items.Add(LogicName.ZZMI);
            ComboBox_LogicName.Items.Add(LogicName.WWMI);
            ComboBox_LogicName.Items.Add(LogicName.UnityCPU);
            ComboBox_LogicName.Items.Add(LogicName.CTXMC);
            ComboBox_LogicName.Items.Add(LogicName.IdentityV2);
            ComboBox_LogicName.Items.Add(LogicName.YYSLS);
            ComboBox_LogicName.Items.Add(LogicName.AILIMIT);
            ComboBox_LogicName.Items.Add(LogicName.UnityVS);
            ComboBox_LogicName.Items.Add(LogicName.UnityCS);
            ComboBox_LogicName.Items.Add(LogicName.SnowBreak);
            ComboBox_LogicName.Items.Add(LogicName.HOK);
            ComboBox_LogicName.Items.Add(LogicName.NierR);

            IsLoading = false;
        }

        /// <summary>
        /// 读取数据类型文件夹下的所有游戏名称的文件夹名称，然后放到下拉菜单中供后续选择
        /// </summary>
        private void InitializeGameTypeFolderList()
        {
            IsLoading = true;

            ComboBox_GameTypeFolder.Items.Clear();

            string[] GameTypeFolderPathList = Directory.GetDirectories(PathManager.Path_GameTypeConfigsFolder);

            foreach (string GameTypeFolderPath in GameTypeFolderPathList)
            {
                string GameTypeFolderName = Path.GetFileName(GameTypeFolderPath);
                ComboBox_GameTypeFolder.Items.Add(GameTypeFolderName);
            }

            IsLoading = false;
        }


        /// <summary>
        /// 读取所有创建的游戏名称，然后根据配置文件决定是否显示图标
        /// </summary>
        private void InitializeGameIconItemList()
        {
            IsLoading = true;
            //初始化图标列表
            if (!Directory.Exists(PathManager.Path_GamesFolder))
            {
                return;
            }

            string[] GamesFolderList = Directory.GetDirectories(PathManager.Path_GamesFolder);

            GameIconItemList.Clear();

            foreach (string GameFolderPath in GamesFolderList)
            {
                string GameFolderName = Path.GetFileName(GameFolderPath);

                GameIconConfig gameIconConfig = new GameIconConfig();

                if (!gameIconConfig.GameName_Show_Dict.ContainsKey(GameFolderName))
                {
                    continue;
                }

                if (!gameIconConfig.GameName_Show_Dict[GameFolderName])
                {
                    continue;
                }

                GameIconItem gameIconItem = new GameIconItem();
                gameIconItem.GameName = GameFolderName;
                gameIconItem.GameIconImage = Path.Combine(PathManager.Path_GamesFolder, GameFolderName + "\\Icon.png");
                GameIconItemList.Add(gameIconItem);
            }

            IsLoading = false;
        }


        /// <summary>
        /// 上面读取游戏图标列表只是把图标列表给加载上，但是还没有选中
        /// 这里我们读取了游戏名称列表，放到下拉菜单中，并且读取历史配置来决定选中哪一个
        /// 自动触发游戏名称改变时的方法，然后游戏图标列表也会自动选中到对应的项
        /// 
        /// 注意:不在isLoading中调用会触发游戏改变方法
        /// </summary>
        private void InitializeGameNameList()
        {
            if (!Directory.Exists(PathManager.Path_GamesFolder))
            {
                return;
            }

            string[] GamesFolderList = Directory.GetDirectories(PathManager.Path_GamesFolder);

            ComboBox_GameName.Items.Clear();

            foreach (string GameFolderPath in GamesFolderList)
            {
                string GameFolderName = Path.GetFileName(GameFolderPath);

                ComboBox_GameName.Items.Add(GameFolderName);
            }

            if (ComboBox_GameName.Items.Contains(GlobalConfig.CurrentGameName))
            {
                ComboBox_GameName.SelectedItem = GlobalConfig.CurrentGameName;
            }
            else
            {
                if (ComboBox_GameName.Items.Count > 0)
                {
                    ComboBox_GameName.SelectedIndex = 0;
                }
            }
        }



        private void InitializeMigotoPackageList()
        {

            IsLoading = true;
            ComboBox_MigotoPackage.Items.Clear();

            ComboBox_MigotoPackage.Items.Add(MigotoPackageName.GIMIPackage);
            ComboBox_MigotoPackage.Items.Add(MigotoPackageName.HIMIPackage);
            ComboBox_MigotoPackage.Items.Add(MigotoPackageName.SRMIPackage);
            ComboBox_MigotoPackage.Items.Add(MigotoPackageName.ZZMIPackage);
            ComboBox_MigotoPackage.Items.Add(MigotoPackageName.MinBasePackage);
            ComboBox_MigotoPackage.Items.Add(MigotoPackageName.NBPPackage);

            ComboBox_MigotoPackage.SelectedIndex = 0;
            IsLoading = false;
        }




    }
}
