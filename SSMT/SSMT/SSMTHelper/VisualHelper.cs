using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WinUI3Helper
{
    public static class VisualHelper
    {
        // ✅ 旧版本：保持兼容 Visual 参数
        public static void CreateFadeAnimation(Visual imageVisual)
        {
            var fadeAnimation = imageVisual.Compositor.CreateScalarKeyFrameAnimation();
            fadeAnimation.InsertKeyFrame(0.0f, 0.0f);
            fadeAnimation.InsertKeyFrame(1.0f, 1.0f);
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(500);
            fadeAnimation.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;

            imageVisual.StartAnimation("Opacity", fadeAnimation);
        }

        public static void CreateScaleAnimation(Visual imageVisual)
        {
            var scaleAnimation = imageVisual.Compositor.CreateVector3KeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(0.0f, new Vector3(1.05f, 1.05f, 1.05f));
            scaleAnimation.InsertKeyFrame(1.0f, new Vector3(1.0f, 1.0f, 1.0f));
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(500);
            scaleAnimation.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;

            imageVisual.StartAnimation("Scale", scaleAnimation);
        }

        // ✅ 新增版本：支持 UIElement 参数（自动取 Visual）
        public static void CreateFadeAnimation(UIElement element)
        {
            var visual = ElementCompositionPreview.GetElementVisual(element);
            CreateFadeAnimation(visual); // 复用旧逻辑
        }

        public static void CreateScaleAnimation(UIElement element)
        {
            var visual = ElementCompositionPreview.GetElementVisual(element);
            CreateScaleAnimation(visual);
        }
    }
}
