namespace Acme.FileBackup.App.Utilities
{
    // Supported Image fortmats by Metadata Extractor
    static public class ImageFormats
    {
        public static List<string> GetImageFormatList()
        {
            var imageFormats = 
                $".Avi;" +
                $".Bmp;" +
                $".Eps;" +
                $".FileSystem;" +
                $".FileType;" +
                $".Gif;" +
                $".Heif;" +
                $".Ico;" +
                $".Jpeg;" +
                $".Jpg;" +
                $".Mpeg;" +
                $".Netpbm;" +
                $".Pcx;" +
                $".Photoshop;" +
                $".Png;" +
                $".QuickTime;" +
                $".Raf;" +
                $".Tga;" +
                $".Tiff;" +
                $".Wav;" +
                $".WebP";

            List<string> imageFormatList = imageFormats.ToLower().Split(';').ToList();
            return imageFormatList;
        }

        public static bool IsImage(string path)
        {
            string ext = Path.GetExtension(path);
            var match = GetImageFormatList().Count(stringToCheck => stringToCheck.Contains(ext));
            return match > 0;
        }
    }
}
