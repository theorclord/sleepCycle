using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SleepAmountCycles
{
    class Program
    {
        static void Main(string[] args)
        {
            // Build list of sleep cycles
            List<SleepCycle> listOfCycles = new List<SleepCycle>();
            string fileLocation = @"C:\finalCSVfileS20.csv";//@"/Users/JennyVej1/Dropbox/Data/UserStudies/finalCSVfileS20.csv";
            int maxSleepGapCount = 90;
            SleepCycle currentCycle = null;
            int sleepGapCount = 0;
            string lastReadSleepMin = null;
            bool exist = File.Exists(fileLocation);
            if (exist)
            {
                Console.WriteLine("File found");
            }
            FileStream fs = File.Open(fileLocation, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            bool skipFirstLine = true;
            // read all lines of the file
            while (!sr.EndOfStream)
            {
                string[] line = sr.ReadLine().Split(',');
                if (skipFirstLine)
                {
                    skipFirstLine = false;
                    continue;
                }
                // read the sleep type column
                if (!string.IsNullOrEmpty(line[8]))
                {
                    lastReadSleepMin = line[0];
                    if (sleepGapCount < maxSleepGapCount)
                    {
                        if (currentCycle == null)
                        {
                            currentCycle = new SleepCycle();
                            string date = line[0];
                            currentCycle.StartTime = Convert.ToDateTime(date);
                        }
                        currentCycle.MinutesOfSleep++;
                        sleepGapCount = 0;
                    }
                }
                else if(currentCycle != null) {
                    currentCycle.MinutesOfSleep++;
                    sleepGapCount++;
                    if (sleepGapCount == maxSleepGapCount && lastReadSleepMin != null)
                    {
                        currentCycle.MinutesOfSleep -= maxSleepGapCount;
                        currentCycle.EndTime = Convert.ToDateTime(lastReadSleepMin);

						            if (currentCycle.MinutesOfSleep > maxSleepGapCount) 
						            {
							            listOfCycles.Add(currentCycle);						
						            }
                        currentCycle = null;
                        sleepGapCount = 0;
                    }
                }
            }

      //Main val for cycling through indexes
      int cycleIndex = 0;
      // Check against light file from phone
      // #################################################################################
      Console.WriteLine("Checking sleep against light from phone");
            string userLightFile = @"C:\865800027079836_UserPresenceLight.csvdateFixed.csv";
            FileStream fsUserLight = File.Open(userLightFile, FileMode.Open);
            StreamReader srUserLight = new StreamReader(fsUserLight);

      Dictionary<DateTime, float> timestampLightvalSleep = new Dictionary<DateTime, float>();
      Dictionary<DateTime, float> timestampLightvalWake = new Dictionary<DateTime, float>();
      float[] userLightSleepStats = new float[2];
      float[] userLightWakeStats = new float[2];
      
      cycleIndex = 0;
      while (!srUserLight.EndOfStream)
      {
        string[] line = srUserLight.ReadLine().Split(',');
        while (cycleIndex< listOfCycles.Count)
        {
          SleepCycle currentCompareCycle = listOfCycles[cycleIndex];
          DateTime timestamp = Convert.ToDateTime(line[0]);
          if (currentCompareCycle.StartTime <= timestamp)
          {
            if(currentCompareCycle.EndTime >= timestamp)
            {
              // the timestamp is in cycle and sleep, change timestamp and update sleep stats
              timestampLightvalSleep.Add(timestamp, float.Parse(line[2]));
              userLightSleepStats[0]++;
              userLightSleepStats[1] += float.Parse(line[2]);
              break;
            } else
            {
              // the timestamp is after the cycle, change cycle and check again
              cycleIndex++;
              continue;
            }
          }
          else
          {
            // the timestamp is before the cycle, change timestamp and update not sleep stats
            timestampLightvalWake.Add(timestamp, float.Parse(line[2]));
            userLightWakeStats[0]++;
            userLightWakeStats[1] += float.Parse(line[2]);
            break;
          }
        }
      }
      Console.WriteLine("Sleep stamps: "+ userLightSleepStats[0] + ", average val: " + userLightSleepStats[1]/userLightSleepStats[0]);
      Console.WriteLine("Wake stamps: " + userLightWakeStats[0] + ", average val: " + userLightWakeStats[1] / userLightWakeStats[0]);

      // Check against light file from phone
      // #################################################################################
      Console.WriteLine("Checking sleep against light from phone");
      string userActivityFile = @"C:\865800027079836_UserActivity.csvdateFixed.csv";
      FileStream fsUserActivity = File.Open(userActivityFile, FileMode.Open);
      StreamReader srUserActivity = new StreamReader(fsUserActivity);

      Dictionary<string, int> countActivitySleep = new Dictionary<string, int>();
      Dictionary<string, int> countActivityWake = new Dictionary<string, int>();

      cycleIndex = 0;
      while (!srUserActivity.EndOfStream)
      {
        string[] line = srUserActivity.ReadLine().Split(',');
        while (cycleIndex < listOfCycles.Count)
        {
          SleepCycle currentCompareCycle = listOfCycles[cycleIndex];
          DateTime timestamp = Convert.ToDateTime(line[0]);
          if (currentCompareCycle.StartTime <= timestamp)
          {
            if (currentCompareCycle.EndTime >= timestamp)
            {
              // the timestamp is in cycle and sleep, change timestamp and update sleep stats
              if (countActivitySleep.ContainsKey(line[1]))
              {
                countActivitySleep[line[1]]++;
              } else
              {
                countActivitySleep.Add(line[1], 1);
              }
              break;
            }
            else
            {
              // the timestamp is after the cycle, change cycle and check again
              cycleIndex++;
              continue;
            }
          }
          else
          {
            // the timestamp is before the cycle, change timestamp and update not sleep stats
            if (countActivityWake.ContainsKey(line[1]))
            {
              countActivityWake[line[1]]++;
            }
            else
            {
              countActivityWake.Add(line[1], 1);
            }
            break;
          }
        }
      }
      Console.WriteLine("Writing sleep stats for activity");
      foreach( KeyValuePair<string,int> pair in countActivitySleep)
      {
        Console.WriteLine(pair.Key + ": " + pair.Value);
      }
      Console.WriteLine("Writing wake stats for activity");
      foreach (KeyValuePair<string, int> pair in countActivityWake)
      {
        Console.WriteLine(pair.Key + ": " + pair.Value);
      }
      Console.WriteLine();

      Console.WriteLine("Sleep comparison done");
      //StringBuilder sb = new StringBuilder();
      //sb.AppendLine("DateStart,DateEnd,sleepMinutes");
      //Console.WriteLine(listOfCycles.Count);
      //foreach (SleepCycle sc in listOfCycles)
      //{
      //    sb.AppendLine(sc.StartTime.ToString() + "," + sc.EndTime.ToString() + "," + sc.MinutesOfSleep);
      //    //Console.WriteLine(sc.StartTime.ToString());
      //    //Console.WriteLine("Amount of sleep " + sc.MinutesOfSleep);
      //    //Console.WriteLine(sc.EndTime.ToString());
      //}

      //File.WriteAllText(@"/Users/JennyVej1/Dropbox/Data/UserStudies/CSVSleepCyclesS20.csv", sb.ToString());
      Console.ReadKey();
        }
    }
}
