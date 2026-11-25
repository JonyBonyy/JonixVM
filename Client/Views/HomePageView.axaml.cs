using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Client.Models;

namespace Client.Views;

public partial class HomePageView : UserControl
{
    private const int TotalSteps = 4;
    private int _currentStep = 1;
    private int _maxCores;
    private long _maxRamMb;
    private long _maxDiskGb;

    private bool _showingCloud = true;
    private VmConfig? _editingVm;
    private readonly List<VmConfig> _cloudVms = new();
    private readonly List<VmConfig> _localVms = new();

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

    // --- Sidebar Navigation & VM Grid ---

    private void CloudVmsButton_Click(object? sender, RoutedEventArgs e)
    {
        ShowVmGrid(isCloud: true);
    }

    private void LocalVmsButton_Click(object? sender, RoutedEventArgs e)
    {
        ShowVmGrid(isCloud: false);
    }

    private void CancelCreateForm_Click(object? sender, RoutedEventArgs e)
    {
        _editingVm = null;
        ShowVmGrid(_showingCloud);
    }

    private void ShowVmGrid(bool isCloud)
    {
        _showingCloud = isCloud;
        _editingVm = null;

        WelcomeText.IsVisible = false;
        CreateVmForm.IsVisible = false;
        CreateCloudVmForm.IsVisible = false;
        VmGridView.IsVisible = true;

        VmGridTitle.Text = isCloud ? "Cloud VMs" : "Local VMs";
        RenderVmCards();
    }

    private void RenderVmCards()
    {
        VmCardsPanel.Children.Clear();

        var vms = _showingCloud ? _cloudVms : _localVms;

        foreach (var vm in vms)
        {
            VmCardsPanel.Children.Add(BuildVmCard(vm));
        }

        // Always add the "Create VM" (+) square at the end (or first if list is empty)
        VmCardsPanel.Children.Add(BuildCreateVmCard());
    }

    private Control BuildVmCard(VmConfig vm)
    {
        string osDisplay = vm.IsCloud
            ? vm.IsoOrImage
            : Path.GetFileName(vm.IsoOrImage);

        string details = vm.IsCloud
            ? $"OS: {osDisplay}\nCloud Hosted"
            : $"OS: {(string.IsNullOrWhiteSpace(osDisplay) ? "No ISO" : osDisplay)}\n{vm.Cores} Cores • {vm.RamMb} MB\n{vm.DiskGb} GB Disk";

        var cardContent = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto, *, Auto")
        };

