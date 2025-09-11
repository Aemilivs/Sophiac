using Microsoft.AspNetCore.Components;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Extensions.Configuration;
using Sophiac.Domain.Settings;

namespace Sophiac.UI.Settings;

public partial class SettingsPage : ComponentBase
{
    [Inject]
    public ISettingsRepository Repository { get; set; }

    public bool AdvancedOptions { get; set; } = false;

    private string _key = string.Empty;
    private string _llmModel = string.Empty;
    private string _selectedProvider = string.Empty;
    private string _ollamaUrl = string.Empty;
    private string _window_size = string.Empty;
    private string _custom_instructions = string.Empty;

    protected async override Task OnInitializedAsync()
    {
        var setting = await Repository.GetByKeyAsync("llm_api_key");

        if (setting == null)
        {
            setting =
                new SophiacSettings
                {
                    Key = "llm_api_key",
                    Value = ""
                };
            await Repository.AddAsync(setting);
        }
        _key = setting.Value;

        setting = await Repository.GetByKeyAsync("llm_model");

        if (setting == null)
        {
            setting =
                new SophiacSettings
                {
                    Key = "llm_model",
                    Value = "qwen3:8b"
                };
            await Repository.AddAsync(setting);
        }
        _llmModel = setting.Value;

        setting = await Repository.GetByKeyAsync("llm_api_provider");

        if (setting == null)
        {
            setting =
                new SophiacSettings
                {
                    Key = "llm_api_provider",
                    Value = "Ollama"
                };
            await Repository.AddAsync(setting);
        }
        _selectedProvider = setting.Value;

        setting = await Repository.GetByKeyAsync("llm_api_url");
        if (setting == null)
        {
            setting =
                new SophiacSettings
                {
                    Key = "llm_api_url",
                    Value = "http://127.0.0.1:11434"
                };
            await Repository.AddAsync(setting);
        }
        _ollamaUrl = setting.Value;

        setting = await Repository.GetByKeyAsync("llm_window_size");
        if (setting == null)
        {
            setting =
                new SophiacSettings
                {
                    Key = "llm_window_size",
                    Value = "50"
                };
            await Repository.AddAsync(setting);
        }
        _window_size = setting.Value;

        setting = await Repository.GetByKeyAsync("llm_custom_instructions");
        if (setting == null)
        {
            setting =
                new SophiacSettings
                {
                    Key = "llm_custom_instructions",
                    Value = string.Empty
                };
            await Repository.AddAsync(setting);
        }
        _custom_instructions = setting.Value;
    }

    private async Task OnLLMModelChangeAsync(ChangeEventArgs arguments)
    {
        if (arguments.Value is string value)
        {
            _llmModel = value;
        }

        try
        {
            var setting =
                new SophiacSettings
                {
                    Key = "llm_model",
                    Value = _llmModel
                };
            await Repository.UpdateAsync(setting);
        }
        catch (Exception exception)
        {
            var toast = Toast.Make($"Failed to save the key permanently: {exception.Message}");
            await toast.Show();
        }
    }

    private async Task OnKeyChangeAsync(ChangeEventArgs arguments)
    {
        if (arguments.Value is string value)
        {
            _key = value;
        }

        try
        {
            var setting =
                new SophiacSettings
                {
                    Key = "llm_api_key",
                    Value = _key
                };
            await Repository.UpdateAsync(setting);
        }
        catch (Exception exception)
        {
            var toast = Toast.Make($"Failed to save the key permanently: {exception.Message}");
            await toast.Show();
        }
    }

    private async Task OnProviderChangeAsync(ChangeEventArgs arguments)
    {
        if (arguments.Value is string value)
        {
            _selectedProvider = value;
        }

        try
        {
            var setting =
                new SophiacSettings
                {
                    Key = "llm_api_provider",
                    Value = _selectedProvider
                };
            await Repository.UpdateAsync(setting);
        }
        catch (Exception exception)
        {
            var toast = Toast.Make($"Failed to save the key permanently: {exception.Message}");
            await toast.Show();
        }
    }

    private async Task OnUrlChangeAsync(ChangeEventArgs arguments)
    {
        if (arguments.Value is string value)
        {
            _ollamaUrl = value;
        }

        try
        {
            var setting =
                new SophiacSettings
                {
                    Key = "llm_api_url",
                    Value = _ollamaUrl
                };
            await Repository.UpdateAsync(setting);
        }
        catch (Exception exception)
        {
            var toast = Toast.Make($"Failed to save the key permanently: {exception.Message}");
            await toast.Show();
        }
    }

    private async Task OnWindowSizeChangeAsync(ChangeEventArgs arguments)
    {
        if (arguments.Value is string value)
        {
            _window_size = value;
        }

        try
        {
            var setting =
                new SophiacSettings
                {
                    Key = "llm_window_size",
                    Value = _window_size
                };
            await Repository.UpdateAsync(setting);
        }
        catch (Exception exception)
        {
            var toast = Toast.Make($"Failed to save the key permanently: {exception.Message}");
            await toast.Show();
        }
    }

    private async Task OnCustomInstructionsChangeAsync(ChangeEventArgs arguments)
    {
        if (arguments.Value is string value)
        {
            _custom_instructions = value;
        }

        try
        {
            var setting =
                new SophiacSettings
                {
                    Key = "llm_custom_instructions",
                    Value = _custom_instructions
                };
            await Repository.UpdateAsync(setting);
        }
        catch (Exception exception)
        {
            var toast = Toast.Make($"Failed to save the key permanently: {exception.Message}");
            await toast.Show();
        }
    }
}
