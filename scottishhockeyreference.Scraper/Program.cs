using AngleSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using MySql.Data.MySqlClient;

namespace scottishhockeyreference.Scraper
{
    internal class Program
    {
        //static HttpClient client;
        private const string LeagueUrl = "https://www.scottish-hockey.org.uk/league-standings/";
        private const string resultsURL = "https://www.scottish-hockey.org.uk/latest-results/";
        private const string htmlFile = "/home/sean/Desktop/Scottish Hockey Results 2020/Men Indoor Nat 3.html";
        // private static readonly string connectionString = "server=aa1su4hgu44u0mv.cxkd3gywhaht.eu-west-1.rds.amazonaws.com; port=3306; database=shr_prod; user=proddb; password=H4ppyF4c3; Persist Security Info=False; Connect Timeout=300";
        private const string connectionString = "server=localhost; port=3306; database=scottishhockeyreference; user=root; password=root; Persist Security Info=False; Connect Timeout=300";

        private static async Task Main()
        //private static void Main()
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
            // await ScrapePoints();
            // await ScrapeResults();
            await TestScrape();
            //CalculateElo(100, 5, 3000, 0, 0);
        }

        private static async Task TestScrape()
        {
            // Get List of Leagues
            var conn = new MySqlConnection(connectionString);
            conn.Open();
            var leagueList = new List<League>();
            var sqlSelect = "SELECT * FROM Leagues;";
            var cmd = new MySqlCommand(sqlSelect, conn);
            using (MySqlDataReader rdr = cmd.ExecuteReader()) {
                while (rdr.Read()) {
                    /* iterate once per row */
                    var league = new League(rdr.GetInt32(0), rdr.GetString(1), rdr.GetInt32(2));
                    leagueList.Add(league);
                }
            }

            // Get List of Teams
            var teamList = new List<Team>();
            sqlSelect = "SELECT * FROM Teams;";
            var cmdTeam = new MySqlCommand(sqlSelect, conn);
            using (MySqlDataReader rdr = cmdTeam.ExecuteReader()) {
                while (rdr.Read()) {
                    /* iterate once per row */
                    var team = new Team
                    {
                        ID = rdr.GetInt32(0),
                        Teamname = (rdr.IsDBNull(1)) ? "" : rdr.GetString(1),
                        League_ID = rdr.GetInt32(2),
                        Hockey_Category_ID = rdr.GetInt32(22)
                    };
                    // System.Console.WriteLine("Name: " + team.Teamname + ", League ID: " + team.League_ID + ", Cat: " + team.Hockey_Category_ID);
                    teamList.Add(team);
                }
            }

            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var htmlText = System.IO.File.ReadAllText(htmlFile);
            //var document = await context.OpenAsync(resultsURL);
            var document = await context.OpenAsync(req => req.Content(htmlText));
            var tableWrap = document.All.Where(m => m.LocalName == "tr");
            DateTime currentDate = DateTime.MinValue;
            var mostRecentDate = GetMostRecentDate();
            //var topDate = DateTime.Parse(tableWrap.First().Text());

            foreach (var row in tableWrap)
            {
                //System.Console.WriteLine(row.Text() + ", out of " + tableWrap.ToArray().Length);
                // If row is a date and not umpires
                if (row.Children.Length == 1 && (!row.Text().Contains("Umpires") && !row.Text().Contains("Officials")))
                {
                    // Console.WriteLine(row.Text());
                    currentDate = DateTime.Parse(row.Text());
                }
                // If row has a bye or missing a column
                else if (row.Children.Length < 5 || row.Children.Length > 7) // Make 6 when on latest fixtures, 5 for league page
                {
                    Console.WriteLine("LENGTH");
                }
                // If row is postponed
                // else if (row.Children.SingleOrDefault(r => r.ClassName == "text-center scores homeScore").Text() == "P")
                // {
                //     // Console.WriteLine(row.Text());
                // }
                else
                {
                    if (DateTime.Compare(currentDate,mostRecentDate) < 0)
                    {
                        // System.Console.WriteLine($"Current Date: {currentDate}, Database Date: {mostRecentDate}");
                        // break;
                    }
                    var fixtureDate = currentDate;
                    var tempLeague = row.Children[0].Text();

                    // ADD WHEN RUNNING IN PRODUCTION
                    // if (tempLeague.Contains("Super") || tempLeague.Contains("Conference"))
                    // {
                    //     continue;
                    // }
                    // IF LEAGUE IS ACTUALLY A CUP IGNORE??

                    // Need to change numbers when not on league page
                    var fixtureLeague = 15; //GetLeagueIDByName(leagueList, tempLeague);
                    var fixtureTeamOne = GetTeamIdByName(teamList, row.Children[0].Text());
                    var fixtureTeamOneScore = Convert.ToInt32(row.Children[1].Text());
                    var fixtureTeamTwoScore = Convert.ToInt32(row.Children[2].Text());
                    var fixtureTeamTwo = GetTeamIdByName(teamList, row.Children[3].Text());
                    var fixtureLocation = row.Children[4].Text();
                    var fixtureCategory = GetCategoryByLeague(leagueList, fixtureLeague);
                    if (fixtureTeamOne == 0 || fixtureTeamTwo == 0) continue;

                    var eloChanges = CalculateElo(fixtureTeamOne, fixtureTeamOneScore, fixtureTeamTwo, fixtureTeamTwoScore, 0);
                    var eloOneChange = eloChanges.Item1;
                    var eloTwoChange = eloChanges.Item2;
                    PostFixtureToDatabase(fixtureDate, fixtureLeague, fixtureTeamOne, fixtureTeamTwo, fixtureTeamOneScore, fixtureTeamTwoScore, fixtureLocation, eloOneChange, fixtureCategory, eloTwoChange);
                    UpdateTeamEloRating(fixtureTeamOne, eloOneChange);
                    UpdateTeamEloRating(fixtureTeamTwo, eloTwoChange);
                    Console.WriteLine($"{fixtureDate.ToShortDateString()}: {fixtureLeague}, {fixtureTeamOne} {fixtureTeamOneScore} - {fixtureTeamTwoScore} {fixtureTeamTwo}, {fixtureLocation}");
                }
            }
            //SetMostRecentDay(topDate);
        }

