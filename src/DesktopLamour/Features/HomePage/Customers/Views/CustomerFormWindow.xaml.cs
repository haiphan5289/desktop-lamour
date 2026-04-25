// Copyright © 2026 DesktopLamour. All rights reserved.
using DesktopLamour.Features.HomePage.Customers.Domain.Models;
using DesktopLamour.Features.HomePage.Customers.ViewModels;
using System.Windows;

namespace DesktopLamour.Features.HomePage.Customers.Views;

public partial class CustomerFormWindow : Window
{
    public CustomerFormViewModel ViewModel { get; }

    public CustomerFormWindow(CustomerFormViewModel viewModel)
    {
        InitializeComponent();
        ViewModel   = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += result => { DialogResult = result; };
    }

    public void Initialize(Customer? customer) => ViewModel.Initialize(customer);

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        await ViewModel.LoadNextCodeAsync();
    }
}
