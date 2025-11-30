using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using WinUI3Helper;
using SharpCompress.Archives;
using SharpCompress.Common;
using SSMT_Core;
using SSMT_Core.IniParser;

namespace SSMT
{
    public partial class ModManagePage
    {

        private void AddModArchiveFile(string archiveFilePath)
        {
            ModCategory PrimaryCategory = Get_SelectedCategoryPrimary();
            ModCategory SecondaryCategory = Get_SelectedCategorySecondary();

            string PrimaryCategoryModFolderPath = Path.Combine(PathManager.Path_ModsFolder, PrimaryCategory.Name + "\\");
            if (!Directory.Exists(PrimaryCategoryModFolderPath))
            {
                Directory.CreateDirectory(PrimaryCategoryModFolderPath);
            }

            string SecondaryCategoryModFolderPath = Path.Combine(PrimaryCategoryModFolderPath, SecondaryCategory.Name + "\\");
            if (!Directory.Exists(SecondaryCategoryModFolderPath))
            {
                Directory.CreateDirectory(SecondaryCategoryModFolderPath);
            }

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(archiveFilePath);
            if (!fileNameWithoutExtension.ToLower().StartsWith("disabled"))
            {
                fileNameWithoutExtension = "DISABLED " + fileNameWithoutExtension;
            }

            string ModCopyFolderPath = Path.Combine(SecondaryCategoryModFolderPath, fileNameWithoutExtension + "\\");
            if (!Directory.Exists(ModCopyFolderPath))
            {
                Directory.CreateDirectory(ModCopyFolderPath);
            }

            // 根据文件扩展名选择解压方法
            string extension = Path.GetExtension(archiveFilePath).ToLower();

            try
            {
                switch (extension)
                {
                    case ".zip":
                        ZipFile.ExtractToDirectory(archiveFilePath, ModCopyFolderPath, overwriteFiles: true);
                        break;
                    case ".rar":
                        ExtractRarFile(archiveFilePath, ModCopyFolderPath);
                        break;
                    case ".7z":
                        Extract7zFile(archiveFilePath, ModCopyFolderPath);
                        break;
                    default:
                        throw new NotSupportedException($"不支持的压缩格式: {extension}");
                }
            }
            catch (Exception ex)
            {
                // 处理解压异常
                System.Diagnostics.Debug.WriteLine($"解压文件失败: {ex.Message}");
                throw;
            }
        }

