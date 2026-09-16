using CommunityToolkit.Mvvm.ComponentModel;

namespace Server.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] public partial string Greeting { get; set; } = "Welcome to Avalonia!";
}