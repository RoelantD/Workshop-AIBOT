using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.AI.MachineLearning.Preview;

// f8a23911-5b75-4e62-8bf7-a0645fdaf006_b5fd51a0-efd6-4400-af14-d5b0389eb89d

namespace TheSeeingPI.App
{
    public sealed class MyCustomVisionModelInput
    {
        public VideoFrame data { get; set; }
    }

    public sealed class MyCustomVisionModelOutput
    {
        public IList<string> classLabel { get; set; }
        public IDictionary<string, float> loss { get; set; }
        public MyCustomVisionModelOutput()
        {
            classLabel = new List<string>();
            loss = new Dictionary<string, float>()
            {
                { "Cosmos DB", float.NaN },
            };
        }
    }

    public sealed class MyCustomVisionModel
    {
        private LearningModelPreview learningModel;
        public static async Task<MyCustomVisionModel> CreateMyCustomVisionModel(StorageFile file)
        {
            LearningModelPreview learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            MyCustomVisionModel model = new MyCustomVisionModel();
            model.learningModel = learningModel;
            return model;
        }
        public async Task<MyCustomVisionModelOutput> EvaluateAsync(MyCustomVisionModelInput input) {
            MyCustomVisionModelOutput output = new MyCustomVisionModelOutput();
            LearningModelBindingPreview binding = new LearningModelBindingPreview(learningModel);
            binding.Bind("data", input.data);
            binding.Bind("classLabel", output.classLabel);
            binding.Bind("loss", output.loss);
            LearningModelEvaluationResultPreview evalResult = await learningModel.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }
}
