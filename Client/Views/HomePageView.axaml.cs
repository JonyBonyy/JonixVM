using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace Client.Views;

public partial class HomePageView : UserControl
{
    private const int TotalSteps = 4;
    private int _currentStep = 1;
    private int _maxCores;
    private long _maxRamMb;
    private long _maxDiskGb;

    public HomePageView()
    {
        InitializeComponent();
        InitializeHardwareLimits();
    }

    private void InitializeHardwareLimits()
    {
        // Scan total CPU cores
        _maxCores = Environment.ProcessorCount;
        CoresSlider.Maximum = _maxCores;
        CoresSlider.Value = Math.Min(2, _maxCores);

        // Scan total physical RAM in MB
        long totalBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        _maxRamMb = totalBytes / (1024 * 1024);
        RamSlider.Maximum = _maxRamMb;
        RamSlider.Value = Math.Min(2048, _maxRamMb);

        // Scan available free disk space in GB on the root/app drive
        try
        {
            var drive = new DriveInfo(Path.GetPathRoot(AppContext.BaseDirectory) ?? "/");
            _maxDiskGb = Math.Max(10, drive.AvailableFreeSpace / (1024L * 1024 * 1024));
        }
        catch
        {
            _maxDiskGb = 100;
        }

        DiskSlider.Maximum = _maxDiskGb;
        DiskSlider.Value = Math.Min(20, _maxDiskGb);

        UpdateHardwareLabels();
    }
    
    private void HardwareSlider_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        UpdateHardwareLabels();
    }

    private void UpdateHardwareLabels()
    {
        if (RamValueText != null && RamSlider != null)
            RamValueText.Text = $"{(int)RamSlider.Value} MB / {_maxRamMb} MB";

        if (CoresValueText != null && CoresSlider != null)
            CoresValueText.Text = $"{(int)CoresSlider.Value} / {_maxCores} Cores";

        if (DiskValueText != null && DiskSlider != null)
            DiskValueText.Text = $"{(int)DiskSlider.Value} GB / {_maxDiskGb} GB free";
    }

    private void NewVmButton_Click(object? sender, RoutedEventArgs e)
    {
        WelcomeText.IsVisible = false;
        CreateVmForm.IsVisible = true;

        _currentStep = 1;
        UpdateStepUI();
    }

    private async void BrowseIsoButton_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select OS ISO Image",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("ISO Disk Images")
                {
                    Patterns = new[] { "*.iso" },
                    MimeTypes = new[] { "application/x-iso9660-image" }
                }
            }
        });

        if (files.Count > 0)
        {
            string path = files[0].Path.LocalPath;
            if (Path.GetExtension(path).Equals(".iso", StringComparison.OrdinalIgnoreCase))
            {
                IsoPathInput.Text = path;
            }
        }
    }

    private void BackStepButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_currentStep > 1)
        {
            _currentStep--;
            UpdateStepUI();
        }
    }

    private void NextStepButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_currentStep < TotalSteps)
        {
            _currentStep++;
            UpdateStepUI();
        }
        else
        {
            // Step 4 confirmed: pass these to PostgreSQL / QEMU
            string name = VmNameInput.Text ?? "";
            string iso = IsoPathInput.Text ?? "";
            int ramMb = (int)RamSlider.Value;
            int cores = (int)CoresSlider.Value;
            int diskGb = (int)DiskSlider.Value;
            string network = (NetworkModeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "NAT";
            bool useKvm = KvmCheckBox.IsChecked ?? true;
        }
    }

    private void UpdateStepUI()
    {
        Step1Panel.IsVisible = (_currentStep == 1);
        Step2Panel.IsVisible = (_currentStep == 2);
        Step3Panel.IsVisible = (_currentStep == 3);
        Step4Panel.IsVisible = (_currentStep == 4);

        StepIndicatorText.Text = $"{_currentStep} / {TotalSteps}";
        BackStepButton.IsEnabled = (_currentStep > 1);

        if (_currentStep == 1)
        {
            StepSubtitle.Text = "Step 1: Basic Information";
            NextStepButton.Content = "→";
        }
        else if (_currentStep == 2)
        {
            StepSubtitle.Text = "Step 2: Hardware Allocation";
            NextStepButton.Content = "→";
        }
        else if (_currentStep == 3)
        {
            StepSubtitle.Text = "Step 3: Storage & Network";
            NextStepButton.Content = "→";
        }
        else if (_currentStep == 4)
        {
            StepSubtitle.Text = "Step 4: Confirm & Create";
            NextStepButton.Content = "Confirm ✓";

            SummaryName.Text = string.IsNullOrWhiteSpace(VmNameInput.Text) ? "Not set" : VmNameInput.Text;
            SummaryIso.Text = string.IsNullOrWhiteSpace(IsoPathInput.Text) ? "Not set" : IsoPathInput.Text;
            SummaryRam.Text = $"{(int)RamSlider.Value} MB";
            SummaryCores.Text = $"{(int)CoresSlider.Value} Cores";
            SummaryDisk.Text = $"{(int)DiskSlider.Value} GB (qcow2)";
            SummaryNetwork.Text = (NetworkModeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "NAT";
            SummaryKvm.Text = (KvmCheckBox.IsChecked ?? false) ? "Enabled" : "Disabled";
        }
    }

    private void LoginButtonClick(object? sender, RoutedEventArgs e)
    {
        this.Content = new LoginView();
    }
}