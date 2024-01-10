#region

using System.Collections.ObjectModel;
using System.Windows.Input;

using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Themes;

using ReactiveUI;

using SkiaSharp;

using Tefin.Core;
using Tefin.Core.Infra.Actors;

#endregion

//using Tefin.Messages;

namespace Tefin.ViewModels.Misc;

public class ChartMiscViewModel : MiscViewModelTabItem {
    private readonly LvcColor[] _colors = ColorPalletes.FluentDesign;
    private int _currentColor;
    private SeriesModel? _selectedSeries;

    public ChartMiscViewModel() {
        this.ClearSeriesCommand = this.CreateCommand(this.OnClearSeries);
        GlobalHub.subscribe<MethodCallMessage>(this.OnReceiveMethodCall);
    }

    public ICommand ClearSeriesCommand { get; }

    public SeriesModel? SelectedSeries {
        get => this._selectedSeries;
        set {
            this.RaiseAndSetIfChanged(ref this._selectedSeries, value);
            if (this._selectedSeries != null) {
                foreach (var s in this.SeriesModels)
                    s.Series.IsVisible = this._selectedSeries == s;
            }
        }
    }

    public ObservableCollection<ISeries> Series { get; } = new();

    public ObservableCollection<SeriesModel> SeriesModels { get; } = new();

    public override string Title { get; } = "Chart";

    public Axis[] XAxes { get; } = {
        new() {
            MinStep = 1,
            TextSize = 0
            // SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
            // {
            //     StrokeThickness = 2,
            //     //PathEffect = new DashEffect(new float[] { 3, 3 })
            // }
        }
    };

    public Axis[] YAxes { get; } = {
        new() {
            MinLimit = 0,
            Name = "Elapsed (msec)",
            NamePaint = new SolidColorPaint(SKColors.SlateGray),
            NameTextSize = 16,
            TextSize = 14,
            LabelsPaint = new SolidColorPaint(SKColors.SlateGray),
            SeparatorsPaint = new SolidColorPaint(SKColors.DimGray) {
                StrokeThickness = 0.5f
                //PathEffect = new DashEffect(new float[] { 3, 3 })
            }
        }
    };

    private void AddPoint(string clientName, string method, double point) {
        lock (this) {
            var seriesModel = this.SeriesModels.FirstOrDefault(t => t.ClientName == clientName && t.Method == method);
            if (seriesModel == null) {
                seriesModel = new SeriesModel(clientName, method, new ColumnSeries<double> {
                    Name = method
                });
                this.SeriesModels.Add(seriesModel);
                this.Series.Add(seriesModel.Series);

                var nextColorIndex = this._currentColor++ % this._colors.Length;
                var color = this._colors[nextColorIndex];
                seriesModel.Series.Fill = new SolidColorPaint(new SKColor(color.R, color.G, color.B, 90));
            }

            this.SelectedSeries = seriesModel;
            seriesModel.Values.Add(point);
        }
    }

    private void OnClearSeries() {
        this.Series.Clear();
        this.SeriesModels.Clear();
        // this._currentColor = 0;
    }

    private void OnReceiveMethodCall(MethodCallMessage obj) {
        this.AddPoint(obj.ClientName, obj.Method, obj.Point);
    }

    public class SeriesModel {
        public SeriesModel(string clientName, string method, ColumnSeries<double> series) {
            this.ClientName = clientName;
            this.Method = method;
            this.Series = series;
            series.Values = this.Values;
            series.MaxBarWidth = 20;
        }

        public string ClientName {
            get;
        }

        public string Id { get => $"{this.ClientName}/{this.Method}"; }

        public string Method {
            get;
        }

        public ColumnSeries<double> Series {
            get;
        }

        public ObservableCollection<double> Values { get; } = new();
    }
}