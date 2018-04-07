using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetChilternFares
{
    using System.Globalization;

    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var setnum = 796;
                var flowidMap = new HashSet<int>();
                var rescodeMap = new HashSet<string>();
                foreach (var line in File.ReadAllLines($"s:\\rjfaf{setnum:D3}.ffl"))
                {
                    if (line.Length <= 2 || line[0] != 'R')
                    {
                        continue;
                    }
                    if (line[1] == 'F')
                    {
                        if (line.Substring(36, 3) == "SCR")
                        {
                            var flowid = Convert.ToInt32(line.Substring(42));
                            flowidMap.Add(flowid);
                        }
                    }
                    else if (line[1] == 'T')
                    {
                        var flowid = Convert.ToInt32(line.Substring(2, 7));
                        var rescode = line.Substring(20, 2);
                        if (rescode != "  " && flowidMap.Contains(flowid))
                        {
                            rescodeMap.Add(rescode);
                            if (rescode == "B5")
                            {
                                Console.WriteLine();
                            }
                        }
                    }
                }

                var dateMap = new Dictionary<string, List<(DateTime startDate, DateTime endDate, string days)>>();
                foreach (var line in File.ReadAllLines($"s:\\rjfaf{setnum:D3}.rst"))
                {
                    if (line.Length <= 2 || line[0] != 'R')
                    {
                        continue;
                    }
                    if (line[1] == 'H' && line[2] == 'D' && line[3] == 'C')
                    {
                        var rescode = line.Substring(4, 2);
                        if (rescodeMap.Contains(rescode))
                        {
                            if (!DateTime.TryParseExact($"2018{line.Substring(6, 4)}",
                                "yyyyMMdd",
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None, out var startDate))
                            {
                                throw new Exception("Invalid date");
                            }
                            if (!DateTime.TryParseExact($"2018{line.Substring(10, 4)}",
                                "yyyyMMdd",
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None, out var endDate))
                            {
                                throw new Exception("Invalid date");
                            }
                            var days = line.Substring(14, 7);
                            DictUtils.AddEntryToList(dateMap, rescode, (startDate, endDate, days));
                        }
                    }
                }
                foreach (var entry in dateMap)
                {
                    Console.WriteLine($"{entry.Key}");

                    var datelist = new List<DateTime>();

                    for (var date = new DateTime(2018, 1, 1); date.Year == 2018; date = date.AddDays(1))
                    {
                        bool unrestricted = true;
                        foreach (var dateRange in entry.Value)
                        {
                            var euroDayofWeek = (int) (7 + (date.DayOfWeek - 1)) % 7;
                            if (dateRange.days[euroDayofWeek] == 'N')
                            {
                                continue;
                            }
                            if (date >= dateRange.startDate.Date && date <= dateRange.endDate.Date)
                            {
                                unrestricted = false;
                                break;
                            }
                        }
                        if (unrestricted)
                        {
                            datelist.Add(date);
                        }
                    }

                    foreach (var d in datelist.Where(x=>x.DayOfWeek != DayOfWeek.Saturday && x.DayOfWeek != DayOfWeek.Sunday))
                    {
                        Console.WriteLine($"{d:dd MMM yyyy}");
                    }
                }
            }
            catch (Exception ex)
            {
                var codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                var progname = Path.GetFileNameWithoutExtension(codeBase);
                Console.Error.WriteLine(progname + ": Error: " + ex.Message);
            }

        }
    }
}
