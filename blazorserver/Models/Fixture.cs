using System;

namespace blazorserver.Models
{
    public class Fixture
    {
        public int Id { get; set; }
        public string Location { get; set; }
        public int Season_ID { get; set; }
        public int Team_1_ID { get; set; }
        public int Team_2_ID { get; set; }
        public int Cup_ID { get; set; }
        public DateTime Date { get; set; }
        public int Elo_Change { get; set; }
        public int League_ID { get; set; }
        public int Team_1_Score { get; set; }
        public int Team_2_Score { get; set; }
    }
}