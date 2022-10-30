using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
namespace MultiQueueModels
{
    public class SimulationSystem
    {
        public SimulationSystem()
        {
            this.Servers = new List<Server>();
            this.InterarrivalDistribution = new List<TimeDistribution>();
            this.PerformanceMeasures = new PerformanceMeasures();
            this.SimulationTable = new List<SimulationCase>();
        }
        ///////////// INPUTS ///////////// 
        public int NumberOfServers { get; set; }
        public int StoppingNumber { get; set; }
        public List<Server> Servers { get; set; }
        public List<TimeDistribution> InterarrivalDistribution { get; set; }
        public Enums.StoppingCriteria StoppingCriteria { get; set; }
        public Enums.SelectionMethod SelectionMethod { get; set; }

        ///////////// OUTPUTS /////////////
        public List<SimulationCase> SimulationTable { get; set; }
        public PerformanceMeasures PerformanceMeasures { get; set; }

        ///////////// Read test Case file to get inputs /////////////
        public void readfile(string testFile)
        {
            FileStream fs = new FileStream(testFile, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                string[] getServ = line.Split('_');
                if (line == "NumberOfServers")
                    this.NumberOfServers = int.Parse(sr.ReadLine());
                else if (line == "StoppingNumber")
                    this.StoppingNumber = int.Parse(sr.ReadLine());
                else if (line == "StoppingCriteria")
                {
                    line = sr.ReadLine();
                    if (line == "1")
                        this.StoppingCriteria = Enums.StoppingCriteria.NumberOfCustomers;
                    else
                        this.StoppingCriteria = Enums.StoppingCriteria.SimulationEndTime;
                }
                else if (line == "SelectionMethod")
                {
                    line = sr.ReadLine();
                    if (line == "1")
                        this.SelectionMethod = Enums.SelectionMethod.HighestPriority;
                    else if (line == "2")
                        this.SelectionMethod = Enums.SelectionMethod.Random;
                    else
                        this.SelectionMethod = Enums.SelectionMethod.LeastUtilization;
                }
                else if (line == "InterarrivalDistribution")
                { decimal cumprop = 0;
                    while((line = sr.ReadLine()) != "")
                    {
                        TimeDistribution t = new TimeDistribution();
                        line = line.Replace(" ", "");
                        string[] s = line.Split(',');
                        
                        t.Time = int.Parse(s[0]);
                        t.Probability = decimal.Parse(s[1]);
                        t.MinRange = (int)(cumprop * 100)+1;
                        t.CummProbability = cumprop + t.Probability;
                        cumprop += t.Probability;
                        t.MaxRange = (int)(cumprop * 100);
                        this.InterarrivalDistribution.Add(t);
                    }
                }
                else if(getServ[0]== "ServiceDistribution")
                {
                    Server serv = new Server();
                    decimal cumprop = 0;
                    while((line=sr.ReadLine())!="" && line!=null)
                    {
                        TimeDistribution t = new TimeDistribution();
                        line = line.Replace(" ", "");
                        string[] s = line.Split(',');
                        t.Time = int.Parse(s[0]);
                        t.Probability = decimal.Parse(s[1]);
                        t.MinRange = (int)(cumprop * 100)+1;
                        t.CummProbability = cumprop + t.Probability;
                        cumprop += t.Probability;
                        t.MaxRange = (int)(cumprop * 100);
                        serv.TimeDistribution.Add(t);
                    }
                    
                    serv.ID = this.Servers.Count+1;
                    serv.FinishTime = 0;
                    serv.Utilization = 0;
                    serv.IdleProbability = 0;
                    serv.TotalWorkingTime = 0;
                    serv.AverageServiceTime = 0;
                    this.Servers.Add(serv);
                }
            }
            fs.Close();
            sr.Close();
        }
        public void StartSimu()
        {
            //computing interarrival
            Random r = new Random();
            int stop = 0;
            while(stop<this.StoppingNumber)
            {
                SimulationCase cas = new SimulationCase();
                cas.CustomerNumber = 1;
                cas.ArrivalTime = 0;
                cas.InterArrival = 0;
                cas.RandomInterArrival = 1;
                cas.RandomService = r.Next(1, 100);
                if (stop != 0)
                {
                    cas.CustomerNumber = this.SimulationTable.Count + 1;
                    cas.RandomInterArrival = r.Next(1, 100);
                    foreach (var item in this.InterarrivalDistribution)
                    {
                        if (item.MaxRange >= cas.RandomInterArrival)
                        {
                            //set interarrival with time of distribution
                            cas.InterArrival = item.Time;
                            break;
                        }
                    }
                    cas.ArrivalTime = this.SimulationTable[this.SimulationTable.Count - 1].ArrivalTime + cas.InterArrival;
                }
                List<Server> avServers = new List<Server>();
                //list ordered by the one who finished firstly
                this.Servers = this.Servers.OrderBy(x => x.FinishTime).ToList();
                // 2 cases for any one to enter the server 
                //if the person came after the server has finished by a time and its idle or the person came 
                //at the time of finishing last one 
                int firstFinish = Servers[0].FinishTime;
                foreach (var item in this.Servers)
                    if (item.FinishTime == firstFinish||item.FinishTime<=cas.ArrivalTime)
                        avServers.Add(item);
                //this case if the finish time of both are equal
                if(this.SelectionMethod==Enums.SelectionMethod.HighestPriority)
                {
                    avServers = avServers.OrderBy(x => x.ID).ToList();
                    cas.AssignedServer = avServers[0];
                }
                else if(this.SelectionMethod==Enums.SelectionMethod.Random)
                {
                    int ran = r.Next(0, avServers.Count);
                    cas.AssignedServer = avServers[ran];
                }
                else
                {
                    //Least Utilization Method
                    avServers = avServers.OrderBy(x => x.Utilization).ToList();
                    cas.AssignedServer = avServers[0];
                }
                //state variables
                //start time is time when customer Enter
                cas.StartTime = Math.Max(cas.ArrivalTime, cas.AssignedServer.FinishTime);
                foreach(var item in cas.AssignedServer.TimeDistribution)
                    if(item.MaxRange >= cas.RandomService)
                    {
                        cas.ServiceTime = item.Time;
                        break;
                    }
                //time of finishing service
                cas.EndTime = cas.StartTime + cas.ServiceTime;
                //Delay time in queue
                cas.TimeInQueue = cas.StartTime - cas.ArrivalTime;
                for (int i = 0; i < this.Servers.Count; i++)
                {
                    if (this.Servers[i].ID == cas.AssignedServer.ID)
                    {
                        this.Servers[i].FinishTime = cas.EndTime;
                        this.Servers[i].Utilization += cas.ServiceTime;
                        this.Servers[i].TotalWorkingTime += cas.ServiceTime;
                        break;
                    }
                }
                this.SimulationTable.Add(cas);
                if (this.StoppingCriteria == Enums.StoppingCriteria.NumberOfCustomers)
                    stop = cas.CustomerNumber;
                else
                    stop = cas.EndTime;
            }
            
        }
        public void set_perfMes()
        {
            //TimeInQueue = start-arrival
            Dictionary<int, int> servServiceTimes = new Dictionary<int, int>();
            this.Servers = this.Servers.OrderBy(x => x.ID).ToList();
            foreach (var item in this.Servers)
            {
                    servServiceTimes[item.ID] = 0;
            }    
            List<int> queuelenghtes = new List<int>();
            decimal totWait = 0;
            decimal waitTimes = 0;
            decimal last = 0;
            int ind = 0;
            foreach(var item in SimulationTable)
            {
                servServiceTimes[item.AssignedServer.ID]++;
                waitTimes += item.TimeInQueue;
                if (item.TimeInQueue > 0)
                    totWait++;
                int len = 0,index1=0;
                foreach(var item1 in SimulationTable)
                {
                    if (item1.StartTime > item.ArrivalTime)
                        len++;
                    if (index1 == ind)
                        break;
                    index1++;
                }
                ind++;
                queuelenghtes.Add(len);
                if (item.EndTime > last)
                    last = item.EndTime;
            }
            queuelenghtes.Sort();
            this.PerformanceMeasures.MaxQueueLength = queuelenghtes[queuelenghtes.Count - 1];
            if (waitTimes != 0 && totWait != 0)
            {
                this.PerformanceMeasures.AverageWaitingTime = (waitTimes / this.SimulationTable.Count);
                this.PerformanceMeasures.WaitingProbability = (totWait / (decimal)(this.SimulationTable.Count));
            }
            for(int i=0;i<this.Servers.Count;i++)
            {
                if(this.Servers[i].TotalWorkingTime!=0 && servServiceTimes[this.Servers[i].ID]!=0)
                    this.Servers[i].AverageServiceTime = (decimal)this.Servers[i].TotalWorkingTime / (decimal)servServiceTimes[this.Servers[i].ID];
                if(last - this.Servers[i].TotalWorkingTime!=0&& last!=0)
                    this.Servers[i].IdleProbability = (decimal)(last - this.Servers[i].TotalWorkingTime) / (decimal)last;
                if(this.Servers[i].Utilization!=0 && last!=0)
                    this.Servers[i].Utilization = (decimal)this.Servers[i].Utilization / (decimal)last;
            }
        }

    }
    
}
