using Microsoft.AspNetCore.Components;
using Sophiac.Domain.TestSets;
using System.Text;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Alerts;
using Sophiac.Domain.Questions;
using System.Text.Json;
using Sophiac.Domain.Generation;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace Sophiac.UI.TestSets;

public partial class TestSetsPage : ComponentBase
{
    [Inject]
    public IQuestionsService QuestionsService { get; set; }

    [Inject]
    public IGenerationService GenerationService { get; set; }

    [Inject]
    public NavigationManager Manager { get; set; }

    public string QuestionsLabel(int count)
    {
        return count switch
        {
            0 => "0 Questions",
            1 => "1 Question",
            _ => $"{count} Questions"
        };
    }

    public CancellationTokenSource CancellationTokenSource { get; set; } = new();

    public bool IsLoading { get; set; } = false;
    public bool GenerationIsCancelled { get; set; } = false;

    private IList<TestSet> _sets;

    protected override async Task OnInitializedAsync()
    {
        await LoadTestSetsAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadTestSetsAsync()
    {
        var sets = await QuestionsService.ReadTestSetsAsync();
        _sets = sets.ToList();
    }

    public void DeleteTestSet(TestSet set)
    {
        _sets.Remove(set);
        QuestionsService.DeleteTestSet(set.Title);
    }

    public async Task ImportTestSetAsync(CancellationToken token)
    {
        var permission = await CheckAndRequestStorageReadPermission();

        if (permission != PermissionStatus.Granted)
        {
            await Toast.Make("Please grant permissions to read the storage first.").Show();
            return;
        }

        try
        {
            var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.json" } },
                    { DevicePlatform.Android, new[] { "application/json" } },
                    { DevicePlatform.WinUI, new[] { ".json", ".json" } },
                    { DevicePlatform.Tizen, new[] { "application/json" } },
                    { DevicePlatform.MacCatalyst, new[] { "json" } },
                });

            var options = new PickOptions()
            {
                PickerTitle = "Please select an examination test set file",
                FileTypes = customFileType,
            };

            var file = await FilePicker.Default.PickAsync(options);
            if (file != null)
            {
                if (file.FileName.EndsWith("json", StringComparison.OrdinalIgnoreCase))
                {
                    var raw = File.ReadAllText(file.FullPath);
                    var serializationOptions = new JsonSerializerOptions { WriteIndented = true };
                    var set = JsonSerializer.Deserialize<TestSet>(raw, serializationOptions);
                    await QuestionsService.CreateTestSetAsync(set);
                    var sets = await QuestionsService.ReadTestSetsAsync();
                    _sets = sets.ToList();
                }
            }
            else
            {
                await Toast.Make("Couldn't import the file!").Show(token);
            }
        }
        catch (Exception ex)
        {
            await Toast.Make("Something went seriously wrong.").Show(token);
        }
    }

    public async Task ExportTestSetAsync(TestSet set, CancellationToken token)
    {
        var permission = await CheckAndRequestStorageWritePermission();

        if (permission != PermissionStatus.Granted)
        {
            await Toast.Make("Please grant permissions to write to the storage first.").Show();
            return;
        }

        var options = new JsonSerializerOptions { WriteIndented = true };
        var raw = JsonSerializer.Serialize(set, options);

        var bites = Encoding.Default.GetBytes(raw);
        using var stream = new MemoryStream(bites);
        var fileSaverResult = await FileSaver.Default.SaveAsync(set.FileName, stream, token);
        if (fileSaverResult.IsSuccessful)
        {
            await Toast.Make($"The file was saved successfully to location: {fileSaverResult.FilePath}").Show(token);
        }
        else
        {
            await Toast.Make($"The file was not saved successfully with error: {fileSaverResult.Exception.Message}").Show(token);
        }
    }

    private async Task CancelGenerationAsync()
    {
        IsLoading = false;
        GenerationIsCancelled = true;
        CancellationTokenSource.Cancel();
    }

    private async Task<PermissionStatus> CheckAndRequestStorageReadPermission()
    {
        PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();

        if (status == PermissionStatus.Granted)
            return status;

        if (Permissions.ShouldShowRationale<Permissions.StorageRead>())
        {
            await Toast.Make("This permission is required in order to import examination test sets and test runs.").Show();
        }

        status = await Permissions.RequestAsync<Permissions.StorageRead>();

        return status;
    }

    private async Task<PermissionStatus> CheckAndRequestStorageWritePermission()
    {
        PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

        if (status == PermissionStatus.Granted)
            return status;

        if (Permissions.ShouldShowRationale<Permissions.StorageWrite>())
        {
            await Toast.Make("This permission is required in order to export examination test sets and test runs.").Show();
        }

        status = await Permissions.RequestAsync<Permissions.StorageWrite>();

        return status;
    }

    private async Task ParsePDFAsync()
    {
        var permission = await CheckAndRequestStorageReadPermission();

        if (permission != PermissionStatus.Granted)
        {
            await Toast.Make("Please grant permissions to read the storage first.").Show();
            return;
        }

        try
        {
            var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.pdf" } },
                    { DevicePlatform.Android, new[] { "application/pdf" } },
                    { DevicePlatform.WinUI, new[] { ".pdf", ".pdf" } },
                    { DevicePlatform.Tizen, new[] { "application/pdf" } },
                    { DevicePlatform.MacCatalyst, new[] { "pdf" } },
                });

            var options = new PickOptions()
            {
                PickerTitle = "Please select a pdf to parse for questions",
                FileTypes = customFileType,
            };

            var file = await FilePicker.Default.PickAsync(options);
            IsLoading = true;
            StateHasChanged();
            if (file != null)
            {
                await Toast.Make("Processing PDF might take some time.").Show();

                var textInput = ExtractText(file.FullPath);

                await QuestionsService.CreateTestSetAsync(file.FileName);

                var processCancellationToken = CancellationTokenSource.Token;
                await GenerationService.GenerateQuestionAsync(file.FileName, textInput, processCancellationToken);

                var set = await QuestionsService.ReadTestSetAsync(file.FileName);
                await QuestionsService.UpdateTestSetAsync(set);

                if (GenerationIsCancelled)
                {
                    GenerationIsCancelled = false;
                    await QuestionsService.DeleteTestSet(file.FileName);
                }

                StateHasChanged();
            }
            else
            {
                await Toast.Make("Couldn't import the file!").Show();
            }
        }
        catch (Exception exception)
        {
            await Toast.Make($"Something went seriously wrong. {exception.Message}", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private string ExtractText(string filePath)
    {
        var pages = ExtractWordsOfPages(filePath);
        return pages.SelectMany(word => word).Select(it => string.Concat(it.Text, ' ')).Aggregate(string.Concat);
    }

    private IEnumerable<IEnumerable<Word>> ExtractWordsOfPages(string filePath)
    {
        using (PdfDocument document = PdfDocument.Open(filePath))
        {
            foreach (var page in document.GetPages())
            {
                string text = ContentOrderTextExtractor.GetText(page);
                yield return page.GetWords(NearestNeighbourWordExtractor.Instance);
            }
        }
    }
}
