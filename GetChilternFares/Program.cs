using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetChilternFares
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var setnum = 789;
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
                        if (line.Substring(36, 3) == "NCH")
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
                        }
                    }
                }
                foreach (var line in File.ReadAllLines($"s:\\rjfaf{setnum:D3}.rst"))
                {
                    if (line.Length <= 2 || line[0] != 'R')
                    {
                        continue;
                    }
                    if (line[1] == 'T' && line[2] == 'R' && line[3] == 'C' && line[10] == 'O' && line[19] == 'D')
                    {
                        var rescode = line.Substring(4, 2);
                        if (rescodeMap.Contains(rescode))
                        {
                            var crs = line.Substring(20, 3);
                            var starttime = line.Substring(11, 4);
                            var endtime = line.Substring(15, 4);
                            string name = crs == "   " ? "NOT STATION SPECIFIC" : StationRefData.GetName(crs);
                            Console.WriteLine($"{rescode} : from {starttime.Substring(0, 2)}:{starttime.Substring(2, 2)} to {endtime.Substring(0, 2)}:{endtime.Substring(2, 2)} at CRS {crs} ({name})");
                        }
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
