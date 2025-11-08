using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSMT_Core;

namespace SSMT
{

    public static partial class HonkaiStarRail
    {

        private static bool IsPositionBlendSlotMatch(string PointlistIndex, int PositionStride, int BlendStride, int VertexCount, string PositionSlot, string BlendSlot)
        {
            string CST0BufferFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, PointlistIndex + "-" + PositionSlot + "=", ".buf");
            string CST0BufferFilePath = FrameAnalysisLogUtilsV2.Get_DedupedFilePath(CST0BufferFileName, GlobalConfig.Path_LatestFrameAnalysisFolder, GlobalConfig.Path_LatestFrameAnalysisLogTxt);
            int CST0BufferFileSize = (int)DBMTFileUtils.GetFileSize(CST0BufferFilePath);
            int CST0VertexCount = CST0BufferFileSize / PositionStride;
            LOG.Info("BlendStride: " + BlendStride);
            LOG.Info("BlendSlot: " + BlendSlot);
            LOG.Info("PointlistIndex: " + PointlistIndex);

            LOG.Info("Assume VertexCount: " + VertexCount);
            LOG.Info("Real VertexCount: " + CST0VertexCount);

            LOG.NewLine();
            if (CST0VertexCount == VertexCount)
            {
                LOG.Info("Position Slot匹配成功，接下来匹配Blend Slot");
                //此时再判断t5的顶点数和BlendStride是否相等
                string CST5BufferFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, PointlistIndex + "-" + BlendSlot + "=", ".buf");
                string CST5BufferFilePath = FrameAnalysisLogUtilsV2.Get_DedupedFilePath(CST5BufferFileName, GlobalConfig.Path_LatestFrameAnalysisFolder, GlobalConfig.Path_LatestFrameAnalysisLogTxt);
                int CST5BufferFileSize = (int)DBMTFileUtils.GetFileSize(CST5BufferFilePath);
                int CST5VertexCount = CST5BufferFileSize / BlendStride;

                LOG.Info("Assume VertexCount: " + VertexCount);
                LOG.Info("Real VertexCount: " + CST5VertexCount);

