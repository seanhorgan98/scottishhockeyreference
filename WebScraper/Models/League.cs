namespace WebScraper.Models
{
    public class League
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int HockeyCategoryID { get; set; }

        public League()
        {
        }
    }
}