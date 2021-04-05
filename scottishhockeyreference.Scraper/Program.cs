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
                await SaveLeague(item.TextContent, GetHockeyCategoryByName(item.TextContent));
                Console.WriteLine("Here");
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
            var leagueResponse = await client.GetAsync("http://localhost:33988/api/Leagues");
            leagueResponse.EnsureSuccessStatusCode();
            var leagueResponseBody = await leagueResponse.Content.ReadAsStringAsync();
            var leagueList = JsonConvert.DeserializeObject<List<League>>(leagueResponseBody);

            // Get all leagues
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
                    var category = GetHockeyCategoryByName(currentTeam);
                    var currentLeague_ID = GetLeagueIDByName(leagueList, currentLeague);

                    await SaveTeam(currentLeague_ID, currentTeam, currentSponsor, category, rank);
                    rank++;
                }
                index++;
            }
        }

        private static int GetLeagueIDByName(List<League> leagueList, string currentLeague)
        {
            foreach(var i in leagueList)
            {
                if(i.Name.Equals(currentLeague))
                {
                    return i.Id;
                }
            }
            return 0;
        }

        private static int GetHockeyCategoryByName(string teamname)
        {
            if (teamname.Contains("Women") && teamname.Contains("Indoor"))
            {
                return 4;
            }
            if (teamname.Contains("Men") && teamname.Contains("Indoor"))
            {
                return 3;
            }
            if (teamname.Contains("Women"))
            {
                return 2;
            }
            return 1;
        }

        public static async Task SaveTeam(int league, string team, string sponsor, int category, int rank)
        {
            var teamToPost = new Team(team, league, sponsor, category, rank);
            Console.WriteLine(JsonConvert.SerializeObject(teamToPost));

            await client.PostAsJsonAsync("http://localhost:33988/api/Teams", teamToPost);

        }


        public static async Task SaveLeague(string name, int category)
        {
            var leagueToPost = new League(name, category);

            var response = await client.PostAsJsonAsync("http://localhost:33988/api/Leagues", leagueToPost);
            Console.WriteLine(response);
        }
    }

    class Team
    {
        public string TeamName { get; set; }
        public int League_ID { get; set; }
        public string Sponsor { get; set; }
        public int Hockey_Category_ID { get; }
        public int League_Rank { get; }

        public Team(string teamname, int league, string sponsor, int hockey_category_id, int league_rank)
        {
            this.TeamName = teamname;
            this.League_ID = league;
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

        public League(string name, int hockey_category_id)
        {
            this.Name = name;
            this.Hockey_Category_ID = hockey_category_id;
        }
    }
}
