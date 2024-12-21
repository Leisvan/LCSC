using LCSC.App.Models;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCSC.App.Adapters
{
    public static class VisualAdapters
    {
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
    }
}