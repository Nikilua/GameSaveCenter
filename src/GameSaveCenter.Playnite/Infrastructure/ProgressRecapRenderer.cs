using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GameSaveCenter.Playnite.Infrastructure
{
    internal sealed class ProgressRecapCard
    {
        internal ProgressRecapCard(BitmapSource? picture, string date, string note)
        {
            Picture = picture;
            Date = date;
            Note = note;
        }

        internal BitmapSource? Picture { get; }
        internal string Date { get; }
        internal string Note { get; }
    }

    internal static class ProgressRecapRenderer
    {
        internal static BitmapSource Render(IReadOnlyList<ProgressRecapCard> cards)
        {
            if (cards.Count < 2 || cards.Count > 9) throw new ArgumentOutOfRangeException(nameof(cards));
            const int width = 300;
            const int padding = 18;
            const int gap = 14;
            const int pictureHeight = 172;
            var columns = Math.Min(cards.Count, 3);
            var day = cards.Select(card => Format(card.Date, 14, Brushes.DimGray, width - 2 * padding)).ToArray();
            var note = cards.Select(card => Format(card.Note, 18, Brushes.Black, width - 2 * padding)).ToArray();
            var rowHeights = Enumerable.Range(0, (cards.Count + columns - 1) / columns)
                .Select(row => Enumerable.Range(row * columns, Math.Min(columns, cards.Count - row * columns))
                    .Max(index => (int)Math.Ceiling(pictureHeight + 3 * padding + day[index].Height + note[index].Height))).ToArray();
            var canvasWidth = width * columns + gap * (columns + 1);
            var canvasHeight = rowHeights.Sum() + gap * (rowHeights.Length + 1);
            var background = new SolidColorBrush(Color.FromRgb(241, 243, 245));
            var placeholder = Format("截图不可用", 16, Brushes.DimGray, width - 2 * padding);
            var drawing = new DrawingVisual();
            using (var context = drawing.RenderOpen())
            {
                context.DrawRectangle(background, null, new Rect(0, 0, canvasWidth, canvasHeight));
                var y = gap;
                for (var row = 0; row < rowHeights.Length; row++)
                {
                    for (var column = 0; column < columns; column++)
                    {
                        var index = row * columns + column;
                        if (index >= cards.Count) break;
                        var x = gap + column * (width + gap);
                        context.DrawRoundedRectangle(Brushes.White, null, new Rect(x, y, width, rowHeights[row]), 10, 10);
                        var imageBounds = new Rect(x + padding, y + padding, width - 2 * padding, pictureHeight);
                        context.DrawRectangle(background, null, imageBounds);
                        var picture = cards[index].Picture;
                        if (picture == null)
                            context.DrawText(placeholder, new Point(imageBounds.X + padding, imageBounds.Y + padding));
                        else
                        {
                            var scale = Math.Min(imageBounds.Width / picture.PixelWidth, imageBounds.Height / picture.PixelHeight);
                            var renderedWidth = picture.PixelWidth * scale;
                            var renderedHeight = picture.PixelHeight * scale;
                            context.DrawImage(picture, new Rect(
                                imageBounds.X + (imageBounds.Width - renderedWidth) / 2,
                                imageBounds.Y + (imageBounds.Height - renderedHeight) / 2, renderedWidth, renderedHeight));
                        }
                        var dateY = imageBounds.Bottom + padding / 2d;
                        context.DrawText(day[index], new Point(x + padding, dateY));
                        context.DrawText(note[index], new Point(x + padding, dateY + day[index].Height + padding / 2d));
                    }
                    y += rowHeights[row] + gap;
                }
            }
            var bitmap = new RenderTargetBitmap(canvasWidth, canvasHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawing);
            bitmap.Freeze();
            return bitmap;
        }

        private static FormattedText Format(string text, double fontSize, Brush brush, double width)
        {
            return new FormattedText(text ?? string.Empty, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                new Typeface("Microsoft YaHei UI"), fontSize, brush, 1)
            {
                MaxTextWidth = width,
                MaxTextHeight = 210,
                Trimming = TextTrimming.CharacterEllipsis
            };
        }

        internal static void SavePng(BitmapSource preview, string destination)
        {
            var fullPath = Path.GetFullPath(destination);
            var temporary = Path.Combine(Path.GetDirectoryName(fullPath)!, $".gsc-recap-{Guid.NewGuid():N}.tmp");
            var ownsTemporary = false;
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    ownsTemporary = true;
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(preview));
                    encoder.Save(stream);
                    stream.Flush(true);
                }
                if (File.Exists(fullPath)) File.Replace(temporary, fullPath, null);
                else File.Move(temporary, fullPath);
            }
            finally
            {
                if (ownsTemporary && File.Exists(temporary)) File.Delete(temporary);
            }
        }
    }
}
