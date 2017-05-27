using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PieControls;
using System.Collections.ObjectModel;

using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using System.IO;
using System.IO.Packaging;

using System.Windows.Xps.Packaging;
using System.Windows.Xps;
using PdfSharp;
using System.Windows.Media.Animation;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Threading;

namespace NXTBurner
{
    public partial class MainWindow
    {
        Music mMusic = new Music();

        int CurrentToneId = 0;
        int SelectedToneId = 0;

        public MainWindow()
        {
            InitializeComponent();

            sli_meta_octave.Minimum = Tone.OctaveToId.Keys.ToList().Min();
            sli_meta_octave.Maximum = Tone.OctaveToId.Keys.ToList().Max();

            num_modifier.Minimum = Tone.ModifierToId.Keys.ToList().Min();
            num_modifier.Maximum = Tone.ModifierToId.Keys.ToList().Max();

            num_frequency.Minimum = Tone.FrequencyToId.Keys.ToList().Min();
            num_frequency.Maximum = Tone.FrequencyToId.Keys.ToList().Max();

            num_duration.Minimum = Tone.DurationToId.Keys.ToList().Min();
            num_duration.Maximum = Tone.DurationToId.Keys.ToList().Max();
            
            num_octave.Minimum = Tone.OctaveToId.Keys.ToList().Min();
            num_octave.Maximum = Tone.OctaveToId.Keys.ToList().Max();
            
            num_delay.Minimum = Tone.DelayToId.Keys.ToList().Min();
            num_delay.Maximum = Tone.DelayToId.Keys.ToList().Max();

            num_alt_modifier.Minimum = Tone.AltModifierToId.Keys.ToList().Min();
            num_alt_modifier.Maximum = Tone.AltModifierToId.Keys.ToList().Max();

            SeriesCollection[] UpdatedData = mMusic.returnData();
            lc_upper_1.Series = UpdatedData[0];
            lc_lower_1.Series = UpdatedData[1];

            lc_upper_2.Series = UpdatedData[2];
            lc_lower_2.Series = UpdatedData[3];
            
            lc_upper_3.Series = UpdatedData[4];
            lc_lower_3.Series = UpdatedData[5];
        }

        private void livechart_DataClick(object sender, ChartPoint chartPoint)
        {
            int id = Int32.Parse(chartPoint.SeriesView.Title);
            selectTone(id);
        }

        private void selectTone(int index)
        {
            if (index != -1 && index > 1)
            {
                SelectedToneId = index;

                Tone selectedTone = mMusic.returnTone(index);
                num_modifier.Value = selectedTone.modifier;
                num_frequency.Value = selectedTone.frequency;
                num_duration.Value = selectedTone.duration;
                num_octave.Value = selectedTone.octave;
                num_delay.Value = selectedTone.delay;
                num_alt_modifier.Value = selectedTone.alt_modifier;
                updatePreview();

                btn_addtone.Content = "Update Tone";
            }
        }

        private void btn_playtone_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not implemented yet");
        }

        private void btn_addtone_Click(object sender, RoutedEventArgs e)
        {
            // Fix the delay if the first two colums are identical
            // This changes the delay to 8 (white) which means it's identical to the modifier value
            checkForDoubleValues();

            int Modifier = (int)Math.Round((double)num_modifier.Value);
            int Frequency = (int)Math.Round((double) num_frequency.Value);
            int Duration = (int)Math.Round((double) num_duration.Value);
            int Octave = (int)Math.Round((double) num_octave.Value);
            int Delay = (int)Math.Round((double)num_delay.Value);
            int AltModifier = (int)Math.Round((double)num_alt_modifier.Value);

            mMusic.addTone(SelectedToneId, Modifier, Frequency, Duration, Octave, Delay, AltModifier);
            CurrentToneId = mMusic.returnNoteCount();


            SelectedToneId = CurrentToneId;

            resetTone();
            updatePreview();

            btn_addtone.Content = "Add Tone";
        }

        private void btn_removetone_Click(object sender, RoutedEventArgs e)
        {
            mMusic.removeTone();
            SelectedToneId = mMusic.returnNoteCount();

            updatePreview();
        }

        private void btn_print_track_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                btn_print_track.IsEnabled = false;
                string selectedId = labelToneId.Text;
                labelToneId.Text = num_meta_id.Value.ToString();

                var dialog = new CommonSaveFileDialog();
                dialog.AddToMostRecentlyUsedList = false;
                dialog.Title = "Where do you want to save the .pdf file?";

                dialog.DefaultFileName = "Disk_" + num_meta_id.Value.ToString() + ".pdf";
                dialog.DefaultExtension = "pdf";
                dialog.Filters.Add(new CommonFileDialogFilter("PDF document", "pdf"));

