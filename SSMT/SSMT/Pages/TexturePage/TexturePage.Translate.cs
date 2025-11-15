using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT
{
    public partial class TexturePage
    {
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // ✅ 每次进入页面都会执行，适合刷新 UI
            // 因为开启了缓存模式之后，是无法刷新页面语言的，只能在这里执行来刷新
            TranslatePage();
        }

        public void TranslatePage() {
            if (GlobalConfig.Chinese)
            {
                Menu_File.Title = "文件";
                //Menu_OpenGameTextureConfigsFolder.Text = "打开当前贴图配置文件夹";
                Menu_OpenCurrentWorkSpaceFolder.Text = "打开当前工作空间文件夹";
                Menu_SeeDDSInfo.Text = "查看DDS格式贴图信息";

                Menu_GenerateTextureMod.Title = "贴图Mod生成";
                Menu_GenerateHashStyleTextureModTemplate.Text = "生成Hash风格贴图Mod模板";

                ComboBoxDrawIB.Header = "绘制IB hash列表";
                ComboBoxComponent.Header = "组件";
                ComboBoxDrawCall.Header = "绘制调用";

                //ComboBox_MarkName.Header = "标记名称";
                Text_Block_MarkName.Text = "标记名称";
                Button_MarkTexture.Content = "标记此贴图";
                TextBox_DIYMarkName.Header = "自定义标记名称";
                Button_DIYMarkTexture.Content = "标记此贴图为自定义名称";
                Button_CancelMarkTexture.Content = "取消名称标记";
                Button_MarkAutoTextureSlotStyle.Content = "标记为槽位风格";
                Button_MarkAutoTextureHashStyle.Content = "标记为Hash风格";

                Button_ApplyToAutoTexture.Content = "应用到自动贴图";
                Button_CancelAutoTextureForCurrentDrawIB.Content = "取消当前绘制IB hash的自动贴图";
                Button_ApplyToPreViewTexture.Content = "应用到预览贴图";
                ToolTipService.SetToolTip(Button_ApplyToPreViewTexture, "此按钮和应用到自动贴图不同，应用后可以在Blender中一键导入时自动上贴图，但是无法参与生成Mod的全自动ini，适合仅预览的模型部分使用也就是只标记DiffuseMap用来上色");

                

            }
            else
            {
                Menu_File.Title = "File";
                //Menu_OpenGameTextureConfigsFolder.Text = "Open Current TextureConfig Folder";
                Menu_OpenCurrentWorkSpaceFolder.Text = "Open Current WorkSpace Folder";
                Menu_SeeDDSInfo.Text = "See DDS File Information";

                Menu_GenerateTextureMod.Title = "Texture Mod";
                Menu_GenerateHashStyleTextureModTemplate.Text = "Generate Hash Style Texture Mod Template";

                ComboBoxDrawIB.Header = "Draw Index Buffer List";
                ComboBoxComponent.Header = "Component";
                ComboBoxDrawCall.Header = "Draw Call";

                //ComboBox_MarkName.Header = "MarkName";
                Text_Block_MarkName.Text = "MarkName";
                Button_MarkTexture.Content = "Mark Texture";
                TextBox_DIYMarkName.Header = "DIY MarkName";
                Button_DIYMarkTexture.Content = "Mark Texture As DIY MarkName";
                Button_CancelMarkTexture.Content = "Cancel Texture Mark";
                Button_MarkAutoTextureSlotStyle.Content = "Mark As Slot Style";
                Button_MarkAutoTextureHashStyle.Content = "Mark As Hash Style";

                Button_ApplyToAutoTexture.Content = "Apply To Auto Texture";
                Button_CancelAutoTextureForCurrentDrawIB.Content = "Cancel Auto Texture";
                Button_ApplyToPreViewTexture.Content = "Apply To Preview Texture";

            }
        }


    }
}
