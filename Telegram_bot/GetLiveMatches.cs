using System;

namespace GetLiveMatchesModels
{
	public class GetLiveMatches
	{
		public List<Data> Data { get; set; }
        public GetLiveMatches()
        {
            Data = new List<Data>();
        }
    }
	public class Data
	{
		public string Name { get; set; }
		public string Status { get; set; }
        public string Status_More { get; set; }
        public Home_Score Home_Score { get; set; }
        public Away_Score Away_Score { get; set; }
        public string Start_at { get; set; }
		public LeagueLive League { get; set; }
    }
	public class Home_Score
	{
		public int Current { get; set; }
	}
    public class Away_Score
    {
        public int Current { get; set; }
    }
	public class LeagueLive
	{
		public string Name { get; set; }
	}
}

