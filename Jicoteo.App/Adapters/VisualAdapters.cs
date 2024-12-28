using LCSC.App.Models;
using LCSC.Common.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.UI;

namespace LCSC.App.Adapters
{
    public static class VisualAdapters
    {
        public static Visibility StringToVisibility(string value)
            => string.IsNullOrWhiteSpace(value) ? Visibility.Collapsed : Visibility.Visible;

        public static ImageSource? ToImageSource(string? pictureUrl, int decodedPixelWidth = 128)
        {
            if (pictureUrl == null)
            {
                return null;
            }
            var uri = new Uri(pictureUrl);
            var bitmapImage = new BitmapImage(uri)
            {
                DecodePixelWidth = decodedPixelWidth
            };
            return bitmapImage;
        }

        public static ImageSource ToMemberPictureImageSource(string? pictureUrl, Race race, int decodedPixelWidth = 128)
        {
            if (string.IsNullOrWhiteSpace(pictureUrl))
            {
                pictureUrl = race switch
                {
                    Race.Random => "ms-appx:///Assets/Images/RandomPortrait.png",
                    Race.Zerg => "ms-appx:///Assets/Images/ZergPortrait.png",
                    Race.Terran => "ms-appx:///Assets/Images/TerranPortrait.png",
                    Race.Protoss => "ms-appx:///Assets/Images/ProtossPortrait.png",
                    _ => "ms-appx:///Assets/Images/UnknownPortrait.png",
                };
            }

            var uri = new Uri(pictureUrl);
            var bitmapImage = new BitmapImage(uri)
            {
                DecodePixelWidth = decodedPixelWidth
            };
            return bitmapImage;
        }

        public static SolidColorBrush ToMemberStateBackgroundBrush(bool isVerified, bool isBanned)
        {
            Color color = Colors.Transparent;
            if (isBanned)
            {
                color = Color.FromArgb(255, 167, 0, 0);
            }
            else if (isVerified)
            {
                color = Color.FromArgb(255, 0, 138, 0);
            }
            return new SolidColorBrush(color);
        }

        public static string ToMemberStateIconGlyph(bool isVerified, bool isBanned)
        {
            if (isBanned)
            {
                return "block";
            }
            if (isVerified)
            {
                return "done";
            }
            return string.Empty;
        }

        public static Visibility ToMemberStateVisibility(bool isVerified, bool isBanned)
            => isVerified || isBanned ? Visibility.Visible : Visibility.Collapsed;

        public static ImageSource? ToRaceImageSource(Race race, int decodedPixelWidth = 128)
        {
            var imagePath = race switch
            {
                Race.Random => "ms-appx:///Assets/Images/RandomRace.png",
                Race.Zerg => "ms-appx:///Assets/Images/ZergRace.png",
                Race.Terran => "ms-appx:///Assets/Images/TerranRace.png",
                Race.Protoss => "ms-appx:///Assets/Images/ProtossRace.png",
                _ => "",
            };
            if (string.IsNullOrEmpty(imagePath))
            {
                return null;
            }
            var uri = new Uri(imagePath);
            var bitmapImage = new BitmapImage(uri)
            {
                DecodePixelWidth = decodedPixelWidth
            };
            return bitmapImage;
        }

        public static bool ToReverseBool(bool value) => !value;

        public static Visibility ToReverseBoolVisibility(bool value)
            => value ? Visibility.Collapsed : Visibility.Visible;
    }
}