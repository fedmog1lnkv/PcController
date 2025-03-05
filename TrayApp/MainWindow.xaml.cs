namespace TrayApp;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        Closing += MainWindow_Closing;
    }
    
    private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;

        Hide();
    }
}