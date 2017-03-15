using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;

public class ImageProcessor
{
    private readonly Image _image;

    public ImageProcessor(byte[] imgData)
    {
        _image = Image.FromStream(new MemoryStream(imgData));
    }

    public byte[] Process()
    {
        var bitmap = new Bitmap(_image);
        var graphics = Graphics.FromImage(bitmap);
        var image = new Image<Bgr, byte>(bitmap);
        foreach (var faceRect in new FaceDetector(bitmap).GetFaceRects())
        {
            var face = image.GetSubRect(faceRect);
            face = face.SmoothBlur(faceRect.Width / 5, faceRect.Height / 5, true);
            graphics.DrawImage(face.Bitmap, faceRect.Location);
        }

        var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Jpeg);
        return ms.ToArray();
    }
}