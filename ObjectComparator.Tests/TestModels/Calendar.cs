namespace ObjectsComparator.Tests.TestModels
{
    internal class Calendar
    {
        public Calendar(int page1, Time timePanel1)
        {
            Page = page1;
            PropTimePanel = timePanel1;
        }

        public int Page { get; set; }
        public Time PropTimePanel { get; set; }
    }
}