        private static (int, int) CalculateElo(int teamOneRating, int scoreOne, int teamTwoRating, int scoreTwo, int league)
        {
            // Variables
            double K = 24;
            var scoreDifference = Math.Abs(scoreOne - scoreTwo);
            System.Console.WriteLine("Score Differece: " + scoreDifference);
            if (scoreDifference == 2)
            {
                K += K * 0.5;
            }else if (scoreDifference == 3)
            {
                K += K * 0.75;
            }else if (scoreDifference > 3){
                K += K * 0.75 + (scoreDifference-3)/8D;
            }
            var denominator = 400;
            float W = 1;              // w is the margin of victory weighting
            float Sa, Sb;
            if (scoreOne > scoreTwo)
            {
                Sa = 1.0f;
                Sb = 0.0f;
            }
            else if (scoreTwo > scoreOne)
            {
                Sa = 0.0f;
                Sb = 1.0f;
            }else
            {
                Sa = 0.5f;
                Sb = 0.5f;
            }
            double ratingDifferenceA = teamTwoRating - teamOneRating;
            double ratingDifferenceB = teamOneRating - teamTwoRating;
            double expoA = ratingDifferenceA/denominator;
            double expoB = ratingDifferenceB/denominator;

            // Expected probability
            var Ea = 1 / (1 + Math.Pow(10D, expoA));
            var Eb = 1 / (1 + Math.Pow(10D, expoB));

            // New Elo calculations
            var teamOneEloChange = Convert.ToInt32(Math.Round(K * W * (Sa - Ea)));
            var teamTwoEloChange = Convert.ToInt32(Math.Round(K * W * (Sb - Eb)));
            var teamOneNewElo = teamOneRating + teamOneEloChange;
            var teamTwoNewElo = teamTwoRating + teamTwoEloChange;

            var EloChange = Math.Abs(Math.Round(K * W * (Sb - Eb)));

            // Absolute Floor
            if (teamOneNewElo < 100)
            {
                teamOneNewElo = teamOneRating;
            }
            if (teamTwoNewElo < 100)
            {
                teamTwoNewElo = teamTwoRating;
            }

            //System.Console.WriteLine($"A Elo: {teamOneRating}, B Elo: {teamTwoRating}\nA' Rating: {teamOneNewElo}, B' Elo: {teamTwoNewElo}, EloChange: {EloChange}, K: {K}");
            return (teamOneEloChange, teamTwoEloChange);
        }