        var titleText = new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(vm.Name) ? "Unnamed VM" : vm.Name,
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = SolidColorBrush.Parse("#2A4552"),
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(0, 4, 34, 8) // leave room for the top-right gear icon
        };
        Grid.SetRow(titleText, 0);

        var detailsText = new TextBlock
        {
            Text = details,
            FontSize = 13,
            Foreground = SolidColorBrush.Parse("#5A7D8E"),
            TextWrapping = TextWrapping.Wrap
        };
        Grid.SetRow(detailsText, 1);

        var launchHint = new TextBlock
        {
            Text = "▶ Start VM",
            FontSize = 13,
            FontWeight = FontWeight.SemiBold,
            Foreground = SolidColorBrush.Parse("#5A7D8E"),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        Grid.SetRow(launchHint, 2);

        cardContent.Children.Add(titleText);
        cardContent.Children.Add(detailsText);
        cardContent.Children.Add(launchHint);

        // Main clickable VM card button
        var mainButton = new Button
        {
            Width = 210,
            Height = 210,
            Padding = new Thickness(18),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch,
            Cursor = new Cursor(StandardCursorType.Hand),
            Content = cardContent
        };
        mainButton.Classes.Add("vm-card");
        mainButton.Click += (_, _) =>
        {
            // Launch or open VM console here
        };

        // Top-right gear settings button
        var gearButton = new Button
        {
            Content = "⚙",
            Width = 32,
            Height = 32,
            Padding = new  Thickness(0),
            FontSize = 16,
            Foreground = SolidColorBrush.Parse("#2A4552"),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 12, 12, 0),
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        gearButton.Classes.Add("card-gear");
        gearButton.Click += (_, _) => OpenEditVmForm(vm);

        var wrapper = new Panel
        {
            Width = 210,
            Height = 210,
            Margin = new Thickness(0, 0, 20, 20)
        };
        wrapper.Children.Add(mainButton);
        wrapper.Children.Add(gearButton);

        return wrapper;
    }

    private Control BuildCreateVmCard()
    {
        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto, *")
        };

        var title = new TextBlock
        {
            Text = "Create VM",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = SolidColorBrush.Parse("#2A4552"),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetRow(title, 0);

        var plus = new TextBlock
        {
            Text = "+",
            FontSize = 72,
            FontWeight = FontWeight.Light,
            Foreground = SolidColorBrush.Parse("#5A7D8E"),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 24)
        };
        Grid.SetRow(plus, 1);

        grid.Children.Add(title);
        grid.Children.Add(plus);

        var button = new Button
        {
            Width = 210,
            Height = 210,
            Margin = new Thickness(0, 0, 20, 20),
            Padding = new Thickness(20),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch,
            Cursor = new Cursor(StandardCursorType.Hand),
            Content = grid
        };
        button.Classes.Add("vm-card");

        button.Click += (_, _) =>
        {
            _editingVm = null;
            if (_showingCloud)
                OpenCloudCreateForm();
            else
                OpenLocalCreateForm();
        };

        return button;
    }

    private void OpenEditVmForm(VmConfig vm)
    {
        _editingVm = vm;

        if (vm.IsCloud)
        {
            OpenCloudCreateForm(vm);
        }
        else
        {
            OpenLocalCreateForm(vm);
        }
    }

    // --- Cloud VM Form ---

    private void OpenCloudCreateForm(VmConfig? existing = null)
    {
        VmGridView.IsVisible = false;
        CreateVmForm.IsVisible = false;
        CreateCloudVmForm.IsVisible = true;

        bool isEditing = existing != null;
        CloudFormTitle.Text = isEditing ? "Edit Cloud VM" : "Create New Cloud VM";
        CreateCloudVmButton.Content = isEditing ? "Save Changes" : "Create Cloud VM";

        CloudVmNameInput.Text = existing?.Name ?? string.Empty;
        CloudVmNameError.IsVisible = false;
        CreateCloudVmButton.IsEnabled = !string.IsNullOrWhiteSpace(CloudVmNameInput.Text);

        if (existing != null)
        {
            for (int i = 0; i < CloudOsCombo.Items.Count; i++)
            {
                if (CloudOsCombo.Items[i] is ComboBoxItem item &&
                    string.Equals(item.Content?.ToString(), existing.IsoOrImage, StringComparison.OrdinalIgnoreCase))
                {
                    CloudOsCombo.SelectedIndex = i;
                    break;
                }
            }
        }
        else
        {
            CloudOsCombo.SelectedIndex = 0;
        }
    }

    private void CloudVmNameInput_TextChanged(object? sender, TextChangedEventArgs e)
    {
        bool isEmpty = string.IsNullOrWhiteSpace(CloudVmNameInput.Text);
        CloudVmNameError.IsVisible = isEmpty;
        CreateCloudVmButton.IsEnabled = !isEmpty;
    }

    private void CreateCloudVmButton_Click(object? sender, RoutedEventArgs e)
    {
        string name = CloudVmNameInput.Text?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(name))
        {
            CloudVmNameError.IsVisible = true;
            return;
        }

        string selectedOs = (CloudOsCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Ubuntu";

        if (_editingVm != null)
        {
            _editingVm.Name = name;
            _editingVm.IsoOrImage = selectedOs;
        }
        else
        {
            _cloudVms.Add(new VmConfig
            {
                Name = name,
                IsoOrImage = selectedOs,
                IsCloud = true
            });
        }

        ShowVmGrid(isCloud: true);
    }

    // --- Local VM Wizard ---

    private void OpenLocalCreateForm(VmConfig? existing = null)
    {
        VmGridView.IsVisible = false;
        CreateCloudVmForm.IsVisible = false;
        CreateVmForm.IsVisible = true;

        bool isEditing = existing != null;
        LocalFormTitle.Text = isEditing ? "Edit Local VM" : "Create New Local VM";

        VmNameInput.Text = existing?.Name ?? string.Empty;
        IsoPathInput.Text = existing?.IsoOrImage ?? string.Empty;
        LocalStep1Error.IsVisible = false;

        if (existing != null)
        {
            RamSlider.Value = Math.Clamp(existing.RamMb, RamSlider.Minimum, RamSlider.Maximum);
            CoresSlider.Value = Math.Clamp(existing.Cores, CoresSlider.Minimum, CoresSlider.Maximum);
            DiskSlider.Value = Math.Clamp(existing.DiskGb, DiskSlider.Minimum, DiskSlider.Maximum);
            KvmCheckBox.IsChecked = existing.EnableHardwareAccel;

            for (int i = 0; i < NetworkModeCombo.Items.Count; i++)
            {
                if (NetworkModeCombo.Items[i] is ComboBoxItem item &&
                    string.Equals(item.Content?.ToString(), existing.NetworkMode, StringComparison.OrdinalIgnoreCase))
                {
                    NetworkModeCombo.SelectedIndex = i;
                    break;
                }
            }
        }
        else
        {
            RamSlider.Value = Math.Min(2048, _maxRamMb);
            CoresSlider.Value = Math.Min(2, _maxCores);
            DiskSlider.Value = Math.Min(20, _maxDiskGb);
            NetworkModeCombo.SelectedIndex = 0;
            KvmCheckBox.IsChecked = true;
        }

        _currentStep = 1;
        UpdateStepUI();
    }

    private void LocalStep1_Changed(object? sender, TextChangedEventArgs e)
    {
        if (_currentStep == 1)
        {
            bool valid = IsLocalStep1Valid();
            NextStepButton.IsEnabled = valid;
            LocalStep1Error.IsVisible = !valid;
        }
    }

    private bool IsLocalStep1Valid()
    {
        return !string.IsNullOrWhiteSpace(VmNameInput.Text) &&
               !string.IsNullOrWhiteSpace(IsoPathInput.Text);
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
        if (_currentStep == 1 && !IsLocalStep1Valid())
        {
            LocalStep1Error.IsVisible = true;
            return;
        }

        if (_currentStep < TotalSteps)
        {
            _currentStep++;
            UpdateStepUI();
        }
        else
        {
            string name = VmNameInput.Text?.Trim() ?? "";
            string iso = IsoPathInput.Text ?? "";
            int ramMb = (int)RamSlider.Value;
            int cores = (int)CoresSlider.Value;
            int diskGb = (int)DiskSlider.Value;
            string network = (NetworkModeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "NAT";
            bool useKvm = KvmCheckBox.IsChecked ?? true;

            if (_editingVm != null)
            {
                _editingVm.Name = name;
                _editingVm.IsoOrImage = iso;
                _editingVm.RamMb = ramMb;
                _editingVm.Cores = cores;
                _editingVm.DiskGb = diskGb;
                _editingVm.NetworkMode = network;
                _editingVm.EnableHardwareAccel = useKvm;
            }
            else
            {
                _localVms.Add(new VmConfig
                {
                    Name = name,
                    IsoOrImage = iso,
                    RamMb = ramMb,
                    Cores = cores,
                    DiskGb = diskGb,
                    NetworkMode = network,
                    EnableHardwareAccel = useKvm,
                    IsCloud = false
                });
            }

            ShowVmGrid(isCloud: false);
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
            NextStepButton.IsEnabled = IsLocalStep1Valid();
        }
        else if (_currentStep == 2)
        {
            StepSubtitle.Text = "Step 2: Hardware Allocation";
            NextStepButton.Content = "→";
            NextStepButton.IsEnabled = true;
        }
        else if (_currentStep == 3)
        {
            StepSubtitle.Text = "Step 3: Storage & Network";
            NextStepButton.Content = "→";
            NextStepButton.IsEnabled = true;
        }
        else if (_currentStep == 4)
        {
            StepSubtitle.Text = _editingVm != null ? "Step 4: Confirm & Save" : "Step 4: Confirm & Create";
            NextStepButton.Content = _editingVm != null ? "Save ✓" : "Confirm ✓";
            NextStepButton.IsEnabled = true;

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
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window != null)
        {
            // Instantly swap the view back to the saved start screen
            window.Content = MainWindow.StartScreenContent;
        }
    }
}