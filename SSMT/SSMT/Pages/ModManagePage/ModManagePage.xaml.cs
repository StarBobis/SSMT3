using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using WinUI3Helper;
using SSMT_Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SSMT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModManagePage : Page
    {
        private ObservableCollection<ModCategory> CategoryPrimaryList = new ObservableCollection<ModCategory>();
        private ObservableCollection<ModCategory> CategorySecondaryList = new ObservableCollection<ModCategory>();
        private ObservableCollection<ModItem> ModItemList = new ObservableCollection<ModItem>();
        private ObservableCollection<ModKey> ModKeyList = new ObservableCollection<ModKey>();


        public ModManagePage()
        {
            InitializeComponent();

            
            this.Loaded += PageLoaded;
        }

        private void PageLoaded(object sender, RoutedEventArgs e) {

            try
            {
                // 将数据绑定到ListView
                ListView_CategoryPrimary.ItemsSource = CategoryPrimaryList;
                ListView_CategorySecondary.ItemsSource = CategorySecondaryList;
                ListView_ModItem.ItemsSource = ModItemList;
                DataGrid_ModKeyList.ItemsSource = ModKeyList;

                ReloadCategoryPrimary();
            }
            catch (Exception ex) {
                _ = SSMTMessageHelper.Show("初始化Mod管理页面失败，错误提示:\n" + ex.ToString());
            }
           
        }


        //设置的显示与隐藏
        private void Button_ShowSetting_Click(object sender, RoutedEventArgs e)
        {
            if (Border_CategorySettings.Visibility == Visibility.Visible)
            {
                Border_CategorySettings.Visibility = Visibility.Collapsed;
            }
            else
            {
                Border_CategorySettings.Visibility = Visibility.Visible;
            }
        }

        private void ReloadCategoryPrimary()
        {
            //先把当前选项记录下来
            ModManageConfig modManageConfig = new ModManageConfig();
            modManageConfig.ReadConfig();

            if (CategoryPrimaryList.Count > 0 && ListView_CategoryPrimary.SelectedIndex > 0)
            {
                modManageConfig.CategoryPrimaryName = Get_SelectedCategoryPrimary().Name;
            }

            if (CategorySecondaryList.Count > 0 && ListView_CategorySecondary.SelectedIndex > 0)
            {
                modManageConfig.CategorySecondaryName = Get_SelectedCategorySecondary().Name;
            }

            if(ModItemList.Count > 0 && ListView_ModItem.SelectedIndex > 0)
            {
                modManageConfig.ModItemName = Get_SelectedModItem().ModName;
            }

            modManageConfig.SaveConfig();


            //重载所有肯定要把所有列表都清理干净
            ModItemList.Clear();
            CategorySecondaryList.Clear();
            CategoryPrimaryList.Clear();

            LOG.Info("ReloadCategoryPrimary::Start");
            MakeSureModRepoExists();

            string[] CategoryPrimaryNameList = Directory.GetDirectories(GlobalConfig.Path_ModsFolder);

            //如果一个分类都没有，那就创建一个默认分类
            if (CategoryPrimaryNameList.Length == 0)
            {
                string DefaultCategoryPath = Path.Combine(GlobalConfig.Path_ModsFolder, "Default\\");
                Directory.CreateDirectory(DefaultCategoryPath);
                CategoryPrimaryNameList = Directory.GetDirectories(GlobalConfig.Path_ModsFolder);
            }

            List<ModCategory> NormalPrimaryCategoryList = new List<ModCategory>();
            List<ModCategory> DisabledPrimaryCategoryList = new List<ModCategory>();

            foreach (string CategoryNamePath in CategoryPrimaryNameList) {
                string CategoryName = Path.GetFileName(CategoryNamePath);
                string CategoryImage = Path.Combine(CategoryNamePath, "Icon.png");

                //获取Mod数量
                int ModNumber = 0;
                if (!Directory.Exists(GlobalConfig.Path_ModsFolder))
                {
                    Directory.CreateDirectory(GlobalConfig.Path_ModsFolder);
                }

                string ModRepoPrimaryPath = Path.Combine(GlobalConfig.Path_ModsFolder, CategoryName + "\\");

                if (Directory.Exists(ModRepoPrimaryPath))
                {
                    string[] ModCategoryPrimaryPathList = Directory.GetDirectories(ModRepoPrimaryPath);
                    ModNumber = ModCategoryPrimaryPathList.Length;
                }
                else
                {
                    Directory.CreateDirectory(ModRepoPrimaryPath);
                }

                bool NotEnabled = CategoryName.ToLower().StartsWith("disabled");
                ModCategory newModCategory = new ModCategory
                {
                    Name = CategoryName,
                    BackgroundImage = CategoryImage,
                    ModNumber = ModNumber.ToString(),
                    NotEnable = NotEnabled
                };

                if (NotEnabled)
                {
                    DisabledPrimaryCategoryList.Add(newModCategory);
                }
                else
                {
                    NormalPrimaryCategoryList.Add(newModCategory);
                }
            }

            foreach (ModCategory modCategory in NormalPrimaryCategoryList)
            {
                CategoryPrimaryList.Add(modCategory);
            }
            foreach (ModCategory modCategory in DisabledPrimaryCategoryList)
            {
                CategoryPrimaryList.Add(modCategory);
            }

            //加载完毕后默认选为0，然后触发次级分类的选择
            if (ListView_CategoryPrimary.Items.Count > 0)
            {
                if(modManageConfig.CategoryPrimaryName != "")
                {
                    for(int i = 0; i < CategoryPrimaryList.Count; i++)
                    {
                        if(CategoryPrimaryList[i].Name == modManageConfig.CategoryPrimaryName)
                        {
                            ListView_CategoryPrimary.SelectedIndex = i;
                            break;
                        }
                    }
                }

                //没有设置过或者没有找到就默认选0
                if (ListView_CategoryPrimary.SelectedIndex < 0)
                {
                    ListView_CategoryPrimary.SelectedIndex = 0;
                }

            }

            LOG.Info("ReloadCategoryPrimary::End");
        }


        private ModCategory Get_SelectedCategoryPrimary()
        {
            int PrimarySelectedIndex = ListView_CategoryPrimary.SelectedIndex;
            ModCategory modCategory = CategoryPrimaryList[PrimarySelectedIndex];
            return modCategory;
        }

        private ModCategory Get_SelectedCategorySecondary()
        {
            int SecondarySelectedIndex = ListView_CategorySecondary.SelectedIndex;
            ModCategory modCategory = CategorySecondaryList[SecondarySelectedIndex];
            return modCategory;
        }

      

        private void ListView_CategoryPrimary_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LOG.Info("ListView_CategoryPrimary_SelectionChanged::Start");
            LOG.Info("CategoryPrimaryList Count:" + CategoryPrimaryList.Count.ToString());
            //一个都没有的时候肯定没法触发的
            if (CategoryPrimaryList.Count == 0)
            {
                LOG.Info("此时还没有任何可选项，不执行选中项改变方法");
                return;
            }

            
            try
            {

                ModCategory modCategoryPrimary = Get_SelectedCategoryPrimary();
                string CategoryPrimaryName = modCategoryPrimary.Name;
                LOG.Info("当前选中了CategoryPrimaryName: " + CategoryPrimaryName);

                ModManageConfig modManageConfig = new ModManageConfig();
                modManageConfig.ReadConfig();
                modManageConfig.CategoryPrimaryName = CategoryPrimaryName;
                modManageConfig.SaveConfig();

                LOG.Info("当前记录的CategorySecondaryName: " + modManageConfig.CategorySecondaryName);

                string CategoryPrimaryPath = Path.Combine(GlobalConfig.Path_ModsFolder, CategoryPrimaryName);

                string[] CategoryNameList = Directory.GetDirectories(CategoryPrimaryPath);

                //如果一个二级分类都没有，那就创建一个默认分类
                if (CategoryNameList.Length == 0)
                {
                    string DefaultCategorySecondaryPath = Path.Combine(CategoryPrimaryPath, "Default\\");
                    Directory.CreateDirectory(DefaultCategorySecondaryPath);
                    CategoryNameList = Directory.GetDirectories(CategoryPrimaryPath);
                }

                ModItemList.Clear();
                CategorySecondaryList.Clear();

                List<ModCategory> NormalSecondaryCategoryList = new List<ModCategory>();
                List<ModCategory> DisabledSecondaryCategoryList = new List<ModCategory>();

                foreach (string CategoryNamePath in CategoryNameList)
                {
                    string CategorySecondaryName = Path.GetFileName(CategoryNamePath);
                    string CategoryImage = Path.Combine(CategoryNamePath, "Icon.png");

                    //获取Mod数量
                    int SecondaryModNumber = 0;
                    if (!Directory.Exists(GlobalConfig.Path_ModsFolder))
                    {
                        Directory.CreateDirectory(GlobalConfig.Path_ModsFolder);
                    }

                    string CategorySecondaryModFolderPath = Path.Combine(GlobalConfig.Path_ModsFolder, CategoryPrimaryName, CategorySecondaryName + "\\");
                    if (!Directory.Exists(CategorySecondaryModFolderPath))
                    {
                        Directory.CreateDirectory(CategorySecondaryModFolderPath);
                    }

                    string[] ModCategorySecondaryPathList = Directory.GetDirectories(CategorySecondaryModFolderPath);
                    SecondaryModNumber = ModCategorySecondaryPathList.Length;

                    bool NotEnabled = CategorySecondaryName.ToLower().StartsWith("disabled");
                    ModCategory newModCategory = new ModCategory
                    {
                        Name = CategorySecondaryName,
                        BackgroundImage = CategoryImage,
                        ModNumber = SecondaryModNumber.ToString(),
                        NotEnable = NotEnabled
                    };


                    if (NotEnabled)
                    {
                        DisabledSecondaryCategoryList.Add(newModCategory);
                    }
                    else
                    {
                        NormalSecondaryCategoryList.Add(newModCategory);
                    }

                }

                foreach (ModCategory modCategory in NormalSecondaryCategoryList)
                {
                    CategorySecondaryList.Add(modCategory);
                }
                foreach (ModCategory modCategory in DisabledSecondaryCategoryList)
                {
                    CategorySecondaryList.Add(modCategory);
                }

                //加载完毕后默认选为0
                if (ListView_CategorySecondary.Items.Count > 0)
                {

                    modManageConfig = new ModManageConfig();    
                    modManageConfig.ReadConfig();

                    if (modManageConfig.CategorySecondaryName != "")
                    {
                        for (int i = 0; i < CategorySecondaryList.Count; i++)
                        {
                            if (CategorySecondaryList[i].Name == modManageConfig.CategorySecondaryName)
                            {
                                ListView_CategorySecondary.SelectedIndex = i;
                                break;
                            }
                        }
                    }

                    if (ListView_CategorySecondary.SelectedIndex < 0)
                    {
                        ListView_CategorySecondary.SelectedIndex = 0;
                    }


                }
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
            LOG.Info("ListView_CategoryPrimary_SelectionChanged::End");
        }





        private void CategorySecondary_ListViewItem_MenuFlyOut_Opening(object sender, object e)
        {
            var menuFlyout = sender as MenuFlyout;

            // 获取关联的ListViewItem
            var listViewItem = XamlHelper.FindParent<ListViewItem>(menuFlyout?.Target);

            if (listViewItem != null)
            {
                // 获取对应的数据项
                var dataItem = ListView_CategorySecondary.ItemFromContainer(listViewItem);

                // 判断是否是当前选中的项
                if (dataItem != ListView_CategorySecondary.SelectedItem)
                {
                    // 如果不是选中的项，取消显示菜单
                    menuFlyout.Hide();
                }
            }
        }


      
        private async void Button_ChangeModPreview_Click(object sender, RoutedEventArgs e)
        {


            List<string> imageExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".webp" };
            string filepath = await SSMTCommandHelper.ChooseFileAndGetPath(imageExtensions);
            if (filepath == "")
            {
                return;
            }

            // 获取文件路径
            if (!filepath.ToLower().EndsWith(".png"))
            {
                _ = SSMTMessageHelper.Show("请拖入png格式预览图");
                return;
            }

            //如果是PNG格式图片，则复制图片到Mod目录下，并命名为preview.png

            int SelectedModIndex = ListView_ModItem.SelectedIndex;

            if (SelectedModIndex < 0)
            {
                _ = SSMTMessageHelper.Show("请先选中一个Mod再来设置预览图吧。");
                return;
            }

            ModItem modItem = ModItemList[SelectedModIndex];

            //获取当前选中的Mod路径

            string SecondaryModFolderPath = Get_CategorySecondary_ModRepoFolderPath();
            string TargetModFolderPath = Path.Combine(SecondaryModFolderPath, modItem.ModName + "\\");
            string PreviewPath = Path.Combine(TargetModFolderPath, "preview" + DBMTStringUtils.GetTimestampForFilename()+".png");

            File.Copy(filepath, PreviewPath, true);

            RefreshModItemList();
        }

        private async void Button_PasteClipboardImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int SelectedModIndex = ListView_ModItem.SelectedIndex;

                if (SelectedModIndex < 0)
                {
                    _ = SSMTMessageHelper.Show("请先选中一个Mod再来设置预览图吧。");
                    return;
                }

                // 检查剪贴板是否有图片
                if (!ClipboardImageHelper.HasImageAsync())
                {
                    _ = SSMTMessageHelper.Show("剪贴板中没有图片");
                    return;
                }


                ModItem modItem = ModItemList[SelectedModIndex];

                //获取当前选中的Mod路径
                string SecondaryModFolderPath = Get_CategorySecondary_ModRepoFolderPath();
                string TargetModFolderPath = Path.Combine(SecondaryModFolderPath, modItem.ModName + "\\");
                string PreviewPath = Path.Combine(TargetModFolderPath, "preview" + DBMTStringUtils.GetTimestampForFilename() + ".png");
                // 处理剪贴板图片
                bool result = await ClipboardImageHelper.SaveClipboardImageToFileAsync(PreviewPath);

                //var bitmapImage = new BitmapImage();
                //// 设置忽略图像缓存
                //bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                //bitmapImage.UriSource = new Uri(PreviewPath);

                //LoadDirectoryPicturesToFlipView(TargetModFolderPath);

                RefreshModItemList();
                //_ = SSMTMessageHelper.Show("已将剪贴板图片保存为Mod预览图");
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }


            
        }

        private void ListViewItem_CategoryPrimary_MenuFlyout_Opening(object sender, object e)
        {
            var menuFlyout = sender as MenuFlyout;

            // 获取关联的ListViewItem
            var listViewItem = XamlHelper.FindParent<ListViewItem>(menuFlyout?.Target);

            if (listViewItem != null)
            {
                // 获取对应的数据项
                var dataItem = ListView_CategoryPrimary.ItemFromContainer(listViewItem);

                // 判断是否是当前选中的项
                if (dataItem != ListView_CategoryPrimary.SelectedItem)
                {
                    // 如果不是选中的项，取消显示菜单
                    menuFlyout.Hide();
                }
            }
        }

        private async void MenuFlyoutItem_CategoryPrimary_DeleteSelectedCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryPrimaryList.Count == 0)
            {
                return;
            }

            try
            {
                bool Confirm = await SSMTMessageHelper.ShowConfirm("确认删除该一级分类及其下面的所有文件？");
                if (!Confirm)
                {
                    return;
                }

                if (ListView_CategoryPrimary.SelectedIndex < 0)
                {
                    _ = SSMTMessageHelper.Show("请至少选中其中一项");
                    return;
                }


                //直接去删除分类及其下面所有的文件，然后重载整个页面
                string ModRepoFolderPrimaryPath = Path.Combine(GlobalConfig.Path_ModsFolder, Get_SelectedCategoryPrimary().Name + "\\") ;
                if (Directory.Exists(ModRepoFolderPrimaryPath))
                {
                    Directory.Delete(ModRepoFolderPrimaryPath, true);
                }

                ReloadCategoryPrimary();
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
        }

        private async void MenuFlyoutItem_CategoryPrimary_ModifyIconPicture_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filepath = await SSMTCommandHelper.ChooseFileAndGetPath(".png");
                if (filepath == "")
                {
                    return;
                }

                //png文件复制到对应目录下并命名为Icon.png

                string ModRepoFolderPrimaryPath = Get_CategoryPrimary_ModRepoFolderPath();
                string TargetIconPath = Path.Combine(ModRepoFolderPrimaryPath, "Icon.png");
                File.Copy(filepath, TargetIconPath, true);

                ReloadCategoryPrimary();

                _ = SSMTMessageHelper.Show("修改完成，但是由于WinUI3的缓存限制，重启整个程序才能生效。");
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());

            }

        }

        private async void MenuFlyoutItem_CategorySecondary_ModifyIconPicture_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filepath = await SSMTCommandHelper.ChooseFileAndGetPath(".png");
                if (filepath == "")
                {
                    return;
                }

                //png文件复制到对应目录下并命名为Icon.png

                string ModRepoFolderSecondaryPath = Get_CategorySecondary_ModRepoFolderPath();
                string TargetIconPath = Path.Combine(ModRepoFolderSecondaryPath, "Icon.png");
                File.Copy(filepath, TargetIconPath, true);

                ReloadCategoryPrimary();

                _ = SSMTMessageHelper.Show("修改完成，但是由于WinUI3的缓存限制，重启整个程序才能生效。");
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());

            }
        }

        private void ListView_CategoryPrimary_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            try
            {
                string ModRepoFolderPrimaryPath = Get_CategoryPrimary_ModRepoFolderPath();
                string FolderName = Path.GetFileName(ModRepoFolderPrimaryPath.TrimEnd('\\', '/'));
            
                if (FolderName.ToLower().StartsWith("disabled"))
                {
                    string NewNameWithoutDisabled = FolderName.Substring("disabled".Length);
                    string NewFolderPath = Path.Combine(GlobalConfig.Path_ModsFolder, NewNameWithoutDisabled + "\\");
                    Directory.Move(ModRepoFolderPrimaryPath, NewFolderPath);
                }
                else
                {
                    string NewFolderName = "DISABLED" + FolderName;
                    string NewFolderPath = Path.Combine(GlobalConfig.Path_ModsFolder, NewFolderName + "\\");
                    Directory.Move(ModRepoFolderPrimaryPath, NewFolderPath);
                }
                ReloadCategoryPrimary();
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
        }

        private void ListView_CategorySecondary_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            try
            {
                try
                {
                    string ModRepoFolderPrimaryPath = Get_CategoryPrimary_ModRepoFolderPath();
                    string ModRepoFolderSecondaryPath = Get_CategorySecondary_ModRepoFolderPath();

                    string FolderName = Path.GetFileName(ModRepoFolderSecondaryPath.TrimEnd('\\', '/'));

                    if (FolderName.ToLower().StartsWith("disabled"))
                    {
                        string NewNameWithoutDisabled = FolderName.Substring("disabled".Length);
                        string NewFolderPath = Path.Combine(ModRepoFolderPrimaryPath, NewNameWithoutDisabled + "\\");
                        Directory.Move(ModRepoFolderSecondaryPath, NewFolderPath);
                    }
                    else
                    {
                        string NewFolderName = "DISABLED" + FolderName;
                        string NewFolderPath = Path.Combine(ModRepoFolderPrimaryPath, NewFolderName + "\\");
                        Directory.Move(ModRepoFolderSecondaryPath, NewFolderPath);
                    }
                    ReloadCategoryPrimary();
                }
                catch (Exception ex)
                {
                    _ = SSMTMessageHelper.Show(ex.ToString());
                }
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
        }

        private void MenuFlyoutItem_CategoryPrimary_AddNewCategory_Click(object sender, RoutedEventArgs e)
        {
            Border_CategorySettings.Visibility = Visibility.Visible;
        }

        private void MenuFlyoutItem_CategorySecondary_AddNewCategory_Click(object sender, RoutedEventArgs e)
        {
            Border_CategorySettings.Visibility = Visibility.Visible;
        }

        private void Button_AddComplete_Click(object sender, RoutedEventArgs e)
        {
            Border_CategorySettings.Visibility = Visibility.Collapsed;
        }

        private void MenuFlyoutItem_DeleteThisModPreviewPicture_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                BitmapImage selectedBitmapImage = FlipView_ModPreview.SelectedItem as BitmapImage;

                if (selectedBitmapImage == null || selectedBitmapImage.UriSource == null)
                {
                    _ = SSMTMessageHelper.Show("当前暂无预览图");
                    return;
                }

                string ImagePath = selectedBitmapImage.UriSource.LocalPath;
                //_ = SSMTMessageHelper.Show(ImagePath);
                if (File.Exists(ImagePath))
                {
                    File.Delete(ImagePath); 
                    RefreshModItemList();
                }
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }

            

        }
    }
}
