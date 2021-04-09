using System;

namespace Server.Models
{
    public class League
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Hockey_Category_Id { get; set; }
    }
}
