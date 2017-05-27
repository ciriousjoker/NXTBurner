using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Definitions.Series;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NXTBurner
{
    class Music
    {

        //private static int DatafieldsPerTrack = 32;
        private static int[] DatafieldsPerTrack = { 37, 37, 32, 32, 16, 16, 16 }; // Add a 7th value as a buffer for when the current row is calculated
        private static int MetadataSize = 6;

        private static int MaxTrack = 3;
        private int CurrentTrack = 0;
        private int ToneCounter = 0;

        private bool hasMetadata = false;

        // Store the chartdata in here. Updating this will update the chart
        SeriesCollection[] ChartRow = new SeriesCollection[6] { new SeriesCollection(), new SeriesCollection(), new SeriesCollection(), new SeriesCollection(), new SeriesCollection(), new SeriesCollection() };

        // Used to store the placeholder in. The placeholders are used to maintain the same width for every tone
        PieSeries[] Placeholder = new PieSeries[6];

        // Store the notes as a separate stream
        List<Tone> dNotes = new List<Tone>();


        public static int DEFAULT_MODIFIER = 1;
        public static int DEFAULT_FREQUENCY = 0;
        public static int DEFAULT_OCTAVE = 0;
        public static int DEFAULT_DELAY = 0;
        public static int DEFAULT_DURATION = 0;
        public static int DEFAULT_ALTMODIFIER = 8;
        public Music()
        {
            for (int i = 0; i < ChartRow.Count(); i++)
            {
                if (ChartRow[i].Count < DatafieldsPerTrack[i])
                {
                    Placeholder[i] = returnPlaceholder((DatafieldsPerTrack[i] - ChartRow[i].Count) * Tone.TileSize, -1);
                    ChartRow[i].Add(Placeholder[i]);
                }
            }
        }

        private void updatePlaceholders()
        {
            // Fill up the rest of the chart in order to maintain the structure
            for (int i = 0; i < ChartRow.Count(); i++)
            {
                // Remove the old placeholder
                ChartRow[i].Remove(Placeholder[i]);

                // Generate and add the new placeholder if there's room for it
                if (DatafieldsPerTrack[i] - ChartRow[i].Count > 0)
                {
                    // TODO: Remove this maybe
                    /*
                    // This would be used to fill up the remaining space with the same tone until the rotation is complete
                    // This is probably unnecessary as the modifier with the value 8 will indicate a finished track anyway
                    
                    Tone lastTone = dNotes[dNotes.Count - 1];(i % 2 == 0)
                    {
                        // Even row id
                        Placeholder[i] = returnPlaceholder(newSize, 8);
                    } else
                    {
                        // Odd row id
                        Placeholder[i] = returnPlaceholder(newSize, lastTone.duration);
                    }
                    */

                    int newSize = (DatafieldsPerTrack[i] - ChartRow[i].Count) * Tone.TileSize;
                    Placeholder[i] = returnPlaceholder(newSize, 8);

                    // Change the stroke to invisible so that nothing animates (not even the border)
                    Placeholder[i].Stroke = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
                    ChartRow[i].Add(Placeholder[i]);
                }
            }
        }

        private PieSeries returnPlaceholder(int size, int color)
        {
            if (color == -1)
            {
                return new PieSeries
                {
                    Title = "-1",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(size) },

                    // TODO: Change color to white
                    Fill = new SolidColorBrush(Color.FromRgb(63, 81, 181)),
                    DataLabels = true,
                    Tag = 0
                };
            }
            else
            {
                return new PieSeries
                {
                    Title = "-1",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(size) },
                    Fill = new SolidColorBrush(Tone.GrayId[color]),
                    DataLabels = true,
                    Tag = 0
                };
            }
        }


        public int addTone(int _id, int _modifier, int _frequency, int _duration, int _octave, int _delay, int _altmodifier)
        {
            /*
            // Change pointer to the next track or return -1 if the disk is full
            if (ToneCounter >= DatafieldsPerTrack)
            {
                if (CurrentTrack >= MaxTrack)
                {
                    return -1;
                }
                CurrentTrack += 1;
                ToneCounter = 1;
            }
            */
            CurrentTrack = calculateCurrentTrack();
            if (CurrentTrack >= MaxTrack)
            {
                return -1;
            }

            // Editing an existing note
            if (dNotes.Count > _id)
            {
                Tone _tone = new Tone(_id, _modifier, _frequency, _duration, _octave, _delay, _altmodifier);
                var newChartData = _tone.returnSegments();

                // Calcuate the starting id of the tone to change
                int startId = 0;
                for(int i = 0; i < _id; i++)
                {
                    startId += dNotes[i].NumberOfElements;
                }

                /*
                //int selectedTrack = 0;
                while(startId >= DatafieldsPerTrack)
                {
                    MessageBox.Show("Elements in row " + (2 * selectedTrack) + ": " + ChartRow[2 * selectedTrack].Count);
                    startId -= ChartRow[2 * selectedTrack].Count;
                    selectedTrack++;
                }
                */

                int selectedTrack = 0;
                for (int i = startId; i >= DatafieldsPerTrack[2 * selectedTrack]; i -= ChartRow[2 * (selectedTrack - 1)].Count)
                {
                    startId -= ChartRow[2 * selectedTrack].Count;
                    selectedTrack++;
                }

                // Remove elements at the starting position for every element in the tone at that position
                for (int i = 0; i < dNotes[_id].NumberOfElements; i++)
                {
                    for (int c = 0; c < newChartData.Count(); c++)
                    {
                        ChartRow[c + (2 * selectedTrack)].RemoveAt(startId);
                    }
                }

                // Insert the changed tone into the chart
                for (int i = 0; i < newChartData.Count(); i++)
                {
                    for (int c = 0; c < newChartData[i].Count(); c++)
                    {
                        var newdata = newChartData[i][c];
                        var SelectedTrack = ChartRow[i + (2 * selectedTrack)];
                        SelectedTrack.Insert(startId + c, newdata);
                    }
                }
                // TODO: Remove the tonecounter subtraction
                ToneCounter -= dNotes[_id].NumberOfElements;


                // Replace the note in the backend dictionary
                dNotes.RemoveAt(_id);
                dNotes.Insert(_id, _tone);

                // TODO: Maybe calculate this value instead of relying on adding it up
                // TODO: Write function to calculate it
                // TODO: Maybe remove this as it would lead to problems (Tonecounter = tones in the current track, NOT all tones added together)

                ToneCounter += _tone.NumberOfElements;

                //ToneCounter = returnElementCount();

                updatePlaceholders();
                return 0;
            }
            else
            {
                // Add new tone
                Tone newTone = new Tone(_id, _modifier, _frequency, _duration, _octave, _delay, _altmodifier);

                dNotes.Add(newTone);

                var newChartData = newTone.returnSegments();

                for (int i = 0; i < newChartData.Count(); i++)
                {
                    for (int c = 0; c < newChartData[i].Count(); c++)
                    {
                        var newdata = newChartData[i][c];
                        var SelectedTrack = ChartRow[i + (2 * CurrentTrack)];
                        SelectedTrack.Insert(SelectedTrack.Count, newdata);
                    }
                }

                ToneCounter += newTone.NumberOfElements;
                //ToneCounter = returnElementCount();

                updatePlaceholders();
                return 1;
            }
        }

        private void resetChart()
        {
            // TODO: Remove this
            CurrentTrack = 0;
            ToneCounter = 1;

            Array.Clear(ChartRow, 0, ChartRow.Length);
            ChartRow = new SeriesCollection[6] { new SeriesCollection(), new SeriesCollection(), new SeriesCollection(), new SeriesCollection(), new SeriesCollection(), new SeriesCollection() };
        }

        public void removeTone() //TODO: implement a custom id inbetween int _id)
        {
            if(returnElementCount() > MetadataSize && dNotes.Count > 2)
            {
                int _id = dNotes.Count - 1;

                int startId = 0;
                foreach (var tone in dNotes)
                {
                    startId += tone.NumberOfElements;
                }
                startId -= dNotes[dNotes.Count - 1].NumberOfElements;

                int selectedTrack = 0;
                //                                                                                                                                                                   (selectedTrack - 1) to account for the ++ during the loop
                for (int i = startId; i >= DatafieldsPerTrack[2 * selectedTrack]; i -= ChartRow[2 * (selectedTrack - 1)].Count)
                {
                    startId -= ChartRow[2 * selectedTrack].Count;
                    selectedTrack++;
                }
                
                // Adjust the startId and selectedTrack for when an existing tone was edited and increased in size in the process
                while(ChartRow[2 * selectedTrack].Count < dNotes[dNotes.Count - 1].NumberOfElements)
                {
                    selectedTrack--;
                    startId += ChartRow[2 * selectedTrack].Count;
                }

                // Remove elements at the starting position for every element in the tone at that position
                for (int i = 0; i < dNotes[_id].NumberOfElements; i++)
                {
                    for (int c = 0; c < dNotes[_id].returnSegments().Count(); c++)
                    {
                        ChartRow[c + (2 * selectedTrack)].RemoveAt(startId);
                    }
                }

                // Set a new selectedTrack if applicable
                CurrentTrack = calculateCurrentTrack();

                // Replace the note in the backend dictionary
                ToneCounter -= dNotes[_id].NumberOfElements;

                //ToneCounter = returnElementCount();
                dNotes.RemoveAt(_id);


                updatePlaceholders();
            }
        }

        private int calculateCurrentTrack()
        {
            int track = 0;
            //                                                      (track -1) to get the track number BEFORE 1 it's increased
            for (int i = returnElementCount(); i >= DatafieldsPerTrack[2 * track]; i -= ChartRow[2 * (track - 1)].Count)
            {
                track++;
            }
            return track;
        }

        public void writeMetaData(int _id, int _octave, int _bpm, int _buf)
        {
            int[] bpm = convertBaseBig(_bpm);
            int[] id = convertBaseSmall(_id);
            int[] buf = convertBaseSmall(_buf);

            if (dNotes.Count < 2)
            {
                /*
                // First part of the meta data
                Tone part1 = new Tone(0, 0, 8, _octave, id[0], id[1], bpm[0]);

                // Second part
                // passing -2 for the octave and the delay will prevent that middle part from being rendered
                Tone part2 = new Tone(1, buf[0], bpm[1], buf[1], -2, -2, bpm[2]);
                */


                // First part of the meta data
                Tone part1 = new Tone(0, 0, 8, id[1], 0, 8, id[0]);

                // Second part
                Tone part2 = new Tone(0, _octave, bpm[0], buf[1], bpm[1], buf[0], bpm[2]);

                // TODO: Fix the -2 issue
                // passing -2 for the octave and the delay will prevent that middle part from being rendered



                dNotes.Add(part1);
                dNotes.Add(part2);

                var newChartData1 = part1.returnSegments();
                for (int i = 0; i < newChartData1.Count(); i++)
                {
                    for (int c = 0; c < newChartData1[i].Count(); c++)
                    {
                        var newdata = newChartData1[i][c];
                        var SelectedTrack = ChartRow[i + (2 * CurrentTrack)];
                        SelectedTrack.Insert(SelectedTrack.Count, newdata);
                    }
                }

                var newChartData2 = part2.returnSegments();
                for (int i = 0; i < newChartData2.Count(); i++)
                {
                    for (int c = 0; c < newChartData2[i].Count(); c++)
                    {
                        var newdata = newChartData2[i][c];
                        var SelectedTrack = ChartRow[i + (2 * CurrentTrack)];
                        SelectedTrack.Insert(SelectedTrack.Count, newdata);
                    }
                }
                
                ToneCounter = returnElementCount();

                updatePlaceholders();
            } else
            {
                // First part of the meta data
                Tone part1 = new Tone(0, 0, 8, id[1], 0, 8, id[0]);

                // Second part
                Tone part2 = new Tone(0, _octave, bpm[0], buf[1], bpm[1], buf[0], bpm[2]);

                var newChartData1 = part1.returnSegments();
                var newChartData2 = part2.returnSegments();
                
                // Locate the meta data at the beginning of the song
                int startId = 0;
                int selectedTrack = 0;

                // Remove 5 the first 5 elements at the beginning of the track
                for (int i = 0; i < MetadataSize; i++)
                {
                    for (int c = 0; c < newChartData1.Count(); c++)
                    {
                        ChartRow[c + (2 * selectedTrack)].RemoveAt(startId);
                    }
                }

                // Insert part 1 of the meta data
                for (int i = 0; i < newChartData1.Count(); i++)
                {
                    for (int c = 0; c < newChartData1[i].Count(); c++)
                    {
                        var newdata = newChartData1[i][c];
                        var SelectedTrack = ChartRow[i + (2 * selectedTrack)];
                        SelectedTrack.Insert(startId + c, newdata);
                    }
                }

                // Insert part 2 of the meta data after the first one
                for (int i = 0; i < newChartData2.Count(); i++)
                {
                    for (int c = 0; c < newChartData2[i].Count(); c++)
                    {
                        var newdata = newChartData2[i][c];
                        var SelectedTrack = ChartRow[i + (2 * selectedTrack)];
                        SelectedTrack.Insert(startId + newChartData1[i].Count() + c, newdata);
                    }
                }

                // TODO: Remove the tonecounter subtraction
                //ToneCounter -= dNotes[_id].NumberOfElements;


                // Replace the meta data in the backend dictionary
                dNotes.RemoveAt(1);
                dNotes.RemoveAt(0);
                dNotes.Insert(0, part1);
                dNotes.Insert(1, part2);

                ToneCounter = returnElementCount();

                updatePlaceholders();
            }

            hasMetadata = true;
        }

        private int[] convertBaseBig(int _num)
        {
            // Converts a base 10 number of up to 728 into a three digit base 9 array
            int[] num = new int[3];
            num[0] = (int) Math.Floor((double) _num / 81);
            num[1] = (int)Math.Floor((double) (_num % 81) / 9);
            num[2] = _num % 9;
            return num;
        }

        private int[] convertBaseSmall(int _num)
        {
            // Converts a base 10 number of up to 80 into a two digit base 9 array
            int[] num = new int[2];
            num[0] = (int)Math.Floor((double)(_num % 81) / 9);
            num[1] = _num % 9;
            return num;
        }

        public SeriesCollection[] returnData()
        {
            return ChartRow;
        }

        public Tone returnTone(int index)
        {
            return dNotes[index];
        }

        public int returnElementCount()
        {
            int size = 0;

            for (int i = 0; i < dNotes.Count; i++)
            {
                size += dNotes[i].NumberOfElements;
            }
            return size;
        }

        public int returnNoteCount()
        {
            return dNotes.Count;
        }

        public bool containsMetadata()
        {
            return hasMetadata;
        }

        public bool canAddTones()
        {
            if (calculateCurrentTrack() >= MaxTrack)
            {
                return false;
            }
            return true;
        }

        public bool canRemoveTones()
        {
            if (returnElementCount() <= MetadataSize)
            {
                return false;
            }
            return true;
        }
    }
}
