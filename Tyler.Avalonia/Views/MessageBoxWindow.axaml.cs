using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

using Net.Essentials;

using System.Threading.Tasks;

namespace Tyler.Views
{
    public partial class MessageBoxWindow : Window
    {
        readonly TextBlock message;
        readonly StackPanel buttonsStack;

        public string? Result { get; private set; }

        public MessageBoxWindow()
        {
            InitializeComponent();

            message = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(10),
            };

            buttonsStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10),
                Spacing = 10,
            };

            Content = new StackPanel
            {
                Margin = new Thickness(10),
                Children =
                {
                    message,
                    buttonsStack
                }
            };
        }

        public static async Task<string?> ShowConfirmDialogAsync(Window parent, string title, string text, string[] buttons)
        {
            var window = new MessageBoxWindow();

            window.Title = title;
            window.message.Text = text;
            if (buttons == null || buttons.Length == 0)
                buttons = new[] { "OK" };
            foreach (var button in buttons)
            {
                var buttonControl = new Button
                {
                    Content = button,
                    Width = 100,
                    Tag = button,
                };
                buttonControl.Click += (s, e) =>
                {
                    window.Result = buttonControl.Tag as string;
                    window.Close(buttonControl.Tag as string);
                };
                buttonControl.Content = L.T(button);
                window.buttonsStack.Children.Add(buttonControl);
            }
            await window.ShowDialog(parent);
            return window.Result;
        }
    }
}
