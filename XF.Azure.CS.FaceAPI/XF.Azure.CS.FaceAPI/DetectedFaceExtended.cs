using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace XF.Azure.CS.FaceAPI
{
    public class DetectedFaceExtended : DetectedFace
    {
        public string PredominantEmotion { get; set; }
    }
}
