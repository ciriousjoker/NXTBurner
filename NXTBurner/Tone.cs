using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PieControls;
using System.Windows.Media;
using LiveCharts.Wpf;
using LiveCharts;
using LiveCharts.Defaults;

namespace NXTBurner
{
    class Tone
    {
        private static int COLOR_WHITE = 8;

        public static Dictionary<double, Color> GrayId = new Dictionary<double, Color>()
        {
            { 0, Color.FromRgb(0, 0, 0) },
            { 1, Color.FromRgb(75, 75, 75) },
            { 2, Color.FromRgb(95, 95, 95) },
            { 3, Color.FromRgb(111, 111, 111) },
            { 4, Color.FromRgb(128, 128, 128) },
            { 5, Color.FromRgb(145, 145, 145) },
            { 6, Color.FromRgb(167, 167, 167) },
            { 7, Color.FromRgb(195, 195, 195) },
            { 8, Color.FromArgb(0, 255, 255, 255) }
        };


        // TODO: remove all the "SomethingToId" Dictionaries
        public static Dictionary<double, int> ModifierToId = new Dictionary<double, int>()
        {
            { 0, 0 },
            { 1, 1 },
            { 2, 2 },
            { 3, 3 },
            { 4, 4 },
            { 5, 5 },
            { 6, 6 },
            { 7, 7 },
            { 8, 8 }
        };

        // Frequency = Tone type
        public static Dictionary<double, int> FrequencyToId = new Dictionary<double, int>()
        {
            { 0, 0 },
            { 1, 1 },
            { 2, 2 },
            { 3, 3 },
            { 4, 4 },
            { 5, 5 },
            { 6, 6 },
            { 7, 7 },
            { 8, 8 }
        };

        public static Dictionary<double, int> DurationToId = new Dictionary<double, int>()
        {
            { 0, 0 },
            { 1, 1 },
            { 2, 2 },
            { 3, 3 },
            { 4, 4 },
            { 5, 5 },
            { 6, 6 },
            { 7, 7 },
            { 8, 8 }
        };

        public static Dictionary<double, int> DelayToId = new Dictionary<double, int>()
        {
            { 0, 0 },
            { 1, 1 },
            { 2, 2 },
            { 3, 3 },
            { 4, 4 },
            { 5, 5 },
            { 6, 6 },
            { 7, 7 },
            { 8, 8 }
        };

        public static Dictionary<double, int> OctaveToId = new Dictionary<double, int>()
        {
            { 0, 0 },
            { 1, 1 },
            { 2, 2 },
            { 3, 3 },
            { 4, 4 },
            { 5, 5 },
            { 6, 6 },
            { 7, 7 }
        };

        public static Dictionary<double, int> AltModifierToId = new Dictionary<double, int>()
        {
            { 0, 0 },
            { 1, 1 },
            { 2, 2 },
            { 3, 3 },
            { 4, 4 },
            { 5, 5 },
            { 6, 6 },
            { 7, 7 },
            { 8, 8 }
        };


        // TODO: Rename variables accordingly
        public int id;
        public int frequency;
        public int duration;
        public int modifier;
        public int delay;
        public int octave;
        public int alt_modifier;

        // This indicates that the middle part should be skipped (used when a tone is added as a meta data)
        //private bool forceSmall;

        public static int TileSize = 5;
        public int NumberOfElements = 2;

        // TODO: Restructure arguments
        public Tone(int _id, int _modifier, int _frequency, int _duration, int _octave = -1, int _delay = -1, int _alt_modifier = -1)//, bool _forceSmall = false)
        {
            id = _id;

            frequency = _frequency;

            duration = _duration;
            modifier = _modifier;

            if(_delay == -1)
            {
                delay = Music.DEFAULT_DELAY;
            } else
            {
                delay = _delay;
            }

            if (_octave == -1)
            {
                octave = Music.DEFAULT_OCTAVE;
            }
            else
            {
                octave = _octave;
            }

            if (_alt_modifier == -1)
            {
                alt_modifier = Music.DEFAULT_ALTMODIFIER;
            }
            else
            {
                alt_modifier = _alt_modifier;
            }

            //forceSmall = _forceSmall;
        }

        // Done
        public PieSeries[][] returnSegments()
        {
            // Mix everything together and return it
            PieSeries[][] ret = new PieSeries[2][];

            // Skip the middle part if it's just meta data
            if(octave == -2 && delay == -2)
            {
                ret[0] = new PieSeries[2];
                ret[1] = new PieSeries[2];
            }
            else if(delay != Music.DEFAULT_DELAY || octave != Music.DEFAULT_OCTAVE || alt_modifier != Music.DEFAULT_ALTMODIFIER)
            {
                ret[0] = new PieSeries[3];
                ret[1] = new PieSeries[3];
            }
            else
            {
                ret[0] = new PieSeries[2];
                ret[1] = new PieSeries[2];
            }


            // 0 2 4
            // 1 3 5
            // 
            // 0 - Frequency
            // 1 - Modifier
            // 2 - Octave
            // 3 - Delay
            // 4 - White / Alt modifier
            // 5 - Duration


            // Frequency
            ret[0][0] = returnPieSeries(id, FrequencyToId[frequency]);

            // Modifier
            ret[1][0] = returnPieSeries(id, ModifierToId[modifier]);

            // Add optional delay and octave
            if (octave == -2 && delay == -2)
            {
                // AltModifier
                ret[0][1] = returnPieSeries(id, AltModifierToId[alt_modifier]);

                // Duration
                ret[1][1] = returnPieSeries(id, DurationToId[duration]);
            }
            else if (delay != Music.DEFAULT_DELAY || octave != Music.DEFAULT_OCTAVE || alt_modifier != Music.DEFAULT_ALTMODIFIER)
            {
                // Octave
                ret[0][1] = returnPieSeries(id, OctaveToId[octave]);

                // Delay
                ret[1][1] = returnPieSeries(id, DelayToId[delay]);

                // White
                ret[0][2] = returnPieSeries(id, AltModifierToId[alt_modifier]);

                // Duration
                ret[1][2] = returnPieSeries(id, DurationToId[duration]);
                NumberOfElements = 3;
            }
            else
            {
                // White
                ret[0][1] = returnPieSeries(id, COLOR_WHITE);

                // Duration
                ret[1][1] = returnPieSeries(id, DurationToId[duration]);
            }

            return ret;
        }

        private PieSeries returnPieSeries(int _id, int _value)
        {
            return new PieSeries
            {
                Title = _id.ToString(),
                Values = new ChartValues<ObservableValue> { new ObservableValue(TileSize) },
                Fill = new SolidColorBrush(GrayId[_value]),
                DataLabels = true,
                Tag = _value
            };

        }
    }
}
