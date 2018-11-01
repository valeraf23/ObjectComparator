namespace ObjectsComparator.Tests.TestModels
{
    internal class Calendar
    {

        public int Page { get; set; }
        public Time PropTimePanel { get; set; }

        public Calendar(int page1, Time timePanel1)
        {
            Page = page1;
            PropTimePanel = timePanel1;
        }
    }
}

