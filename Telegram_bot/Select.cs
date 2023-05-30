using System;
namespace SelectPlayers
{
	public class SelectPlayer
	{
		public List<Players> Players { get; set; }
	}
	public class Players
	{
		public int id { get; set; }
		public string firstname { get; set; }
        public string lastname { get; set; }
        public int age { get; set; }
        public string height { get; set; }
        public string weight { get; set; }
        public string position { get; set; }
        public string rating { get; set; }
        public string clubname { get; set; }
    }
}

