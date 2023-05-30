using System;
namespace GetLeagueIdModels
{
    public class GetLeagueId
    {
        public List<ResponseLeague> Response { get; set; }
        public GetLeagueId()
        {
            Response = new List<ResponseLeague>();
        }
    }

    public class ResponseLeague
    {
        public LeagueId League { get; set; }
    }
    public class LeagueId
    {
        public int Id { get; set; }
    }

}


