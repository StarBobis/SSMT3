using CommunityToolkit.WinUI.Animations;
using DirectXTexNet;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json.Linq;
using SSMT_Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using static System.Net.Mime.MediaTypeNames;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SSMT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TexturePage : Page
    {

        private ObservableCollection<ImageItem> imageCollection = new ObservableCollection<ImageItem>();

        public TexturePage()
        {
            this.InitializeComponent();

            GameConfig gameConfig = new GameConfig();


            if (gameConfig.LogicName == LogicName.ZZMI)
            {
                ZZMI_MenuListBox.Visibility = Visibility.Visible;
                Default_MenuListBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                ZZMI_MenuListBox.Visibility = Visibility.Collapsed;
                Default_MenuListBox.Visibility = Visibility.Visible;
            }


            // 设置ListView的数据源为imageCollection
            ImageListView.ItemsSource = imageCollection;

            //至少要知道当前DrawIB列表，读取后自动触发Component对应的CallIndex的贴图读取
            ReadDrawIBList();


            ComboBox_MarkName.SelectedIndex = 0;
           
        }

        public void ReadDrawIBList()
        {
            ComboBoxDrawIB.Items.Clear();
            List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();
            foreach (string DrawIB in DrawIBList)
            {
                ComboBoxDrawIB.Items.Add(DrawIB);
            }

            //如果一个DrawIB都没有，就算了
            if (ComboBoxDrawIB.Items.Count == 0)
            {
                return;
            }

            TexturePageIndexConfig texturePageIndexConfig = new TexturePageIndexConfig();
            texturePageIndexConfig.ReadConfig();
            if (texturePageIndexConfig.DrawIBIndex < 0)
            {
                ComboBoxDrawIB.SelectedIndex = 0;
            }
            else
            {
                if (texturePageIndexConfig.DrawIBIndex < ComboBoxDrawIB.Items.Count)
                {
                    ComboBoxDrawIB.SelectedIndex = texturePageIndexConfig.DrawIBIndex;
                }
                else
                {
                    ComboBoxDrawIB.SelectedIndex = 0;
                }
            }

            //texturePageIndexConfig.DrawIBIndex = ComboBoxDrawIB.SelectedIndex;
            //texturePageIndexConfig.SaveConfig();

        }

        private void ComboBoxDrawIB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("ComboBoxDrawIB_SelectionChanged::Begin");

            string DrawIB = ComboBoxDrawIB.SelectedItem.ToString();


            //根据DrawIB获取当前DrawIB的名称
            List<DrawIBPair> drawIBPairList = WorkSpaceHelper.GetDrawIBPairListFromWorkSpaceConfig();
            foreach (DrawIBPair drawIBPair in drawIBPairList)
            {
                if (drawIBPair.DrawIB == DrawIB)
                {
                    ComboBoxDrawIB.Header = "DrawIB " + drawIBPair.Alias;
                    break;
                }
            }

            Dictionary<string, List<string>> ComponentName_DrawCallIndexList_Dict = DrawIBConfig.Get_ComponentName_DrawCallIndexList_Dict_FromJson(DrawIB);

            if (ComponentName_DrawCallIndexList_Dict.Count == 0)
            {
                return;
            }

            ComboBoxComponent.Items.Clear();
            foreach (var item in ComponentName_DrawCallIndexList_Dict)
            {
                ComboBoxComponent.Items.Add(item.Key);
            }

            TexturePageIndexConfig texturePageIndexConfig = new TexturePageIndexConfig();
            texturePageIndexConfig.ReadConfig();
            if (texturePageIndexConfig.ComponentIndex < 0)
            {
                ComboBoxComponent.SelectedIndex = 0;
            }
            else
            {
                if (texturePageIndexConfig.ComponentIndex < ComboBoxComponent.Items.Count)
                {
                    ComboBoxComponent.SelectedIndex = texturePageIndexConfig.ComponentIndex;
                }
                else
                {
                    ComboBoxComponent.SelectedIndex = 0;
                }
            }

            Debug.WriteLine("ComboBoxDrawIB_SelectionChanged::End");
        }

        private void ComboBoxComponent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("ComboBoxComponent_SelectionChanged::Begin");
            if (ComboBoxComponent.SelectedIndex < 0)
            {
                return;
            }
            
            string DrawIB = ComboBoxDrawIB.SelectedItem.ToString();
            string ComponentName = ComboBoxComponent.SelectedItem.ToString();
            List<string> DrawCallIndexList = DrawIBConfig.Read_DrawCallIndexList(DrawIB, ComponentName);

            ComboBoxDrawCall.Items.Clear();
            foreach (string DrawCallIndex in DrawCallIndexList)
            {
                ComboBoxDrawCall.Items.Add(DrawCallIndex);
            }

            if (ComboBoxDrawCall.Items.Count == 0)
            {
                LOG.Info("无任何DrawCall，结束");
                return;
            }
            else
            {
                LOG.Info("DrawCall数量: " + ComboBoxDrawCall.Items.Count.ToString());
            }


            TexturePageIndexConfig texturePageIndexConfig = new TexturePageIndexConfig();
            texturePageIndexConfig.ReadConfig();


            if (ComboBoxDrawCall.Items.Count > 1)
            {
                if (texturePageIndexConfig.DrawCallIndex < 0)
                {
                    LOG.Info("上次并未记忆DrawCall选中项，默认选第一个DrawCall");
                    ComboBoxDrawCall.SelectedIndex = 0;
                }
                else
                {
                    if (texturePageIndexConfig.DrawCallIndex < ComboBoxDrawCall.Items.Count)
                    {
                        LOG.Info("选中了上次记忆的DrawCall选项");
                        ComboBoxDrawCall.SelectedIndex = texturePageIndexConfig.DrawCallIndex;
                    }
                    else
                    {
                        LOG.Info("上次记忆DrawCall选中项数量比现在的多，默认选第一个DrawCall");
                        ComboBoxDrawCall.SelectedIndex = 0;
                    }
                }


               

            }
            else if(ComboBoxDrawCall.Items.Count == 1)
            { 
                //只有一个的情况下只能选0啊
                ComboBoxDrawCall.SelectedIndex = 0;
            }

            

            Debug.WriteLine("ComboBoxComponent_SelectionChanged::End");
        }

        private void ComboBoxDrawCall_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("ComboBoxDrawCall_SelectionChanged::Begin");
            if (ComboBoxDrawCall.SelectedItem == null)
            {
                Debug.WriteLine("Select Nothing, Skip!");
                return;
            }
            Debug.WriteLine("Load New Config");


            string DrawCallIndex = ComboBoxDrawCall.SelectedItem.ToString();
            string DrawIB = ComboBoxDrawIB.SelectedItem.ToString();
            
          

            List<ImageItem> ImageList = TextureConfig.Read_ImageItemList(DrawIB,DrawCallIndex );
            

            Debug.WriteLine("ImageList Size: " + ImageList.Count.ToString());

            // 清空当前的贴图集合，加入新的信息以在GUI中显示所有贴图信息供操作。
            imageCollection.Clear();
            foreach (ImageItem item in ImageList)
            {
                imageCollection.Add(item);
            }

            ReadCurrentTextureConfig();


            TexturePageIndexConfig texturePageIndexConfig = new TexturePageIndexConfig();

            texturePageIndexConfig.DrawIBIndex = ComboBoxDrawIB.SelectedIndex;
            texturePageIndexConfig.ComponentIndex = ComboBoxComponent.SelectedIndex;
            texturePageIndexConfig.DrawCallIndex = ComboBoxDrawCall.SelectedIndex;

            LOG.Info("保存当前选中项: DrawIBIndex:" + texturePageIndexConfig.DrawIBIndex.ToString());
            LOG.Info("保存当前选中项: ComponentIndex:" + texturePageIndexConfig.ComponentIndex.ToString());
            LOG.Info("保存当前选中项: DrawCallIndex:" + texturePageIndexConfig.DrawCallIndex.ToString());
            texturePageIndexConfig.SaveConfig();

            Debug.WriteLine("ComboBoxDrawCall_SelectionChanged::End");
        }

        public string GetCurrentPSValue()
        {
            string PSHashValue = "";
            if (imageCollection.Count != 0)
            {
                PSHashValue = DBMTStringUtils.GetPSHashFromFileName(imageCollection[0].FileName);
            }
            return PSHashValue;
        }


        private void ReadCurrentTextureConfig()
        {
            Debug.WriteLine("Read Current Texture Config::");
            string TextureConfigName = GetCurrentPSValue();
            if (TextureConfigName == "")
            {
                Debug.WriteLine("TextureConfigName is empty, skip reading texture config.");
                return;
            }
            LOG.Info("PS: " + TextureConfigName);

            Debug.WriteLine("TextureConfigName: " + TextureConfigName);
            string TextureConfigSavePath = PathManager.Path_GameTextureConfigFolder + TextureConfigName + ".json";
            Debug.WriteLine("TextureConfigSavePath: " + TextureConfigSavePath);
            if (File.Exists(TextureConfigSavePath))
            {
                Dictionary<string, SlotObject> PixeSlot_MarkName_Dict = TextureConfig.Read_PixelSlot_SlotObject_Dict(TextureConfigSavePath);

                Debug.WriteLine("Count: " + imageCollection.Count.ToString());
                for (int i = 0; i < imageCollection.Count; i++)
                {
                    ImageItem imageItem = imageCollection[i];
                    if(PixeSlot_MarkName_Dict.ContainsKey(imageItem.PixelSlot)){
                        SlotObject slot_obj = PixeSlot_MarkName_Dict[imageItem.PixelSlot];
                        imageItem.MarkName = slot_obj.MarkName;
                        imageItem.MarkStyle = slot_obj.MarkStyle;
                    }

                    imageCollection[i] = imageItem;

                    Debug.WriteLine(imageCollection[i].MarkName);

                }
            }
            else
            {
                Debug.WriteLine("TextureConfigSavePath doesn't exists: " + TextureConfigSavePath);
            }

        }

        public void SaveCurrentTextureConfig()
        {
            //贴图标记的名称，就是当前DrawCall的Index的对应PS的Hash值
            //由于对于一个DrawCall的Index来说，各个槽位上的PS名称都是一样的，所以我们可以直接获取第一个贴图的名称来获取PS值
            string PSHashValue = GetCurrentPSValue();
            if (PSHashValue != "")
            {
                string TextureConfigSavePath = Path.Combine(PathManager.Path_GameTextureConfigFolder, PSHashValue + ".json");
                TextureConfig.SaveTextureConfig(imageCollection, TextureConfigSavePath);
            }
        }

        private void Menu_OpenGameTextureConfigsFolder_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(PathManager.Path_TextureConfigsFolder))
            {
                SSMTCommandHelper.ShellOpenFolder(PathManager.Path_GameTextureConfigFolder);
            }
            else
            {
                _ = SSMTMessageHelper.Show("当前游戏的贴图目录不存在，请至少保存一个贴图配置。");
            }
        }
        


        private void Button_CancelMarkTexture_Click(object sender, RoutedEventArgs e)
        {
            // 获取选中的项
            int selectedIndex = ImageListView.SelectedIndex;

            if (selectedIndex < 0)
            {
                _ = SSMTMessageHelper.Show("请先选中要取消标记的自动贴图");
                return;
            }

            ImageItem selected_item = imageCollection[selectedIndex];

            selected_item.MarkName = "";

            imageCollection[selectedIndex] = selected_item;

            ImageListView.SelectedIndex = selectedIndex;

            SaveCurrentTextureConfig();

        }

        private void ApplyToAutoTexture()
        {
            SaveCurrentTextureConfig();

            string DrawIB = ComboBoxDrawIB.SelectedItem?.ToString();
            string ComponentName = ComboBoxComponent.SelectedItem?.ToString();

            if (DrawIB == null || ComponentName == null)
                return;

            try
            {
                TextureConfig.ApplyTextureConfig(imageCollection.ToList(), DrawIB, ComponentName);
                // Log：应用成功但不弹窗
                Debug.WriteLine("Apply Texture Success.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }


            //_ = SSMTMessageHelper.Show("应用成功", "Apply Texture Success!");
        }


        private void Menu_OpenCurrentWorkSpaceFolder_Click(object sender, RoutedEventArgs e)
        {
            SSMTCommandHelper.ShellOpenFolder(PathManager.Path_CurrentWorkSpaceFolder);
        }






        private void Menu_GenerateHashStyleTextureModTemplate_Click(object sender, RoutedEventArgs e)
        {
            //获取DrawIB
            string DrawIB = ComboBoxDrawIB.SelectedItem.ToString();
            string ComponentName = ComboBoxComponent.SelectedItem.ToString();
            string PartName = ComponentName.Substring("Component ".Length);
            //创建生成贴图Mod的文件夹
            string GenerateTextureModFolderPath = PathManager.Path_CurrentWorkSpaceGeneratedModFolder;
            Directory.CreateDirectory(GenerateTextureModFolderPath);

            Dictionary<string, TextureDeduped> dictionary = TextureConfig.Read_TrianglelistDedupedFileNameDict_FromJson(DrawIB);
            //
            List<string> TextureModIniList = [];
            TextureModIniList.Add("[TextureOverride_IB_SlotCheck]");
            TextureModIniList.Add("hash = " + DrawIB);
            TextureModIniList.Add("match_priority = 0");

            //添加SlotCheck
            if (GlobalConfig.CurrentGameName == "ZZZ")
            {
                TextureModIniList.Add("run = CommandListSkinTexture");
                TextureModIniList.Add("");

            }
            else
            {
                foreach (ImageItem imageItem in imageCollection)
                {
                    if (imageItem.MarkName.Trim() == "")
                    {
                        continue;
                    }

                    string PixelSlot = DBMTStringUtils.GetPixelSlotFromTextureFileName(imageItem.FileName);
                    TextureModIniList.Add("checktextureoverride = " + PixelSlot);
                }
                TextureModIniList.Add("");
            }

            foreach (ImageItem imageItem in imageCollection)
            {
                //有标记的才能参与生成Mod
                if (imageItem.MarkName.Trim() == "")
                {
                    continue;
                }

                string suffix = Path.GetExtension(imageItem.FileName);
                string CurrentDrawIBOutputFolder = Path.Combine(PathManager.Path_CurrentWorkSpaceFolder, DrawIB + "\\");

                string FALogDedupedFileName = dictionary[imageItem.FileName].FALogDedupedFileName;
                string ImageSourcePath = CurrentDrawIBOutputFolder + "DedupedTextures\\" + FALogDedupedFileName;
                string TextureHash = DBMTStringUtils.GetFileHashFromFileName(imageItem.FileName);

                //string TargetImageFileName = DrawIB + "-" + TextureHash + "-" + PartName + "-" + imageItem.MarkName + suffix;
                string TargetImageFileName = imageItem.MarkName + "_" + FALogDedupedFileName;

                TextureModIniList.Add("[TextureOverride_Texture_" + TextureHash + "]");
                TextureModIniList.Add("hash = " + TextureHash);
                TextureModIniList.Add("this = ResourceTexture_" + TextureHash);
                TextureModIniList.Add("");

                TextureModIniList.Add("[ResourceTexture_" + TextureHash + "]");
                TextureModIniList.Add("filename = " + TargetImageFileName);
                TextureModIniList.Add("");


                //复制贴图过去
                File.Copy(ImageSourcePath, GenerateTextureModFolderPath + TargetImageFileName, true);
            }

            string TextureModIniFileName = DrawIB + "_TextureMod.ini";
            File.WriteAllLines(GenerateTextureModFolderPath +  TextureModIniFileName, TextureModIniList );

            SSMTCommandHelper.ShellOpenFolder(GenerateTextureModFolderPath);
        }





        private void Button_MarkAutoTextureHashStyle_Click(object sender, RoutedEventArgs e)
        {
            // 获取选中的项
            int selectedIndex = ImageListView.SelectedIndex;

            if (selectedIndex < 0)
            {
                _ = SSMTMessageHelper.Show("请先选中要标记的贴图", "Please select a texture");
                return;
            }

            ImageItem selected_item = imageCollection[selectedIndex];

            selected_item.MarkStyle = "Hash";

            imageCollection[selectedIndex] = selected_item;

            ImageListView.SelectedIndex = selectedIndex;

            SaveCurrentTextureConfig();

            if (selected_item.MarkName != "")
            {
                ApplyToAutoTexture();
            }
        }

        private void Button_MarkAutoTextureSlotStyle_Click(object sender, RoutedEventArgs e)
        {
            // 获取选中的项
            int selectedIndex = ImageListView.SelectedIndex;

            if (selectedIndex < 0)
            {
                _ = SSMTMessageHelper.Show("请先选中要标记的贴图", "Please select a texture");
                return;
            }

            ImageItem selected_item = imageCollection[selectedIndex];

            selected_item.MarkStyle = "Slot";

            imageCollection[selectedIndex] = selected_item;

            ImageListView.SelectedIndex = selectedIndex;

            SaveCurrentTextureConfig();

            if (selected_item.MarkName != "")
            {
                ApplyToAutoTexture();
            }
        }

        private async void Menu_SeeDDSInfo_Click(object sender, RoutedEventArgs e)
        {
            string FilePath = await SSMTCommandHelper.ChooseFileAndGetPath(".dds");
            if (FilePath != "" && File.Exists(FilePath))
            {
                TexHelper.Instance.LoadFromDDSFile(FilePath, DDS_FLAGS.NONE);
                TexMetadata tex = TexHelper.Instance.GetMetadataFromDDSFile(FilePath, DDS_FLAGS.NONE);
                // 构建信息字符串
                string info = $"宽度: {tex.Width}\n" +
                              $"高度: {tex.Height}\n" +
                              $"深度: {tex.Depth}\n" +
                              $"Mip 级别: {tex.MipLevels}\n" +
                              $"数组大小: {tex.ArraySize}\n" +
                              $"格式: {tex.Format}\n" +
                              $"维度: {tex.Dimension}\n" +
                              $"杂项标志: {tex.MiscFlags}\n" +
                              $"杂项标志2: {tex.MiscFlags2}";

                _ = SSMTMessageHelper.Show(info);
            }
            
        }

        private void Button_CancelAutoTextureForCurrentDrawIB_Click(object sender, RoutedEventArgs e)
        {
            string DrawIB = ComboBoxDrawIB.SelectedItem.ToString();
            TextureConfig.ApplyEmptyTextureForDrawIB(DrawIB);
            _ = SSMTMessageHelper.Show("取消当前DrawIB的自动贴图成功");
        }

        private void Button_DIYMarkTexture_Click(object sender, RoutedEventArgs e)
        {
            // 获取选中的项
            int selectedIndex = ImageListView.SelectedIndex;

            if (selectedIndex < 0)
            {
                _ = SSMTMessageHelper.Show("请先选中要标记的贴图", "Please select a texture");
                return;
            }

            ImageItem selected_item = imageCollection[selectedIndex];

            selected_item.MarkName = TextBox_DIYMarkName.Text;

            imageCollection[selectedIndex] = selected_item;

            ImageListView.SelectedIndex = selectedIndex;

            //每次标记完自动保存配置
            SaveCurrentTextureConfig();

            if (selected_item.MarkName != "")
            {
                ApplyToAutoTexture();
            }
        }

   

        private void OnMenuTapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is ListBoxItem selectedItem)
            {
                // 获取 Tag
                string tag = selectedItem.Tag?.ToString();

                // 立即清除选择状态
                DispatcherQueue.TryEnqueue(() =>
                {
                    listBox.SelectedItem = null;
                });

                // 根据 Tag 执行不同逻辑
                if (!string.IsNullOrEmpty(tag))
                {
                    ExecuteMenuLogic(tag);
                }
            }
        }

        private void ExecuteMenuLogic(string tag)
        {
            // 获取选中的项
            int selectedIndex = ImageListView.SelectedIndex;

            if (selectedIndex < 0)
            {
                _ = SSMTMessageHelper.Show("请先选中要标记的贴图", "Please select a texture");
                return;
            }

            ImageItem selected_item = imageCollection[selectedIndex];

            selected_item.MarkName = tag;

            imageCollection[selectedIndex] = selected_item;

            ImageListView.SelectedIndex = selectedIndex;

            //每次标记完自动保存配置
            //应用到自动贴图会自动调用，无需重复调用
            //SaveCurrentTextureConfig();

            ApplyToAutoTexture();
        }

        private async void Menu_MarkAllHashStyle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (imageCollection.Count ==0)
                {
                    _ = SSMTMessageHelper.Show("没有可标记的贴图", "No textures to mark");
                    return;
                }

                for (int i =0; i < imageCollection.Count; i++)
                {
                    var item = imageCollection[i];
                    item.MarkStyle = "Hash";
                    imageCollection[i] = item;
                }

                SaveCurrentTextureConfig();
                ApplyToAutoTexture();
                _ = SSMTMessageHelper.Show("已将所有贴图标记为Hash风格", "All textures marked as Hash style");
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
        }

        private async void Menu_MarkAllSlotStyle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (imageCollection.Count ==0)
                {
                    _ = SSMTMessageHelper.Show("没有可标记的贴图", "No textures to mark");
                    return;
                }

                for (int i =0; i < imageCollection.Count; i++)
                {
                    var item = imageCollection[i];
                    item.MarkStyle = "Slot";
                    imageCollection[i] = item;
                }

                SaveCurrentTextureConfig();
                ApplyToAutoTexture();
                _ = SSMTMessageHelper.Show("已将所有贴图标记为Slot风格", "All textures marked as Slot style");
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
        }
    }
}


