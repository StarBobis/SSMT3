using CommunityToolkit.WinUI.Behaviors;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Newtonsoft.Json.Linq;
using SSMT.SSMTHelper;
using SSMT_Core;
using SSMT_Core.InfoItemClass;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI;
using Windows.UI.ViewManagement;
using WinUI3Helper;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SSMT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        /// <summary>
        /// 游戏图标列表
        /// </summary>
        private ObservableCollection<GameIconItem> GameIconItemList = new ObservableCollection<GameIconItem>();



        /// <summary>
        /// 用于控制当前状态是否处于读取配置过程中，以此防止内容改变时触发方法然后递归的改变。
        /// </summary>
        private bool IsLoading = false;


        public HomePage()
        {
            this.InitializeComponent();
            this.Loaded += HomePageLoaded;
        }
       


        private void HomePageLoaded(object sender, RoutedEventArgs e)
        {

            

            GameIconGridView.ItemsSource = GameIconItemList;

            

            //初始化LogcName列表，在当前游戏改变时会自动选中对应的条目
            InitializeLogicNameList();
            //初始化GameType Folder列表，在当前游戏改变时会自动选中对应的条目
            InitializeGameTypeFolderList();

            //初始化游戏预设列表，仅填充并默认选中，在后续的GameNameChanged中会读取配置进行选中
            InitializeGamePresetComboBox();

            InitializeGameIconItemList();

            InitializeGameNameList();

            InitializeMigotoPackageList();

            GameNameChanged(GlobalConfig.CurrentGameName);
        }


   
       


        private async void GameNameChanged(string ChangeToGameName)
        {
            //清楚顶部InfoBar内容
			NotificationQueue.Clear();

            //游戏名称改到目标游戏名称，然后保存配置，这样Blender插件就能实时同步知道游戏发生了变化
			GlobalConfig.CurrentGameName = ChangeToGameName;
            GlobalConfig.SaveConfig();



            InitializePanel();


            ReadConfigsToPanel();



            //判断当前3Dmigoto目录是否存在，如果不存在则默认设置为SSMT缓存文件夹中的3Dmigoto目录
            if (TextBox_3DmigotoPath.Text.Trim() == "" || !Directory.Exists(TextBox_3DmigotoPath.Text.Trim()))
            {
                if (Directory.Exists(GlobalConfig.SSMTCacheFolderPath))
                {
                    string DefaultGameMigotoPath = Path.Combine(GlobalConfig.SSMTCacheFolderPath, "3Dmigoto\\" + GlobalConfig.CurrentGameName + "\\");
                    if (!Directory.Exists(DefaultGameMigotoPath))
                    {
                        Directory.CreateDirectory(DefaultGameMigotoPath);
                    }
                    TextBox_3DmigotoPath.Text = DefaultGameMigotoPath;

                    //设置完要保存3Dmigoto路径
                    DoAfter3DmigotoPathChanged();

                    //同步复制过去dll文件
                    InstallBasicDllFileTo3DmigotoFolder();
                }
            }
            else {
                string d3dxIniPath = Path.Combine(TextBox_3DmigotoPath.Text.Trim(), "d3dx.ini");
                if (!File.Exists(d3dxIniPath)) {
                    var notification = new Notification
                    {
                        Title = "Tips",
                        Message = "您当前游戏: " + GlobalConfig.CurrentGameName+ " 的3Dmigoto目录下还没有对应的Package文件，请点击【从Github检查更新并自动下载最新3Dmigoto加载器包】来自动下载更新或者点击【选择3Dmigoto文件夹】来选择你自己的3Dmigoto文件夹以此来结合第三方工具例如XXMI Launcher，d3dxSkinManager，JASM等工具一起使用",
                        Severity = InfoBarSeverity.Warning
                    };

                    //我去，这里指定持续时间会导致报错，全是BUG啊这WinUI3
                    //暂时只能无限时间显示了。
                    NotificationQueue.Show(notification);
                    
                    VisualHelper.CreateInfoBarShowAnimation(InforBar_NorificationQueue);
				}
            }


            InitializeToggleConfig();

            IsLoading = true;

            //读取LogicName
            GameConfig gameConfig = new GameConfig();

            //Nico: 切换游戏时，要更新当前的工作空间，这样跳转到其他页面时才能正确加载对应工作空间内容
            //例如切换游戏后直接跳转到贴图标记页面，如果不在这里更新工作空间的话，贴图标记页面就是空的
            GlobalConfig.CurrentWorkSpace = gameConfig.WorkSpace;

            

            //读取dll初始化延迟

            NumberBox_DllInitializationDelay.Value = gameConfig.DllInitializationDelay;
            ComboBox_DllPreProcess.SelectedIndex = gameConfig.DllPreProcessSelectedIndex;
            ComboBox_DllReplace.SelectedIndex = gameConfig.DllReplaceSelectedIndex;
            ComboBox_AutoSetAnalyseOptions.SelectedIndex = gameConfig.AutoSetAnalyseOptionsSelectedIndex;
            ToggleSwitch_PureGameMode.IsOn = gameConfig.PureGameMode;
            //LOG.Info("MigotoPackage设为: " + gameConfig.MigotoPackage);




            //是否显示防报错按钮
            if (gameConfig.LogicName == LogicName.GIMI )
            {
                SettingsCard_ClearGICache.Visibility = Visibility.Visible;
                SettingsCard_RunIgnoreGIError40.Visibility = Visibility.Visible;
            }
            else
            {
				SettingsCard_ClearGICache.Visibility = Visibility.Collapsed;
				SettingsCard_RunIgnoreGIError40.Visibility = Visibility.Collapsed;
			}


            SelectGameIconToCurrentGame();

            //这里调用初始化游戏名称下拉菜单的目的是
            //让当前选中项重新选中到当前的游戏上
            //可能比较难理解对吧，因为GameNameChanged也会被图标的选中触发
            //所以要把设置页面的下拉菜单同步选中，此时复用方法就是最方便的
            //但是注意必须设置IsLoading = true再调用，调用完设置IsLoading = false
            //不然就会死循环调用
            InitializeGameNameList();


			//最后如果有d3dx.ini的话，如果有哪些路径配置还是空的，就试图从d3dx.ini中解析读取，保底机制
			string d3dxini_path = Path.Combine(TextBox_3DmigotoPath.Text, "d3dx.ini");
			if (File.Exists(d3dxini_path))
			{
				//如果当前的target = 为空的话，就尝试读取
				if (TextBox_TargetPath.Text.Trim() == "")
				{
					TextBox_TargetPath.Text = D3dxIniConfig.ReadAttributeFromD3DXIni(d3dxini_path, "target");
				}
			}

			IsLoading = false;


            //Nico: 设置了游戏预设就不需要设置LogicName、GameTypeFolder、MigotoPackage了
            //因为这里会触发他们的自动设置
            //这里必须在IsLoading之外设置，才能触发其它三个的联动设置

            LOG.Info("GamePreset: " + gameConfig.GamePreset);
            ComboBox_GamePreset.SelectedItem = gameConfig.GamePreset;

            //游戏切换后要把Package标识以及版本号改一下
            UpdatePackageVersionLink();

            //背景图放到最后更新，没必要更新那么早
            //根据当前游戏，初始化背景图或者背景视频
            await MainWindow.CurrentWindow.InitializeBackground(GlobalConfig.CurrentGameName);

        }
     

        private void UpdatePackageVersionLink()
        {
            GameConfig gameConfig = new GameConfig();
            //设置左上角Package版本
            RepositoryInfo repositoryInfo = GithubUtils.GetCurrentRepositoryInfo(gameConfig.MigotoPackage);
            HyperlinkButton_MigotoPackageVersion.Content = repositoryInfo.RepositoryName + " " + gameConfig.GithubPackageVersion;
            var url = $"https://github.com/{repositoryInfo.OwnerName}/{repositoryInfo.RepositoryName}/releases/latest";
            HyperlinkButton_MigotoPackageVersion.NavigateUri = new Uri(url);
        }

      
        private void SelectGameIconToCurrentGame()
        {
            int i = 0;
            foreach (GameIconItem gameIconItem in GameIconItemList)
            {
                if (gameIconItem.GameName == GlobalConfig.CurrentGameName)
                {
                    GameIconGridView.SelectedIndex = i;
                    break;
                }
                i++;
            }
        }

        private void GameIconGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoading)
            {
                return;
            }
            string IconGameName = GameIconItemList[GameIconGridView.SelectedIndex].GameName;
            GameNameChanged(IconGameName);
        }

        private void ComboBox_GameName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoading)
            {
                return;
            }
            string ComboBoxGameName = ComboBox_GameName.SelectedItem.ToString();
            GameNameChanged(ComboBoxGameName);
        }

        private void InitializeToggleConfig()
        {
            IsLoading = true;

            GameConfig gameConfig = new GameConfig();


            //读取三个设置
            string d3dxiniPath = Path.Combine(gameConfig.MigotoPath, "d3dx.ini");
            if (File.Exists(d3dxiniPath))
            {




                string ShowWarningsStr = D3dxIniConfig.ReadAttributeFromD3DXIni(d3dxiniPath, "show_warnings").Trim();
                if (ShowWarningsStr.Trim() == "1")
                {
                    ComboBox_ShowWarning.SelectedIndex = 0;
                }
                else if (ShowWarningsStr.Trim() == "0")
                {
					ComboBox_ShowWarning.SelectedIndex = 1;
				}
                else
                {
					ComboBox_ShowWarning.SelectedIndex = 0;
				}



			}


            GameIconConfig gameIconConfig = new GameIconConfig();

            bool ShowIcon = false;
            if (gameIconConfig.GameName_Show_Dict.ContainsKey(GlobalConfig.CurrentGameName))
            {
                ShowIcon = gameIconConfig.GameName_Show_Dict[GlobalConfig.CurrentGameName];
            }
            else
            {
                ShowIcon = false;
            }

            ToggleSwitch_ShowIcon.IsOn = ShowIcon;

            IsLoading = false;
        }

      




        





        









        public async void InstallBasicDllFileTo3DmigotoFolder()
        {
            //默认路径
            string MigotoFolder = Path.Combine(GlobalConfig.SSMTCacheFolderPath, "3Dmigoto\\");
            Directory.CreateDirectory(MigotoFolder);

            string CurrentGame3DmigotoFolder = Path.Combine(MigotoFolder, GlobalConfig.CurrentGameName);

            //如果手动设置了当前3Dmigoto的路径，则使用手动设置的路径
            string SelectedMigotoFolderPath = TextBox_3DmigotoPath.Text.Trim();
            if (Directory.Exists(SelectedMigotoFolderPath) && SelectedMigotoFolderPath != "")
            {
                CurrentGame3DmigotoFolder = SelectedMigotoFolderPath;
            }

            string MigotoSourceDll = Path.Combine(PathManager.Path_AssetsFolder, "ReleaseX64Dev\\d3d11.dll");
            string MigotoTargetDll = Path.Combine(CurrentGame3DmigotoFolder, "d3d11.dll");

            //只有dll不存在时才复制
            if (!File.Exists(MigotoTargetDll))
            {
                File.Copy(MigotoSourceDll, MigotoTargetDll, true);
            }

            string MigotoSource47Dll = Path.Combine(PathManager.Path_AssetsFolder, "d3dcompiler_47.dll");
            string MigotoTarget47Dll = Path.Combine(CurrentGame3DmigotoFolder, "d3dcompiler_47.dll");
            string MigotoSource46Dll = Path.Combine(PathManager.Path_AssetsFolder, "d3dcompiler_46.dll");
            string MigotoTarget46Dll = Path.Combine(CurrentGame3DmigotoFolder, "d3dcompiler_46.dll");

            try
            {
                string targetDll = File.Exists(MigotoSource47Dll) ? MigotoTarget47Dll : MigotoTarget46Dll;
                string sourceDll = File.Exists(MigotoSource47Dll) ? MigotoSource47Dll : MigotoSource46Dll;

                //47只有WWMI用到，46其他游戏用到，后面再写逻辑判断吧。


                // 检查文件是否被占用
                if (File.Exists(targetDll))
                {
                    var lockingProcesses = FileLockHelper.GetLockingProcesses(targetDll);
                    if (lockingProcesses.Count > 0)
                    {
                        string list = string.Join("\r\n", lockingProcesses.ConvertAll(p => $"{p.ProcessName} (PID: {p.Id})"));
                        bool kill = await SSMTMessageHelper.ShowConfirm(
                            $"检测到以下进程正在占用 {Path.GetFileName(targetDll)}：\r\n\r\n{list}\r\n\r\n是否结束这些进程并重试？");

                        if (kill)
                        {
                            foreach (var p in lockingProcesses)
                            {
                                try
                                {
                                    if (p.Id != Process.GetCurrentProcess().Id)
                                        p.Kill();
                                }
                                catch { /* 忽略无权限或系统进程 */ }
                            }

                            await Task.Delay(1500); // 等待释放句柄
                        }
                        else
                        {
                            _ = SSMTMessageHelper.Show("用户取消了更新操作，未替换被占用的文件。");
                            return;
                        }
                    }
                }

                // 文件不存在或已解锁后执行复制
                if (!File.Exists(targetDll) || new FileInfo(sourceDll).Length != new FileInfo(targetDll).Length)
                {
                    File.Copy(sourceDll, targetDll, true);
                }
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show($"复制 3Dmigoto 运行库时出错：\r\n{ex}");
            }
        }




        private void Button_ShowSetting_Click(object sender, RoutedEventArgs e)
        {
            if (Border_GameConfig.Visibility == Visibility.Collapsed)
            {
                Border_GameConfig.Visibility = Visibility.Visible;
            }
            else
            {
                Border_GameConfig.Visibility = Visibility.Collapsed;
            }
        }


        private void InitializePanel()
        {
            IsLoading = true;

            TextBox_3DmigotoPath.Text = "";
            TextBox_TargetPath.Text = "";
            TextBox_LaunchPath.Text = "";
            TextBox_LaunchArgsPath.Text = "";

			IsLoading = false;
        }

        private void ReadConfigsToPanel()
        {
            LOG.Info("ReadConfigsToPanel::Start");

            GameConfig CurrentGameConfig = new GameConfig();

            //只有路径存在时才进行设置
            if (Directory.Exists(CurrentGameConfig.MigotoPath)) { 
                TextBox_3DmigotoPath.Text = CurrentGameConfig.MigotoPath;
            }

            //如果是具有文件夹层级的路径，则必须判断是否存在
            //如果没有就直接设置，比如有些人会直接填写YuanShen.exe
            if (CurrentGameConfig.TargetPath.Contains("\\"))
            {
                if (File.Exists(CurrentGameConfig.TargetPath.Trim()))
                {
                    TextBox_TargetPath.Text = CurrentGameConfig.TargetPath;
                }
            }
            else
            {
                TextBox_TargetPath.Text = CurrentGameConfig.TargetPath;
            }

            LOG.Info("尝试设置LaunchPath:" + CurrentGameConfig.LaunchPath);
            if (File.Exists(CurrentGameConfig.LaunchPath.Trim()))
            {
                LOG.Info("存在保存的LaunchPath:" + CurrentGameConfig.LaunchPath + "  现在进行设置");
                TextBox_LaunchPath.Text = CurrentGameConfig.LaunchPath;
            }
            else
            {
                LOG.Info("文件中保存的LaunchPath不存在，无法设置");
            }

            TextBox_LaunchArgsPath.Text = CurrentGameConfig.LaunchArgs;

            LOG.Info("ReadConfigsToPanel::End");

        }




        private void Button_Open3DmigotoFolder_Click(object sender, RoutedEventArgs e)
        {
            GameConfig gameConfig = new GameConfig();
            if (Directory.Exists(gameConfig.MigotoPath))
            {
                SSMTCommandHelper.ShellOpenFolder(gameConfig.MigotoPath);
            }
        }


        private void Button_CreateNewGame_Click(object sender, RoutedEventArgs e)
        {
            string GameName = ComboBox_GameName.Text;

            try
            {
                string NewGameDirectory = Path.Combine(PathManager.Path_GamesFolder, GameName + "\\");
                Directory.CreateDirectory(NewGameDirectory);

                ToggleSwitch_ShowIcon.IsOn = true;

                GlobalConfig.CurrentGameName = GameName;
                GlobalConfig.SaveConfig();

                Frame.Navigate(typeof(HomePage));

            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }

        }

        private void Button_DeleteSelectedGame_Click(object sender, RoutedEventArgs e)
        {
            string GameName = ComboBox_GameName.Text;

            try
            {
                Directory.Delete(PathManager.Path_CurrentGamesFolder,true);
                Frame.Navigate(typeof(HomePage));
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
        }

  

        private void ComboBox_LogicName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string LogicNameStr = ComboBox_LogicName.SelectedItem.ToString();


            if (IsLoading)
            {
                return;
            }
            GameConfig gameConfig = new GameConfig();
            gameConfig.LogicName = LogicNameStr;
            gameConfig.SaveConfig();
        }

        private void ToggleSwitch_ShowIcon_Toggled(object sender, RoutedEventArgs e)
        {
            if (IsLoading)
            {
                return;
            }
            GameIconConfig gameIconConfig = new GameIconConfig();
            gameIconConfig.GameName_Show_Dict[GlobalConfig.CurrentGameName] = ToggleSwitch_ShowIcon.IsOn;
            gameIconConfig.SaveConfig();

            IsLoading = true;

            InitializeGameIconItemList();
            SelectGameIconToCurrentGame();
            IsLoading = false;
        }

        private async void Button_ChooseGameIcon_Click(object sender, RoutedEventArgs e)
        {
            string filepath = await SSMTCommandHelper.ChooseFileAndGetPath(".png");
            if (filepath == "")
            {
                return;
            }


            try
            {
                string NewBackgroundPath = Path.Combine(PathManager.Path_CurrentGamesFolder, "Icon.png");
                File.Copy(filepath, NewBackgroundPath, true);

                IsLoading = true;
                InitializeGameIconItemList();
                SelectGameIconToCurrentGame();

                IsLoading = false;
                _ = SSMTMessageHelper.Show("图标已更换成功，请重启SSMT使图标生效");
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }

        }




        private async void Button_RunIgnoreGIError40_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string IgnoreGIErrorExePath = Path.Combine(PathManager.Path_PluginsFolder, PathManager.Name_Plugin_GoodWorkGI);
                if (!File.Exists(IgnoreGIErrorExePath))
                {
                    _ = SSMTMessageHelper.Show("您还没有安装此插件，请在爱发电上赞助NicoMico的SSMT技术社群方案，加入技术社群获取并安装此插件，您可以在SSMT的设置页面中右侧看到直达赞助链接的按钮。","Not Supported Yet.");
                    return;
                }

                await SSMTCommandHelper.ProcessRunFile(IgnoreGIErrorExePath, "", "");
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
                return;
            }

        }

   



        private void Button_CleanGICache_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //获取原神安装路径
                string TargetExePath = TextBox_TargetPath.Text;


                string localLow = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "AppData", "LocalLow");

                string LocalLogFilePath = Path.Combine(localLow,"miHoYo", "原神", "LocalLog.log");
                if (File.Exists(LocalLogFilePath))
                {
                    File.Delete(LocalLogFilePath);
                }

                string OutputLogFilePath = Path.Combine(localLow, "miHoYo", "原神", "output_log.txt");
                if (File.Exists(OutputLogFilePath))
                {
                    File.Delete(OutputLogFilePath);
                }

                _ = SSMTMessageHelper.Show("缓存日志清理完成");

            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }

        }

        private void ComboBox_GameTypeFolder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string GameTypeName = ComboBox_GameTypeFolder.SelectedItem.ToString();


            if (IsLoading)
            {
                return;
            }
            GameConfig gameConfig = new GameConfig();
            gameConfig.GameTypeName = GameTypeName;
            gameConfig.SaveConfig();
        }

        private void NumberBox_DllInitializationDelay_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsLoading)
            {
                return;
            }
            GameConfig gameConfig = new GameConfig();
            gameConfig.DllInitializationDelay = (int)NumberBox_DllInitializationDelay.Value;
            gameConfig.SaveConfig();
        }

        private void ComboBox_DllReplace_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoading)
            {
                return;
            }

            GameConfig gameConfig = new GameConfig();
            gameConfig.DllReplaceSelectedIndex = ComboBox_DllReplace.SelectedIndex;
            gameConfig.SaveConfig();

        }

        private void ComboBox_DllPreProcess_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoading)
            {
                return;
            }

            GameConfig gameConfig = new GameConfig();
            gameConfig.DllPreProcessSelectedIndex = ComboBox_DllPreProcess.SelectedIndex;
            gameConfig.SaveConfig();
        }

        private void ComboBox_AutoSetAnalyseOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			if (IsLoading)
			{
				return;
			}

			GameConfig gameConfig = new GameConfig();
			gameConfig.AutoSetAnalyseOptionsSelectedIndex = ComboBox_AutoSetAnalyseOptions.SelectedIndex;
			gameConfig.SaveConfig();
		}

 

        private void ComboBox_ShowWarning_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			if (IsLoading)
			{
				return;
			}

			if (ComboBox_ShowWarning.SelectedIndex == 0)
			{
				D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[Logging]", "show_warnings", "1");
                _ = SSMTMessageHelper.Show("启用成功，游戏中F10刷新即可生效","Enable Success, Press F10 in game to reload.");
			}
			else
			{
				D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[Logging]", "show_warnings", "0");
				_ = SSMTMessageHelper.Show("关闭成功，游戏中F10刷新即可生效", "Disable Success, Press F10 in game to reload.");
			}
		}

        private void ToggleSwitch_PureGameMode_Toggled(object sender, RoutedEventArgs e)
        {
            if (IsLoading)
            {
                return;
            }

            GameConfig gameConfig = new GameConfig();
            gameConfig.PureGameMode = ToggleSwitch_PureGameMode.IsOn;
            gameConfig.SaveConfig();
        }

        private void ComboBox_MigotoPackage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoading)
            {
                return;
            }

            GameConfig gameConfig = new GameConfig();
            gameConfig.MigotoPackage = ComboBox_MigotoPackage.SelectedItem.ToString();
            gameConfig.SaveConfig();
        }

        private void ComboBox_GamePreset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoading)
            {
                return;
            }

            string CurrentGamePreset = ComboBox_GamePreset.SelectedItem.ToString();

            

            //根据当前预设，调整LogicName，MigotoPackage，GameTypeFolder等选项

            if (CurrentGamePreset == GamePreset.GIMI)
            {
                ComboBox_LogicName.SelectedItem = LogicName.GIMI;
                ComboBox_GameTypeFolder.SelectedItem = "GIMI";
                ComboBox_MigotoPackage.SelectedItem = MigotoPackageName.GIMIPackage;
            }
            else if (CurrentGamePreset == GamePreset.HIMI)
            {
                ComboBox_LogicName.SelectedItem = LogicName.HIMI;
                ComboBox_GameTypeFolder.SelectedItem = "HIMI";
                ComboBox_MigotoPackage.SelectedItem = MigotoPackageName.HIMIPackage;
            }
            else if (CurrentGamePreset == GamePreset.SRMI)
            {
                ComboBox_LogicName.SelectedItem = LogicName.SRMI;
                ComboBox_GameTypeFolder.SelectedItem = "SRMI";
                ComboBox_MigotoPackage.SelectedItem = MigotoPackageName.SRMIPackage;
            }
            else if (CurrentGamePreset == GamePreset.ZZMI)
            {
                ComboBox_LogicName.SelectedItem = LogicName.ZZMI;
                ComboBox_GameTypeFolder.SelectedItem = "ZZMI";
                ComboBox_MigotoPackage.SelectedItem = MigotoPackageName.ZZMIPackage;
            }
            else if (CurrentGamePreset == GamePreset.WWMI)
            {
                ComboBox_LogicName.SelectedItem = LogicName.WWMI;
                ComboBox_GameTypeFolder.SelectedItem = "WWMI";
                ComboBox_MigotoPackage.SelectedItem = MigotoPackageName.WWMIPackage;
            }
            else if (CurrentGamePreset == GamePreset.GF2)
            {
                ComboBox_LogicName.SelectedItem = LogicName.UnityCPU;
                ComboBox_GameTypeFolder.SelectedItem = "GF2";
                ComboBox_MigotoPackage.SelectedItem = MigotoPackageName.GIMIPackage;
            }
            else if (CurrentGamePreset == GamePreset.IdentityVNeoX2)
            {
                ComboBox_LogicName.SelectedItem = LogicName.CTXMC;
                ComboBox_GameTypeFolder.SelectedItem = "IdentityV";
                ComboBox_MigotoPackage.SelectedItem = MigotoPackageName.MinBasePackage;
            }
            else if (CurrentGamePreset == GamePreset.IdentityVNeoX2)
            {
                ComboBox_LogicName.SelectedItem = LogicName.IdentityV2;
                ComboBox_GameTypeFolder.SelectedItem = "IdentityV2";
                ComboBox_MigotoPackage.SelectedItem = MigotoPackageName.MinBasePackage;
            }
            else if (CurrentGamePreset == GamePreset.AILIMIT)
            {
                ComboBox_LogicName.SelectedItem = LogicName.AILIMIT;
                ComboBox_GameTypeFolder.SelectedItem = "AILIMIT";
                ComboBox_MigotoPackage.SelectedItem = MigotoPackageName.MinBasePackage;
            }
            else if (CurrentGamePreset == GamePreset.BloodySpell)
            {
                ComboBox_LogicName.SelectedItem = LogicName.GIMI;
                ComboBox_GameTypeFolder.SelectedItem = "GIMI";
                ComboBox_MigotoPackage.SelectedItem = MigotoPackageName.GIMIPackage;
            }
            else if (CurrentGamePreset == GamePreset.DOAV)
            {
                ComboBox_LogicName.SelectedItem = LogicName.CTXMC;
                ComboBox_GameTypeFolder.SelectedItem = "DOAV";
                ComboBox_MigotoPackage.SelectedItem = MigotoPackageName.MinBasePackage;
            }
            else if (CurrentGamePreset == GamePreset.MiSide)
            {
                ComboBox_LogicName.SelectedItem = LogicName.UnityCS;
                ComboBox_GameTypeFolder.SelectedItem = "MiSide";
                ComboBox_MigotoPackage.SelectedItem = MigotoPackageName.MinBasePackage;
            }
            else if (CurrentGamePreset == GamePreset.SnowBreak)
            {
                ComboBox_LogicName.SelectedItem = LogicName.SnowBreak;
                ComboBox_GameTypeFolder.SelectedItem = "SnowBreak";
                ComboBox_MigotoPackage.SelectedItem = MigotoPackageName.MinBasePackage;
            }
            else if (CurrentGamePreset == GamePreset.Strinova)
            {
                ComboBox_LogicName.SelectedItem = LogicName.SnowBreak;
                ComboBox_GameTypeFolder.SelectedItem = "SnowBreak";
                ComboBox_MigotoPackage.SelectedItem = MigotoPackageName.MinBasePackage;
            }
            else if (CurrentGamePreset == GamePreset.Nioh2)
            {
                ComboBox_LogicName.SelectedItem = LogicName.CTXMC;
                ComboBox_GameTypeFolder.SelectedItem = "Nioh2";
                ComboBox_MigotoPackage.SelectedItem = MigotoPackageName.MinBasePackage;
            }
            else if (CurrentGamePreset == GamePreset.YYSLS)
            {
                ComboBox_LogicName.SelectedItem = LogicName.YYSLS;
                ComboBox_GameTypeFolder.SelectedItem = "YYSLS";
                ComboBox_MigotoPackage.SelectedItem = MigotoPackageName.MinBasePackage;
            }
            else
            {
                //没选中的话可能是DIY，此时就读取已保存的设置
                GameConfig gameConfig2 = new GameConfig();
                ComboBox_LogicName.SelectedItem = gameConfig2.LogicName;
                ComboBox_GameTypeFolder.SelectedItem = gameConfig2.GameTypeName;
                ComboBox_MigotoPackage.SelectedItem = gameConfig2.MigotoPackage;
            }


            if (CurrentGamePreset == GamePreset.DIY)
            {
                LOG.Info("设置显示自定义配置");
                Expander_DIYSettings.Visibility = Visibility.Visible;
            }
            else
            {
                LOG.Info("设置不显示自定义配置");
                Expander_DIYSettings.Visibility = Visibility.Collapsed;
            }

            //最后保存到配置
            GameConfig gameConfig = new GameConfig();
            gameConfig.GamePreset = CurrentGamePreset;
            gameConfig.SaveConfig();
        }
    }
}
