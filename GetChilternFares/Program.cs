// Copyright (c) Adrian Sims 2018
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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

                // list of all flow Ids for the TOC in question:
                var flowidMap = new HashSet<int>();

                // list of all 
                var rescodeMap = new HashSet<string>();

                // flow id -> RF line:
                var flowIdToFlowMap = new Dictionary<int, string>();

                // restriction code to a list of F records:
                var resCodeToFlowList = new SortedDictionary<string, List<string>>();

                foreach (var line in File.ReadAllLines($"s:\\rjfaf{setnum:D3}.ffl"))
                {
                    var flow = line.Substring(2, 8);

                    if (line.Length <= 2 || line[0] != 'R' || !"QRST01234567889".Contains(flow[0]) || !"QRST01234567889".Contains(flow[4]))
                    {
                        continue;
                    }
                    if (line[1] == 'F')
                    {
                        if (line.Substring(36, 3) == "SCR")
                        {
                            var flowid = Convert.ToInt32(line.Substring(42));
                            flowidMap.Add(flowid);
                            flowIdToFlowMap.Add(flowid, line);
                        }
                    }
                    else if (line[1] == 'T')
                    {
                        var flowid = Convert.ToInt32(line.Substring(2, 7));
                        var rescode = line.Substring(20, 2);
                        if (rescode != "  " && flowidMap.Contains(flowid))
                        {
                            rescodeMap.Add(rescode);
                            DictUtils.AddEntryToList(resCodeToFlowList, rescode , flowIdToFlowMap[flowid]);
                        }
                    }
                }

                var outFilename = "rescode-usage.txt";
                using (var str = new StreamWriter(outFilename))
                {
                    foreach (var entry in resCodeToFlowList)
                    {
                        str.WriteLine($"{entry.Key}: {entry.Value.Count} flow records.");
                    }
                    foreach (var entry in resCodeToFlowList.Where(x=>x.Value.Count() > 0))
                    {
                        str.WriteLine($"{entry.Key}:");
                        foreach (var flow in entry.Value)
                        {
                            str.WriteLine($"{flow}");
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
                    var restrictionCode = entry.Key;
                    var dateRestrictionList = entry.Value;

                    Console.Write($"{restrictionCode}:");

                    var datelist = new List<DateTime>();

                    for (var date = new DateTime(2018, 1, 1); date.Year == 2018; date = date.AddDays(1))
                    {
                        bool unrestricted = true;
                        foreach (var dateRange in dateRestrictionList)
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

                    var weekdayList = datelist.Where(x => x.DayOfWeek != DayOfWeek.Saturday && x.DayOfWeek != DayOfWeek.Sunday);
                    if (weekdayList.Any())
                    {
                        Console.WriteLine();
                        foreach (var d in weekdayList)
                        {
                            Console.WriteLine($"    {d:dd MMM yyyy}");
                        }
                    }
                    else
                    {
                        Console.WriteLine(" no unrestricted days.");
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
