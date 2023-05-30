using System;
namespace GetNSMatchesModels
{
    public class GetNSMatches
    {
        public int Results { get; set; }
        public List<ResponseMatch> Response { get; set; }
        public GetNSMatches()
        {
            Response = new List<ResponseMatch>();
        }
    }

    public class ResponseMatch
    {
        public Fixture Fixture { get; set; }
        public Teams Teams { get; set; }
        public LeagueMatch League { get; set; }
    }
    public class Fixture
    {
        public string Date { get; set; }
    }
    public class Teams
    {
        public Home Home { get; set; }
        public Away Away { get; set; }
    }
    public class Home
    {
        public string Name { get; set; }
    }
    public class Away
    {
        public string Name { get; set; }
    }
    public class LeagueMatch
    {
        public string Name { get; set; }
    }
    
}

