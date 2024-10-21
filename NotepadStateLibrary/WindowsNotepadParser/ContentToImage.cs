using NotepadStateLibrary;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Text;




namespace WindowsNotepadParser
{
    internal class ContentToImage
    {
        public ContentToImage(byte[] origContent, List<UnsavedBufferChunk> chunks, string outputFilename, int frameDelay = 100, string font = "Arial", int widthModifier = 24, int heightModifier = 36, int fontSize = 36)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Encoding.Unicode.GetString(origContent));
            List<string> contents = new List<string>();
            contents.Add(sb.ToString());

            foreach (var c in chunks)
            {
                if (c.DeletionAction > 0)
                {
                    sb.Remove((int)c.CursorPosition, (int)c.DeletionAction);
                }
                if (c.AdditionAction > 0)
                {
                    sb.Insert((int)c.CursorPosition, Encoding.Unicode.GetString(c.CharactersAdded));
                }
                contents.Add(sb.ToString());
            }

            int longest = 0;
            int lines = 0;
            foreach (var c in contents)
            {
                var x = FindLongestLine(c);
                if (x.Item1 > lines)
                {
                    lines = x.Item1;
                }
                if (x.Item2 > longest)
                {
                    longest = x.Item2;
                }
            }

            //TODO: 18 and 24 are hardcoded. Can we figure this out from the Font and Fontsize?
            int width = longest * widthModifier; 
            int height = lines * heightModifier;

            //width = 800;
            //height = 800;

            //Initial Image/Content            
            using Image img = new Image<Rgba32>(width, height, Color.White);
            img.Mutate(ctx => ctx
                .DrawText(Encoding.Unicode.GetString(origContent).ReplaceLineEndings(), new Font(SystemFonts.Get(font), fontSize), Color.Black, new PointF(0, 0)));

            var gifMetadata = img.Metadata.GetGifMetadata();
            gifMetadata.RepeatCount = 0;

            GifFrameMetadata metadata = img.Frames.RootFrame.Metadata.GetGifMetadata();
            metadata.FrameDelay = frameDelay;

            //Later Images/Contents
            foreach (var c in contents)
            {
                using Image cimg = new Image<Rgba32>(width, height, Color.White);

                cimg.Mutate(ctx => ctx
                .DrawText(c, new Font(SystemFonts.Get(font), fontSize), Color.Black, new PointF(0, 0)));

                metadata = cimg.Frames.RootFrame.Metadata.GetGifMetadata();
                metadata.FrameDelay = frameDelay;
                img.Frames.AddFrame(cimg.Frames.RootFrame);
            }

            img.SaveAsGif(outputFilename);
        }
        static (int, int) FindLongestLine(string input)
        {
            var lines = input.Split(new[] { '\r', '\n' });
            string longestLine = "";

            foreach (string line in lines)
            {
                if (line.Length > longestLine.Length)
                {
                    longestLine = line;
                }
            }

            return (lines.Length, longestLine.Length);
        }
    }
}