        private static void UpdateTeamEloRating(int teamID, int eloChange)
        {
            // Update Elo Rating
            var conn = new MySqlConnection(connectionString);
            conn.Open();
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Teams
SET Movement=@ELOCHANGE,
    Rating = Rating + @ELOCHANGE
WHERE Id = @TEAMID";
            cmd.Parameters.AddWithValue("@ELOCHANGE", eloChange);
            cmd.Parameters.AddWithValue("@TEAMID", teamID);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        private static void PostFixtureToDatabase(DateTime date, int league, int teamOne, int teamTwo, int teamOneScore, int teamTwoScore, string location, int eloOne, int category, int eloTwo)
        {
            var conn = new MySqlConnection(connectionString);
            conn.Open();
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Fixtures
    (Date,
    Team_1_ID,
    Team_2_ID,
    Location,
    Season_ID,
    League_ID,
    Team_1_Score,
    Team_2_Score,
    Team_1_Elo_Change,
    Hockey_Category_ID,
    Team_2_Elo_Change)
VALUES
	(@MRDATE,
    @TEAMONE,
    @TEAMTWO,
    @LOCATION,
    @SEASON,
    @LEAGUE,
    @SCOREONE,
    @SCORETWO,
    @ELOONE,
    @CATEGORY,
    @ELOTWO);";
            cmd.Parameters.AddWithValue("@MRDATE", date);
            cmd.Parameters.AddWithValue("@TEAMONE", teamOne);
            cmd.Parameters.AddWithValue("@TEAMTWO", teamTwo);
            cmd.Parameters.AddWithValue("@LOCATION", location);
            cmd.Parameters.AddWithValue("@SEASON", 1);
            cmd.Parameters.AddWithValue("@LEAGUE", league);
            cmd.Parameters.AddWithValue("@SCOREONE", teamOneScore);
            cmd.Parameters.AddWithValue("@SCORETWO", teamTwoScore);
            cmd.Parameters.AddWithValue("@ELOONE", eloOne);
            cmd.Parameters.AddWithValue("@CATEGORY", category);
            cmd.Parameters.AddWithValue("@ELOTWO", eloTwo);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        private static DateTime GetMostRecentDate()
        {
            var conn = new MySqlConnection(connectionString);
            conn.Open();
            DateTime mrDate = new DateTime();
            var sqlSelect = "SELECT * FROM Scraper;";
            var cmd = new MySqlCommand(sqlSelect, conn);
            using (MySqlDataReader rdr = cmd.ExecuteReader()) {
                while (rdr.Read()) {
                    /* iterate once per row */
                    mrDate = rdr.GetDateTime(0);
                }
            }
            return mrDate;
        }

        private static void SetMostRecentDay(DateTime mrDate)
        {
            var conn = new MySqlConnection(connectionString);
            conn.Open();
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Scraper SET MostRecentDay = @MRDATE";
            cmd.Parameters.AddWithValue("@MRDATE", mrDate);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        private static int GetCategoryByLeague(IEnumerable<League> leagueList, int currentLeagueId)
        {
            League temp = leagueList.SingleOrDefault(x => x.Id == currentLeagueId);
            if (temp != null)
            {
                return temp.Hockey_Category_ID;
            }else{
                return 1;
            }
        }

        private static int GetTeamIdByName(IEnumerable<Team> teamList, string currentTeam)
        {
            foreach (var team in teamList)
            {
                if (!team.Teamname.Equals(currentTeam)) continue;
                return team.ID;
            }
            return 0;
        }

        private static int GetLeagueIDByName(IEnumerable<League> leagueList, string currentLeague)
        {
            foreach (var league in leagueList)
            {
                if (!league.Name.Equals(currentLeague)) continue;
                return league.Id;
            }
            return 0;
        }

        private static async Task ScrapeResults()
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(resultsURL);
            var table = document.QuerySelectorAll("table.table.table-bordered.latest-results-table");
            // var temp = document.All.Where(m => m.Id);
            foreach (var month in table)
            {
                var teamScan = month.QuerySelectorAll("td.scores.team");
                var teamOnes = new List<string>();
                var teamTwos = new List<string>();
                for (var j = 0; j < teamScan.Length; j++)
                {
                    if (j % 2 == 0)
                    {
                        teamOnes.Add(teamScan[j].TextContent);
                    }
                    else
                    {
                        teamTwos.Add(teamScan[j].TextContent);
                    }
                }

                var divisionScan = month.QuerySelectorAll("td.scores.division");
                var divisions = divisionScan.Select(i => i.TextContent).ToList();

                var versusScan = month.QuerySelectorAll("td.text-center.scores.versus");
                var versus = versusScan.Select(i => i.TextContent).ToList();

                var homeScoreScan = month.QuerySelectorAll("td.text-center.scores.homeScore");
                var homeScore = homeScoreScan.Select(i => i.TextContent).ToList();

                var awayScoreScan = month.QuerySelectorAll("td.text-center.scores.awayScore");
                var awayScore = awayScoreScan.Select(i => i.TextContent).ToList();

                var locationScan = month.QuerySelectorAll("td.scores.venue");
                var location = locationScan.Select(i => i.TextContent).ToList();

                for (var i = 0; i < divisions.Count; i++)
                {
                    Console.WriteLine($"Division: {divisions[i]}, Team 1: {teamOnes[i]}: {homeScore[i]} - {awayScore[i]}: {teamTwos[i]}, Location: {location[i]}");
                }
            }
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

        private static async Task ScrapeLeagues()
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(LeagueUrl);
            var allLeagues = document.QuerySelectorAll("h2.text-uppercase");

            System.Console.WriteLine("THERE");
            foreach (var item in allLeagues)
            {
                // Console.WriteLine("HERE");
                if (item.TextContent.Contains("Conference") || item.TextContent.Contains("Super"))
                {
                    System.Console.WriteLine("Skipped non-standard league: " + item.TextContent);
                    continue;
                }
                SaveLeagueSql(item.TextContent, GetLeagueHockeyCategoryByName(item.TextContent));
            }
        }

        private static void SaveLeagueSql(string name, int category)
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

        private static async Task ScrapePoints()
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(LeagueUrl);

            // Get list of all teams
            var teamList = new List<Team>();
            var conn = new MySqlConnection(connectionString);
            conn.Open();
            const string sqlSelect = @"SELECT ID,
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
            await using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    /* iterate once per row */
                    var team = new Team
                    {
                        Teamname = (rdr.IsDBNull(13)) ? "" : rdr.GetString(13),
                        ID = rdr.GetInt32(0),
                        League_ID = rdr.GetInt32(1),
                        Hockey_Category_ID = rdr.GetInt32(2),
                        Sponsor = (rdr.IsDBNull(3)) ? "" : rdr.GetString(3),
                        League_Rank = rdr.GetInt32(4),
                        SeasonPlayed = rdr.GetInt32(5),
                        SeasonWon = rdr.GetInt32(6),
                        SeasonDrawn = rdr.GetInt32(7),
                        SeasonLost = rdr.GetInt32(8),
                        SeasonGoalsFor = rdr.GetInt32(9),
                        SeasonGoalsAgainst = rdr.GetInt32(10),
                        SeasonGoalDifference = rdr.GetInt32(11),
                        SeasonPoints = rdr.GetInt32(12)
                    };
                    teamList.Add(team);
                    System.Console.WriteLine(team.Teamname);
                }
            }

