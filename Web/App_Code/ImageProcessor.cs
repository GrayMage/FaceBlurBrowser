using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

public class ImageProcessor
{
    private readonly Image _image;

    public ImageProcessor(byte[] imgData)
    {
        _image = Image.FromStream(new MemoryStream(imgData));
    }

    public byte[] Process()
    {
        var fromImage = Graphics.FromImage(_image);
        foreach (var faceRect in new FaceDetector(new Bitmap(_image)).GetFaceRects())
            fromImage.DrawRectangle(new Pen(Color.Red, 4), faceRect);

        var ms = new MemoryStream();
        _image.Save(ms, ImageFormat.Jpeg);
        return ms.ToArray();
    }
}