        // 解压 RAR 文件
        private void ExtractRarFile(string rarFilePath, string extractToDirectory)
        {
            using (var archive = SharpCompress.Archives.Rar.RarArchive.Open(rarFilePath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (!entry.IsDirectory)
                    {
                        entry.WriteToDirectory(extractToDirectory, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
            }
        }

        // 解压 7z 文件
        private void Extract7zFile(string sevenZipFilePath, string extractToDirectory)
        {
            using (var archive = SharpCompress.Archives.SevenZip.SevenZipArchive.Open(sevenZipFilePath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (!entry.IsDirectory)
                    {
                        entry.WriteToDirectory(extractToDirectory, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
            }
        }


        /// <summary>
        /// 点击按钮的方式添加Mod
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_AddNewMod_Click(object sender, RoutedEventArgs e)
        {
            try {
                List<string> SuffixList = new List<string>();
                SuffixList.Add(".zip");
                SuffixList.Add(".7z");
                SuffixList.Add(".rar");


                string filepath = await SSMTCommandHelper.ChooseFileAndGetPath(SuffixList);
                if (filepath == "")
                {
                    return;
                }

                AddModArchiveFile(filepath);
                //AddModZipFile(filepath);
                RefreshModItemList();
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
                return;
            }

        }

        /// <summary>
        /// 拖拽的方式添加Mod
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ListView_ModItem_Drop(object sender, DragEventArgs e)
        {
            if (!e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                return;
            }

            try
            {
                ModCategory PrimaryCategory = Get_SelectedCategoryPrimary();
                ModCategory SecondaryCategory = Get_SelectedCategorySecondary();

                string PrimaryCategoryModFolderPath = Path.Combine(PathManager.Path_ModsFolder, PrimaryCategory.Name + "\\");
                if (!Directory.Exists(PrimaryCategoryModFolderPath))
                {
                    Directory.CreateDirectory(PrimaryCategoryModFolderPath);
                }

                string SecondaryCategoryModFolderPath = Path.Combine(PrimaryCategoryModFolderPath, SecondaryCategory.Name + "\\");
                if (!Directory.Exists(SecondaryCategoryModFolderPath))
                {
                    Directory.CreateDirectory(SecondaryCategoryModFolderPath);
                }

                // 获取拖拽的文件
                var items = await e.DataView.GetStorageItemsAsync();
                foreach (var item in items)
                {
                    if (item is StorageFile file)
                    {
                        // 获取文件路径
                        string filePath = file.Path;
                        string fileName = Path.GetFileName(filePath);
                        

                        if (fileName.ToLower().EndsWith(".zip") || fileName.ToLower().EndsWith(".7z") || fileName.ToLower().EndsWith(".rar"))
                        {
                            AddModArchiveFile(filePath);
                            RefreshModItemList();
                        }
                        else
                        {
                            _ = SSMTMessageHelper.Show("暂不支持此格式，请使用.zip/.7z/.rar格式.");
                            return;
                        }


                    }
                    else if (item is StorageFolder folder)
                    {
                        string folderPath = folder.Path;
                        string folderName = Path.GetFileName(folderPath);

                        if (!folderName.ToLower().StartsWith("disabled"))
                        {
                            folderName = "DISABLED " + folderName;
                        }

                        string ModCopyFolderPath = Path.Combine(SecondaryCategoryModFolderPath, folderName + "\\");
                        if (!Directory.Exists(ModCopyFolderPath))
                        {
                            Directory.CreateDirectory(ModCopyFolderPath);
                        }
                        DBMTFileUtils.CopyDirectory(folderPath, ModCopyFolderPath, true);
                        RefreshModItemList();
                    }
                }
            }
            catch (Exception ex)
            {

                _ = SSMTMessageHelper.Show(ex.ToString());
            }

        }

        /// <summary>
        /// 刷新Mod列表
        /// </summary>
        private void RefreshModItemList()
        {
            string SelectedModName = "";

            //先看看当前是否有选中项
            if (ListView_ModItem.SelectedIndex > 0 && ModItemList.Count > 0)
            {
                SelectedModName = Get_SelectedModItem().ModName;
            }

            //有选中项，可能是在有的时候刷新的所以不需要读取配置，但是如果没有，可能是在没有的时候刷新的，需要读取配置
            if (SelectedModName == "")
            {
                ModManageConfig modManageConfig = new ModManageConfig();
                modManageConfig.ReadConfig();
                SelectedModName = modManageConfig.ModItemName;
            }


            LOG.Info("RefreshModItemList::Start");
            ModItemList.Clear();

            ModCategory PrimaryCategory = Get_SelectedCategoryPrimary();
            ModCategory SecondaryCategory = Get_SelectedCategorySecondary();

            string PrimaryCategoryModFolderPath = Path.Combine(PathManager.Path_ModsFolder, PrimaryCategory.Name + "\\");
            if (!Directory.Exists(PrimaryCategoryModFolderPath))
            {
                Directory.CreateDirectory(PrimaryCategoryModFolderPath);
            }

            string SecondaryCategoryModFolderPath = Path.Combine(PrimaryCategoryModFolderPath, SecondaryCategory.Name + "\\");
            if (!Directory.Exists(SecondaryCategoryModFolderPath))
            {
                Directory.CreateDirectory(SecondaryCategoryModFolderPath);
            }

            string[] ModFolderPathList = Directory.GetDirectories(SecondaryCategoryModFolderPath);

            List<ModItem> NormalModItemList = new List<ModItem>();
            List<ModItem> DisabledModItemList = new List<ModItem>();

            foreach (string ModFolderPath in ModFolderPathList)
            {
                string DirectoryName = Path.GetFileName(ModFolderPath);



                ModItem newModItem = new ModItem();
                newModItem.ModName = DirectoryName;

                if (DirectoryName.ToLower().StartsWith("disabled"))
                {
                    newModItem.Enable = false;
                    DisabledModItemList.Add(newModItem);
                }
                else
                {
                    newModItem.Enable = true;
                    NormalModItemList.Add(newModItem);
                }

            }

            foreach (ModItem thisModItem in NormalModItemList)
            {
                ModItemList.Add(thisModItem);
            }
            foreach (ModItem thisModItem in DisabledModItemList)
            {
                ModItemList.Add(thisModItem);
            }


            if (ModItemList.Count != 0)
            {
                bool findSelected = false;
                foreach (ModItem modItem in ModItemList)
                {
                    if (modItem.ModName == SelectedModName)
                    {
                        ListView_ModItem.SelectedItem = modItem;
                        findSelected = true;
                        break;
                    }
                }

                if (!findSelected)
                {
                    ListView_ModItem.SelectedIndex = 0;
                }
            }

            LOG.Info("RefreshModItemList::End");

        }

        private void OpenSelectedModFolderPath() {
            try
            {
                //双击时，获取当前选中项
                int SelectedIndex = ListView_ModItem.SelectedIndex;

                if (SelectedIndex < 0)
                {
                    return;
                }

                ModItem SelectedModItem = ModItemList[SelectedIndex];

                string ModFolderPath = Path.Combine(PathManager.Path_ModsFolder, Get_SelectedCategoryPrimary().Name, Get_SelectedCategorySecondary().Name, SelectedModItem.ModName + "\\");

                if (Directory.Exists(ModFolderPath))
                {
                    SSMTCommandHelper.ShellOpenFolder(ModFolderPath);
                }
                else
                {
                    _ = SSMTMessageHelper.Show(ModFolderPath + " 路径不存在");
                }
            }
            catch(Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
                return;
            }

        }

        private void MenuFlyoutItem_OpenSelectedModFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedModFolderPath();
        }

        /// <summary>
        /// 双击切换Mod关闭或开启
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_ModItem_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            try
            {
                ModItem modItem = Get_SelectedModItem();
                string SecondaryModFolderPath = Get_CategorySecondary_ModRepoFolderPath();
                string TargetModFolderPath = Path.Combine(SecondaryModFolderPath, modItem.ModName + "\\");
                if (modItem.ModName.ToLower().StartsWith("disabled"))
                {
                    string NewNameWithoutDisabled = modItem.ModName.Substring("disabled".Length);
                    string NewModFolderPath = Path.Combine(SecondaryModFolderPath, NewNameWithoutDisabled + "\\");
                    Directory.Move(TargetModFolderPath, NewModFolderPath);

                    RefreshModItemList();
                }
                else
                {
                    string NewNameWithoutDisabled = "DISABLED" + modItem.ModName;
                    string NewModFolderPath = Path.Combine(SecondaryModFolderPath, NewNameWithoutDisabled + "\\");
                    Directory.Move(TargetModFolderPath, NewModFolderPath);

                    RefreshModItemList();
                }
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }

        }

        /// <summary>
        /// 右键删除此Mod
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuFlyoutItem_ModItem_DeleteSelectedMod_Click(object sender, RoutedEventArgs e)
        {
            try {


                ModItem modItem = Get_SelectedModItem();
                string SecondaryModFolderPath = Get_CategorySecondary_ModRepoFolderPath();
                string TargetModFolderPath = Path.Combine(SecondaryModFolderPath, modItem.ModName + "\\");
                bool Confirm = await SSMTMessageHelper.ShowConfirm("您确认要删除此Mod" + modItem.ModName + "吗？");

                if (Confirm)
                {
                    Directory.Delete(TargetModFolderPath, true);
                    RefreshModItemList();
                }

            }
            catch(Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
                return;
            }


        }

        private void ListView_ModItem_DragOver(object sender, DragEventArgs e)
        {
            // 检查拖拽的数据是否包含文件路径
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }
        }

        private void ModItem_ListViewItem_MenuFlyOut_Opening(object sender, object e)
        {
            var menuFlyout = sender as MenuFlyout;

            // 获取关联的ListViewItem
            var listViewItem = XamlHelper.FindParent<ListViewItem>(menuFlyout?.Target);

            if (listViewItem != null)
            {
                // 获取对应的数据项
                var dataItem = ListView_ModItem.ItemFromContainer(listViewItem);

                // 判断是否是当前选中的项
                if (dataItem != ListView_ModItem.SelectedItem)
                {
                    // 如果不是选中的项，取消显示菜单
                    menuFlyout.Hide();
                }
            }
        }

        private ModItem Get_SelectedModItem()
        {
            int SelectedModIndex = ListView_ModItem.SelectedIndex;
            ModItem modItem = ModItemList[SelectedModIndex];
            return modItem;
        }

        /// <summary>
        /// 递归获取指定目录及其所有子目录下所有INI文件的完整路径
        /// </summary>
        /// <param name="directoryPath">要搜索的目录路径</param>
        /// <param name="searchPattern">搜索模式，默认为"*.ini"</param>
        /// <returns>包含所有INI文件完整路径的列表</returns>
        public static List<string> GetAllIniFilesRecursively(string directoryPath, string searchPattern = "*.ini")
        {
            var iniFileList = new List<string>();

            // 检查目录路径是否为空或空白
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentException("目录路径不能为空或空白字符串", nameof(directoryPath));
            }

            // 检查目录是否存在
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"指定的目录不存在: {directoryPath}");
            }

            try
            {
                // 递归搜索文件
                SearchIniFilesRecursively(directoryPath, searchPattern, iniFileList);
            }
            catch (UnauthorizedAccessException ex)
            {
                // 处理权限不足的情况
                Console.WriteLine($"警告: 无法访问某些目录 - {ex.Message}");
            }
            catch (PathTooLongException ex)
            {
                // 处理路径过长的情况
                Console.WriteLine($"警告: 某些路径过长无法访问 - {ex.Message}");
            }
            catch (Exception ex)
            {
                // 处理其他异常
                Console.WriteLine($"搜索过程中发生错误: {ex.Message}");
            }

            return iniFileList;
        }

