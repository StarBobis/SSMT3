using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT
{
    public partial class WorkPage
    {
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // ✅ 每次进入页面都会执行，适合刷新 UI
            // 因为开启了缓存模式之后，是无法刷新页面语言的，只能在这里执行来刷新
            TranslatePage();
        }

        public void TranslatePage()
        {
            if (GlobalConfig.Chinese)
            {
                Menu_File.Title = "文件";

                Menu_OpenWorkSpaceGeneratedModFolder.Text = "打开当前工作空间生成Mod的文件夹";
                Menu_OpenModsFolder.Text = "打开Mods文件夹";
                Menu_Open3DmigotoFolder.Text = "打开3Dmigoto文件夹";
                Menu_OpenD3dxini.Text = "打开d3dx.ini";
                Menu_OpenShaderFixesFolder.Text = "打开ShaderFixes文件夹";

                Menu_OpenLatestFrameAnalysisFolder.Text = "打开最新的FrameAnalysis文件夹";
                Menu_OpenLatestFrameAnalysisLogTxt.Text = "打开最新的FrameAnalysis log.txt";
                Menu_OpenLatestFrameAnalysisDedupedFolder.Text = "打开最新的FrameAnalysis文件夹中的deduped文件夹";

                Menu_OpenDBMTLocationFolder.Text = "打开SSMT缓存文件夹";
                Menu_OpenLogsFolder.Text = "打开Logs文件夹";
                Menu_OpenLatestLogFile.Text = "打开最新的日志文件";
                Menu_OpenConfigsFolder.Text = "打开Configs文件夹";
                Menu_OpenPluginsFolder.Text = "打开Plugins文件夹";

                Menu_GameTypeFolder.Text = "打开数据类型文件夹";
                Menu_OpenAssetsFolder.Text = "打开Assets文件夹";
                Menu_OpenGlobalConfigFolder.Text = "打开全局配置所在文件夹";

                Menu_ToolBox.Title = "工具箱";
                Menu_AutoDetect_UnityVS_GPUPreSkinning_DrawIBList.Text = "自动检测UntiyVS_GPU-PreSkinning的DrawIB列表";
                Menu_AutoDetect_CPUPreSkinning_DrawIBList.Text = "自动检测CPU-PreSkinning的DrawIB列表";


                Menu_Texture.Title = "贴图";

                Menu_ConvertDDSToTargetFormat.Text = "批量转换指定文件夹中的dds文件到目标格式";
                Menu_ExtractTextureFiles.Text = "提取所有类型贴图文件";

                Menu_ExtractDedupedTextures.Text = "提取DedupedTextures";
                Menu_ExtractTrianglelistTextures.Text = "提取TrianglelistTextures";
                Menu_ExtractRenderTextures.Text = "提取RenderTextures";



                //侧边栏
                WorkGameSelectionComboBox.Header = "游戏名称";
                ComboBoxWorkSpaceSelection.Header = "工作空间";
                Button_CreateWorkSpace.Content = "创建工作空间";
                Button_OpenWorkSpaceFolder.Content = "打开当前工作空间文件夹";
                Button_CleanWorkSpace.Content = "清空当前工作空间内容";
                Button_DeleteCurrentWorkSpaceFolder.Content = "删除当前工作空间";

                ToolTipService.SetToolTip(ToggleSwitch_DumpIBListConfig, "指定IB列表Dump模式下，禁用Mods文件夹，并且F8只会Dump DrawIB列表中的IB，适合提取特定角色或场景的模型。全局Dump模式下，启用Mods文件夹，Dump游戏运行时所有当前帧内容。");
                ToggleSwitch_DumpIBListConfig.OnContent = "当前Dump模式：指定IB列表";
                ToggleSwitch_DumpIBListConfig.OffContent = "当前Dump模式：全局";

                ToggleSwitch_ConvertDedupedTextures.OnContent = "转换DedupedTextures贴图";
                ToggleSwitch_ConvertDedupedTextures.OffContent = "不转换DedupedTextures贴图";
                ToolTipService.SetToolTip(ToggleSwitch_ConvertDedupedTextures, "推荐开启，开启后将会在提取模型后把DedupedTextures中的dds格式贴图转换为你设置中指定格式的贴图，关闭后将导致贴图标记相关功能无法使用");

                //主要内容
                TextBlock_DrawIBList.Text = "DrawIB列表";
                ToolTipService.SetToolTip(TextBlock_DrawIBList, "SSMT使用IB提取模型，DrawIB顾名思义就是当前帧用于绘制指定内容的IndexBuffer，一般为Hunting界面小键盘7和8选择隐藏的部位，小键盘9复制出来的IndexBuffer的Hash值");

                Menu_DeleteDrawIBLine.Text = "删除选中行";
                DataGridTextColumn_DrawIBList_DrawIB.Header = "绘制IB值";
                DataGridTextColumn_DrawIBList_Alias.Header = "别名";
                Button_ExtractModel.Content = "提取模型";
                Button_InitializeDrawIBConfig.Content = "清空DrawIB列表";
                Button_SaveDrawIBList.Content = "保存当前DrawIB列表";
                Button_CleanLastExtract.Content = "清除上次提取的内容";
                ToolTipService.SetToolTip(Button_CleanLastExtract, "清除上一次提取的DrawIB文件夹，方便新填写的DrawIB提取不会和上一次的内容混淆，偷懒不创建新工作空间测试不同DrawIB提取时常用");
                Button_AutoDetectGameTypeDrawIBList.Content = "检测选定数据类型的DrawIB列表";
                ToolTipService.SetToolTip(Button_AutoDetectGameTypeDrawIBList, "目前仅ZZZ可用，一般用于那些频繁走动的NPC很难找DrawIB的情况。");

                TextBlock_SkipIBList.Text = "SkipIB列表";
                ToolTipService.SetToolTip(TextBlock_SkipIBList, "一般用于生成指定IB的handling = skip，方便快速查看指定IB隐藏后的效果");
                Menu_DeleteSkipIBLine.Text = "删除选中行";
                DataGridTextColumn_SkipIBList_SkipIB.Header = "隐藏IB值";
                DataGridTextColumn_SkipIBList_Alias.Header = "别名";

                Button_CleanSkipIBList.Content = "清空SkipIB列表";
                Button_SaveSkipIBList.Content = "保存当前SkipIB列表";
                Button_SkipIBDraw.Content = "隐藏IB绘制";

                TextBlock_VSCheckList.Text = "VSCheck列表";
                ToolTipService.SetToolTip(TextBlock_VSCheckList, "部分游戏由于3Dmigoto配置中不适合使用全局Check，所以使用VSCheck技术对当前帧Dump的VertexShader进行手动Check来让Mod生效");

                Menu_DeleteVSCheckLine.Text = "删除选中行";
                DataGridCheckBoxColumn_VSCheckList_Enable.Header = "启用";
                DataGridTextColumn_VSCheckList_VSHash.Header = "顶点Shader Hash值";

                Button_ClearVSCheckList.Content = "清空当前VSCheck列表内容";
                Button_UpdateVSCheckList.Content = "更新VSCheck列表";
                Button_GenerateVSCheckIni.Content = "生成VSCheck";

            }
            else
            {
                Menu_File.Title = "File";

                Menu_OpenWorkSpaceGeneratedModFolder.Text = "Open Current WorkSpace's Generated Mod Folder";
                Menu_OpenModsFolder.Text = "Open Mods Folder";
                Menu_Open3DmigotoFolder.Text = "Open 3Dmigoto Folder";
                Menu_OpenD3dxini.Text = "Open d3dx.ini";
                Menu_OpenShaderFixesFolder.Text = "Open ShaderFixes Folder";

                Menu_OpenLatestFrameAnalysisFolder.Text = "Open Latest FrameAnalysis Folder";
                Menu_OpenLatestFrameAnalysisLogTxt.Text = "Open Latest FrameAnalysis log.txt";
                Menu_OpenLatestFrameAnalysisDedupedFolder.Text = "Open Latest FrameAnalysis Folder's deduped Folder";

                Menu_OpenDBMTLocationFolder.Text = "Open SSMT Cache Folder";
                Menu_OpenLogsFolder.Text = "Open Logs Folder";
                Menu_OpenLatestLogFile.Text = "Open Latest Log File";
                Menu_OpenConfigsFolder.Text = "Open Configs Folder";
                Menu_OpenPluginsFolder.Text = "Open Plugins Folder";

                Menu_GameTypeFolder.Text = "Open GameType Folder";
                Menu_OpenAssetsFolder.Text = "Open Assets Folder";
                Menu_OpenGlobalConfigFolder.Text = "Open Global Configs Folder";

                Menu_ToolBox.Title = "ToolBox";
                Menu_AutoDetect_UnityVS_GPUPreSkinning_DrawIBList.Text = "Auto Detect UntiyVS_GPU-PreSkinning DrawIB List";
                Menu_AutoDetect_CPUPreSkinning_DrawIBList.Text = "Auto Detect CPU-PreSkinning DrawIB List";


                Menu_Texture.Title = "Texture";

                Menu_ConvertDDSToTargetFormat.Text = "Batch Convert dds Texture In Target Folder To Target Format";
                Menu_ExtractTextureFiles.Text = "Extract All Kinds Of Textures";

                Menu_ExtractDedupedTextures.Text = "Extract DedupedTextures";
                Menu_ExtractTrianglelistTextures.Text = "Extract TrianglelistTextures";
                Menu_ExtractRenderTextures.Text = "Extract RenderTextures";

                

                //侧边栏
                WorkGameSelectionComboBox.Header = "GameName";
                ComboBoxWorkSpaceSelection.Header = "WorkSpace";
                Button_CreateWorkSpace.Content = "Create WorkSpace";
                Button_OpenWorkSpaceFolder.Content = "Open Current WorkSpace";
                Button_CleanWorkSpace.Content = "Clean Current WorkSpace Files";
                Button_DeleteCurrentWorkSpaceFolder.Content = "Delete Current WorkSpace";

                ToggleSwitch_DumpIBListConfig.OnContent = "Dump Specific IB List";
                ToggleSwitch_DumpIBListConfig.OffContent = "Dump Everything";

                ToggleSwitch_ConvertDedupedTextures.OnContent = "Convert DedupedTextures";
                ToggleSwitch_ConvertDedupedTextures.OffContent = "Not Convert DedupedTextures";

                //主要内容
                TextBlock_DrawIBList.Text = "DrawIB List";

                Menu_DeleteDrawIBLine.Text = "Delete Selected Line";
                DataGridTextColumn_DrawIBList_DrawIB.Header = "DrawIB";
                DataGridTextColumn_DrawIBList_Alias.Header = "Alias";
                Button_ExtractModel.Content = "Extract Model";
                Button_InitializeDrawIBConfig.Content = "Clear DrawIB List";
                Button_SaveDrawIBList.Content = "Save DrawIB List";
                Button_CleanLastExtract.Content = "Clean Last Time Extracted Files";
                Button_AutoDetectGameTypeDrawIBList.Content = "Detect DrawIB List For Selected GameType";

                TextBlock_SkipIBList.Text = "SkipIB List";
                Menu_DeleteSkipIBLine.Text = "Delete Selected Line";
                DataGridTextColumn_SkipIBList_SkipIB.Header = "SkipIB";
                DataGridTextColumn_SkipIBList_Alias.Header = "Alias";

                Button_CleanSkipIBList.Content = "Clear SkipIB List";
                Button_SaveSkipIBList.Content = "Save SkipIB List";
                Button_SkipIBDraw.Content = "Generate SkipIB";

                TextBlock_VSCheckList.Text = "VSCheck List";

                Menu_DeleteVSCheckLine.Text = "Delete Selected Line";
                DataGridCheckBoxColumn_VSCheckList_Enable.Header = "Enable";
                DataGridTextColumn_VSCheckList_VSHash.Header = "Vertex Shader Hash";

                Button_ClearVSCheckList.Content = "Clear VSCheck List";
                Button_UpdateVSCheckList.Content = "Update VSCheck List";
                Button_GenerateVSCheckIni.Content = "Generate VSCheck";

            }
        }

    }
}
