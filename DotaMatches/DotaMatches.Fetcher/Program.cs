using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotaMatches.Fetcher
{
    public class Program
    {
        static ulong[] ReadInput(String line)
        {
            var ids = line.Split(new char[] {' ', ',', '|', });

            List<ulong> results = new List<ulong>();
            foreach (var i in ids)
            {
                ulong value;
                if (ulong.TryParse(i, out value))
                {
                    results.Add(value);
                }

                if (results.Count == 20)
                {
                    break;
                }
            }

            return results.ToArray();
        }

        public const int HeroColumnLength = 13;
        public const int NameColumnLength = 20;

        static void PrintPlayer(Player p)
        {
            var name = p.Name ?? "Bot";

            StringBuilder sb = new StringBuilder();
            sb.Append("\t");
            sb.Append(name.Length > Program.NameColumnLength ? name.Substring(0, Program.NameColumnLength) : name.PadRight(Program.NameColumnLength));
            sb.Append("\t");
            var hero = ((Hero)p.HeroId).ToString();
            sb.Append(hero.Length > Program.HeroColumnLength ? hero.Substring(0, Program.HeroColumnLength) : hero.PadRight(Program.HeroColumnLength));
            sb.Append("\t");
            sb.Append(p.Kills);
            sb.Append("\t");
            sb.Append(p.Deaths);
            sb.Append("\t");
            sb.Append(p.Assists);
            sb.Append("\t");
            Console.WriteLine(sb.ToString());
        }

        static void PrintMatchResult(Match m)
        {
            Console.WriteLine("Match {0}: ", m.Id);

            Console.WriteLine("\n\t{0}\t{1}\tK\tD\tA", "Radiant".PadRight(Program.NameColumnLength), "".PadRight(Program.HeroColumnLength));
            Console.WriteLine("\t----------------------------------------------------------------");
            foreach (var p in m.Players.Where(p => p.TeamName == TeamName.Radiant).OrderBy(p => p.Slot))
            {
                Program.PrintPlayer(p);
            }

            Console.WriteLine("\n\t{0}\t{1}\tK\tD\tA", "Dire".PadRight(Program.NameColumnLength), "".PadRight(Program.HeroColumnLength));
            Console.WriteLine("\t----------------------------------------------------------------");
            foreach (var p in m.Players.Where(p => p.TeamName == TeamName.Dire).OrderBy(p => p.Slot))
            {
                Program.PrintPlayer(p);
            }

            Console.WriteLine("\n\t{0} Victory", m.WinningTeam);
            Console.WriteLine("\n\tReplay URL: {0}", m.ReplayUrl);
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            SteamApi.Initialize();
            var steamMessenger = new SteamMessenger();
            var matchFetcher = new MatchFetcher(steamMessenger);

            String line;
            if (args.Length == 0)
            {
                Console.WriteLine("Enter up to 20 match ids to retrieve results...");
                line = Console.ReadLine();
            }
            else
            {
                line = String.Join(" ", args);
            }

            var matchIds = Program.ReadInput(line);
            if (matchIds.Length == 0)
            {
                Console.WriteLine("No valid match ids entered.");
                return;
            }

            Console.WriteLine("Fetching match data for the following match ids: {0}", String.Join(", ", matchIds));
            Console.WriteLine();

            try
            {
                var results = matchFetcher.FetchGames(matchIds, true);

                foreach (var r in results)
                {
                    switch (r.Result)
                    {
                        case ResultType.Success:
                            Program.PrintMatchResult(r.Match);
                            break;
                        case ResultType.Exception:
                            Console.WriteLine("Unable to parse the likely valid match results for match {0}", r.MatchId);
                            break;
                        case ResultType.NotFound:
                            Console.WriteLine("Unable to find results for match {0}, are you sure it exists?", r.MatchId);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            SteamApi.Shutdown();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
