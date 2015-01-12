using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Encog.Util.Arrayutil;
using Newtonsoft.Json;

namespace ANNA
{
    class Entity
    {
    }

    public class NetworkConfiguration
    {
        [JsonIgnore]
        private const string ConfigurationDirectory = "Configurations";
        [JsonIgnore]
        private const string ConfigurationExtension = "cfg";

        public string Name { get; set; }
        public string NetworkType { get; set; }
        public List<NetworkLayer> NetworkLayers { get; set; }
        public LearnConfig LearnConfig { get; set; }
        public string FilePath { get; set; }
        public string TrainedNormPath { get; set; }
        public string EvalNormPath { get; set; }
        public string TrainedNetworkFilePath { get; set; }
        public string AnalystFilePath { get; set; }
        public List<NormalizationParams> NormalizationParams { get; set; }
        public NormalizationAction[] NormalizationActions { get; set; }
        public bool HasHeaders { get; set; }

        public void WriteToFile()
        {
            Directory.CreateDirectory(ConfigurationDirectory);
       
            FilePath = String.Format(@"{0}\{1}.{2}", ConfigurationDirectory, Helper.CleanFileName(Name),
                ConfigurationExtension);
            TrainedNormPath = String.Format(@"{0}\{1}{2}", ConfigurationDirectory, Helper.CleanFileName(Name), "_Data_Train_Norm.csv");
            EvalNormPath = String.Format(@"{0}\{1}{2}", ConfigurationDirectory, Helper.CleanFileName(Name), "_Data_Eval_Norm.csv");
            TrainedNetworkFilePath = String.Format(@"{0}\{1}", ConfigurationDirectory,
                Helper.CleanFileName(Name) + "_Data_Train.eg");
            AnalystFilePath = String.Format(@"{0}\{1}", ConfigurationDirectory,
                Helper.CleanFileName(Name) + "_Data_Analyst.eg");
            var file = new System.IO.StreamWriter(FilePath);
            var output = JsonConvert.SerializeObject(this);
            file.WriteLine(output);
            file.Close();
            Config.NormalizedTrainingFile.CopyTo(TrainedNormPath, true);
            Config.TrainedNetworkFile.CopyTo(TrainedNetworkFilePath, true);
            Config.AnalystFile.CopyTo(AnalystFilePath, true);


        }

        public static List<NetworkConfiguration> GetFromFile()
        {
            var ncList = new List<NetworkConfiguration>();
            Directory.CreateDirectory(ConfigurationDirectory);
            foreach (var filepath in Directory.GetFiles(ConfigurationDirectory))
            {
                FileInfo fi= new FileInfo(filepath);
                if (fi.Extension == ".cfg")
                {
                    using (var sr = new StreamReader(filepath))
                    {
                        String line = sr.ReadToEnd();
                        ncList.Add(JsonConvert.DeserializeObject<NetworkConfiguration>(line));
                    }
                }
            }
            return ncList;
        }
    }

    public class NetworkLayer
    {
        public string Name { get; set; }
        public int NeuronCount { get; set; }
        public string ActivationFunction { get; set; }
    }

    public class LearnConfig
    {
        public string Algorithm { get; set; }
        public float LearningRate { get; set; }
        public float Momentum { get; set; }
        public int IterationCount { get; set; }
        public float MaximumError { get; set; }
        public float AvarageError { get; set; }
        public bool LimitedWithMaximumError { get; set; }
        public bool LimitedWithAvarageError { get; set; }
    }

    public class NormalizationParams
    {
        public string ColName { get; set; }
        public string ColType { get; set; }
        public string DataType { get; set; }
    }
    public class Delimeter
    {
        public string Name { get; set; }
        public string Char { get; set; }
    }
}
