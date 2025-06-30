using System.IO;
using System.Text;
using System.ComponentModel;

namespace RedmineExpansion;
public class MainWindowViewModel : INotifyPropertyChanged
{
    private string _redmineUrl;
    public string RedmineUrl
    {
        get { return _redmineUrl; }
        private set
        {
            _redmineUrl = value;
            OnPropertyChanged(nameof(RedmineUrl));
        }
    }

    private string _apiKey;
    public string ApiKey
    {
        get { return _apiKey; }
        private set
        {
            _apiKey = value;
            OnPropertyChanged(nameof(ApiKey));
        }
    }

    private string _errorMessage;
    public string ErrorMessage
    {
        get { return _errorMessage; }
        private set
        {
            _errorMessage = value;
            OnPropertyChanged(nameof(ErrorMessage));
        }
    }

    public MainWindowViewModel()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        string iniPath = "..\\..\\..\\settings.ini";
        if (!File.Exists(iniPath))
        {
            ErrorMessage = $"Settings file not found: {iniPath}";
            return;
        }
        foreach (var line in File.ReadAllLines(iniPath, Encoding.UTF8))
        {
            if (line.StartsWith("RedmineUrl="))
                RedmineUrl = line.Substring("RedmineUrl=".Length).Trim();
            else if (line.StartsWith("ApiKey="))
                ApiKey = line.Substring("ApiKey=".Length).Trim();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