        /// <summary>
        /// 递归搜索INI文件的辅助方法
        /// </summary>
        private static void SearchIniFilesRecursively(string currentDirectory, string searchPattern, List<string> resultList)
        {
            try
            {
                // 获取当前目录下的所有INI文件
                string[] files = Directory.GetFiles(currentDirectory, searchPattern);
                resultList.AddRange(files);

                // 获取当前目录的所有子目录
                string[] subDirectories = Directory.GetDirectories(currentDirectory);

                // 递归搜索每个子目录
                foreach (string subDirectory in subDirectories)
                {
                    SearchIniFilesRecursively(subDirectory, searchPattern, resultList);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // 跳过没有访问权限的目录
                // 这里可以选择记录日志或忽略
            }
            catch (PathTooLongException)
            {
                // 跳过路径过长的目录
                // 这里可以选择记录日志或忽略
            }
        }

        /// <summary>
        /// Mod选中项变化时，更新Mod详细信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_ModItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModItemList.Count == 0)
            {
                return;
            }

            //获取该Mod的路径
            int SelectedModIndex = ListView_ModItem.SelectedIndex;
            ModItem modItem = ModItemList[SelectedModIndex];
            string SecondaryModFolderPath = Get_CategorySecondary_ModRepoFolderPath();
            string TargetModFolderPath = Path.Combine(SecondaryModFolderPath, modItem.ModName + "\\");

            //获取到所有的ini文件的抽象层级，备用
            List<string> iniFiles = GetAllIniFilesRecursively(TargetModFolderPath);
            LOG.Info("Ini File Number: " + iniFiles.Count.ToString());

            List<MigotoIniFile> migotoIniFileList = new List<MigotoIniFile>();
            foreach (string iniFilePath in iniFiles)
            {
                LOG.Info(iniFilePath);
                string iniFileNameLower = Path.GetFileName(iniFilePath).ToLower();
                if (iniFileNameLower.StartsWith("disabled"))
                {
                    continue;
                }

                MigotoIniFile migotoIniFile = new MigotoIniFile(iniFilePath);
                migotoIniFileList.Add(migotoIniFile);
            }

            //获取所有的贴图资源
            List<IniResource> TextureResourceList = new List<IniResource>();
            foreach (MigotoIniFile migotoIniFile in migotoIniFileList)
            {
                TextureResourceList.AddRange(migotoIniFile.GetTextureResourceList());
            }


            LoadDirectoryPicturesToFlipView(TargetModFolderPath,TextureResourceList);

            //尝试读取所有可能的按键切换
            ModKeyList.Clear();
            
            //对每个ini文件，都打开，然后读取分析里面的Key部分
            foreach (MigotoIniFile migotoIniFile in migotoIniFileList) {
                
                List<ModKey> modKeyList = migotoIniFile.ParseSelf_ModKeyList();

                foreach (ModKey modKey in modKeyList)
                {
                    //if (modKey.KeyType == "")
                    //{
                    //    continue;
                    //}
                    if (modKey.KeyValue == "")
                    {
                        continue;
                    }

                    LOG.Info(modKey.KeyName);
                    ModKeyList.Add(modKey);
                }
            }

            LOG.Info(ModKeyList.Count.ToString());

        }


