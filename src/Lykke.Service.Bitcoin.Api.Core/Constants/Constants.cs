namespace Lykke.Service.Bitcoin.Api.Core.Constants
{
    public static class Constants
    {
        public static class Assets
        {
            public static class Bitcoin
            {
                public const string AssetId = "BTC";
                public const int Accuracy = 8;
                public const string Name = "Bitcoin";
            }

            public static class Lkk
            {
                public const string AssetId = "LKK";
                public const int Accuracy = 6;
                public const string Name = "Lykke coins";
            }

            public static class Lkk1Y
            {
                public const string AssetId = "LKK1Y";
                public const int Accuracy = 6;
                public const string Name = "One-year forwards on Lykke shares";
            }

            public static class Tree
            {
                public const string AssetId = "HCP";
                public const int Accuracy = 6;
                public const string Name = "Heyerdahl Climate Pioneers";
            }
        }
    }
}
