using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;

public class FaceDetector
{
    private readonly CascadeClassifier _cascadeClassifier;
    private readonly Image<Gray, byte> _image;

    public FaceDetector(Bitmap bitmap)
    {
        _image = new Image<Gray, byte>(bitmap);
        var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "Bin\\haarcascade_frontalface_default.xml");
        _cascadeClassifier = new CascadeClassifier(fileName);
    }

    public IEnumerable<Rectangle> GetFaceRects()
    {
        return _cascadeClassifier.DetectMultiScale(_image, 1.1, 10, Size.Empty);
    }
}