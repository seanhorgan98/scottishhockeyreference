using System;

namespace WebScraper.Models
{
    public class Fixture
    {
            public DateTime Date { get; set; }
            public int TeamOne { get; set; }
            public int TeamTwo { get; set; }
            public int TeamOneScore { get; set; }
            public int TeamTwoScore { get; set; }
            public int League { get; set; }
            public int Category { get; set; }
            public string Location { get; set; }
            public int ID { get; set; }

            public Fixture()
            {
            }
    }
}