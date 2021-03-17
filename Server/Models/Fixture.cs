using System;

namespace Server.Models
{
    public class Fixture
    {
        public long Id { get; set; }
        public string Location { get; set; }
        public string League { get; set; }
        public string teamOne { get; set; }
        public string teamTwo { get; set; }
        public int teamOneScore { get; set; }
        public int teamTwoScore { get; set; }
        public DateTime fixtureDate { get; set; }
    }
}
