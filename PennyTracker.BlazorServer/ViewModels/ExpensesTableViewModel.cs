﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using PennyTracker.BlazorServer.Events;
using PennyTracker.BlazorServer.Pages;
using PennyTracker.BlazorServer.Services;
using PennyTracker.Shared.Models;

using Prism.Events;

using Radzen;

namespace PennyTracker.BlazorServer.ViewModels
{
    public class ExpensesTableViewModel : IExpensesTableViewModel
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IExpenseService expenseService;
        private readonly NotificationService notificationService;
        private readonly IDialogService dialogService;

        public event EventHandler StateChanged;

        public IList<Expense> Transactions { get; set; }

        public IEnumerable<string> Periods { get; }

        public string SelectedPeriod { get; set; }

        public IReadOnlyDictionary<string, object> EditButtonAttributes => new Dictionary<string, object>() { { "title", "Edit" } };
        public IReadOnlyDictionary<string, object> DeleteButtonAttributes => new Dictionary<string, object>() { { "title", "Delete" } };

        public ExpensesTableViewModel(
            IEventAggregator eventAggregator,
            IExpenseService expenseService, 
            NotificationService notificationService,
            IDialogService dialogService)
        {
            this.eventAggregator = eventAggregator;
            this.expenseService = expenseService;
            this.notificationService = notificationService;
            this.dialogService = dialogService;

            this.Periods = new List<string> { "Last Month", "Current Month", "Custom" };
            this.SelectedPeriod = this.Periods.Skip(1).First();
        }

        public async Task OnInitalializedAsync()
        {
            await this.OnPeriodChangedAsync(null);
        }

        public async Task OnButtonAddClickAsync()
        {
            await this.OpenCreateExpenseDialog(
                title: "Create New Expense",
                model: new Expense { SpentDate = DateTime.UtcNow }, 
                messageSummary: "Create Expense", 
                messageDetail: "Added Successfully");

            await this.OnPeriodChangedAsync(null);
        }

        public async Task OnButtonEditClickAsync(int id)
        {
            var model = await this.expenseService.GetAsync(id);

            await this.OpenCreateExpenseDialog(
                title: "Update Expense",
                model: model,
                messageSummary: "Update Expense",
                messageDetail: "Updated Successfully");

            await this.OnPeriodChangedAsync(null);
        }

        public async Task OnButtonDeleteClickAsync(int id)
        {
            await this.expenseService.DeleteAsync(id);
            var itemToRemove = this.Transactions.FirstOrDefault(x => x.Id == id);

            this.Transactions.Remove(itemToRemove);
        }

        public async Task OnPeriodChangedAsync(object args)
        {
            switch (this.SelectedPeriod)
            {
                case "Last Month":
                    {
                        var now = DateTime.UtcNow;

                        var from = new DateTime(now.Year, now.AddMonths(-1).Month, 1);
                        var to = new DateTime(now.Year, now.Month, 1);

                        this.Transactions = await this.expenseService.GetRangeAsync(from, to);
                        this.StateChanged.Invoke(this, EventArgs.Empty);

                        break;
                    }
                case "Current Month":
                    {
                        var now = DateTime.UtcNow;

                        var from = new DateTime(now.Year, now.Month, 1);
                        var to = now;

                        this.Transactions = await this.expenseService.GetRangeAsync(from, to);
                        this.StateChanged.Invoke(this, EventArgs.Empty);

                        break;
                    }
            }
        }

        private async Task OpenCreateExpenseDialog(
            string title, 
            Expense model, 
            string messageSummary, 
            string messageDetail)
        {
            var result = await this.dialogService.OpenAsync<CreateExpense>(
                title: title,
                parameters: new Dictionary<string, object> { { "model", model } },
                options: new DialogOptions() { Width = "500px", Height = "auto", Left = "calc(50% - 250px)" });

            if (result)
            {
                this.notificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = messageSummary,
                    Detail = messageDetail,
                    Duration = 4000
                });

                this.eventAggregator.GetEvent<UpdateStateEvent>().Publish();
            }
        }
    }
}