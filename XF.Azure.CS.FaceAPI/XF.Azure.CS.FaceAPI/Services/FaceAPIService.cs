using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Plugin.Media.Abstractions;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using XF.Azure.CS.FaceAPI.Model;

namespace XF.Azure.CS.FaceAPI.Services
{
    public class FaceAPIService
    {
        private string APIKEY = "50c78f754e734425adca685ee646701b";
        private string ENDPPOINT = "https://centralindia.api.cognitive.microsoft.com/";
        private FaceClient faceClient;

        public FaceAPIService()
        {
            InitializeFaceClient();
        }

        public async Task<List<DetectedFaceExtended>> GetFaces(MediaFile image)
        {
            List<DetectedFaceExtended> detectedFaces = null;
            var faceApiResponseList = await faceClient.Face.DetectWithStreamAsync(image.GetStream(), returnFaceAttributes: new List<FaceAttributeType> { { FaceAttributeType.Emotion } });
            DetectedFaceExtended decFace = null;
            if (faceApiResponseList.Count > 0)
            {
                detectedFaces = new List<DetectedFaceExtended>();
                foreach (DetectedFace face in faceApiResponseList)
                {
                    decFace = new DetectedFaceExtended
                    {
                        FaceRectangle = face.FaceRectangle,
                    };
                    decFace.PredominantEmotion = FindDetectedEmotion(face.FaceAttributes.Emotion);
                    detectedFaces.Add(decFace);
                }
            }
            return detectedFaces;
        }

        private string FindDetectedEmotion(Emotion emotion)
        {
            double max = 0;
            PropertyInfo prop = null;

            var emotionsValues = typeof(Emotion).GetProperties();
            foreach (PropertyInfo property in emotionsValues)
            {
                var val = (double)property.GetValue(emotion);

                if (val > max)
                {
                    max = val;
                    prop = property;
                }
            }
            return prop.Name.ToString();
        }

        private void InitializeFaceClient()
        {
            ApiKeyServiceClientCredentials credentials = new ApiKeyServiceClientCredentials(APIKEY);
            faceClient = new FaceClient(credentials);
            faceClient.Endpoint = ENDPPOINT;
            FaceOperations faceOperations = new FaceOperations(faceClient);
        }
    }
}