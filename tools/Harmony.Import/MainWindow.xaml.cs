using System.Windows;

namespace Harmony.Import;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void LogTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        // Auto-scroll to bottom when text changes
        LogScrollViewer.ScrollToEnd();
    }
}