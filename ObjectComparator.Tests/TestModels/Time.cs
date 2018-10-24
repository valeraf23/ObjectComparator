using System.Collections.Generic;

namespace ObjectComparator.Tests.TestModels
{
    internal class Time
    {
        public float Seconds;
        public short Day;
        public double Period;
        public List<string> Week;
        public int Month;
        public int Hours;
        public string PropYear { get; set; }

        public Time(string year1, float seconds1, short day1, double period1, List<string> week1, int month1,
            int hours1)
        {
            Seconds = seconds1;
            PropYear = year1;
            Day = day1;
            Period = period1;
            Week = week1;
            Month = month1;
            Hours = hours1;
        }

    }
}