            var played = 0;
            var won = 0;
            var drawn = 0;
            var lost = 0;
            var gfor = 0;
            var gagainst = 0;
            var gd = 0;
            var points = 0;
            var rank = 0;

            // For each league table
            var leagueTeams = document.QuerySelectorAll("table.league-standings");
            foreach (var league in leagueTeams)
            {
                // For each row in league
                var teamRow = league.QuerySelectorAll("tr.mobile-border");
                foreach (var div in teamRow)
                {
                    Console.WriteLine("THERE");
                    var currentTeam = "";
                    // Take only the teamname and the sponsor
                    var teamDetails = div.QuerySelectorAll("th.no-border.team-details");
                    var i = 0;
                    foreach (var item in teamDetails)
                    {
                        if (i == 1)
                        {
                            currentTeam = item.TextContent;

                        }
                        i++;
                    }
                    var scoreDetails = div.QuerySelectorAll("th.no-border.text-center.scores");
                    var j = 0;
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

        private static async Task ScrapeNewTeams()
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(LeagueUrl);

            // Get Leagues from Database
            var dbLeagueList = new List<League>();
            var conn = new MySqlConnection(connectionString);
            conn.Open();
            const string sqlSelect = "SELECT * FROM Leagues";
            var cmd = new MySqlCommand(sqlSelect, conn);
            await using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    /* iterate once per row */
                    var league = new League(rdr.GetInt32(0), (rdr.IsDBNull(1)) ? "" : rdr.GetString(1), rdr.GetInt32(2));
                    dbLeagueList.Add(league);
                }
            }


