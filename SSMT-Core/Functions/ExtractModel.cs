using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSMT_Core;

namespace SSMT
{
    public partial class CoreFunctions
    {

        public static bool ExtractModel(List<DrawIBItem> DrawIBItemList)
        {

            bool RunResult = false;

            try
            {
                
                LOG.Info("FrameAnalysisFolderPath: " + PathManager.Path_LatestFrameAnalysisFolder);

                

                GameConfig gameConfig = new GameConfig();

                if (gameConfig.LogicName == LogicName.SRMI)
                {
                    //HSR重写渲染管线和Shader，很特殊
                    RunResult = HonkaiStarRail.ExtractHSR32New(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.WWMI)
                {
                    RunResult = WutheringWaves.ExtractWWMI(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.YYSLS)
                {
                    RunResult = YYSLS.ExtractModel(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.CTXMC)
                {
                    RunResult = IdentityV.ExtractCTX(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.IdentityV2)
                {
                    RunResult = IdentityV2.ExtractModel(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.HOK)
                {
                    RunResult = HOK.ExtractModel(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.ZZMI)
                {
                    RunResult = ZenlessZoneZero.ExtractModel(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.UnityCPU)
                {
                    RunResult = UnityCPU.ExtractModel(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.AILIMIT)
                {
                    RunResult = AILimit.ExtractUnityVS(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.UnityCS)
                {
                    RunResult = UnityCS.ExtractUnityVS(DrawIBItemList);
                }
                else if (   gameConfig.LogicName == LogicName.UnityVS)
                {
                    RunResult = UnityVS.ExtractUnityVS(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.NierR)
                {
                    RunResult = NierR.ExtractUnityVS(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.GIMI)
                {
                    RunResult = GenshinImpact.ExtractUnityVS(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.HIMI)
                {
                    RunResult = HonkaiImpact3.ExtractUnityVS(DrawIBItemList);
                }
                else if (gameConfig.LogicName == LogicName.SnowBreak)
                {
                    RunResult = SnowBreak.ExtractModel(DrawIBItemList);
                }
                else
                {
                    LOG.Error("未知的执行逻辑名称: " + gameConfig.LogicName + "\n请先前往主页的游戏设置中指定执行逻辑");
                    RunResult = false;
                }
            }
            catch (Exception ex)
            {
                LOG.Error(ex.ToString());
                RunResult = false;
            }

            return RunResult;
        }
    }
}
