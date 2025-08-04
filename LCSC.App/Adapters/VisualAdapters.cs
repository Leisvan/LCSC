using CommunityToolkit.WinUI.Helpers;
using LCSC.Models;
using LCSC.Models.Airtable;
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
        private static readonly Color CommunityColor = "#ffd4e0".ToColor();
        private static readonly Color ErrorColor = "#A70000".ToColor();
        private static readonly Color OfficialColor = "#76f480".ToColor();
        private static readonly Color PartnerColor = "#feeab6".ToColor();
        private static readonly Color SuccessColor = "#008a00".ToColor();
        private static readonly Color WarningColor = "#e0aa21".ToColor();

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

        public static SolidColorBrush ToMemberStateBackgroundBrush(MemberRecord record, bool validProfiles)
        {
            Color color = Colors.Transparent;
            if (record.Banned)
            {
                color = ErrorColor;
            }
            else if (!validProfiles)
            {
                color = WarningColor;
            }
            else if (record.Verified)
            {
                color = SuccessColor;
            }
            return new SolidColorBrush(color);
        }

        public static string ToMemberStateIconGlyph(bool isVerified, bool isBanned, bool validProfiles)
        {
            if (isBanned)
            {
                return "block";
            }
            if (!validProfiles)
            {
                return "person_off";
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

        public static Brush ToTournamentAffiliationForegroundBrush(TournamentAffiliation affiliation)
        {
            var color = affiliation switch
            {
                TournamentAffiliation.Official => OfficialColor,
                TournamentAffiliation.Partner => PartnerColor,
                TournamentAffiliation.Community => CommunityColor,
                _ => Colors.Transparent,
            };
            return new SolidColorBrush(color);
        }

        public static string ToTournamentAffiliationGlyph(TournamentAffiliation affiliation)
        {
            return affiliation switch
            {
                TournamentAffiliation.Official => "verified",
                TournamentAffiliation.Partner => "handshake",
                TournamentAffiliation.Community => "home",
                _ => string.Empty,
            };
        }
    }
}