        private void MenuFlyoutItem_ModItem_OpenThisMod_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ModItem modItem = Get_SelectedModItem();
                string SecondaryModFolderPath = Get_CategorySecondary_ModRepoFolderPath();
                string TargetModFolderPath = Path.Combine(SecondaryModFolderPath, modItem.ModName + "\\");
                if (modItem.ModName.ToLower().StartsWith("disabled"))
                {
                    string NewNameWithoutDisabled = modItem.ModName.Substring("disabled".Length);
                    string NewModFolderPath = Path.Combine(SecondaryModFolderPath, NewNameWithoutDisabled + "\\");
                    Directory.Move(TargetModFolderPath, NewModFolderPath);

                    RefreshModItemList();

                }
                else
                {
                    _ = SSMTMessageHelper.Show("该Mod已经启用，无需重复启用");
                }
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }

          
        }

        private void MenuFlyoutItem_ModItem_CloseThisMod_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ModItem modItem = Get_SelectedModItem();
                string SecondaryModFolderPath = Get_CategorySecondary_ModRepoFolderPath();
                string TargetModFolderPath = Path.Combine(SecondaryModFolderPath, modItem.ModName + "\\");
                if (modItem.ModName.ToLower().StartsWith("disabled"))
                {
                    _ = SSMTMessageHelper.Show("该Mod已经关闭，无需重复关闭");

                   
                }
                else
                {
                    string NewNameWithoutDisabled = "DISABLED" + modItem.ModName;
                    string NewModFolderPath = Path.Combine(SecondaryModFolderPath, NewNameWithoutDisabled + "\\");
                    Directory.Move(TargetModFolderPath, NewModFolderPath);

                    RefreshModItemList();
                }
                
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
        }

        private void LoadDirectoryPicturesToFlipView(string imageFolderPath,List<IniResource> ExcludeTextureResourceList)
        {
            try
            {
                LOG.Info("要排除的文件; " + ExcludeTextureResourceList.Count.ToString());
                List<string> TextureResourceFilePathList = new List<string>();
                foreach (IniResource textureIniResource in ExcludeTextureResourceList)
                {
                    LOG.Info(textureIniResource.FileName);
                    LOG.Info(textureIniResource.FilePath);
                    TextureResourceFilePathList.Add(textureIniResource.FilePath);
                }


                List<string> imageFiles = new List<string>();
                try
                {
                    //递归获取所有子目录中的图片文件，防止遗漏
                    imageFiles = Directory.GetFiles(imageFolderPath, "*.*", SearchOption.AllDirectories)
                        .Where(file => DBMTStringUtils.IsImageFilePath(file))
                        .ToList();
                }
                catch (UnauthorizedAccessException)
                {
                    // 跳过无法访问的目录
                    LOG.Info("LoadDirectoryPicturesToFlipView: skipped directories due to access restrictions.");
                }
                catch (PathTooLongException)
                {
                    LOG.Info("LoadDirectoryPicturesToFlipView: skipped files due to path too long.");
                }
                catch (Exception ex)
                {
                    LOG.Info("LoadDirectoryPicturesToFlipView error: " + ex.Message);
                }

                // 创建 BitmapImage 列表
                var imageSources = new List<BitmapImage>();

                foreach (string filePath in imageFiles)
                {
                    if (TextureResourceFilePathList.Contains(filePath))
                    {
                        
                        continue;
                    }

                    var bitmapImage = new BitmapImage();
                    bitmapImage.UriSource = new Uri(filePath);
                    imageSources.Add(bitmapImage);
                }

                FlipView_ModPreview.ItemsSource = imageSources;

                if (imageSources.Count > 0)
                {
                    TextBlock_ModPreviewTips.Visibility = Visibility.Collapsed;
                }
                else
                {
                    TextBlock_ModPreviewTips.Visibility = Visibility.Visible;
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载图片失败: {ex.Message}");
            }
        }

      


    }
}
