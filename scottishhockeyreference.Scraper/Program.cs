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
            var clientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
            };

            // Pass the handler to httpclient(from you are calling api)
            client = new HttpClient(clientHandler)
            {
                BaseAddress = new Uri("http://localhost:33988/")
            };
            client.DefaultRequestHeaders.Accept.Add(
               new MediaTypeWithQualityHeaderValue("application/json"));
            await PrintLeagues();
        }

        static async Task PrintLeagues()
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(leagueURL);
            var AllLeagues = document.QuerySelectorAll("h2.text-uppercase");

            foreach (var item in AllLeagues)
            {
                SaveLeague(item.TextContent);
            }
        }

        static async Task PrintAllTeams()
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

        static async Task PrintAllInfo()
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(leagueURL);

            // Get all leagues
            var LeagueList = new List<string>();
            var AllLeagues = document.QuerySelectorAll("h2.text-uppercase");

            foreach (var item in AllLeagues)
            {
                LeagueList.Add(item.TextContent);
            }


            int index = 0;
            _ = document.QuerySelectorAll("div.tableWrap");

            // Loop through all league tables
            var leagueTeams = document.QuerySelectorAll("table.league-standings");
            foreach(var league in leagueTeams)
            {
                // Loop through each row in league
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
                    SaveTeam(currentLeague, currentTeam, currentSponsor);
                }
                index++;
            }
        }

        public static async void SaveTeam(string league, string team, string sponsor)
        {
            var teamToPost = new Team(team, league, sponsor);
            Console.WriteLine(JsonConvert.SerializeObject(teamToPost));

            var response = await client.PostAsJsonAsync("http://localhost:33988/api/Teams", teamToPost);
            Console.WriteLine(response);

        }


        public static async void SaveLeague(string name)
        {
            var leagueToPost = new League(name, 1);
            // Console.WriteLine(JsonConvert.SerializeObject(leagueToPost));
            var response = await client.PostAsJsonAsync("api/Leagues/", value: leagueToPost);
            bool returnValue = await response.Content.ReadAsAsync<bool>();
            Console.WriteLine(returnValue);
        }
    }

    class Team
    {
        public string TeamName { get; set; }
        public string League { get; set; }
        public string Sponsor { get; set; }

        public Team(string teamname, string league, string sponsor)
        {
            this.TeamName = teamname;
            this.League = league;
            this.Sponsor = sponsor;
        }
    }

    class League
    {
        public string Name { get; set; }
        public int Hockey_Category_ID { get; set; }

        public League(string name, int hockey_category_id)
        {
            this.Name = name;
            this.Hockey_Category_ID = hockey_category_id;
        }
    }
}