                CommonFileDialogResult result = dialog.ShowDialog();

                if (result == CommonFileDialogResult.Ok)
                {
                    MemoryStream lMemoryStream = new MemoryStream(32768);
                    Package package = Package.Open(lMemoryStream, FileMode.Create);
                    XpsDocument doc = new XpsDocument(package);
                    XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(doc);
                    writer.Write(ChartArea);
                    doc.Close();
                    package.Close();

                    var pdfXpsDoc = PdfSharp.Xps.XpsModel.XpsDocument.Open(lMemoryStream);
                    PdfSharp.Xps.XpsConverter.Convert(pdfXpsDoc, dialog.FileName, 0);

                    labelToneId.Text = selectedId;
                }

                btn_print_track.IsEnabled = true;
            }
        }

        private void resetTone()
        {
            num_modifier.Value = Music.DEFAULT_MODIFIER;
            num_frequency.Value = Music.DEFAULT_FREQUENCY;
            num_duration.Value = Music.DEFAULT_DURATION;
            num_octave.Value = Music.DEFAULT_OCTAVE;
            num_delay.Value = Music.DEFAULT_DELAY;
            num_alt_modifier.Value = Music.DEFAULT_ALTMODIFIER;

            updatePreview();
        }

        private void checkForDoubleValues()
        {
            if (IsLoaded)
            {
                if (num_frequency.Value == num_octave.Value && num_modifier.Value == num_delay.Value)
                {
                    num_delay.Value = 8;
                }
            }
        }

        private void updatePreview()
        {
            if(IsLoaded)
            {
                double test = (double) num_modifier.Value;

                rectFrequency.Background = new SolidColorBrush(Tone.GrayId[(int)Math.Round((double)num_frequency.Value)]);
                rectModifier.Background = new SolidColorBrush(Tone.GrayId[(int)Math.Round((double)num_modifier.Value)]);
                rectOctave.Background = new SolidColorBrush(Tone.GrayId[(int)Math.Round((double)num_octave.Value)]);
                rectDelay.Background = new SolidColorBrush(Tone.GrayId[(int)Math.Round((double)num_delay.Value)]);
                rectDuration.Background = new SolidColorBrush(Tone.GrayId[(int)Math.Round((double)num_duration.Value)]);
                rectAltModifier.Background = new SolidColorBrush(Tone.GrayId[(int)Math.Round((double)num_alt_modifier.Value)]);

                txtModifierExplanation.Text = IDConverter.ModiferExplanation[(int)num_modifier.Value];
                txtAltModifierExplanation.Text = IDConverter.AltModiferExplanation[(int)num_alt_modifier.Value];

                labelToneId.Text = SelectedToneId.ToString();
                updateButtons();
            }
        }

        public void updateButtons()
        {
            if (IsLoaded)
            {
                if (!mMusic.containsMetadata())
                {
                    btn_addtone.IsEnabled = false;
                    btn_playtone.IsEnabled = false;
                    btn_play_track.IsEnabled = false;
                    btn_print_track.IsEnabled = false;
                    btn_removetone.IsEnabled = false;
                    return;
                } else
                {
                    btn_print_track.IsEnabled = true;
                }


                if (!mMusic.canRemoveTones())
                {
                    btn_removetone.IsEnabled = false;
                } else
                {
                    btn_removetone.IsEnabled = true;
                }

                if (!mMusic.canAddTones())
                {
                    btn_addtone.IsEnabled = false;
                }
                else
                {
                    btn_addtone.IsEnabled = true;
                }
            }
                
        }

        private void NumericUpDown_MouseDown(object sender, MouseButtonEventArgs e)
        {
            (sender as UIElement).Focus();
        }

        private void NumericUpDown_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as UIElement).Focus();
        }

        private void numChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            updatePreview();
            checkForDoubleValues();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            resetTone();
        }

        private void btn_update_metadata_Click(object sender, RoutedEventArgs e)
        {
            if(IsLoaded)
            {
                int MetaId = (int)Math.Round((double)num_meta_id.Value);
                int MetaBpm = (int)Math.Round((double)num_meta_bpm.Value);
                int MetaBuffer = (int)Math.Round((double)num_meta_buffer.Value);
                int MetaOctave = (int)Math.Round((double)sli_meta_octave.Value);

                mMusic.writeMetaData(MetaId, MetaOctave, MetaBpm, MetaBuffer);

                CurrentToneId = mMusic.returnNoteCount();


                SelectedToneId = CurrentToneId;

                updateButtons();
            }
        }
    }
}
