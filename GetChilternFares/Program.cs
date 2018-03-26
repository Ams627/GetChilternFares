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
                var flowidMap = new HashSet<int>();
                foreach (var line in File.ReadAllLines("s:\\rjfaf782.ffl"))
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
                            Console.WriteLine($"{rescode}");   
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
