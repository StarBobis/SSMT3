using Microsoft.UI.Xaml.Data;
using System;
using SSMT.ViewModels;

namespace SSMT.Converters
{
 public class FpsOptionToTextConverter : IValueConverter
 {
 public object Convert(object value, Type targetType, object parameter, string language)
 {
 if (value is FpsOption fps)
 {
 return fps switch
 {
 FpsOption.Fps30 => "30FPS",
 FpsOption.Fps60 => "60FPS",
 _ => value.ToString()
 };
 }
 return value?.ToString() ?? string.Empty;
 }

 public object ConvertBack(object value, Type targetType, object parameter, string language)
 {
 if (value is string str)
 {
 return str switch
 {
 "30FPS" => FpsOption.Fps30,
 "60FPS" => FpsOption.Fps60,
 _ => FpsOption.Fps30
 };
 }
 return FpsOption.Fps30;
 }
 }
}
