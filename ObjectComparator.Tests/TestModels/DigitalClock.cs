using System.Collections.Generic;

namespace ObjectComparator.Tests.TestModels
{
    internal class DigitalClock : Time
    {
        public bool ClickTimer;
        public int[] NumberMonth;
        public Calendar PropCalendar { get; set; }

        public DigitalClock(bool clickTimer, int[] numberMonth, Calendar calendar1, string year1, float seconds1,
            short day1, double period1, List<string> week1, int month1, int hours1)
            : base(year1, seconds1, day1, period1, week1, month1, hours1)
        {
            ClickTimer = clickTimer;
            NumberMonth = numberMonth;
            PropCalendar = calendar1;
        }

    }
}
