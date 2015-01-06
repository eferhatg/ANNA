using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Encog.Util.File;

namespace ANNA
{
    class Config
    {
        public static FileInfo BasePath = new FileInfo(Environment.CurrentDirectory+@"\Data\");
        #region step1
        public static FileInfo BaseFile = FileUtil.CombinePath(BasePath, "Data.csv");
        public static FileInfo ShuffledBaseFile = FileUtil.CombinePath(BasePath, "Data_Shuffled.csv");
        #endregion

        #region step2
        public static FileInfo TrainingFile = FileUtil.CombinePath(BasePath, "Data_Train.csv");
        public static FileInfo EveluateFile = FileUtil.CombinePath(BasePath, "Data_Eval.csv");
        #endregion

        #region step3
        public static FileInfo NormalizedTrainingFile = FileUtil.CombinePath(BasePath, "Data_Train_Norm.csv");
        public static FileInfo NormalizedEveluateFile = FileUtil.CombinePath(BasePath, "Data_Eval_Norm.csv");
        public static FileInfo AnalystFile = FileUtil.CombinePath(BasePath, "Data_Analyst.ega");
        #endregion

        #region step4
        public static FileInfo TrainedNetworkFile = FileUtil.CombinePath(BasePath, "Data_Train.eg");

        #endregion
        #region step5
        public static FileInfo ValidationResults = FileUtil.CombinePath(BasePath, "Data_ValidationResult.csv");

        #endregion
    }
}
