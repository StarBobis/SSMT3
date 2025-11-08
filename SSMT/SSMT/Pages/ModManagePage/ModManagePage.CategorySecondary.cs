using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSMT_Core;

namespace SSMT
{
    public partial class ModManagePage
    {

        private void ListView_CategorySecondary_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LOG.Info("ListView_CategorySecondary_SelectionChanged::Start");
            LOG.Info("CategorySecondaryList Count: " + CategorySecondaryList.Count.ToString());
            if (CategorySecondaryList.Count == 0)
            {
                LOG.Info("此时还没有任何可选项，不执行选中项改变方法");
                return;
            }

            ModManageConfig modManageConfig = new ModManageConfig();
            modManageConfig.ReadConfig();
            modManageConfig.CategorySecondaryName = Get_SelectedCategorySecondary().Name;
            modManageConfig.SaveConfig();

            RefreshModItemList();
            LOG.Info("ListView_CategorySecondary_SelectionChanged::End");
        }

        public string Get_CategorySecondary_ModRepoFolderPath()
        {
            ModCategory ModCategoryPrimary = Get_SelectedCategoryPrimary();
            ModCategory ModCategorySecondary = Get_SelectedCategorySecondary();

            string ModRepoFolderSecondaryPath = Path.Combine(GlobalConfig.Path_ModsFolder, ModCategoryPrimary.Name, ModCategorySecondary.Name + "\\");
            return ModRepoFolderSecondaryPath;
        }

        public string Get_CategoryPrimary_ModRepoFolderPath() {
            string ModRepoFolderPrimaryPath = Path.Combine(GlobalConfig.Path_ModsFolder, Get_SelectedCategoryPrimary().Name + "\\");

            return ModRepoFolderPrimaryPath;
        }

 

        private async void MenuFlyoutItem_CategorySecondary_DeleteSelectedLine_Click(object sender, RoutedEventArgs e)
        {
            if (CategorySecondaryList.Count == 0)
            {
                return;
            }

            try
            {
                bool Confirm = await SSMTMessageHelper.ShowConfirm("确认删除该二级分类及其下面的所有文件？");
                if (!Confirm)
                {
                    return;
                }

                if (ListView_CategorySecondary.SelectedIndex < 0)
                {
                    _ = SSMTMessageHelper.Show("请至少选中其中一项");
                    return;
                }


                //直接去删除分类及其下面所有的文件，然后重载整个页面
                string ModRepoFolderSecondaryPath = Get_CategorySecondary_ModRepoFolderPath();
                if (Directory.Exists(ModRepoFolderSecondaryPath))
                {
                    Directory.Delete(ModRepoFolderSecondaryPath, true);
                }

                ReloadCategoryPrimary();
            }
            catch (Exception ex) {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
            
        }


    }


}
