using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using System.Windows.Input;

namespace SSMT.Pages.TextureToolBoxPage
{
    public static class DragDropBehavior
    {
        public static readonly DependencyProperty FileExtensionsProperty = DependencyProperty.RegisterAttached(
            "FileExtensions",
            typeof(string),
            typeof(DragDropBehavior),
            new PropertyMetadata(string.Empty, OnFileExtensionsChanged));

        public static void SetFileExtensions(UIElement element, string value) => element.SetValue(FileExtensionsProperty, value);
        public static string GetFileExtensions(UIElement element) => (string)element.GetValue(FileExtensionsProperty);

        public static readonly DependencyProperty DropCommandProperty = DependencyProperty.RegisterAttached(
            "DropCommand",
            typeof(ICommand),
            typeof(DragDropBehavior),
            new PropertyMetadata(null, OnDropCommandChanged));

        public static void SetDropCommand(UIElement element, ICommand value) => element.SetValue(DropCommandProperty, value);
        public static ICommand GetDropCommand(UIElement element) => (ICommand)element.GetValue(DropCommandProperty);

        private static void OnFileExtensionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // nothing to do here; Drop handler will read extensions
        }

        private static void OnDropCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement uiElement)
            {
                if (e.OldValue == null && e.NewValue != null)
                {
                    uiElement.DragOver += UiElement_DragOver;
                    uiElement.Drop += UiElement_Drop;
                }
                else if (e.OldValue != null && e.NewValue == null)
                {
                    uiElement.DragOver -= UiElement_DragOver;
                    uiElement.Drop -= UiElement_Drop;
                }
            }
        }

        private static void UiElement_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    e.AcceptedOperation = DataPackageOperation.Copy;
                }
                else
                {
                    e.AcceptedOperation = DataPackageOperation.None;
                }
            }
            catch { }
        }

        private static async void UiElement_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (!e.DataView.Contains(StandardDataFormats.StorageItems))
                    return;

                var items = await e.DataView.GetStorageItemsAsync().AsTask();
                if (items == null || items.Count == 0)
                    return;

                var first = items.FirstOrDefault();
                if (first is StorageFile file)
                {
                    string path = file.Path;
                    string extensions = string.Empty;
                    if (sender is UIElement ue)
                    {
                        extensions = GetFileExtensions(ue) ?? string.Empty;
                    }

                    if (string.IsNullOrWhiteSpace(extensions) || IsMatchingExtension(path, extensions))
                    {
                        if (sender is UIElement element)
                        {
                            var cmd = GetDropCommand(element);
                            if (cmd != null && cmd.CanExecute(path))
                            {
                                cmd.Execute(path);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private static bool IsMatchingExtension(string path, string extensions)
        {
            if (string.IsNullOrWhiteSpace(extensions))
                return true;

            var list = extensions.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().TrimStart('*')).Where(s => s != "").ToList();

            string ext = System.IO.Path.GetExtension(path);
            if (string.IsNullOrEmpty(ext))
                return false;

            ext = ext.ToLowerInvariant();
            return list.Any(e => e.StartsWith('.') ? e.Equals(ext, StringComparison.OrdinalIgnoreCase) : ("." + e).Equals(ext, StringComparison.OrdinalIgnoreCase));
        }
    }
}
