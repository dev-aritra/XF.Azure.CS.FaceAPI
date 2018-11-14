using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace XF.Azure.CS.FaceAPI.Model
{
    public class DetectedFaceExtended : DetectedFace
    {
        public string PredominantEmotion { get; set; }
    }
}