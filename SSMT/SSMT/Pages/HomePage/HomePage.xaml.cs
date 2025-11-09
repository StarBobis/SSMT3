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
        private ObservableCollection<GameIconItem> GameIconItemList = new ObservableCollection<GameIconItem>();
        private Visual imageVisual;
        private bool IsLoading = false;




        public HomePage()
        {
            this.InitializeComponent();
            this.Loaded += HomePageLoaded;

            
        }
       


        private void HomePageLoaded(object sender, RoutedEventArgs e)
        {

            // 初始化Composition组件
            // 获取Image控件的Visual对象
            imageVisual = ElementCompositionPreview.GetElementVisual(MainWindowImageBrush);

            GameIconGridView.ItemsSource = GameIconItemList;

            //初始化LogcName列表，在当前游戏改变时会自动选中对应的条目
            InitializeLogicNameList();
            //初始化GameType Folder列表，在当前游戏改变时会自动选中对应的条目
            InitializeGameTypeFolderList();

            InitializeGameIconItemList();

            InitializeGameNameList();


            

            GameNameChanged(GlobalConfig.CurrentGameName);

        }



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

        private void InitializeGameTypeFolderList()
        {
            IsLoading = true;

            ComboBox_GameTypeFolder.Items.Clear();

            string[] GameTypeFolderPathList = Directory.GetDirectories (PathManager.Path_GameTypeConfigsFolder);

            foreach (string GameTypeFolderPath in GameTypeFolderPathList)
            {
                string GameTypeFolderName = Path.GetFileName(GameTypeFolderPath);
                ComboBox_GameTypeFolder.Items.Add(GameTypeFolderName);
            }

            IsLoading = false;
        }


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

     

        private void GameNameChanged(string ChangeToGameName)
        {
            GlobalConfig.CurrentGameName = ChangeToGameName;
            GlobalConfig.SaveConfig();

            string folder = PathManager.Path_CurrentGamesFolder;
            string BackgroundWebpPath = Path.Combine(folder, "Background.webp");
            string BackgroundPngPath = Path.Combine(folder, "Background.png");
            string BackgroundMp4Path = Path.Combine(folder, "Background.mp4");

            // 默认：隐藏视频，清空图片
            if (BackgroundVideo != null)
            {
                BackgroundVideo.Visibility = Visibility.Collapsed;
                BackgroundVideo.SetMediaPlayer(null);
            }
            MainWindowImageBrush.Source = null;

            // 优先级：mp4 > webp > png
            if (File.Exists(BackgroundMp4Path))
            {
                MainWindowImageBrush.Visibility = Visibility.Collapsed;
                BackgroundVideo.Visibility = Visibility.Visible;

                var player = new MediaPlayer
                {
                    Source = MediaSource.CreateFromUri(new Uri(BackgroundMp4Path)),
                    IsLoopingEnabled = true
                };
                BackgroundVideo.SetMediaPlayer(player);
                player.Play();

                VisualHelper.CreateFadeAnimation(BackgroundVideo);
            }
            else if (File.Exists(BackgroundWebpPath))
            {
                BackgroundVideo.Visibility = Visibility.Collapsed;
                MainWindowImageBrush.Visibility = Visibility.Visible;

                VisualHelper.CreateScaleAnimation(MainWindowImageBrush);
                VisualHelper.CreateFadeAnimation(MainWindowImageBrush);
                MainWindowImageBrush.Source = new BitmapImage(new Uri(BackgroundWebpPath));
            }
            else if (File.Exists(BackgroundPngPath))
            {
                BackgroundVideo.Visibility = Visibility.Collapsed;
                MainWindowImageBrush.Visibility = Visibility.Visible;

                VisualHelper.CreateScaleAnimation(MainWindowImageBrush);
                VisualHelper.CreateFadeAnimation(MainWindowImageBrush);
                MainWindowImageBrush.Source = new BitmapImage(new Uri(BackgroundPngPath));
            }
            else
            {
                // 没有任何背景文件时
                BackgroundVideo.Visibility = Visibility.Collapsed;
                MainWindowImageBrush.Source = null;
            }


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

            InitializeToggleConfig();

            IsLoading = true;

            //读取LogicName
            GameConfig gameConfig = new GameConfig();
            if (ComboBox_LogicName.Items.Contains(gameConfig.LogicName))
            {
                ComboBox_LogicName.SelectedItem = gameConfig.LogicName;
            }
            else
            {
                ComboBox_LogicName.SelectedIndex = 0;
            }

            //读取GameType Folder
            if (ComboBox_GameTypeFolder.Items.Contains(gameConfig.GameTypeName))
            {
                ComboBox_GameTypeFolder.SelectedItem = gameConfig.GameTypeName;
            }
            else
            {
                ComboBox_GameTypeFolder.SelectedIndex = 0;
            }

            //读取dll初始化延迟

            NumberBox_DllInitializationDelay.Value = gameConfig.DllInitializationDelay;
            ComboBox_DllPreProcess.SelectedIndex = gameConfig.DllPreProcessSelectedIndex;
            ComboBox_DllReplace.SelectedIndex = gameConfig.DllReplaceSelectedIndex;
            ComboBox_AutoSetAnalyseOptions.SelectedIndex = gameConfig.AutoSetAnalyseOptionsSelectedIndex;


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
            InitializeGameNameList();

            IsLoading = false;


			//最后保底配置，如果真的还有没配置的，就会触发这里的从d3dx.ini读取配置
			IsLoading = true;

			//target,launch,launch_args,show_warnings,symlink
			string d3dxini_path = Path.Combine(TextBox_3DmigotoPath.Text, "d3dx.ini");
			if (File.Exists(d3dxini_path))
			{
				//如果当前的target = 为空的话，就尝试读取
				if (TextBox_TargetPath.Text.Trim() == "")
				{
					TextBox_TargetPath.Text = D3dxIniConfig.ReadAttributeFromD3DXIni(d3dxini_path, "target");
				}

				if (TextBox_LaunchPath.Text.Trim() == "")
				{
					LOG.Info("切换游戏后，发现LaunchPath为空，重新读取");
					TextBox_LaunchPath.Text = D3dxIniConfig.ReadAttributeFromD3DXIni(d3dxini_path, "launch");
				}

				if (TextBox_LaunchArgsPath.Text.Trim() == "")
				{
					TextBox_LaunchArgsPath.Text = D3dxIniConfig.ReadAttributeFromD3DXIni(d3dxini_path, "launch_args");
				}


			}


			IsLoading = false;

			LoadAtLeastPicture();

            UpdatePackageVersionLink();
        }
     

        private void UpdatePackageVersionLink()
        {
            GameConfig gameConfig = new GameConfig();
            //设置左上角Package版本
            string PackageName = ComboBox_LogicName.SelectedItem.ToString();
            RepositoryInfo repositoryInfo = GithubUtils.GetCurrentRepositoryInfo(PackageName);
            HyperlinkButton_MigotoPackageVersion.Content = repositoryInfo.RepositoryName + " " + gameConfig.GithubPackageVersion;
            var url = $"https://github.com/{repositoryInfo.OwnerName}/{repositoryInfo.RepositoryName}/releases/latest";
            HyperlinkButton_MigotoPackageVersion.NavigateUri = new Uri(url);
        }

        private void LoadAtLeastPicture()
        {
            //只有米的四个游戏会根据游戏名称默认触发保底背景图更新
            try
            {
                string CurrentGameName = ComboBox_GameName.SelectedItem.ToString();

                if (CurrentGameName == LogicName.GIMI ||
                    CurrentGameName == LogicName.SRMI ||
                    CurrentGameName == LogicName.HIMI ||
                    CurrentGameName == LogicName.ZZMI
                    )
                {
                    //_ = SSMTMessageHelper.Show(CurrentLogicName);
                    string PossibleWebpPicture = Path.Combine(PathManager.Path_CurrentGamesFolder, "Background.webp");
                    string PossiblePngBackgroundPath = Path.Combine(PathManager.Path_CurrentGamesFolder, "Background.png");

                    if (!File.Exists(PossibleWebpPicture))
                    {
                        if (!File.Exists(PossiblePngBackgroundPath))
                        {
                            //自动加载当前背景图，因为满足LogicName且并未设置背景图
                            AutoUpdateBackgroundPicture(CurrentGameName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

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
                    ToggleSwitch_ShowWarning.IsOn = false;
                }
                else if (ShowWarningsStr.Trim() == "0")
                {
                    ToggleSwitch_ShowWarning.IsOn = true;
                }
                else
                {
                    ToggleSwitch_ShowWarning.IsOn = false;
                }


                string AnalyseOptions = D3dxIniConfig.ReadAttributeFromD3DXIni(d3dxiniPath, "analyse_options");
                if (AnalyseOptions.Contains("symlink"))
                {
                    ToggleSwitch_Symlink.IsOn = true;
                }
                else
                {
                    ToggleSwitch_Symlink.IsOn = false;
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





        private void SyncD3D11DllFile()
        {
            try
            {
                //这个函数只会在初始化的时候调用，所以默认复制Dev版本的d3d11.dll
                string DllModeFolderName = "ReleaseX64Dev";

                if (ComboBox_DllReplace.SelectedIndex == 1)
                {
                    //如果是Play版本，则复制Play版本的d3d11.dll
                    DllModeFolderName = "ReleaseX64Play";
                }


                string MigotoSourceDll = Path.Combine(PathManager.Path_AssetsFolder, DllModeFolderName + "\\d3d11.dll");


                string MigotoFolder = Path.Combine(GlobalConfig.SSMTCacheFolderPath, "3Dmigoto\\");
                Directory.CreateDirectory(MigotoFolder);

                string TargetCopyDllDir = Path.Combine(MigotoFolder, GlobalConfig.CurrentGameName);



                if (TextBox_3DmigotoPath.Text.Trim() != "")
                {
                    if (Directory.Exists(TextBox_3DmigotoPath.Text))
                    {
                        TargetCopyDllDir = TextBox_3DmigotoPath.Text;
                    }
                }

                string MigotoTargetDll = Path.Combine(TargetCopyDllDir, "d3d11.dll");

                //0是Dev 1是Play 2是None，所以只有0和1时才替换d3d11.dll
                if (ComboBox_DllReplace.SelectedIndex == 0 || ComboBox_DllReplace.SelectedIndex == 1)
                {
                    File.Copy(MigotoSourceDll, MigotoTargetDll, true);
                }
                
            }
            catch(Exception ex)
            {

                _ = SSMTMessageHelper.Show(ex.ToString());
            }
            
        }
   

    



        private void ToggleSwitch_ShowWarning_Toggled(object sender, RoutedEventArgs e)
        {
            if (IsLoading)
            {
                return;
            }

            if (ToggleSwitch_ShowWarning.IsOn)
            {
                D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI,"[Logging]", "show_warnings", "0");
            }
            else
            {
                D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[Logging]", "show_warnings", "1");
            }
        }


      




        private void ToggleSwitch_Symlink_Toggled(object sender, RoutedEventArgs e)
        {
            if (IsLoading)
            {
                return;
            }

            if (!File.Exists(PathManager.Path_D3DXINI))
            {
                _ = SSMTMessageHelper.Show("请先选择正确的3Dmigoto路径，确保d3dx.ini存在于当前选择的3Dmigoto路径下。");
                return;
            }

            //设置symlink特性
            string AnalyseOptions = D3dxIniConfig.ReadAttributeFromD3DXIni(PathManager.Path_D3DXINI, "analyse_options");
            if (AnalyseOptions == "")
            {
                _ = SSMTMessageHelper.Show("当前3Dmigoto的d3dx.ini中暂未设置analyse_options，无法开启symlink特性");
                return;
            }

            if (ToggleSwitch_Symlink.IsOn)
            {
                if (!AnalyseOptions.Contains("symlink"))
                {
                    D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[hunting]", "analyse_options", AnalyseOptions + " symlink");
                }
                _ = SSMTMessageHelper.Show("Symlink特性已开启，游戏中F10刷新即可生效");
            }
            else
            {
                AnalyseOptions = AnalyseOptions.Replace("symlink", " ");
                D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[hunting]", "analyse_options", AnalyseOptions);
                _ = SSMTMessageHelper.Show("Symlink特性已关闭，游戏中F10刷新即可生效");
            }

        }


        public void InstallBasicDllFileTo3DmigotoFolder()
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

			//文件不存在时才复制过去，不然他娘滴这个文件经常被占用，然后SSMT就会复制失败闪退
			if (File.Exists(MigotoSource47Dll) )
            {
                if (!File.Exists(MigotoTarget47Dll)) {
                    File.Copy(MigotoSource47Dll, MigotoTarget47Dll, true);
				}

			}
            else if(File.Exists(MigotoSource46Dll))
            {
                if (!File.Exists(MigotoTarget46Dll)) {
					File.Copy(MigotoSource46Dll, MigotoTarget46Dll, true);
				}
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
            if (IsLoading)
            {
                return;
            }

            GameConfig CurrentGameConfig = new GameConfig();

            //只有路径存在时才进行设置
            if (Directory.Exists(CurrentGameConfig.MigotoPath)) { 
                TextBox_3DmigotoPath.Text = CurrentGameConfig.MigotoPath;
            }

            //如果是具有文件夹层级的路径，则必须判断是否存在
            //如果没有就直接设置，比如有些人会直接填写YuanShen.exe
            if (CurrentGameConfig.TargetPath.Contains("\\"))
            {
                if (File.Exists(CurrentGameConfig.TargetPath))
                {
                    TextBox_TargetPath.Text = CurrentGameConfig.TargetPath;
                }
            }
            else
            {
                TextBox_TargetPath.Text = CurrentGameConfig.TargetPath;
            }

            LOG.Info("尝试设置LaunchPath:");
            if (File.Exists(CurrentGameConfig.LaunchPath))
            {
				LOG.Info("存在保存的LaunchPath:" + CurrentGameConfig.LaunchPath + "  现在进行设置" );
				TextBox_LaunchPath.Text = CurrentGameConfig.LaunchPath;
            }

            TextBox_LaunchArgsPath.Text = CurrentGameConfig.LaunchArgs;

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

        private void ToggleSwitch_AutoSetAnalyseOptions_Toggled(object sender, RoutedEventArgs e)
        {
            if (IsLoading)
            {
                return;
            }

            GameConfig gameConfig = new GameConfig();
            gameConfig.AutoSetAnalyseOptionsSelectedIndex = ComboBox_AutoSetAnalyseOptions.SelectedIndex;
            gameConfig.SaveConfig();
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
    }
}
