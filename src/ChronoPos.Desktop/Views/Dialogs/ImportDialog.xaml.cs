using System.Windows;
using System.Windows.Input;

namespace ChronoPos.Desktop.Views.Dialogs;

public partial class ImportDialog : Window
{
    public enum ImportAction
    {
        None,
        DownloadTemplate,
        UploadFile
    }

    public ImportAction SelectedAction { get; private set; } = ImportAction.None;

    public ImportDialog()
    {
        InitializeComponent();
    }

    private void DownloadTemplate_Click(object sender, MouseButtonEventArgs e)
    {
        SelectedAction = ImportAction.DownloadTemplate;
        DialogResult = true;
        Close();
    }

    private void UploadFile_Click(object sender, MouseButtonEventArgs e)
    {
        SelectedAction = ImportAction.UploadFile;
        DialogResult = true;
        Close();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        SelectedAction = ImportAction.None;
        DialogResult = false;
        Close();
    }
}
