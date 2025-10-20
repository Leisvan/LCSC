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
                color = "ErrorColor".GetAppResource<Color>();
            }
            else if (!validProfiles)
            {
                color = "WarningColor".GetAppResource<Color>();
            }
            else if (record.Verified)
            {
                color = "SuccessColor".GetAppResource<Color>();
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
                return "check_small";
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

        public static ImageSource? ToRegionLeagueBadgePath(LadderRegionRecord? record, bool useEmptyAsset)
        {
            string? badgeAssetName = null;
            if (record != null)
            {
                badgeAssetName = (record.League, record.Tier) switch
                {
                    (0, 0) => "Bronze1",
                    (0, 1) => "Bronze2",
                    (0, 2) => "Bronze3",
                    (1, 0) => "Silver1",
                    (1, 1) => "Silver2",
                    (1, 2) => "Silver3",
                    (2, 0) => "Gold1",
                    (2, 1) => "Gold2",
                    (2, 2) => "Gold3",
                    (3, 0) => "Platinum1",
                    (3, 1) => "Platinum2",
                    (3, 2) => "Platinum3",
                    (4, 0) => "Diamond1",
                    (4, 1) => "Diamond2",
                    (4, 2) => "Diamond3",
                    (5, 0) => "Master1",
                    (5, 1) => "Master2",
                    (5, 2) => "Master3",
                    (0, _) => "Bronze",
                    (1, _) => "Silver",
                    (2, _) => "Gold",
                    (3, _) => "Platinum",
                    (4, _) => "Diamond",
                    (5, _) => "Master",
                    (6, _) => "Grandmaster",
                    _ => null,
                };
            }
            if (badgeAssetName == null)
            {
                if (useEmptyAsset)
                {
                    badgeAssetName = "Placement";
                }
                else
                {
                    return null;
                }
            }
            return new BitmapImage { UriSource = new Uri($"ms-appx:///Assets/Leagues/{badgeAssetName}.png") };
        }

        public static bool ToReverseBool(bool value) => !value;

        public static Visibility ToReverseBoolVisibility(bool value)
            => value ? Visibility.Collapsed : Visibility.Visible;

        public static Brush ToTournamentAffiliationForegroundBrush(TournamentAffiliation affiliation)
        {
            var color = affiliation switch
            {
                TournamentAffiliation.Official => "Tag1Color".GetAppResource<Color>(),
                TournamentAffiliation.Partner => "Tag2Color".GetAppResource<Color>(),
                TournamentAffiliation.Community => "Tag3Color".GetAppResource<Color>(),
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
                TournamentAffiliation.Community => "partner_exchange",
                _ => string.Empty,
            };
        }
    }
}