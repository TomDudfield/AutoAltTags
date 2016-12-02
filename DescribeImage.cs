using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Microsoft.ProjectOxford.Vision;

namespace AutoAltTags
{
    class DescribeImage
    {
        private readonly string _apiKey;

        public DescribeImage(string apiKey)
        {
            _apiKey = apiKey;
        }

        public string GetDescription(Stream stream)
        {
            string description = string.Empty;

            using (Image oldImage = Image.FromStream(stream))
            {
                Size newSize = CalculateDimensions(oldImage.Size, 2048);

                using (Bitmap bitmap = new Bitmap(oldImage, newSize))
                {
                    MemoryStream outputStream = new MemoryStream();
                    bitmap.Save(outputStream, ImageFormat.Jpeg);
                    outputStream.Position = 0;

                    VisionServiceClient visionServiceClient = new VisionServiceClient(_apiKey);
                    var analysisResult = visionServiceClient.DescribeAsync(outputStream);

                    if (analysisResult.Result.Description.Captions != null && analysisResult.Result.Description.Captions.Any())
                        description = analysisResult.Result.Description.Captions.First().Text;

                    return description;
                }
            }
        }

        private static Size CalculateDimensions(Size oldSize, int maxSize)
        {
            Size newSize = new Size(oldSize.Width, oldSize.Height);

            if (oldSize.Height > oldSize.Width && oldSize.Height > maxSize)
            {
                newSize.Width = (int)(oldSize.Width * ((float)maxSize / oldSize.Height));
                newSize.Height = maxSize;
            }
            else if (oldSize.Height > oldSize.Width && oldSize.Width > maxSize)
            {
                newSize.Width = maxSize;
                newSize.Height = (int)(oldSize.Height * ((float)maxSize / oldSize.Width));
            }

            return newSize;
        }
    }
}
