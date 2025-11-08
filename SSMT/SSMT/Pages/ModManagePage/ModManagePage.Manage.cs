using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinUI3Helper;
using SSMT_Core;

namespace SSMT
{
    public partial class ModManagePage
    {
        private async void Button_AddCategoryPrimaryIcon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filepath = await SSMTCommandHelper.ChooseFileAndGetPath(".png");
                if (filepath == "")
                {
                    return;
                }
                TextBox_ImageCategoryPrimaryPath.Text = filepath;
                Image_AddCategoryPrimary.Source = new BitmapImage(new Uri(filepath));
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
        }

        private void MakeSureModRepoExists()
        {
            try
            {
                if (!Directory.Exists(GlobalConfig.Path_ModsFolder))
                {
                    Directory.CreateDirectory(GlobalConfig.Path_ModsFolder);
                }
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
        }

        private void Button_AddCategoryPrimary_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MakeSureModRepoExists();

                string NewCategoryName = TextBox_AddCategoryPrimaryName.Text;

                string NewCategoryPath = Path.Combine(GlobalConfig.Path_ModsFolder,NewCategoryName + "\\");

                if (!Directory.Exists(NewCategoryPath))
                {
                    Directory.CreateDirectory(NewCategoryPath);

                    //如果设置了图标就复制过去
                    string ImagePath = TextBox_ImageCategoryPrimaryPath.Text;
                    
                    if (File.Exists(ImagePath))
                    {
                        string TargetImagePath = Path.Combine(NewCategoryPath,"Icon.png");
                        File.Copy(ImagePath, TargetImagePath, true);
                    }

                    //添加完成后刷新整个页面
                    ReloadCategoryPrimary();

                    _ = SSMTMessageHelper.Show("添加完成");
                }
                else
                {
                    _ = SSMTMessageHelper.Show("这个名称的Mod分类已经存在了");
                }

            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
        }

        private async void Button_AddCategorySecondaryIcon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filepath = await SSMTCommandHelper.ChooseFileAndGetPath(".png");
                if (filepath == "")
                {
                    return;
                }
                TextBox_ImageCategorySecondaryPath.Text = filepath;
                Image_AddCategorySecondary.Source = new BitmapImage(new Uri(filepath));
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
        }

        private void Button_AddCategorySecondary_Click(object sender, RoutedEventArgs e)
        {
            LOG.Info("Button_AddCategorySecondary_Click::Start");
            try
            {
                MakeSureModRepoExists();

                string NewCategoryName = TextBox_AddCategorySecondaryName.Text;

                ModCategory PrimaryCategory = Get_SelectedCategoryPrimary();
                LOG.Info("当前PrimaryCategory: " + PrimaryCategory.Name);

                string PrimaryCategoryPath = Path.Combine(GlobalConfig.Path_ModsFolder, PrimaryCategory.Name + "\\");

                string NewCategoryPath = Path.Combine(PrimaryCategoryPath, NewCategoryName + "\\");

                if (!Directory.Exists(NewCategoryPath))
                {
                    Directory.CreateDirectory(NewCategoryPath);

                    //如果设置了图标就复制过去
                    string ImagePath = TextBox_ImageCategorySecondaryPath.Text;

                    if (File.Exists(ImagePath))
                    {
                        string TargetImagePath = Path.Combine(NewCategoryPath, "Icon.png");
                        File.Copy(ImagePath, TargetImagePath, true);
                    }

                    //添加完成后刷新整个页面
                    LOG.NewLine("二级菜单添加完成，准备刷新整个页面");

                    ReloadCategoryPrimary();

                    _ = SSMTMessageHelper.Show("添加完成");
                }
                else
                {
                    _ = SSMTMessageHelper.Show("这个名称的Mod分类已经存在了");
                }

            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }

            LOG.Info("Button_AddCategorySecondary_Click::End");
        }

    }
}
