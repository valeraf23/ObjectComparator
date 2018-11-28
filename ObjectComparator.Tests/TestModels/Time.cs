using System.Collections.Generic;

namespace ObjectsComparator.Tests.TestModels
{
    internal class Time
    {
        public short Day;
        public int Hours;
        public int Month;
        public double Period;
        public float Seconds;
        public List<string> Week;

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

        public string PropYear { get; set; }
    }
}