namespace pm02Ivan.Views;
using pm02Ivan.ViewModel;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System;


public partial class PageListMemory : ContentPage
{
	public PageListMemory()
	{
		InitializeComponent();

		BindingContext = new ViewModel.ListViewModel(Navigation);
    }

    
}