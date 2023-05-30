using System;
using GetNSMatchesModels;

namespace GetNewsModels
{
	public class GetNews
	{
		public List<Results> Results { get; set; }
		public GetNews()
		{
            Results = new List<Results>();
        }
	}
	public class Results
	{
		public string Link { get; set; }
	}
}

