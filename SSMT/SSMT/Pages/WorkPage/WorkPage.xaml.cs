using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.Storage.Pickers;
using Windows.Storage;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using CommunityToolkit.WinUI.UI.Controls;
using System.Diagnostics;
using WinUI3Helper;
using SSMT_Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SSMT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WorkPage : Page
    {
        // 定义一个简单的数据模型类
        private ObservableCollection<DrawIBItem> DrawIBItems = new ObservableCollection<DrawIBItem>();
        private ObservableCollection<DrawIBItem> SkipIBItems = new ObservableCollection<DrawIBItem>();
        private ObservableCollection<VSCheckItem> VSCheckItems = new ObservableCollection<VSCheckItem>();

        public WorkPage()
        {
            this.InitializeComponent();

            this.Loaded += OnMyCustomPageLoaded;
        }

        private void OnMyCustomPageLoaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("WorkPageLoadStart");

            // 设置 DataGrid 的数据源
            myDataGrid.ItemsSource = DrawIBItems;
            SkipIBDataGrid.ItemsSource = SkipIBItems;
            DataGrid_VSCheck.ItemsSource = VSCheckItems;

            DrawIBListAddBlankRow();
            SkipIBListAddBlankRow();
            VSCheckAddBlankRow();

            try
            {
                GlobalConfig.ReadConfig();

                //初始化游戏列表，会触发工作空间改变
                InitializeGameNameComboBox();

                Debug.WriteLine("切换到工作空间页面，当前工作空间: " + GlobalConfig.CurrentWorkSpace);
                InitializeWorkSpace(GlobalConfig.CurrentWorkSpace);
            }
            catch (Exception ex)
            {
                _ = MessageHelper.Show(this.XamlRoot, "Error: " + ex.ToString());
            }
            

        }


        private void InitializeGameNameComboBox()
        {
            LOG.NewLine("InitializeGameNameComboBox::Start");
            List<GameIconItem> gameIconItems = SSMTResourceUtils.GetGameIconItems();

            WorkGameSelectionComboBox.Items.Clear();
            foreach (GameIconItem gameIconItem in gameIconItems)
            {
                WorkGameSelectionComboBox.Items.Add(gameIconItem.GameName);
            }

            if (GlobalConfig.CurrentGameName == "")
            {
                WorkGameSelectionComboBox.SelectedIndex = 0;
            }
            else
            {
                WorkGameSelectionComboBox.SelectedItem = GlobalConfig.CurrentGameName;
            }
            LOG.NewLine("InitializeGameNameComboBox::End");

        }





        private async void WorkGameSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LOG.Info("WorkGameSelectionComboBox_SelectionChanged::Start");
            // 获取触发事件的 ComboBox 实例
            var comboBox = sender as ComboBox;

            if (comboBox == null || comboBox.SelectedItem == null)
            {
                return;
            }

            //获取选中的项并进行处理
            string selectedGame = comboBox.SelectedItem.ToString();
            GlobalConfig.CurrentGameName = selectedGame;

            //读取并设置当前3Dmigoto文件夹，如果没读取到则弹出对话框提醒。
            GameConfig gameConfig = new GameConfig();

            

            if (!Directory.Exists(gameConfig.MigotoPath))
            {
                await MessageHelper.Show(this.XamlRoot, "您当前选中的游戏尚未设置3Dmigoto文件夹，请到主页进行设置。");

  
                Frame.Navigate(typeof(HomePage));
            }

            GlobalConfig.SaveConfig();


            //切换游戏后，要读取当前游戏的配置，来确定当前选择的是哪个工作空间
            //如果没有就算了

            string SavedWorkSpace = gameConfig.WorkSpace;
            if (SavedWorkSpace == "")
            {
                LOG.Info("上次保存的工作空间为空，初始化空的工作空间");
                InitializeWorkSpace();
            }
            else
            {
                string TargetWorkSpaceFolder = Path.Combine(GlobalConfig.Path_TotalWorkSpaceFolder, GlobalConfig.CurrentGameName + "\\" + SavedWorkSpace + "\\");
                if (Directory.Exists(TargetWorkSpaceFolder))
                {
                    InitializeWorkSpace(SavedWorkSpace);
                }
                else
                {
                    LOG.Info("不存在工作空间文件夹，初始化空的工作空间");
                    InitializeWorkSpace();
                }
            }

            //切换游戏时，检测当前总工作空间目录下是否有文件夹，没有就新建
            string[] CurrentGameWorkSpaceList = Directory.GetDirectories(GlobalConfig.Path_CurrentGameTotalWorkSpaceFolder);
            if (CurrentGameWorkSpaceList.Length == 0)
            {
                CreateNewWorkSpace("Default");
            }
            else
            {
                LOG.Info("当前游戏工作空间列表: " + CurrentGameWorkSpaceList.Length.ToString());
            }

            InitializeGameTypeComboBox();

            
            if (gameConfig.LogicName == LogicName.ZZMI)
            {
                StackPanel_GameTypeRegion.Visibility = Visibility.Visible;
            }
            else
            {
                StackPanel_GameTypeRegion.Visibility = Visibility.Collapsed;
            }
            Debug.WriteLine("WorkGameSelectionComboBox_SelectionChanged::End");

        }

        private void InitializeGameTypeComboBox()
        {
            ComboBox_GameType.Items.Clear();
            ComboBox_GameType.Items.Add("Auto");
            ComboBox_GameType.SelectedIndex = 0;

            if (!Directory.Exists(GlobalConfig.Path_CurrentGame_GameTypeFolder))
            {
                return;
            }

            string[] FilePathList = Directory.GetFiles(GlobalConfig.Path_CurrentGame_GameTypeFolder);
            foreach (string FilePath in FilePathList)
            {
                string FileName = Path.GetFileNameWithoutExtension(FilePath);
                ComboBox_GameType.Items.Add(FileName);
            }
        }



        //工作空间改变时执行的方法
        private void ComboBoxWorkSpaceSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("切换工作空间::Start");
            if (ComboBoxWorkSpaceSelection.Items.Contains(ComboBoxWorkSpaceSelection.SelectedItem))
            {
                //切换游戏前，保存当前的工作空间
                GlobalConfig.CurrentWorkSpace = (string)ComboBoxWorkSpaceSelection.SelectedItem;
                GlobalConfig.SaveConfig();

                //并且要把当前工作空间保存到当前游戏的配置里
                GameConfig gameConfig = new GameConfig();
                gameConfig.WorkSpace = GlobalConfig.CurrentWorkSpace;
                gameConfig.SaveConfig();

                ReadDrawIBListFromWorkSpace();
                ReadSkipIBListFromWorkSpace();
                ReadVSCheckListFromWorkSpace();
            }
            Debug.WriteLine("切换工作空间::End");

        }





        public void InitializeWorkSpace(string WorkSpaceName = "")
        {
            Debug.WriteLine("初始化工作空间::Start");
            Debug.WriteLine("工作空间名称: " + WorkSpaceName);
            GlobalConfig.CurrentWorkSpace = WorkSpaceName;

            if (!Directory.Exists(GlobalConfig.Path_CurrentWorkSpaceFolder))
            {
                Debug.WriteLine("创建工作空间文件夹: " + GlobalConfig.Path_CurrentWorkSpaceFolder);
                Directory.CreateDirectory(GlobalConfig.Path_CurrentWorkSpaceFolder);
            }

            ComboBoxWorkSpaceSelection.Items.Clear();

            string[] WorkSpaceNameList = DBMTFileUtils.ReadWorkSpaceNameList(GlobalConfig.Path_CurrentGameTotalWorkSpaceFolder);
            foreach (string WorkSpaceNameItem in WorkSpaceNameList)
            {
                ComboBoxWorkSpaceSelection.Items.Add(WorkSpaceNameItem);
            }

            if (ComboBoxWorkSpaceSelection.Items.Count >= 1)
            {
                if (WorkSpaceName != "" && ComboBoxWorkSpaceSelection.Items.Contains(WorkSpaceName))
                {
                    ComboBoxWorkSpaceSelection.SelectedItem = WorkSpaceName;
                }
                //判断当前WorkSpace是否在Items里，如果在的话就设为当前工作空间
                else if (ComboBoxWorkSpaceSelection.Items.Contains(GlobalConfig.CurrentWorkSpace))
                {
                    ComboBoxWorkSpaceSelection.SelectedItem = GlobalConfig.CurrentWorkSpace;
                }
                else
                {
                    //否则默认选择第一个工作空间
                    Debug.WriteLine("默认选择第一个工作空间");
                    ComboBoxWorkSpaceSelection.SelectedItem = ComboBoxWorkSpaceSelection.Items[0];
                }
                //ReadDrawIBListFromWorkSpace();
            }

            Debug.WriteLine("初始化工作空间::End");

        }


        public void CreateNewWorkSpace(string WorkSpaceName)
        {
            ////如果包含了此命名空间，就不创建文件夹，否则就创建
            if (!ComboBoxWorkSpaceSelection.Items.Contains(WorkSpaceName))
            {
                string NewWorkSpaceFolderPath = Path.Combine(GlobalConfig.Path_CurrentGameTotalWorkSpaceFolder, WorkSpaceName + "\\");
                Directory.CreateDirectory(NewWorkSpaceFolderPath);
                
                InitializeWorkSpace(WorkSpaceName);
            }
        }

        public async void CreateWorkSpaceFolder(object sender, RoutedEventArgs e)
        {
            if (ComboBoxWorkSpaceSelection.Text.Trim() == "")
            {
                await SSMTMessageHelper.Show("工作空间名称不能为空","WorkSpace name can't be empty.");
                return;
            }

            ////如果包含了此命名空间，就不创建文件夹，否则就创建
            if (!ComboBoxWorkSpaceSelection.Items.Contains(ComboBoxWorkSpaceSelection.Text))
            {
                CreateNewWorkSpace(ComboBoxWorkSpaceSelection.Text);
                await SSMTMessageHelper.Show("工作空间创建成功");
            }
            else
            {
                await SSMTMessageHelper.Show("当前工作空间已存在,无法重复创建");
            }
        }

        public async void CleanCurrentWorkSpaceFile(object sender, RoutedEventArgs e)
        {
            try
            {
                bool confirm = await SSMTMessageHelper.ShowConfirm("请再次确认是否清除当前工作空间","Please confirm if you want to clean WorkSpace");
                if (confirm)
                {
                    string WorkSpaceFolderPath = GlobalConfig.Path_CurrentGameTotalWorkSpaceFolder + ComboBoxWorkSpaceSelection.Text;
                    Directory.Delete(WorkSpaceFolderPath, true);
                    Directory.CreateDirectory(WorkSpaceFolderPath);
                    InitializeWorkSpace(GlobalConfig.CurrentWorkSpace);
                    _ = SSMTMessageHelper.Show("清理成功", "Clean Success");
                }
                
            }
            catch(Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
           
        }



        public async void CleanDrawIBList(object sender, RoutedEventArgs e)
        {
            bool confirm = await SSMTMessageHelper.ShowConfirm("请再次确认是否清除当前DrawIB列表","Please confirm if you want to clean DrawIB list");
            if (confirm)
            {
                DrawIBItems.Clear();
                DrawIBListAddBlankRow();
            }
        }

        public bool PreCheckBeforeExtract()
        {
            bool findvalidDrawIB = false;
            foreach (DrawIBItem item in DrawIBItems)
            {
                if (item.DrawIB.Trim() != "")
                {
                    findvalidDrawIB = true;
                    break;
                }
            }

            if (!findvalidDrawIB)
            {
                _ = SSMTMessageHelper.Show("在运行之前请填写您的绘制IB的哈希值并进行配置", "Please fill your DrawIB and config it before run.");
                return false;
            }

            if (ComboBoxWorkSpaceSelection.Text.Trim() == "")
            {
                _ = SSMTMessageHelper.Show("请先指定工作空间");
                return false;
            }

            return true;
        }

        private void AutoDetectPointlistDrawIBList_UnityVSPreSkinning(object sender, RoutedEventArgs e)
        {

            try
            {
                CoreFunctions.DetectPointlistDrawIBList();
                InitializeWorkSpace(GlobalConfig.CurrentWorkSpace);
                _ = SSMTMessageHelper.Show("检测Pointlist DrawIB列表成功", "Read DrawIB List Success");
            }

            catch(Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }

            
        }



        public void ExtractModel(object sender, RoutedEventArgs e)
        {
            //初始化日志类
            LOG.Initialize(GlobalConfig.Path_LogsFolder);

            try
            {
                bool Prepare = PreCheckBeforeExtract();
                if (!Prepare)
                {
                    return;
                }
                SaveDrawIBList();


                //检查是否存在FrameAnalysis文件夹
                List<string> frameAnalysisFileList = SSMTResourceUtils.GetFrameAnalysisFileNameList();
                if (frameAnalysisFileList.Count == 0)
                {
                    _ = SSMTMessageHelper.Show("您的当前3Dmigoto文件夹下面没有检测到任何FrameAnalysis文件夹，请先到游戏里F8 Dump，如果你确实F8 Dump了，请再次检查确保3Dmigoto路径选择正确。");
                    return;
                }

                GameConfig gameConfig = new GameConfig();
                LOG.Info("游戏名称: " + GlobalConfig.CurrentGameName);
                LOG.Info("执行逻辑名称: " + gameConfig.LogicName);
                LOG.Info("数据类型文件夹名称: " + gameConfig.GameTypeName);
                LOG.NewLine();

                List<DrawIBItem> DrawIBItemList = [];
                foreach (DrawIBItem drawIBItem in DrawIBItems)
                {
                    DrawIBItemList.Add(drawIBItem);
                }

                string GameType = ComboBox_GameType.SelectedItem.ToString();

                bool RunResult = CoreFunctions.ExtractModel(DrawIBItemList);
                if (RunResult)
                {
                    

                    LOG.Info("PostDoAfterExtract:");
                    CoreFunctions.PostDoAfterExtract();


                    LOG.Info("OpenCurrentWorkSpaceFolder:");

                    LOG.SaveFile(GlobalConfig.Path_LogsFolder);

                    OpenCurrentWorkSpaceFolder(sender, e);

                }
                else
                {
                    LOG.SaveFile(GlobalConfig.Path_LogsFolder);
                    OpenLatestLogFile(sender, e);
                }
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show("Error: " + ex.ToString());
            }
            

        }



        public async void CleanSkipIBListTextBox(object sender, RoutedEventArgs e)
        {
            bool confirm = await SSMTMessageHelper.ShowConfirm("请再次确认是否清除当前SkipIB列表","Please confirm if you want to clean SkipIB list");
            if (confirm)
            {
                SkipIBItems.Clear();
                SkipIBListAddBlankRow();
            }
        }

        private List<string> GetSkipIBList()
        {
            List<string> SkipIBList = new List<string>();

            foreach (DrawIBItem item in SkipIBItems)
            {
                if (item.DrawIB.Trim() != "")
                {
                    SkipIBList.Add(item.DrawIB);
                }
            }
            return SkipIBList; 
        }

        private List<DrawIBItem> GetSkipIBItemList()
        {
            List<DrawIBItem> SkipIBList = new List<DrawIBItem>();

            foreach (DrawIBItem item in SkipIBItems)
            {
                if (item.DrawIB.Trim() != "")
                {
                    SkipIBList.Add(item);
                }
            }
            return SkipIBList;
        }

        public void ExecuteSkipIB(object sender, RoutedEventArgs e)
        {

            List<DrawIBItem> SkipIBItemList = GetSkipIBItemList();
            GenerateSkipIB(SkipIBItemList);
            SSMTCommandHelper.ShellOpenFolder(GlobalConfig.Path_CurrentWorkSpaceGeneratedModFolder);
        }


    

        private void Menu_AutoDetect_CPUPreSkinning_DrawIBList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CoreFunctions.DetectTrianglelistDrawIBList();
                InitializeWorkSpace(GlobalConfig.CurrentWorkSpace);
                _ = SSMTMessageHelper.Show("检测Trianglelist DrawIB列表成功", "Read DrawIB List Success");
            }
            catch (Exception ex) {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }

        }

        private void Menu_ExtractTextureFiles_Click(object sender, RoutedEventArgs e)
        {
            SaveDrawIBList();
            CoreFunctions.ExtractTextures();
            SSMTTextureHelper.ConvertDedupedTexturesToTargetFormat();
            SSMTCommandHelper.ShellOpenFolder(GlobalConfig.Path_CurrentWorkSpaceFolder);
        }

        private async void Menu_ConvertDDSToTargetFormat_Click(object sender, RoutedEventArgs e)
        {
            string selected_folder_path = await SSMTCommandHelper.ChooseFolderAndGetPath();
            if (selected_folder_path == "")
            {
                return;
            }

            try
            {
                SSMTTextureHelper.ConvertAllTexturesIntoConvertedTextures(selected_folder_path);

                SSMTCommandHelper.ShellOpenFolder(selected_folder_path + "\\ConvertedTextures\\");

            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }


        }

        private void Menu_ExtractDedupedTextures_Click(object sender, RoutedEventArgs e)
        {
            SaveDrawIBList();
            CoreFunctions.ExtractDedupedTextures();
            SSMTTextureHelper.ConvertDedupedTexturesToTargetFormat();
            SSMTCommandHelper.ShellOpenFolder(GlobalConfig.Path_CurrentWorkSpaceFolder);
        }

        private void Menu_ExtractTrianglelistTextures_Click(object sender, RoutedEventArgs e)
        {
            SaveDrawIBList();
            CoreFunctions.ExtractTrianglelistTextures();
            SSMTCommandHelper.ShellOpenFolder(GlobalConfig.Path_CurrentWorkSpaceFolder);
        }

        private void Menu_ExtractRenderTextures_Click(object sender, RoutedEventArgs e)
        {
            SaveDrawIBList();
            CoreFunctions.ExtractRenderTextures();
            SSMTCommandHelper.ShellOpenFolder(GlobalConfig.Path_CurrentWorkSpaceFolder);
        }

        private void Menu_OpenPluginsFolder_Click(object sender, RoutedEventArgs e)
        {
            SSMTCommandHelper.ShellOpenFolder(GlobalConfig.Path_PluginsFolder);
        }





        private void Menu_GameTypeFolder_Click(object sender, RoutedEventArgs e)
        {
            string CurrentGameTypeFolder = Path.Combine(GlobalConfig.Path_GameTypeConfigsFolder, GlobalConfig.CurrentGameName + "\\");
            if (Directory.Exists(CurrentGameTypeFolder))
            {
                SSMTCommandHelper.ShellOpenFolder(CurrentGameTypeFolder);
            }
        }


        private async void Button_SaveDrawIBList_Click(object sender, RoutedEventArgs e)
        {
            SaveDrawIBList();
            await SSMTMessageHelper.Show("保存成功");
        }



        private void Button_CleanLastExtract_Click(object sender, RoutedEventArgs e)
        {
            List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();
            try
            {
                foreach (string DrawIB in DrawIBList)
                {
                
                        string DrawIBOutputFolder = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\");
                        if (Directory.Exists(DrawIBOutputFolder))
                        {
                            Directory.Delete(DrawIBOutputFolder,true);
                        }

               
                }

                _ = SSMTMessageHelper.Show("清理完成");
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show("清理失败，文件可能被进程占用: " + ex.ToString());
            }
        }



    
        private void Button_SaveSkipIBList_Click(object sender, RoutedEventArgs e)
        {
            SaveSkipIBList();
            _ = SSMTMessageHelper.Show("保存成功");
        }


 



        private void Button_AutoDetectGameTypeDrawIBList_Click(object sender, RoutedEventArgs e)
        {
            LOG.Initialize(GlobalConfig.Path_LogsFolder);

            try
            {
                string GameType = ComboBox_GameType.SelectedItem.ToString();
                //1.获取所有可能的DrawIB
                List<string> TotalDrawIBList = new List<string>();
                List<string> IBFileNameList = FrameAnalysisDataUtils.FilterFrameAnalysisFile(GlobalConfig.WorkFolder, "-ib=", ".buf");
                foreach (string IBFileName in IBFileNameList)
                {
                    string DrawIB = IBFileName.Substring(10, 8);
                    if (!TotalDrawIBList.Contains(DrawIB))
                    {
                        TotalDrawIBList.Add(DrawIB);
                    }
                }

                GameConfig gameConfig = new GameConfig();

                 
                D3D11GameTypeLv2 d3D11GameTypeLv2 = new D3D11GameTypeLv2(gameConfig.GameTypeName);
                List<string> MatchedDrawIBList = new List<string>();

                foreach (string DrawIB in TotalDrawIBList)
                {
                    LOG.Info("当前DrawIB: " + DrawIB);
                    string PointlistIndex = FrameAnalysisLogUtilsV2.Get_PointlistIndex_ByHash(DrawIB, GlobalConfig.Path_LatestFrameAnalysisLogTxt);
                    LOG.Info("当前识别到的PointlistIndex: " + PointlistIndex);
                    if (PointlistIndex == "")
                    {
                        LOG.Info("当前识别到的PointlistIndex为空，此DrawIB对应的模型可能为CPU-PreSkinning类型。");
                    }
                    LOG.NewLine();

                    List<string> TrianglelistIndexList = FrameAnalysisLogUtilsV2.Get_DrawCallIndexList_ByHash(DrawIB, false, GlobalConfig.Path_LatestFrameAnalysisLogTxt);
                    foreach (string TrianglelistIndex in TrianglelistIndexList)
                    {
                        LOG.Info("TrianglelistIndex: " + TrianglelistIndex);
                    }
                    LOG.NewLine();

                    List<D3D11GameType> d3D11GameTypeList = ZenlessZoneZero.GetPossibleGameTypeList_UnityVS(DrawIB, PointlistIndex, TrianglelistIndexList, d3D11GameTypeLv2);
                    foreach (D3D11GameType d3D11GameType in d3D11GameTypeList)
                    {
                        if (d3D11GameType.GameTypeName == GameType)
                        {

                            MatchedDrawIBList.Add(DrawIB);
                            break;
                        }
                    }
                }

                LOG.Info("识别到的DrawIB列表:");
                //保存到Json文件
                JArray jArray = new JArray();
                foreach (string IB in MatchedDrawIBList)
                {
                    JObject jObject = DBMTJsonUtils.CreateJObject();
                    jObject["DrawIB"] = IB;
                    jObject["Alias"] = "";
                    jArray.Add(jObject);
                    LOG.Info(IB);
                }

                DBMTJsonUtils.SaveJObjectToFile(jArray, GlobalConfig.Path_CurrentWorkSpaceFolder + "Config.json");

                InitializeWorkSpace(GlobalConfig.CurrentWorkSpace);

                LOG.SaveFile(GlobalConfig.Path_LogsFolder);

                _ = SSMTMessageHelper.Show("识别完成，从" +TotalDrawIBList.Count.ToString() + "个DrawIB中，共识别到: " + MatchedDrawIBList.Count.ToString()  + "个DrawIB，已自动填写到DrawIB列表");
            }
            catch (Exception ex)
            {
                LOG.SaveFile(GlobalConfig.Path_LogsFolder);
                _ = SSMTMessageHelper.Show(ex.ToString());
            }

            
        }

        private async void Button_DeleteCurrentWorkSpaceFolder_Click(object sender, RoutedEventArgs e)
        {
            bool confirm = await SSMTMessageHelper.ShowConfirm("请再次确认是否删除当前工作空间", "Please confirm if you want to delete current WorkSpace");
            if (!confirm)
            {
                return;
            }

            try
            {
                string WorkSpaceOutputFolder = GlobalConfig.Path_CurrentGameTotalWorkSpaceFolder + ComboBoxWorkSpaceSelection.Text + "\\";
                if (!string.IsNullOrEmpty(WorkSpaceOutputFolder))
                {
                    if (Directory.Exists(WorkSpaceOutputFolder))
                    {
                        Directory.Delete(WorkSpaceOutputFolder, true);
                        //删除当前工作空间后，需要切换页面
                        Frame.Navigate(typeof(WorkPage));
                    }
                    else
                    {
                        _ = SSMTMessageHelper.Show("此目录不存在，请检查您的工作空间是否设置正确", "This folder doesn't exists,please check if your WorkSpace is correct.");
                    }
                }
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show("删除工作空间失败: " + ex.ToString());
            }
            
        }

        private void SetD3dxConfig_DumpSpecificIBListConfig()
        {
            //根据IB列表生成特定的Dump配置
            List<string> DumpIBListConfigIniList = new List<string>();

            //根据游戏给出指定VS的Dump配置
            foreach (DrawIBItem item in DrawIBItems)
            {
                if (item.DrawIB.Trim() != "")
                {

                    if (item.DrawIB.Trim() != "")
                    {
                        DumpIBListConfigIniList.Add("[TextureOverride_IB_Dump_" + item.DrawIB + "]");
                        DumpIBListConfigIniList.Add("hash = " + item.DrawIB);
                        DumpIBListConfigIniList.Add("analyse_options = " + GlobalConfig.analyse_options);
                        DumpIBListConfigIniList.Add("");
                    }

                }
            }

            GameConfig gameConfig = new GameConfig();
            List<string> RootShaderList = new List<string>();

            if (gameConfig.LogicName == LogicName.SRMI)
            {
                RootShaderList.Add("1c932707d4d8df41");
                RootShaderList.Add("fee307b98a965c16");
                RootShaderList.Add("4d9c23fd387846c7");
                RootShaderList.Add("d50694eedd2a8595");
                RootShaderList.Add("c9f2b46571d22858");
            }
            else if (gameConfig.LogicName == LogicName.GIMI)
            {
                RootShaderList.Add("653c63ba4a73ca8b");
            }
            else if (gameConfig.LogicName == LogicName.ZZMI)
            {
                RootShaderList.Add("e8425f64cfb887cd");
                RootShaderList.Add("a0b21a8e787c5a98");
                RootShaderList.Add("9684c4091fc9e35a");
            }
            else if (gameConfig.LogicName == LogicName.CTXMC || gameConfig.LogicName == LogicName.IdentityV2)
            {
                LOG.Info("第五人格没有用到前置VS，当场计算当场渲染的");
            }
            else
            {
                _ = SSMTMessageHelper.Show("当前LogicName执行逻辑" + gameConfig.LogicName + "不支持此设置，请去主页切换您的执行逻辑后重试");
                return;
            }

            foreach (string RootShader in RootShaderList)
            {
                DumpIBListConfigIniList.Add("[ShaderOverride_Dump_" + RootShader + "]");
                DumpIBListConfigIniList.Add("hash = " + RootShader);
                //DumpIBListConfigIniList.Add("allow_duplicate_hash = overrule");
                DumpIBListConfigIniList.Add("analyse_options = " + GlobalConfig.analyse_options);
                DumpIBListConfigIniList.Add("");
            }

           
            File.WriteAllLines(GlobalConfig.Path_DumpIBListConfig, DumpIBListConfigIniList);
            //string analyse_options = "deferred_ctx_immediate dump_rt dump_cb dump_vb dump_ib buf txt dds dump_tex dds";
            //全局Dump设为空
            D3dxIniConfig.SaveAttributeToD3DXIni(GlobalConfig.Path_D3DXINI, "[hunting]", "analyse_options", "");
        }

        

        private void SetD3dxConfig_RecoverGlobalDumpConfig()
        {
  
            if (File.Exists(GlobalConfig.Path_DumpIBListConfig))
            {
                File.Delete(GlobalConfig.Path_DumpIBListConfig);
            }

            D3dxIniConfig.SaveAttributeToD3DXIni(GlobalConfig.Path_D3DXINI, "[hunting]", "analyse_options", GlobalConfig.analyse_options);
        }
       

        private void Menu_SpecificIBListConfig_Click(object sender, RoutedEventArgs e)
        {
            SetD3dxConfig_DumpSpecificIBListConfig();
        }

        private void Menu_RecoverGlobalDumpConfig_Click(object sender, RoutedEventArgs e)
        {
            SetD3dxConfig_RecoverGlobalDumpConfig();
        }

        private void ToggleSwitch_DumpIBListConfig_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ToggleSwitch_DumpIBListConfig.IsOn)
                {
                    //读取d3dx.ini
                    MigotoIniFile migotoIniFile = new MigotoIniFile(GlobalConfig.Path_D3DXINI);

                    //关闭include_recursive = Mods
                    migotoIniFile.ReplaceSelf_IniSectionIniLine("include", "include_recursive = Mods", ";include_recursive = Mods");
                    //新增include_recursive = Mods\DumpIBListConfig.ini
                    migotoIniFile.ReplaceSelf_IniSectionIniLine("include", ";include = DumpIBListConfig.ini", "include = DumpIBListConfig.ini");
                    migotoIniFile.ReplaceSelf_AddNewLineIfNotExists("include", "include = DumpIBListConfig.ini");
                    //暂时先存到这里，测试通过再存d3dx.ini
                    migotoIniFile.SaveSelf();

                    //清除全局Dump
                    D3dxIniConfig.SaveAttributeToD3DXIni(GlobalConfig.Path_D3DXINI, "[hunting]", "analyse_options", "");

                    //生成特定Dump配置
                    SetD3dxConfig_DumpSpecificIBListConfig();

                    _ = SSMTMessageHelper.Show("已成功禁用Mods文件夹，并启用特定IB列表Dump配置");
                }
                else
                {
                    //移除特定Dump配置
                    string DumpIBListConfigIniFilePath = Path.Combine(GlobalConfig.Path_ModsFolder, "DumpIBListConfig.ini");
                    if (File.Exists(DumpIBListConfigIniFilePath))
                    {
                        File.Delete(DumpIBListConfigIniFilePath);
                    }

                    //恢复全局Dump
                    D3dxIniConfig.SaveAttributeToD3DXIni(GlobalConfig.Path_D3DXINI, "[hunting]", "analyse_options", GlobalConfig.analyse_options);

                    //恢复Mods引用并取消特定Dump引用
                    MigotoIniFile migotoIniFile = new MigotoIniFile(GlobalConfig.Path_D3DXINI);
                    //开启include_recursive = Mods
                    migotoIniFile.ReplaceSelf_IniSectionIniLine("include", ";include_recursive = Mods", "include_recursive = Mods");
                    migotoIniFile.ReplaceSelf_AddNewLineIfNotExists("include", "include_recursive = Mods");
                    //新增include_recursive = Mods\DumpIBListConfig.ini
                    migotoIniFile.ReplaceSelf_IniSectionIniLine("include", "include = DumpIBListConfig.ini", ";include = DumpIBListConfig.ini");
                    //暂时先存到这里，测试通过再存d3dx.ini
                    migotoIniFile.SaveSelf();

                    _ = SSMTMessageHelper.Show("已成功启用Mods文件夹，并恢复全局Dump配置");
                }
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
        }

        
    }
}
