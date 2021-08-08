namespace blazorserver.Models
{
    public class League
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Hockey_Category_Id { get; set; }
        public int Active { get; set; }
    }
}