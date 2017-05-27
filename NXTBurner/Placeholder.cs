using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NXTBurner
{
    static class Placeholder
    {

        static PieSeries returnPlaceholder(int size)
        {
            return new PieSeries
            {
                Title = "-1",
                Values = new ChartValues<ObservableValue> { new ObservableValue(size) },
                // TODO: Change color to white
                Fill = new SolidColorBrush(Color.FromRgb(255, 255, 0)),
                DataLabels = true,
                Tag = 0
            };
        }
    }
}