                if (CST5VertexCount == VertexCount)
                {
                    LOG.Info("匹配成功");
                    return true;
                }
            }
            LOG.Info("匹配失败");
            return false;
        }


        private static bool IsBlendSlotMatch(string PointlistIndex, int BlendStride, int VertexCount, string BlendSlot)
        {
            LOG.Info("IsBlendSlotMatch:");
            //此时再判断t5的顶点数和BlendStride是否相等
            string CST5BufferFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, PointlistIndex + "-" + BlendSlot + "=", ".buf");
            string CST5BufferFilePath = FrameAnalysisLogUtilsV2.Get_DedupedFilePath(CST5BufferFileName, GlobalConfig.Path_LatestFrameAnalysisFolder, GlobalConfig.Path_LatestFrameAnalysisLogTxt);
            int CST5BufferFileSize = (int)DBMTFileUtils.GetFileSize(CST5BufferFilePath);
            int CST5VertexCount = CST5BufferFileSize / BlendStride;

            LOG.Info("BlendStride: " + BlendStride);
            LOG.Info("BlendSlot: " + BlendSlot);
            LOG.Info("PointlistIndex: " + PointlistIndex);
            LOG.Info("BufferFileName: " + CST5BufferFileName);

            LOG.Info("Assume VertexCount: " + VertexCount);
            LOG.Info("Real VertexCount: " + CST5VertexCount);
            LOG.NewLine();
            if (CST5VertexCount == VertexCount)
            {
                LOG.Info("匹配成功");
                return true;
            }
            
            LOG.Info("匹配失败");
            return false;
        }




        public static string FilterTrianglelistIndex_UnityVS(List<string> TrianglelistIndexList, D3D11GameType d3D11GameType)
        {
            string FinalTrianglelistIndex = "";
            foreach (string TrianglelistIndex in TrianglelistIndexList)
            {
                bool AllSlotExists = true;
                foreach (var item in d3D11GameType.CategoryTopologyDict)
                {
                    string Category = item.Key;
                    string Topology = item.Value;

                    if (Topology != "trianglelist")
                    {
                        continue;
                    }

                    //获取当前Category的Slot
                    string CategorySlot = d3D11GameType.CategorySlotDict[Category];

                    //寻找对应Buf文件
                    string CategoryBufFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, TrianglelistIndex + "-" + CategorySlot, ".buf");
                    if (CategoryBufFileName == "")
                    {
                        AllSlotExists = false;
                        break;
                    }
                }

                if (AllSlotExists)
                {
                    FinalTrianglelistIndex = TrianglelistIndex;
                    break;
                }
            }

            return FinalTrianglelistIndex;
        }

        public static List<D3D11GameTypeWrapper> AutoGameTypeDetect_1c932707d4d8df41(string DrawIB, D3D11GameTypeLv2 d3D11GameTypeLv2, string PointlistIndex, List<string> TrianglelistIndexList)
        {
            List<D3D11GameTypeWrapper> PossibleD3d11GameTypeList = new List<D3D11GameTypeWrapper>();


            //先匹配出正确的数据类型，顺便得到从哪个Slot中提取的。
            foreach (D3D11GameType d3D11GameType in d3D11GameTypeLv2.Ordered_GPU_CPU_D3D11GameTypeList)
            {
                if (!d3D11GameType.GPUPreSkinning)
                {
                    continue;
                }
                string PositionSlot = "";
                string BlendSlot = "";

                D3D11GameTypeWrapper d3D11GameTypeWrapper = new D3D11GameTypeWrapper(d3D11GameType);
                //首先肯定得有vb1槽位，否则无法提取
                bool ExistsVB1Slot = false;
                foreach (var item in d3D11GameType.CategorySlotDict)
                {
                    if (item.Value == "vb1")
                    {
                        ExistsVB1Slot = true;
                        break;
                    }
                }

                if (!ExistsVB1Slot)
                {
                    continue;
                }


                //获取第一个TrianglelistIndex
                string TrianglelistIndex = d3D11GameTypeLv2.FilterTrianglelistIndex_UnityVS(TrianglelistIndexList, d3D11GameType);


                //获取Buffer文件
                string VB1BufferFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, TrianglelistIndex + "-vb1=", ".buf");


                string VB1BufferFilePath = FrameAnalysisLogUtilsV2.Get_DedupedFilePath(VB1BufferFileName, GlobalConfig.Path_LatestFrameAnalysisFolder, GlobalConfig.Path_LatestFrameAnalysisLogTxt);
                int VB1Size = (int)DBMTFileUtils.GetFileSize(VB1BufferFilePath);

                //求出预期顶点数
                int VertexCount = VB1Size / d3D11GameType.CategoryStrideDict["Texcoord"];

                int PositionStride = d3D11GameType.CategoryStrideDict["Position"];
                int BlendStride = d3D11GameType.CategoryStrideDict["Blend"];

                //随后依次判断t0到t5的Position的顶点数
                bool t0t5 = IsPositionBlendSlotMatch(PointlistIndex, PositionStride, BlendStride, VertexCount, "cs-t0", "cs-t5");
                if (t0t5)
                {

                    PositionSlot = "cs-t0";
                    BlendSlot = "cs-t5";
                    LOG.Info("识别到数据类型: " + d3D11GameType.GameTypeName);

                    d3D11GameTypeWrapper.PositionExtractSlot = PositionSlot;
                    d3D11GameTypeWrapper.BlendExtractSlot = BlendSlot;
                    d3D11GameTypeWrapper.PositionExtractIndex = PointlistIndex;
                    d3D11GameTypeWrapper.BlendExtractIndex = PointlistIndex;
                    LOG.Info("PositionSlot: " + PositionSlot);
                    LOG.Info("BlendSlot: " + BlendSlot);
                    LOG.NewLine();
                    PossibleD3d11GameTypeList.Add(d3D11GameTypeWrapper);
                    continue;


                }

                bool t1t6 = IsPositionBlendSlotMatch(PointlistIndex, PositionStride, BlendStride, VertexCount, "cs-t1", "cs-t6");
                if (t1t6)
                {

                    PositionSlot = "cs-t1";
                    BlendSlot = "cs-t6";
                    LOG.Info("识别到数据类型: " + d3D11GameType.GameTypeName);

                    d3D11GameTypeWrapper.PositionExtractSlot = PositionSlot;
                    d3D11GameTypeWrapper.BlendExtractSlot = BlendSlot;
                    d3D11GameTypeWrapper.PositionExtractIndex = PointlistIndex;
                    d3D11GameTypeWrapper.BlendExtractIndex = PointlistIndex;
                    LOG.Info("PositionSlot: " + PositionSlot);
                    LOG.Info("BlendSlot: " + BlendSlot);
                    LOG.NewLine();
                    PossibleD3d11GameTypeList.Add(d3D11GameTypeWrapper);
                    continue;
                }

                bool t2t7 = IsPositionBlendSlotMatch(PointlistIndex, PositionStride, BlendStride, VertexCount, "cs-t2", "cs-t7");
                if (t2t7)
                {

                    PositionSlot = "cs-t2";
                    BlendSlot = "cs-t7";
                    LOG.Info("识别到数据类型: " + d3D11GameType.GameTypeName);
                    d3D11GameTypeWrapper.PositionExtractSlot = PositionSlot;
                    d3D11GameTypeWrapper.BlendExtractSlot = BlendSlot;
                    d3D11GameTypeWrapper.PositionExtractIndex = PointlistIndex;
                    d3D11GameTypeWrapper.BlendExtractIndex = PointlistIndex;
                    LOG.Info("PositionSlot: " + PositionSlot);
                    LOG.Info("BlendSlot: " + BlendSlot);
                    LOG.NewLine();
                    PossibleD3d11GameTypeList.Add(d3D11GameTypeWrapper);
                    continue;
                }

                bool t3t8 = IsPositionBlendSlotMatch(PointlistIndex, PositionStride, BlendStride, VertexCount, "cs-t3", "cs-t8");
                if (t3t8)
                {

                    PositionSlot = "cs-t3";
                    BlendSlot = "cs-t8";
                    LOG.Info("识别到数据类型: " + d3D11GameType.GameTypeName);
                    d3D11GameTypeWrapper.PositionExtractSlot = PositionSlot;
                    d3D11GameTypeWrapper.BlendExtractSlot = BlendSlot;
                    d3D11GameTypeWrapper.PositionExtractIndex = PointlistIndex;
                    d3D11GameTypeWrapper.BlendExtractIndex = PointlistIndex;
                    LOG.Info("PositionSlot: " + PositionSlot);
                    LOG.Info("BlendSlot: " + BlendSlot);
                    LOG.NewLine();
                    PossibleD3d11GameTypeList.Add(d3D11GameTypeWrapper);
                    continue;
                }

                bool t4t9 = IsPositionBlendSlotMatch(PointlistIndex, PositionStride, BlendStride, VertexCount, "cs-t4", "cs-t9");
                if (t4t9)
                {
                    PositionSlot = "cs-t4";
                    BlendSlot = "cs-t9";
                    LOG.Info("识别到数据类型: " + d3D11GameType.GameTypeName);

                    d3D11GameTypeWrapper.PositionExtractSlot = PositionSlot;
                    d3D11GameTypeWrapper.BlendExtractSlot = BlendSlot;
                    d3D11GameTypeWrapper.PositionExtractIndex = PointlistIndex;
                    d3D11GameTypeWrapper.BlendExtractIndex = PointlistIndex;
                    LOG.Info("PositionSlot: " + PositionSlot);
                    LOG.Info("BlendSlot: " + BlendSlot);
                    LOG.NewLine();
                    PossibleD3d11GameTypeList.Add(d3D11GameTypeWrapper);

                    continue;
                }
                LOG.NewLine();


            }
            return PossibleD3d11GameTypeList;
        }

        public static List<D3D11GameTypeWrapper> AutoGameTypeDetect_fee307b98a965c16_Universal(string DrawIB, D3D11GameTypeLv2 d3D11GameTypeLv2, string PointlistIndex, List<string> TrianglelistIndexList)
        {
            //这里的数据类型算法仍然有问题，我们必须获取Texcoord所在的Category
            //然后因为这个Category必定在Trianglelist中出现，所以我们可以直接获取到


            List<D3D11GameTypeWrapper> PossibleD3d11GameTypeList = new List<D3D11GameTypeWrapper>();

            bool findAtLeastOneGPUType = false;
            //先匹配出正确的数据类型，顺便得到从哪个Slot中提取的。
            foreach (D3D11GameType d3D11GameType in d3D11GameTypeLv2.Ordered_GPU_CPU_D3D11GameTypeList)
            {
           
                D3D11GameTypeWrapper d3D11GameTypeWrapper = new D3D11GameTypeWrapper(d3D11GameType);


                if (findAtLeastOneGPUType && !d3D11GameType.GPUPreSkinning)
                {
                    LOG.Info("自动优化:已经找到了满足条件的GPU类型，所以这个CPU类型就不用判断了");
                    continue;
                }

                LOG.Info("当前数据类型:" + d3D11GameType.GameTypeName);

                //传递过来一堆TrianglelistIndex，但是我们要找到满足条件的那个,即Buffer文件都存在的那个
                string TrianglelistIndex = FilterTrianglelistIndex_UnityVS(TrianglelistIndexList, d3D11GameType);
                LOG.Info("TrianglelistIndex: " + TrianglelistIndex);


                if (TrianglelistIndex == "")
                {
                    LOG.Info("当前GameType无法找到符合槽位存在条件的TrianglelistIndex，跳过此项");
                    continue;
                }

                //获取每个Category的Buffer文件
                Dictionary<string, string> CategoryBufFileMap = new Dictionary<string, string>();
                Dictionary<string, int> CategoryBufFileSizeMap = new Dictionary<string, int>();
                bool AllFileExists = true;
                foreach (var item in d3D11GameType.CategoryTopologyDict)
                {
                    string CategoryName = item.Key;
                    string ExtractIndex = TrianglelistIndex;
                    int Stride = d3D11GameType.CategoryStrideDict[CategoryName];
                    if (item.Value == "pointlist" && PointlistIndex != "")
                    {
                        ExtractIndex = PointlistIndex;
                    }
                    string CategorySlot = d3D11GameType.CategorySlotDict[CategoryName];


                    if (CategoryName == "Position")
                    {
                        d3D11GameTypeWrapper.PositionExtractIndex = ExtractIndex;
                        d3D11GameTypeWrapper.PositionExtractSlot = CategorySlot;
                    }
                    else if (CategoryName == "Blend")
                    {
                        d3D11GameTypeWrapper.BlendExtractIndex = ExtractIndex;
                        d3D11GameTypeWrapper.BlendExtractSlot = CategorySlot;
                    }


                    LOG.Info("当前分类:" + CategoryName + " 提取Index: " + ExtractIndex + " 提取槽位:" + CategorySlot);
                    //获取文件名存入对应Dict
                    string CategoryBufFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, ExtractIndex + "-" + CategorySlot, ".buf");

                    LOG.Info("CategoryBufFileName: " + CategoryBufFileName);
                    CategoryBufFileMap[item.Key] = CategoryBufFileName;

                    //获取文件大小存入对应Dict
                    string CategoryBufFilePath = FrameAnalysisLogUtilsV2.Get_DedupedFilePath(CategoryBufFileName, GlobalConfig.WorkFolder, GlobalConfig.Path_LatestFrameAnalysisLogTxt);
                    LOG.Info("Category: " + item.Key + " Buf File:" + CategoryBufFilePath);

                    if (!File.Exists(CategoryBufFilePath))
                    {
                        LOG.Error("对应Buffer文件未找到,此数据类型无效。");
                        AllFileExists = false;
                        break;
                    }

                    long FileSize = (long)DBMTFileUtils.GetRealFileSize_NullTerminated_ByStride(CategoryBufFilePath, Stride);
                    CategoryBufFileSizeMap[item.Key] = (int)FileSize;
                }

                if (!AllFileExists)
                {
                    LOG.Info("当前数据类型的部分槽位文件无法找到，跳过此数据类型识别。");
                    continue;
                }

                //校验顶点数是否在各Buffer中保持一致
                //TODO 通过校验顶点数的方式并不能100%确定，因为如果只有一个Category的话就会无法匹配步长
                int VertexNumber = 0;
                bool AllMatch = true;

                foreach (string CategoryName in d3D11GameType.OrderedCategoryNameList)
                {
                    int CategoryStride = d3D11GameType.CategoryStrideDict[CategoryName];
                    int FileSize = CategoryBufFileSizeMap[CategoryName];
                    int TmpNumber = FileSize / CategoryStride;
                    LOG.Info("CategoryName: " + CategoryName + " CategoryStride: " + CategoryStride.ToString());
                    LOG.Info("FileSize: " + FileSize.ToString() + "  TmpNumber: " + TmpNumber.ToString());

                    if (TmpNumber == 0)
                    {
                        LOG.Info("槽位的文件大小不能为0，槽位匹配失败，跳过此数据类型");
                        AllMatch = false;
                        break;
                    }

                    if (!d3D11GameType.GPUPreSkinning)
                    {
                        //IdentityV: 使用精准匹配机制来过滤数据类型，如果有余数，说明此分类不匹配。
                        int YuShu = FileSize % CategoryStride;
                        if (YuShu != 0)
                        {
                            LOG.Error("余数不为0: " + YuShu.ToString() + "  ，文件步长除以类别步长，不能含有余数，否则为不支持的匹配方式，比如PatchNull，或者数据类型匹配错误，类型错误时自然会产生余数。");
                            AllMatch = false;
                            break;
                        }
                    }

                    if (!d3D11GameType.GPUPreSkinning)
                    {
                        string CategorySlot = d3D11GameType.CategorySlotDict[CategoryName];
                        string CategoryTxtFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, TrianglelistIndex + "-" + CategorySlot, ".txt");
                        if (CategoryTxtFileName == "")
                        {
                            LOG.Info("槽位的txt文件不存在，跳过此数据类型。");
                            AllMatch = false;
                            break;
                        }
                        else
                        {
                            string CategoryTxtFilePath = FrameAnalysisLogUtilsV2.Get_DedupedFilePath(CategoryTxtFileName, GlobalConfig.Path_LatestFrameAnalysisFolder, GlobalConfig.Path_LatestFrameAnalysisLogTxt);
                            string VertexCountTxtShow = DBMTFileUtils.FindMigotoIniAttributeInFile(CategoryTxtFilePath, "vertex count");
                            int TxtShowVertexCount = int.Parse(VertexCountTxtShow);
                            LOG.Info("TxtShowVertexCount: " + TxtShowVertexCount);
                            if (TmpNumber < TxtShowVertexCount)
                            {
                                LOG.Info("Buffer数据类型统计顶点数小于槽位的txt文件展示顶点数，跳过此数据类型。");
                                AllMatch = false;
                                break;
                            }

                            if (TxtShowVertexCount != TmpNumber && d3D11GameType.CategorySlotDict.Count > 1)
                            {
                                LOG.Info("槽位的txt文件顶点数与Buffer数据类型统计顶点数不符，跳过此数据类型 Count> 1。");
                                AllMatch = false;
                                break;
                            }
                        }
                    }


                    if (VertexNumber == 0)
                    {
                        VertexNumber = TmpNumber;
                    }
                    else if (VertexNumber != TmpNumber)
                    {
                        LOG.Info("VertexNumber: " + VertexNumber.ToString() + " 当前槽位数量: " + TmpNumber.ToString());
                        LOG.Info("槽位匹配失败");
                        LOG.NewLine();

                        AllMatch = false;
                        break;
                    }
                    else
                    {
                        LOG.Info(CategoryName + " Match!");
                        LOG.NewLine();
                    }
                }

                //LOG.Info("VertexNumber: " + VertexNumber.ToString());


                if (AllMatch)
                {
                    LOG.NewLine("MatchGameType: " + d3D11GameType.GameTypeName);
                    PossibleD3d11GameTypeList.Add(d3D11GameTypeWrapper);
                }

                //如果找到了一个GPUPreSkinning就标记一下，这样后面就不会匹配CPU类型了。
                if (!findAtLeastOneGPUType)
                {
                    foreach (D3D11GameTypeWrapper d3D11GameTypeWrapper2 in PossibleD3d11GameTypeList)
                    {
                        if (d3D11GameTypeWrapper2.d3d11GameType.GPUPreSkinning)
                        {
                            findAtLeastOneGPUType = true;
                            break;
                        }
                    }
                }




                LOG.NewLine();

            }




            return PossibleD3d11GameTypeList;
        }




        public static List<D3D11GameTypeWrapper> AutoGameTypeDetect_4d9c23fd387846c7(string DrawIB, D3D11GameTypeLv2 d3D11GameTypeLv2, string PointlistIndex, List<string> TrianglelistIndexList)
        {
            List<D3D11GameTypeWrapper> PossibleD3d11GameTypeList = new List<D3D11GameTypeWrapper>();


            //先匹配出正确的数据类型，顺便得到从哪个Slot中提取的。
            foreach (D3D11GameType d3D11GameType in d3D11GameTypeLv2.Ordered_GPU_CPU_D3D11GameTypeList)
            {

                if (!d3D11GameType.GPUPreSkinning)
                {
                    LOG.Info("不是GPU PreSkinning，跳过处理");
                    continue;
                }
                else {
                    LOG.Info("是GPU-PreSkinning类型");
                }


                string PositionSlot = "";
                string BlendSlot = "";
                string PositionExtractIndex = "";

                D3D11GameTypeWrapper d3D11GameTypeWrapper = new D3D11GameTypeWrapper(d3D11GameType);
                LOG.Info("当前匹配数据类型: " + d3D11GameType.GameTypeName);
                //首先肯定得有vb1槽位，否则无法提取
                bool ExistsVB1Slot = false;
                foreach (var item in d3D11GameType.CategorySlotDict)
                {
                    if (item.Value == "vb1")
                    {
                        ExistsVB1Slot = true;
                        break;
                    }
                }

                if (!ExistsVB1Slot)
                {
                    continue;
                }


                //获取满足条件的TrianglelistIndex
                string TrianglelistIndex = d3D11GameTypeLv2.FilterTrianglelistIndex_UnityVS(TrianglelistIndexList, d3D11GameType);

                //获取并且输出txt文件中的顶点数
                string VB1TxtFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, TrianglelistIndex + "-vb1=", ".txt");
                string VB1TxtFilePath = FrameAnalysisLogUtilsV2.Get_DedupedFilePath(VB1TxtFileName, GlobalConfig.Path_LatestFrameAnalysisFolder, GlobalConfig.Path_LatestFrameAnalysisLogTxt);
                LOG.Info("VB1TxtFileName: " + VB1TxtFileName);
                LOG.Info("VB1TxtFilePath: " + VB1TxtFilePath);


                string ShowVertexCount = DBMTFileUtils.FindMigotoIniAttributeInFile(VB1TxtFilePath, "vertex count");
                LOG.Info("VB1 Txt ShowVertexCount: " + ShowVertexCount);

                //获取Buffer文件
                LOG.Info("Txt读取完毕，查找其buf文件并根据步长计算预期顶点数：");
                string VB1BufferFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, TrianglelistIndex + "-vb1=", ".buf");
                string VB1BufferFilePath = FrameAnalysisLogUtilsV2.Get_DedupedFilePath(VB1BufferFileName, GlobalConfig.Path_LatestFrameAnalysisFolder, GlobalConfig.Path_LatestFrameAnalysisLogTxt);
                LOG.Info("VB1BufferFileName: " + VB1BufferFileName);
                LOG.Info("VB1BufferFilePath: " + VB1BufferFilePath);


                int VB1Size = (int)DBMTFileUtils.GetFileSize(VB1BufferFilePath);

                //求出预期顶点数
                int VertexCount = VB1Size / d3D11GameType.CategoryStrideDict["Texcoord"];
                LOG.Info("预期顶点数: " + VertexCount.ToString());

                int PositionStride = d3D11GameType.CategoryStrideDict["Position"];
                int BlendStride = d3D11GameType.CategoryStrideDict["Blend"];

                //随后依次判断t0到t6的Position的顶点数,对应u0到u6
                //bool BlendT0 = IsBlendSlotMatch(PointlistIndex, BlendStride, VertexCount, "cs-t5");
                bool BlendT0 = IsPositionBlendSlotMatch(PointlistIndex,PositionStride, BlendStride, VertexCount,"cs-t0", "cs-t5");
                if (BlendT0)
                {
                    BlendSlot = "cs-t5";
                    PositionSlot = "cs-t0";
                    d3D11GameTypeWrapper.PositionExtractSlot = PositionSlot;
                    d3D11GameTypeWrapper.BlendExtractSlot = BlendSlot;
                    d3D11GameTypeWrapper.PositionExtractIndex = PointlistIndex;
                    d3D11GameTypeWrapper.BlendExtractIndex = PointlistIndex;

                    
                    LOG.Info("PositionSlot: " + PositionSlot);
                    LOG.Info("BlendSlot: " + BlendSlot);
                    //寻找Position的上一个Hash
                    string Index = PointlistIndex;
                    if (Index != "")
                    {
                        PositionExtractIndex = Index;
                        PossibleD3d11GameTypeList.Add(d3D11GameTypeWrapper);
                        continue;
                    }
                }

                bool BlendT1 = IsPositionBlendSlotMatch(PointlistIndex, PositionStride, BlendStride, VertexCount, "cs-t1", "cs-t6");
                if (BlendT1)
                {
                    BlendSlot = "cs-t6";
                    PositionSlot = "cs-t1";
                    d3D11GameTypeWrapper.PositionExtractSlot = PositionSlot;
                    d3D11GameTypeWrapper.BlendExtractSlot = BlendSlot;
                    d3D11GameTypeWrapper.PositionExtractIndex = PointlistIndex;
                    d3D11GameTypeWrapper.BlendExtractIndex = PointlistIndex;
                    LOG.Info("PositionSlot: " + PositionSlot);
                    LOG.Info("BlendSlot: " + BlendSlot);
                    //寻找Position的上一个Hash
                    string Index = PointlistIndex;
                    if (Index != "")
                    {
                        PositionExtractIndex = Index;
                        PossibleD3d11GameTypeList.Add(d3D11GameTypeWrapper);
                        continue;
                    }
                }

                bool BlendT2 = IsPositionBlendSlotMatch(PointlistIndex, PositionStride, BlendStride, VertexCount, "cs-t2", "cs-t7");
                if (BlendT2)
                {
                    BlendSlot = "cs-t7";
                    PositionSlot = "cs-t2";
                    d3D11GameTypeWrapper.PositionExtractSlot = PositionSlot;
                    d3D11GameTypeWrapper.BlendExtractSlot = BlendSlot;
                    d3D11GameTypeWrapper.PositionExtractIndex = PointlistIndex;
                    d3D11GameTypeWrapper.BlendExtractIndex = PointlistIndex;
                    LOG.Info("PositionSlot: " + PositionSlot);
                    LOG.Info("BlendSlot: " + BlendSlot);
                    //寻找Position的上一个Hash
                    string Index = PointlistIndex;
                    if (Index != "")
                    {
                        PositionExtractIndex = Index;
                        PossibleD3d11GameTypeList.Add(d3D11GameTypeWrapper);
                        continue;
                    }
                }

                bool BlendT3 = IsPositionBlendSlotMatch(PointlistIndex, PositionStride, BlendStride, VertexCount, "cs-t3", "cs-t8");
                if (BlendT3)
                {
                    BlendSlot = "cs-t8";
                    PositionSlot = "cs-t3";
                    d3D11GameTypeWrapper.PositionExtractSlot = PositionSlot;
                    d3D11GameTypeWrapper.BlendExtractSlot = BlendSlot;
                    d3D11GameTypeWrapper.PositionExtractIndex = PointlistIndex;
                    d3D11GameTypeWrapper.BlendExtractIndex = PointlistIndex;
                    LOG.Info("PositionSlot: " + PositionSlot);
                    LOG.Info("BlendSlot: " + BlendSlot);
                    //寻找Position的上一个Hash
                    string Index = PointlistIndex;
                    if (Index != "")
                    {
                        PositionExtractIndex = Index;
                        PossibleD3d11GameTypeList.Add(d3D11GameTypeWrapper);
                        continue;
                    }
                }

                bool BlendT4 = IsPositionBlendSlotMatch(PointlistIndex, PositionStride, BlendStride, VertexCount, "cs-t4", "cs-t9");
                if (BlendT4)
                {
                    BlendSlot = "cs-t9";
                    PositionSlot = "cs-t4";

                    d3D11GameTypeWrapper.PositionExtractSlot = PositionSlot;
                    d3D11GameTypeWrapper.BlendExtractSlot = BlendSlot;
                    d3D11GameTypeWrapper.PositionExtractIndex = PointlistIndex;
                    d3D11GameTypeWrapper.BlendExtractIndex = PointlistIndex;
                    LOG.Info("PositionSlot: " + PositionSlot);
                    LOG.Info("BlendSlot: " + BlendSlot);

                    //寻找Position的上一个Hash
                    string Index = PointlistIndex;
                    if (Index != "")
                    {
                        PositionExtractIndex = Index;
                        PossibleD3d11GameTypeList.Add(d3D11GameTypeWrapper);
                        continue;
                    }
                }


                LOG.NewLine();

            }
            return PossibleD3d11GameTypeList;
        }

        private static bool Extract_Model_New(string DrawIB, List<D3D11GameTypeWrapper> d3D11GameTypeWrapperList, string PointlistIndex, List<string> TrianglelistIndexList)
        {

            LOG.Info("识别到的数据类型: ");
            foreach (D3D11GameTypeWrapper d3D11GameTypeWrapper in d3D11GameTypeWrapperList)
            {
                LOG.Info(d3D11GameTypeWrapper.d3d11GameType.GameTypeName);
            }
            LOG.NewLine();


            if (d3D11GameTypeWrapperList.Count == 0)
            {
                LOG.Error("无法找到任何已知数据类型，请进行添加");
                return false;
            }

            //直接走提取逻辑
            //接下来提取出每一种可能性
            //读取一个MatchFirstIndex_IBFileName_Dict
            SortedDictionary<int, string> MatchFirstIndex_IBTxtFileName_Dict = new SortedDictionary<int, string>();
            foreach (string TrianglelistIndex in TrianglelistIndexList)
            {
                string IBFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, TrianglelistIndex + "-ib", ".txt");
                if (IBFileName == "")
                {
                    continue;
                }
                LOG.Info("IBFileName: " + IBFileName);
                string IBFilePath = FrameAnalysisLogUtilsV2.Get_DedupedFilePath(IBFileName, GlobalConfig.WorkFolder, GlobalConfig.Path_LatestFrameAnalysisLogTxt);
                
                LOG.Info("IBFilePath: " + IBFilePath);

                IndexBufferTxtFile IBTxtFile = new IndexBufferTxtFile(IBFilePath, false);
                MatchFirstIndex_IBTxtFileName_Dict[int.Parse(IBTxtFile.FirstIndex)] = IBFileName;
            }

            foreach (var item in MatchFirstIndex_IBTxtFileName_Dict)
            {
                LOG.Info("MatchFirstIndex: " + item.Key.ToString() + " IBFileName: " + item.Value);
            }
            LOG.NewLine();

            foreach (D3D11GameTypeWrapper d3D11GameTypeWrapper in d3D11GameTypeWrapperList)
            {

                LOG.Info("Get PositionSlot: " + d3D11GameTypeWrapper.PositionExtractSlot);
                LOG.Info("Get PositionIndex: " + d3D11GameTypeWrapper.PositionExtractIndex);
                LOG.Info("Get BlendSlot: " + d3D11GameTypeWrapper.BlendExtractSlot);
                LOG.Info("Get BlendIndex: " + d3D11GameTypeWrapper.BlendExtractIndex);

                D3D11GameType d3D11GameType = d3D11GameTypeWrapper.d3d11GameType;
                //先设置一下CategorySlot，因为可能数据类型文件里面没写
                d3D11GameType.CategorySlotDict["Position"] = d3D11GameTypeWrapper.PositionExtractSlot;
                d3D11GameType.CategorySlotDict["Blend"] = d3D11GameTypeWrapper.BlendExtractSlot;

                string TrianglelistIndex = FilterTrianglelistIndex_UnityVS(TrianglelistIndexList, d3D11GameType);

                Dictionary<string, string> CategoryBufFileMap = new Dictionary<string, string>();
                foreach (var item in d3D11GameType.CategoryTopologyDict)
                {
                    string CategoryName = item.Key;
                    string ExtractIndex = TrianglelistIndex;
                    if (item.Value == "pointlist" && PointlistIndex != "")
                    {
                        ExtractIndex = PointlistIndex;

                        if (CategoryName == "Position")
                        {
                            ExtractIndex = d3D11GameTypeWrapper.PositionExtractIndex;
                        }
                        else if (CategoryName == "Blend")
                        {
                            ExtractIndex = d3D11GameTypeWrapper.BlendExtractIndex;
                        }
                    }

                    LOG.Info("Final ExtractIndex: " + ExtractIndex);
                    string CategorySlot = d3D11GameType.CategorySlotDict[item.Key];

                    if (CategoryName == "Position")
                    {
                        CategorySlot = d3D11GameTypeWrapper.PositionExtractSlot;
                    }
                    else if (CategoryName == "Blend")
                    {
                        CategorySlot = d3D11GameTypeWrapper.BlendExtractSlot;
                    }

                    LOG.Info("Final CategorySlot: " + CategorySlot);

                    //获取文件名存入对应Dict
                    string CategoryBufFileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, ExtractIndex + "-" + CategorySlot + "=", ".buf");

                    LOG.Info("Get CategoryBufFileName :" + CategoryBufFileName);

                    CategoryBufFileMap[item.Key] = CategoryBufFileName;
                }

                string GameTypeFolderName = "TYPE_" + d3D11GameType.GameTypeName;
                string DrawIBFolderPath = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\");
                string GameTypeOutputPath = Path.Combine(DrawIBFolderPath, GameTypeFolderName + "\\");
                if (!Directory.Exists(GameTypeOutputPath))
                {
                    Directory.CreateDirectory(GameTypeOutputPath);
                }

                LOG.Info("开始从各个Buffer文件中读取数据:");
                //接下来从各个Buffer中读取并且拼接为FinalVB0

                List<Dictionary<int, byte[]>> BufDictList = new List<Dictionary<int, byte[]>>();
                foreach (var item in CategoryBufFileMap)
                {
                    string CategoryName = item.Key;
                    string CategoryBufFileName = item.Value;
                    string CategoryBufFilePath = FrameAnalysisLogUtilsV2.Get_DedupedFilePath(CategoryBufFileName, GlobalConfig.Path_LatestFrameAnalysisFolder, GlobalConfig.Path_LatestFrameAnalysisLogTxt);
                    int CategoryStride = d3D11GameType.CategoryStrideDict[CategoryName];

                    LOG.Info("CategoryName: " + CategoryName);
                    LOG.Info("CategoryBufFileName: " + CategoryBufFileName);
                    LOG.Info("CategoryBufFilePath: " + CategoryBufFilePath);
                    LOG.Info("CategoryStride: " + CategoryStride);

                    Dictionary<int, byte[]> BufDict = new Dictionary<int, byte[]>();

                    //如果只有BLENDINDICES没有BLENDWEIGHTS，此时BLENDINDICES可能是空的00，所以我们参数填写不进行校验
                    if (!d3D11GameType.ElementNameD3D11ElementDict.ContainsKey("BLENDWEIGHTS") && d3D11GameType.ElementNameD3D11ElementDict.ContainsKey("BLENDINDICES"))
                    {
                        LOG.Info("因为只有BLENDINDICES，不对BLENDINDICES进行校验");
                        BufDict = DBMTBinaryUtils.ReadBinaryFileByStride(CategoryBufFilePath, CategoryStride, false);
                    }
                    else
                    {
                        BufDict = DBMTBinaryUtils.ReadBinaryFileByStride(CategoryBufFilePath, CategoryStride, true);
                    }
                    LOG.Info("BufDict Size: " + BufDict.Count.ToString());

                    BufDictList.Add(BufDict);
                }
                LOG.NewLine();

                Dictionary<int, byte[]> MergedVB0Dict = DBMTBinaryUtils.MergeByteDicts(BufDictList);
                int OriginalVertexCount = MergedVB0Dict.Count;
                byte[] FinalVB0 = DBMTBinaryUtils.MergeDictionaryValues(MergedVB0Dict);

                //接下来遍历MatchFirstIndex_IBFileName的Map，对于每个MarchFirstIndex
                //都读取IBTxt文件里的数值，然后进行分割并输出。
                int OutputCount = 1;
                foreach (var item in MatchFirstIndex_IBTxtFileName_Dict)
                {
                    int MatchFirstIndex = item.Key;
                    string IBTxtFileName = item.Value;
                    //拼接出一个IBBufFileName
                    string IBBufFileName = Path.GetFileNameWithoutExtension(IBTxtFileName) + ".buf";

                    string IBTxtFilePath = FrameAnalysisLogUtilsV2.Get_DedupedFilePath(IBTxtFileName, GlobalConfig.Path_LatestFrameAnalysisFolder, GlobalConfig.Path_LatestFrameAnalysisLogTxt);
                    string IBBufFilePath = FrameAnalysisLogUtilsV2.Get_DedupedFilePath(IBBufFileName, GlobalConfig.Path_LatestFrameAnalysisFolder, GlobalConfig.Path_LatestFrameAnalysisLogTxt);

                    IndexBufferTxtFile IBTxtFile = new IndexBufferTxtFile(IBTxtFilePath, true);
                    LOG.Info(IBTxtFilePath);
                    LOG.Info("FirstIndex: " + IBTxtFile.FirstIndex);
                    LOG.Info("IndexCount: " + IBTxtFile.IndexCount);

                    string NamePrefix = DrawIB + "-" + OutputCount.ToString();

                    string OutputIBBufFilePath = Path.Combine(GameTypeOutputPath, NamePrefix + ".ib");
                    string OutputVBBufFilePath = Path.Combine(GameTypeOutputPath, NamePrefix + ".vb");
                    string OutputFmtFilePath = Path.Combine(GameTypeOutputPath, NamePrefix + ".fmt");

                    //通过D3D11GameType合成一个FMT文件并且输出
                    FmtFile fmtFile = new FmtFile(d3D11GameType);
                    fmtFile.OutputFmtFile(OutputFmtFilePath);

                    //写出IBBufFile
                    IndexBufferBufFile IBBufFile = new IndexBufferBufFile(IBBufFilePath, IBTxtFile.Format);


                    //这里使用IndexNumberCount的话，只能用于正向提取
                    //如果要兼容逆向提取，需要换成IndexCount
                    //但是还有个问题，那就是即使换成IndexCount，如果IB文件的替换不是一个整体的Buffer，而是各个独立分开的Buffer
                    //则这里的SelfDivide是不应该存在的步骤，所以这里是无法逆向提取的。
                    //综合来看，逆向提取其实是一种适用性不强，并且很容易受到ini中各种因素干扰的提取方式
                    //但是如果能获取到DrawIndexed的具体数值呢？可以通过解析log.txt的方式进行获取
                    //但是解析很玛法，而且就算能获取到，那如果有复杂的CommandList混淆，投入与产出不成正比了就
                    //使用逆向Mod的ini的方式更加优雅。
                    IBBufFile.SelfDivide(int.Parse(IBTxtFile.FirstIndex), (int)IBTxtFile.IndexNumberCount);
                    IBBufFile.SaveToFile_UInt32(OutputIBBufFilePath, -1 * (int)IBBufFile.MinNumber);

                    //写出VBBufFile
                    VertexBufferBufFile VBBufFile = new VertexBufferBufFile(FinalVB0);
                    if (IBBufFile.MinNumber > IBBufFile.MaxNumber)
                    {
                        LOG.Error("当前IB文件最小值大于IB文件中的最大值，疑似逆向提取Mod模型出错，跳过此模型输出。");
                    }

                    VBBufFile.SelfDivide((int)IBBufFile.MinNumber, (int)IBBufFile.MaxNumber, d3D11GameType.GetSelfStride());
                    VBBufFile.SaveToFile(OutputVBBufFilePath);

                    OutputCount += 1;
                }


                ImportJson importJson = new ImportJson();
                string VB0FileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, TrianglelistIndex + "-vb0", ".txt");

                importJson.DrawIB = DrawIB;
                importJson.OriginalVertexCount = OriginalVertexCount;
                importJson.VertexLimitVB = VB0FileName.Substring(11, 8);
                importJson.d3D11GameType = d3D11GameType;
                importJson.Category_BufFileName_Dict = CategoryBufFileMap;
                importJson.MatchFirstIndex_IBTxtFileName_Dict = MatchFirstIndex_IBTxtFileName_Dict;

                //TODO 暂时叫tmp.json，后面再改
                string ImportJsonSavePath = Path.Combine(GameTypeOutputPath, "tmp.json");
                importJson.SaveToFile(ImportJsonSavePath);
            }

            LOG.NewLine();

            return true;
        }



        public static bool ExtractHSR32New(List<DrawIBItem> DrawIBItemList)
        {
            GameConfig gameConfig = new GameConfig();
            D3D11GameTypeLv2 d3D11GameTypeLv2 = new D3D11GameTypeLv2(gameConfig.GameTypeName);

            LOG.Info("开始提取 HSR 3.2 测试:");
            foreach (DrawIBItem drawIBItem in DrawIBItemList)
            {
                string DrawIB = drawIBItem.DrawIB;

                if (DrawIB.Trim() == "")
                {
                    continue;
                }
                else
                {
                    LOG.Info("当前DrawIB: " + DrawIB);
                }
                LOG.NewLine();

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


                /*
                 * 接下来要判断是否是脸部和头发的特殊Shader
                 因为在崩铁3.2版本更新后，有多种ComputeShader分别负责不同部分的渲染。
                    - 脸部、头发 1c932707d4d8df41
                    - 身体
                    - 组队界面多角色同时渲染 1c932707d4d8df41
                    - NPC集体渲染
                 */

                string CSCB0FileName = FrameAnalysisDataUtils.FilterFirstFile(GlobalConfig.WorkFolder, PointlistIndex + "-cs-cb0=", ".buf");
                if (CSCB0FileName.Contains("1c932707d4d8df41"))
                {
                    LOG.NewLine("执行提取:1c932707d4d8df41");
                    //t0t5到t4t9特殊提取
                    List<D3D11GameTypeWrapper> d3D11GameTypeWrapperList = AutoGameTypeDetect_1c932707d4d8df41(DrawIB, d3D11GameTypeLv2, PointlistIndex, TrianglelistIndexList);
                    bool result = Extract_Model_New(DrawIB, d3D11GameTypeWrapperList, PointlistIndex, TrianglelistIndexList);
                    if (!result)
                    {
                        return false;
                    }
                }

                //组队界面的，比较特殊，需要重新写提取逻辑。
                else if (CSCB0FileName.Contains("d50694eedd2a8595"))
                {
                    LOG.NewLine("执行提取:d50694eedd2a8595");
                    List<D3D11GameTypeWrapper> d3D11GameTypeWrapperList = AutoGameTypeDetect_d50694eedd2a8595(DrawIB, d3D11GameTypeLv2, PointlistIndex, TrianglelistIndexList);

                    bool result = Extract_Model_New(DrawIB, d3D11GameTypeWrapperList, PointlistIndex, TrianglelistIndexList);
                    if (!result)
                    {
                        return false;
                    }
                }
                else if (CSCB0FileName.Contains("4d9c23fd387846c7"))
                {
                    LOG.NewLine("执行提取:4d9c23fd387846c7");

                    List<D3D11GameTypeWrapper> d3D11GameTypeWrapperList = AutoGameTypeDetect_4d9c23fd387846c7(DrawIB, d3D11GameTypeLv2, PointlistIndex, TrianglelistIndexList);

                    bool result = Extract_Model_New(DrawIB, d3D11GameTypeWrapperList, PointlistIndex, TrianglelistIndexList);
                    if (!result)
                    {
                        return false;
                    }
                }
                //c9f2b46571d22858 新增的Shader
                else if (CSCB0FileName.Contains("c9f2b46571d22858"))
                {
                    LOG.NewLine("执行提取:4d9c23fd387846c7 的同类分支: c9f2b46571d22858");

                    List<D3D11GameTypeWrapper> d3D11GameTypeWrapperList = AutoGameTypeDetect_4d9c23fd387846c7(DrawIB, d3D11GameTypeLv2, PointlistIndex, TrianglelistIndexList);

                    bool result = Extract_Model_New(DrawIB, d3D11GameTypeWrapperList, PointlistIndex, TrianglelistIndexList);
                    if (!result)
                    {
                        return false;
                    }
                }

                else
                {

                    LOG.NewLine("执行提取:通用提取fee307b98a965c16");

                    List<D3D11GameTypeWrapper> d3D11GameTypeWrapperList = AutoGameTypeDetect_fee307b98a965c16_Universal(DrawIB, d3D11GameTypeLv2, PointlistIndex, TrianglelistIndexList);

                    //普通提取，主要是为了支持CPU类型，但是它和另一个是通用的
                    bool result = Extract_Model_New(DrawIB, d3D11GameTypeWrapperList, PointlistIndex, TrianglelistIndexList);
                    if (!result)
                    {
                        return false;
                    }
                }

            }




            LOG.NewLine("提取正常执行完成");
            return true;
        }
    }
}
