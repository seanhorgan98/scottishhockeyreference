using System;
using System.Collections.Generic;
using WebScraper.Models;

namespace WebScraper.Interfaces
{
    public interface IDbInteraction
    {
        void UpdatePoints(Team teamToUpdate);
        void UpdateEloRating(int teamID, int eloChange);

        void InsertFixture(DateTime date, int league, int teamOne, int teamTwo, int teamOneScore,
            int teamTwoScore, string location, int eloOne, int category, int eloTwo);

        DateTime GetMostRecentDate();
        void UpdateMostRecentDate(DateTime mrDate);
        int GetTeamRating(int teamID);
        void InsertTeam(Team teamToPost);
        void InsertLeague(string name, int category);
        List<Team> GetAllTeams();
        List<League> GetAllLeagues();
    }
}