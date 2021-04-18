using AngleSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace scottishhockeyreference.Scraper
{
    class Program
    {
        //static HttpClient client;
        private static readonly string leagueURL = "https://www.scottish-hockey.org.uk/league-standings/";
        private static readonly string connectionString = "server=aa1su4hgu44u0mv.cxkd3gywhaht.eu-west-1.rds.amazonaws.com; port=3306; database=shr_prod; user=proddb; password=H4ppyF4c3; Persist Security Info=False; Connect Timeout=300";
        static async Task Main()
        {
            //var clientHandler = new HttpClientHandler
            //{
            //    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
            //};

            //// Pass the handler to httpclient(from you are calling api)
            //client = new HttpClient(clientHandler);

            // DatabaseTest();
            // await ScrapeLeagues();
            // await ScrapeNewTeams();
            await ScrapePoints();
        }

        //private static void DatabaseTest()
        //{
        //    var conn = new MySqlConnection(connectionString);
        //    conn.Open();
        //    var teamList = new List<Team>();
        //    var sqlSelect = "SELECT * FROM teams order by id desc;";
        //    var cmd = new MySqlCommand(sqlSelect, conn);
        //    using (MySqlDataReader rdr = cmd.ExecuteReader()) {
        //        while (rdr.Read()) {
        //            /* iterate once per row */
        //            var team = new Team
        //            {
        //                ID = rdr.GetInt32(0),
        //                Teamname = (rdr.IsDBNull(1)) ? "" : rdr.GetString(1),
        //                League_ID = rdr.GetInt32(2),
        //                Hockey_Category_ID = rdr.GetInt32(22)
        //            };
        //            // System.Console.WriteLine("Name: " + team.Teamname + ", League ID: " + team.League_ID + ", Cat: " + team.Hockey_Category_ID);
        //            teamList.Add(team);
        //        }
        //    }
        //    System.Console.WriteLine(teamList.Single(x => x.ID == 823).Teamname);


        //}

        static async Task ScrapeLeagues()
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(leagueURL);
            var AllLeagues = document.QuerySelectorAll("h2.text-uppercase");

            System.Console.WriteLine("THERE");
            foreach (var item in AllLeagues)
            {
                // Console.WriteLine("HERE");
                if (item.TextContent.Contains("Conference") || item.TextContent.Contains("Super"))
                {
                    System.Console.WriteLine("Skipped non-standard league: " + item.TextContent);
                    continue;
                }
                SaveLeagueSQL(item.TextContent, GetLeagueHockeyCategoryByName(item.TextContent));
            }
        }

        private static void SaveLeagueSQL(string name, int category)
        {
            var conn = new MySqlConnection(connectionString);
            conn.Open();
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Leagues (Name, Hockey_Category_ID) VALUES (@NAME, @CATEGORY)";
            cmd.Parameters.AddWithValue("@NAME", name);
            cmd.Parameters.AddWithValue("@CATEGORY", category);
            cmd.ExecuteNonQuery();
            conn.Close();

        }

        static async Task ScrapePoints()
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(leagueURL);

            // Get list of all teams
            var teamList = new List<Team>();
            var conn = new MySqlConnection(connectionString);
            conn.Open();
            var sqlSelect = @"SELECT ID,
    League_ID,
    Hockey_Category_ID,
    Sponsor,
    League_Rank,
    SeasonPlayed,
    SeasonWon,
    SeasonDrawn,
    SeasonLost,
    SeasonGoalsFor,
    SeasonGoalsAgainst,
    SeasonGoalDifference,
    SeasonPoints,
    Teamname FROM Teams; ";
            var cmd = new MySqlCommand(sqlSelect, conn);
            using (MySqlDataReader rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    /* iterate once per row */
                    var team = new Team
                    {
                        Teamname = (rdr.IsDBNull(13)) ? "" : rdr.GetString(13)
                    };

                    team.ID = rdr.GetInt32(0);
                    team.League_ID = rdr.GetInt32(1);
                    team.Hockey_Category_ID = rdr.GetInt32(2);
                    team.Sponsor = (rdr.IsDBNull(3)) ? "" : rdr.GetString(3);
                    team.League_Rank = rdr.GetInt32(4);
                    team.SeasonPlayed = rdr.GetInt32(5);
                    team.SeasonWon = rdr.GetInt32(6);
                    team.SeasonDrawn = rdr.GetInt32(7);
                    team.SeasonLost = rdr.GetInt32(8);
                    team.SeasonGoalsFor = rdr.GetInt32(9);
                    team.SeasonGoalsAgainst = rdr.GetInt32(10);
                    team.SeasonGoalDifference = rdr.GetInt32(11);
                    team.SeasonPoints = rdr.GetInt32(12);
                    teamList.Add(team);
                    System.Console.WriteLine(team.Teamname);
                }
            }

            int played = 0;
            int won = 0;
            int drawn = 0;
            int lost = 0;
            int gfor = 0;
            int gagainst = 0;
            int gd = 0;
            int points = 0;
            int rank = 0;

            // For each league table
            var leagueTeams = document.QuerySelectorAll("table.league-standings");
            foreach (var league in leagueTeams)
            {
                // For each row in league
                var teamRow = league.QuerySelectorAll("tr.mobile-border");
                foreach (var div in teamRow)
                {
                    Console.WriteLine("THERE");
                    string currentTeam = "";
                    // Take only the teamname and the sponsor
                    var teamDetails = div.QuerySelectorAll("th.no-border.team-details");
                    int i = 0;
                    foreach (var item in teamDetails)
                    {
                        if (i == 1)
                        {
                            currentTeam = item.TextContent;

                        }
                        i++;
                    }
                    var scoreDetails = div.QuerySelectorAll("th.no-border.text-center.scores");
                    int j = 0;
                    foreach (var item in scoreDetails)
                    {
                        if (j == 0)
                        {
                            rank = Int32.Parse(item.TextContent);
                        }
                        else if (j == 1)
                        {
                            // Played
                            played = Int32.Parse(item.TextContent);
                        }
                        else if (j == 2)
                        {
                            // Won
                            won = Int32.Parse(item.TextContent);
                        }
                        else if (j == 3)
                        {
                            // Drawn
                            drawn = Int32.Parse(item.TextContent);
                        }
                        else if (j == 4)
                        {
                            // Lost
                            lost = Int32.Parse(item.TextContent);
                        }
                        else if (j == 5)
                        {
                            // Goals For
                            gfor = Int32.Parse(item.TextContent);
                        }
                        else if (j == 6)
                        {
                            // Goals Against
                            gagainst = Int32.Parse(item.TextContent);
                        }
                        else if (j == 7)
                        {
                            // Goal Difference
                            gd = Int32.Parse(item.TextContent);
                        }
                        else if (j == 8)
                        {
                            // Points
                            points = Int32.Parse(item.TextContent);
                        }
                        j++;
                    }
                    // Update team with new points
                    var teamToUpdate = new Team
                    {
                        Teamname = currentTeam
                    };
                    if (teamList.Any(x => x.Teamname == teamToUpdate.Teamname))
                    {
                        teamToUpdate.ID = teamList.FirstOrDefault(x => x.Teamname == teamToUpdate.Teamname).ID;
                        teamToUpdate.League_ID = teamList.FirstOrDefault(x => x.ID == teamToUpdate.ID).League_ID;
                        teamToUpdate.Hockey_Category_ID = teamList.FirstOrDefault(x => x.ID == teamToUpdate.ID).Hockey_Category_ID;
                        teamToUpdate.Sponsor = teamList.FirstOrDefault(x => x.ID == teamToUpdate.ID).Sponsor;
                        teamToUpdate.League_Rank = rank;
                        teamToUpdate.SeasonPlayed = played;
                        teamToUpdate.SeasonWon = won;
                        teamToUpdate.SeasonDrawn = drawn;
                        teamToUpdate.SeasonLost = lost;
                        teamToUpdate.SeasonGoalsFor = gfor;
                        teamToUpdate.SeasonGoalsAgainst = gagainst;
                        teamToUpdate.SeasonGoalDifference = gd;
                        teamToUpdate.SeasonPoints = points;
                        System.Console.WriteLine(JsonConvert.SerializeObject(teamToUpdate));
                        SavePointsSQL(teamToUpdate);
                    }
                }
            }
        }

        private static void SavePointsSQL(Team teamToUpdate)
        {
            var conn = new MySqlConnection(connectionString);
            conn.Open();
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Teams
SET League_Rank = @LEAGUE_RANK,
    SeasonPlayed = @PLAYED,
    SeasonWon = @WON,
    SeasonDrawn = @DRAWN,
    SeasonLost = @LOST,
    SeasonGoalsFor = @GFOR,
    SeasonGoalsAgainst = @GAGAINST,
    SeasonGoalDifference = @GDIFFERENCE,
    SeasonPoints = @POINTS
WHERE ID = @ID;";
            cmd.Parameters.AddWithValue("@LEAGUE_RANK", teamToUpdate.League_Rank);
            cmd.Parameters.AddWithValue("@PLAYED", teamToUpdate.SeasonPlayed);
            cmd.Parameters.AddWithValue("@WON", teamToUpdate.SeasonWon);
            cmd.Parameters.AddWithValue("@DRAWN", teamToUpdate.SeasonDrawn);
            cmd.Parameters.AddWithValue("@LOST", teamToUpdate.SeasonLost);
            cmd.Parameters.AddWithValue("@GFOR", teamToUpdate.SeasonGoalsFor);
            cmd.Parameters.AddWithValue("@GAGAINST", teamToUpdate.SeasonGoalsAgainst);
            cmd.Parameters.AddWithValue("@GDIFFERENCE", teamToUpdate.SeasonGoalDifference);
            cmd.Parameters.AddWithValue("@POINTS", teamToUpdate.SeasonPoints);
            cmd.Parameters.AddWithValue("@ID", teamToUpdate.ID);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        //static async Task ScrapeTeamNames()
        //{
        //    var TeamList = new List<string>();
        //    var config = Configuration.Default.WithDefaultLoader();
        //    var context = BrowsingContext.New(config);
        //    var document = await context.OpenAsync(leagueURL);
        //    var AllTeams = document.QuerySelectorAll("th.no-border.team-details");

        //    int i = 0;
        //    foreach (var item in AllTeams)
        //    {
        //        if (i % 3 == 0)
        //        {
        //            i++;
        //            continue;
        //        }
        //        else if (i % 3 == 1)
        //        {
        //            TeamList.Add(item.TextContent);
        //        }
        //        i++;
        //    }
        //    var sortedList = TeamList.OrderBy(x => x).ToList();
        //    foreach (var item in sortedList)
        //    {
        //        Console.WriteLine(item);
        //    }
        //}

        static async Task ScrapeNewTeams()
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(leagueURL);

            // Get Leagues from Database
            var leagueList = new List<League>();
            var conn = new MySqlConnection(connectionString);
            conn.Open();
            var sqlSelect = "SELECT * FROM Leagues";
            var cmd = new MySqlCommand(sqlSelect, conn);
            using (MySqlDataReader rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    /* iterate once per row */
                    var league = new League(rdr.GetInt32(0), (rdr.IsDBNull(1)) ? "" : rdr.GetString(1), rdr.GetInt32(2));
                    leagueList.Add(league);
                }
            }


            //// var leagueResponse = await client.GetAsync("http://localhost:33988/api/Leagues");
            //var leagueResponse = await client.GetAsync("http://localhost:5000/api/Leagues");
            //leagueResponse.EnsureSuccessStatusCode();
            //var leagueResponseBody = await leagueResponse.Content.ReadAsStringAsync();
            //var leagueList = JsonConvert.DeserializeObject<List<League>>(leagueResponseBody);

            // Scrape all leagues
            var LeagueList = new List<string>();
            var AllLeagues = document.QuerySelectorAll("h2.text-uppercase");

            foreach (var item in AllLeagues)
            {
                LeagueList.Add(item.TextContent);
            }

            int index = 0;
            _ = document.QuerySelectorAll("div.tableWrap");

            // For each league table
            var leagueTeams = document.QuerySelectorAll("table.league-standings");
            foreach (var league in leagueTeams)
            {
                if (LeagueList[index].Contains("Conference") || LeagueList[index].Contains("Super"))
                {
                    Console.WriteLine("Skipped non-standard league: " + LeagueList[index]);
                    index++;
                    continue;
                }
                var rank = 1;
                // For each row in league
                var teamRow = league.QuerySelectorAll("tr.mobile-border");
                foreach (var div in teamRow)
                {
                    string currentLeague = LeagueList[index];
                    string currentTeam = "";
                    string currentSponsor = "";
                    // Take only the teamname and the sponsor
                    var teamDetails = div.QuerySelectorAll("th.no-border.team-details");

                    int i = 0;
                    foreach (var item in teamDetails)
                    {
                        if (i % 3 == 0)
                        {
                            i++;
                            continue;
                        }
                        else if (i % 3 == 1)
                        {
                            currentTeam = item.TextContent;

                        }
                        else if (i % 3 == 2)
                        {
                            currentSponsor = item.TextContent;

                        }
                        i++;
                    }
                    // Get Category
                    var teamToPost = new Team
                    {
                        Teamname = currentTeam,
                        Sponsor = currentSponsor,
                        League_Rank = rank
                    };
                    // Console.WriteLine(JsonConvert.SerializeObject(teamToPost));
                    GetLeagueIDAndCategoryByName(leagueList, currentLeague, teamToPost);

                    SaveTeamSQL(teamToPost);
                    rank++;
                }
                index++;
            }
        }

        private static void SaveTeamSQL(Team teamToPost)
        {
            Console.WriteLine(JsonConvert.SerializeObject(teamToPost));
            var conn = new MySqlConnection(connectionString);
            conn.Open();
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Teams (TeamName, League_ID, Sponsor, League_Rank, Hockey_Category_ID) " +
                "VALUES (@TEAMNAME, @LEAGUE_ID, @SPONSOR, @LEAGUE_RANK, @CATEGORY)";
            cmd.Parameters.AddWithValue("@TEAMNAME", teamToPost.Teamname);
            cmd.Parameters.AddWithValue("@LEAGUE_ID", teamToPost.League_ID);
            cmd.Parameters.AddWithValue("@SPONSOR", teamToPost.Sponsor);
            cmd.Parameters.AddWithValue("@LEAGUE_RANK", teamToPost.League_Rank);
            cmd.Parameters.AddWithValue("@CATEGORY", teamToPost.Hockey_Category_ID);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        private static void GetLeagueIDAndCategoryByName(List<League> leagueList, string currentLeague, Team team)
        {
            foreach (var league in leagueList)
            {
                if (league.Name.Equals(currentLeague))
                {
                    team.League_ID = league.Id;
                    team.Hockey_Category_ID = league.Hockey_Category_ID;
                }
            }
        }

        private static int GetLeagueHockeyCategoryByName(string teamname)
        {
            if ((teamname.Contains("Women") || teamname.Contains("Ladies")) && teamname.Contains("Indoor"))
            {
                return 4;
            }
            if (teamname.Contains("Indoor"))
            {
                return 3;
            }
            if (teamname.Contains("Women") || teamname.Contains("Ladies"))
            {
                return 2;
            }
            return 1;
        }

        //public static async Task SaveTeam(Team teamToPost)
        //{
        //    System.Console.WriteLine(JsonConvert.SerializeObject(teamToPost));
        //    // await client.PostAsJsonAsync("http://localhost:33988/api/Teams", teamToPost);
        //    var response = await client.PostAsJsonAsync("http://localhost:5000/api/Teams", teamToPost);
        //    System.Console.WriteLine(response);

        //}


        //public static async Task SaveLeague(string name, int category)
        //{
        //    var leagueToPost = new League(0, name, category);

        //    var response = await client.PostAsJsonAsync("https://localhost:33988/leagues/create", leagueToPost);
        //    // var response = await client.PostAsJsonAsync("http://localhost:5000/api/Leagues", leagueToPost);
        //    Console.WriteLine(response);
        //}
    }

    class Team
    {
        public int ID { get; set; }
        public string Teamname { get; set; }
        public int League_ID { get; set; }
        public string Sponsor { get; set; }
        public int Hockey_Category_ID { get; set; }
        public int League_Rank { get; set; }
        public int SeasonDrawn { get; set; }
        public int SeasonGoalDifference { get; set; }
        public int SeasonGoalsAgainst { get; set; }
        public int SeasonGoalsFor { get; set; }
        public int SeasonLost { get; set; }
        public int SeasonPlayed { get; set; }
        public int SeasonPoints { get; set; }
        public int SeasonWon { get; set; }

        public Team()
        {
        }
    }

    class League
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Hockey_Category_ID { get; set; }

        public League(int id, string name, int hockey_category_id)
        {
            this.Id = id;
            this.Name = name;
            this.Hockey_Category_ID = hockey_category_id;
        }
    }
}
