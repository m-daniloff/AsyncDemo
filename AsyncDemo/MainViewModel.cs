using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;

namespace AsyncDemo
{
	public class MainViewModel : ViewModelBase
	{
		public MainViewModel()
		{

			AsyncExecuteCommand = new DelegateCommand(OnExecuteAsync);
			AsyncParallelExecuteCommand = new DelegateCommand(OnExecuteParallelAsync);
			CancelCommand = new DelegateCommand(OnCancel);
		}

		public ICommand AsyncExecuteCommand { get; set; }
		public ICommand AsyncParallelExecuteCommand { get; set; }
		public ICommand CancelCommand { get; set; }
		CancellationTokenSource cts;



		string _resultsText;
		public string ResultsText
		{
			get { return _resultsText; }
			set
			{
				_resultsText = value;
				OnPropertyChanged();
			}
		}

		private int _total;
		public int Total
		{
			get { return _total; }
			set
			{
				_total = value;
				OnPropertyChanged();
			}
		}

		private void  OnCancel(object obj)
		{
			cts.Cancel();
		}

		private async void OnExecuteAsync(object obj)
		{
			Total = 0;
			cts = new CancellationTokenSource();
			Progress<ProgressReportModel> progress = new Progress<ProgressReportModel>();
			progress.ProgressChanged += ReportProgress;

			var watch = Stopwatch.StartNew();

			try
			{
				var results = await DemoMethods.RunDownloadAsync(progress, cts.Token);
				PrintResults(results);
			}
			catch (OperationCanceledException)
			{
				ResultsText += $"The async download was cancelled. { Environment.NewLine }";
			}

			watch.Stop();
			var elapsed = watch.ElapsedMilliseconds;

			ResultsText += $"Total execution time: { elapsed }";
		}

		private async void OnExecuteParallelAsync(object obj)
		{
			Total = 0;
			Progress<ProgressReportModel> progress = new Progress<ProgressReportModel>();
			progress.ProgressChanged += ReportProgress;

			var watch = System.Diagnostics.Stopwatch.StartNew();

			var results = await DemoMethods.RunDownloadParallelAsyncV2(progress);
			PrintResults(results);

			watch.Stop();
			var elapsedMs = watch.ElapsedMilliseconds;

			ResultsText += $"Total execution time: { elapsedMs }";
		}

		private void ReportProgress(object sender, ProgressReportModel e)
		{
			//dashboardProgress.Value = e.PercentageComplete;
			Total += 1;
			PrintResults(e.SitesDownloaded);
		}

		private void PrintResults(List<WebsiteDataModel> results)
		{
			ResultsText = "";
			foreach (var item in results)
			{
				ResultsText += $"{ item.WebsiteUrl } downloaded: { item.WebsiteData.Length } characters long.{ Environment.NewLine }";
			}
		}
	}

	
}
