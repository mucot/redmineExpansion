using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace RedmineExpansion;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MainWindowViewModel _viewModel;
    private readonly List<string> defaultTaskFields = new List<string> { "id", "subject", "status", "priority", "assigned_to", "author", "start_date", "due_date", "done_ratio", "description" };

    public MainWindow()
    {
        InitializeComponent();
        if (TaskFieldListBox != null)
        {
            TaskFieldListBox.ItemsSource = defaultTaskFields;
            foreach (var item in TaskFieldListBox.Items)
                TaskFieldListBox.SelectedItems.Add(item);
        }
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel = DataContext as MainWindowViewModel;
        if (string.IsNullOrEmpty(_viewModel.RedmineUrl) || string.IsNullOrEmpty(_viewModel.ApiKey))
        {
            MessageBox.Show("Redmine URL or API key is not set.");
            return;
        }
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri(_viewModel.RedmineUrl);
            client.DefaultRequestHeaders.Add("X-Redmine-API-Key", _viewModel.ApiKey);
            try
            {
                // 例: プロジェクト一覧を取得
                var response = await client.GetAsync("/projects.json");
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    // 必要に応じてJSONをパース
                }
                else
                {
                    MessageBox.Show($"API access failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"API access error: {ex.Message}");
            }
        }
    }

    private async void ShowTaskButton_Click(object sender, RoutedEventArgs e)
    {
        string ticketNo = TicketNumberTextBox.Text.Trim();
        if (string.IsNullOrEmpty(ticketNo))
        {
            MessageBox.Show("Please enter a ticket number.");
            return;
        }
        if (string.IsNullOrEmpty(_viewModel.RedmineUrl) || string.IsNullOrEmpty(_viewModel.ApiKey))
        {
            MessageBox.Show("Redmine URL or API key is not set.");
            return;
        }
        var selectedFields = new List<string>();
        foreach (var item in TaskFieldListBox.SelectedItems)
            selectedFields.Add(item.ToString());
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri(_viewModel.RedmineUrl);
            client.DefaultRequestHeaders.Add("X-Redmine-API-Key", _viewModel.ApiKey);
            try
            {
                var response = await client.GetAsync($"/issues/{ticketNo}.json?include=children,journals,watchers");
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(result);
                    var root = doc.RootElement.GetProperty("issue");
                    var sb = new StringBuilder();
                    foreach (var field in selectedFields)
                    {
                        if (root.TryGetProperty(field, out var value))
                        {
                            sb.AppendLine($"{field}={value.ToString()}");
                        }
                    }
                    TaskParamsTextBox.Text = sb.ToString();
                }
                else
                {
                    MessageBox.Show($"Failed to get ticket: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"API access error: {ex.Message}");
            }
        }
    }
}