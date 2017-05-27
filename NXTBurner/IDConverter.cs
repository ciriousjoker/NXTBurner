using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NXTBurner
{
    static class IDConverter
    {
        // TODO: Verify that all of them are correct
        public static List<string> ModiferExplanation = new List<string>()
        {
            { "Calibration field" },                    // 0 - Black
            { "" },
            { "Sets a loop marker: "},
            { "Uses modifier ID as delay in cases where the first column would be the seme as the second one" },
            { "Play the note twice" },
            { "Play the note four times" },
            { "Reduce by a half tone" },
            { "Increase by a half tone" },
            { "Marks the end of the track" }            // 8 - White
        };

        public static List<string> AltModiferExplanation = new List<string>()
        {
            { "Nothing yet" },  // 0 - Black
            { "Nothing yet" },
            { "Repeat 2x"},
            { "Repeat 3x" },
            { "Repeat 4x" },
            { "Repeat 5x" },
            { "Nothing yet" },
            { "Nothing yet" },
            { "" }              // 8 - White
        };


    }
}
