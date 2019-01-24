using EmotionPlatzi.Web.Models;
using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Emotion;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace EmotionPlatzi.Web.Util
{
    public class EmotionHelper
    {
        public EmotionServiceClient emoClient;

        public EmotionHelper(string key)
        {
            this.emoClient = new EmotionServiceClient(key);
        }

        public async Task<EmoPicture> DetectAndExtractFaceAsyn(Stream imageStream)
        {
            Emotion[] emotions = await this.emoClient.RecognizeAsync(imageStream);

            var emoPicture = new EmoPicture();
            emoPicture.Faces = ExtractFaces(emotions, emoPicture);

            return emoPicture;
        }

        private ObservableCollection<EmoFace> ExtractFaces(Emotion[] emotions,
            EmoPicture emoPicture)
        {
            var listaFaces = new ObservableCollection<EmoFace>();

            foreach (var emotion in emotions)
            {
                var emoFace = new EmoFace()
                {
                    X = emotion.FaceRectangle.Left,
                    Y = emotion.FaceRectangle.Top,
                    With = emotion.FaceRectangle.Width,
                    Height = emotion.FaceRectangle.Height,
                    Picture = emoPicture,
                };

                emoFace.Emotions = ProcessEmotions(emotion.Scores, emoFace);

                listaFaces.Add(emoFace);
            }

            return listaFaces;
        }

        private ObservableCollection<EmoEmotion> ProcessEmotions(EmotionScores scores, EmoFace emoFace)
        {
            var emotionList = new ObservableCollection<EmoEmotion>();
            var properties = scores.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            //var filterProperties = properties.Where(p => p.PropertyType == typeof(float));
            var filterProperties = from p in properties
                                   where p.PropertyType == typeof(float)
                                   select p;

            var emoType = EmoEmotionEnum.Undetermined;

            foreach (var prop in filterProperties)
            {
                if (!Enum.TryParse<EmoEmotionEnum>(prop.Name, out emoType))
                {
                    emoType = EmoEmotionEnum.Undetermined;
                }

                var emoEmotion = new EmoEmotion()
                {
                    Score = (float)prop.GetValue(scores),
                    EmotionType = emoType,
                    Face = emoFace
                };

                emotionList.Add(emoEmotion);
            }

            return emotionList;
        }
    }
}