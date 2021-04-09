using AngleSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace scottishhockeyreference.Scraper
{
    class Program
    {
        static HttpClient client;
        private static readonly string leagueURL = "https://www.scottish-hockey.org.uk/league-standings/";
        static async Task Main()
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            // Pass the handler to httpclient(from you are calling api)
            client = new HttpClient(clientHandler);


            await ScrapeNewTeams();
        }

        static async Task ScrapeLeagues()
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(leagueURL);
            var AllLeagues = document.QuerySelectorAll("h2.text-uppercase");

            foreach (var item in AllLeagues)
            {
                await SaveLeague(item.TextContent, GetLeagueHockeyCategoryByName(item.TextContent));
            }
        }

        static async Task ScrapeTeamNames()
        {
            var TeamList = new List<string>();
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(leagueURL);
            var AllTeams = document.QuerySelectorAll("th.no-border.team-details");

            int i = 0;
            foreach (var item in AllTeams)
            {
                if (i%3 == 0)
                {
                    i++;
                    continue;
                }
                else if(i%3==1)
                {
                    TeamList.Add(item.TextContent);
                }
                i++;
            }
            var sortedList = TeamList.OrderBy(x => x).ToList();
            foreach(var item in sortedList)
            {
                Console.WriteLine(item);
            }
        }

        static async Task ScrapeNewTeams()
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(leagueURL);

            // Get Leagues from Database
            // var leagueResponse = await client.GetAsync("http://localhost:33988/api/Leagues");
            var leagueResponse = await client.GetAsync("http://localhost:5000/api/Leagues");
            leagueResponse.EnsureSuccessStatusCode();
            var leagueResponseBody = await leagueResponse.Content.ReadAsStringAsync();
            var leagueList = JsonConvert.DeserializeObject<List<League>>(leagueResponseBody);

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
            foreach(var league in leagueTeams)
            {
                var rank = 1;
                // For each row in league
                var teamRow = league.QuerySelectorAll("tr.mobile-border");
                foreach(var div in teamRow)
                {
                    string currentLeague = LeagueList[index];
                    string currentTeam = "";
                    string currentSponsor = "";
                    // Take only the teamname and the sponsor
                    var teamDetails = div.QuerySelectorAll("th.no-border.team-details");

                    int i = 0;
                    foreach (var item in teamDetails)
                    {
                        if (i%3 == 0)
                        {
                            i++;
                            continue;
                        }
                        else if(i%3==1)
                        {
                            currentTeam = item.TextContent;

                        }
                        else if(i%3==2)
                        {
                            currentSponsor = item.TextContent;

                        }
                        i++;
                    }
                    // Get Category
                    var teamToPost = new Team(currentTeam, 0, currentSponsor, 0, rank);
                    GetLeagueIDAndCategoryByName(leagueList, currentLeague, teamToPost);

                    await SaveTeam(teamToPost);
                    rank++;
                }
                index++;
            }
        }

        private static void GetLeagueIDAndCategoryByName(List<League> leagueList, string currentLeague, Team team)
        {
            foreach(var league in leagueList)
            {
                if(league.Name.Equals(currentLeague))
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

        public static async Task SaveTeam(Team teamToPost)
        {
            System.Console.WriteLine(JsonConvert.SerializeObject(teamToPost));
            // await client.PostAsJsonAsync("http://localhost:33988/api/Teams", teamToPost);
            var response = await client.PostAsJsonAsync("http://localhost:5000/api/Teams", teamToPost);
            System.Console.WriteLine(response);

        }


        public static async Task SaveLeague(string name, int category)
        {
            var leagueToPost = new League(0, name, category);

            // var response = await client.PostAsJsonAsync("http://localhost:33988/api/Leagues", leagueToPost);
            var response = await client.PostAsJsonAsync("http://localhost:5000/api/Leagues", leagueToPost);
            Console.WriteLine(response);
        }
    }

    class Team
    {
        public string Teamname { get; set; }
        public int League_ID { get; set; }
        public string Sponsor { get; set; }
        public int Hockey_Category_ID { get; set; }
        public int League_Rank { get; set; }

        public Team(string teamname, int league_id, string sponsor, int hockey_category_id, int league_rank)
        {
            this.Teamname = teamname;
            this.League_ID = league_id;
            this.Sponsor = sponsor;
            this.Hockey_Category_ID = hockey_category_id;
            this.League_Rank = league_rank;
        }
    }

    class League
    {
        public int Id { get; }
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
