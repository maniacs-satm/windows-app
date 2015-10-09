using System.Collections.Generic;
using Newtonsoft.Json;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace wallabag.Models
{
    [SQLite.Table("Tags")]
    public class Tag
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [SQLite.Ignore]
        public SolidColorBrush Color { get; set; }

        public override string ToString() { return Label; }

        public static List<SolidColorBrush> PossibleColors
        { get; }
        = new List<SolidColorBrush>() {
        new SolidColorBrush(ColorHelper.FromArgb(0xFF,0xFF,0xB9,0x00)),
        new SolidColorBrush(ColorHelper.FromArgb(0xFF,0xFF,0x8C,0x00)),
        new SolidColorBrush(ColorHelper.FromArgb(0xFF,0xF7,0x63,0x0C)),
        new SolidColorBrush(ColorHelper.FromArgb(0xFF,0xCA,0x50,0x10)),
        new SolidColorBrush(ColorHelper.FromArgb(0xFF,0xEF,0x69,0x50)),
        new SolidColorBrush(ColorHelper.FromArgb(0xFF,0xE8,0x11,0x23)),
        new SolidColorBrush(ColorHelper.FromArgb(0xFF,0xE3,0x00,0x8C)),
        new SolidColorBrush(ColorHelper.FromArgb(0xFF,0x9A,0x00,0x89)),
        new SolidColorBrush(ColorHelper.FromArgb(0xFF,0x00,0x78,0xD7)),
        new SolidColorBrush(ColorHelper.FromArgb(0xFF,0x00,0x63,0xB1)),
        new SolidColorBrush(ColorHelper.FromArgb(0xFF,0x6B,0x69,0xD6)),
        new SolidColorBrush(ColorHelper.FromArgb(0xFF,0x88,0x17,0x98)),
        new SolidColorBrush(ColorHelper.FromArgb(0xFF,0x00,0x99,0xBC)),
        new SolidColorBrush(ColorHelper.FromArgb(0xFF,0x00,0xB7,0xC3)),
        new SolidColorBrush(ColorHelper.FromArgb(0xFF,0x00,0xB2,0x94)),
        new SolidColorBrush(ColorHelper.FromArgb(0xFF,0x00,0xCC,0x6A)),
        new SolidColorBrush(ColorHelper.FromArgb(0xFF,0x10,0x89,0x3E))
        };
    }
}