            //// var leagueResponse = await client.GetAsync("http://localhost:33988/api/Leagues");
            //var leagueResponse = await client.GetAsync("http://localhost:5000/api/Leagues");
            //leagueResponse.EnsureSuccessStatusCode();
            //var leagueResponseBody = await leagueResponse.Content.ReadAsStringAsync();
            //var leagueList = JsonConvert.DeserializeObject<List<League>>(leagueResponseBody);

            // Scrape all leagues
            var leagueList = new List<string>();
            var allLeagues = document.QuerySelectorAll("h2.text-uppercase");

            foreach (var item in allLeagues)
            {
                leagueList.Add(item.TextContent);
            }

            var index = 0;
            _ = document.QuerySelectorAll("div.tableWrap");

            // For each league table
            var leagueTeams = document.QuerySelectorAll("table.league-standings");
            foreach (var league in leagueTeams)
            {
                if (leagueList[index].Contains("Conference") || leagueList[index].Contains("Super"))
                {
                    Console.WriteLine("Skipped non-standard league: " + leagueList[index]);
                    index++;
                    continue;
                }
                var rank = 1;
                // For each row in league
                var teamRow = league.QuerySelectorAll("tr.mobile-border");
                foreach (var div in teamRow)
                {
                    var currentLeague = leagueList[index];
                    var currentTeam = "";
                    var currentSponsor = "";
                    // Take only the teamname and the sponsor
                    var teamDetails = div.QuerySelectorAll("th.no-border.team-details");

                    var i = 0;
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
                    GetLeagueIdAndCategoryByName(dbLeagueList, currentLeague, teamToPost);

                    SaveTeamSql(teamToPost);
                    rank++;
                }
                index++;
            }
        }

        private static void SaveTeamSql(Team teamToPost)
        {
            Console.WriteLine(JsonConvert.SerializeObject(teamToPost));
            var conn = new MySqlConnection(connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Teams (TeamName, League_ID, Sponsor, League_Rank, Hockey_Category_ID)
VALUES (@TEAMNAME, @LEAGUE_ID, @SPONSOR, @LEAGUE_RANK, @CATEGORY)";
            cmd.Parameters.AddWithValue("@TEAMNAME", teamToPost.Teamname);
            cmd.Parameters.AddWithValue("@LEAGUE_ID", teamToPost.League_ID);
            cmd.Parameters.AddWithValue("@SPONSOR", teamToPost.Sponsor);
            cmd.Parameters.AddWithValue("@LEAGUE_RANK", teamToPost.League_Rank);
            cmd.Parameters.AddWithValue("@CATEGORY", teamToPost.Hockey_Category_ID);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        private static void GetLeagueIdAndCategoryByName(IEnumerable<League> leagueList, string currentLeague, Team team)
        {
            foreach (var league in leagueList)
            {
                if (!league.Name.Equals(currentLeague)) continue;
                team.League_ID = league.Id;
                team.Hockey_Category_ID = league.Hockey_Category_ID;
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

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class Team
    {
        public int ID { get; set; }
        public string Teamname { get; init; }
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

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class League
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
