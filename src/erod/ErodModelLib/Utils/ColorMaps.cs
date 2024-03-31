using System;
using System.Drawing;

namespace ErodModelLib.Utils
{
    public partial class ColorMaps
    {
        public enum ColorMapTypes { Plasma_Sequential, Viridis_Sequential, Inferno_Sequential, Cividis_Sequential, Blues_Sequential, BuGn_Sequential, YlGnBu_Sequential, YOrRd_Sequential, CoolWarm_Diverging, RdYlBu_Diverging, RdGy_Diverging, RdBu_Diverging, Cool_Sequential2, Hot_Sequential2, Bone_Sequential2, Binary_Sequential2, Turbo_Miscellaneous, Twilight_Cyclic, TwilightShifted_Cyclic, HSV_Cyclic }

        public static Color[] GetColorMap(ColorMapTypes colorMapType, int alpha)
        {
            Color[] colormap;
            switch (colorMapType)
            {
                case ColorMapTypes.Plasma_Sequential:
                    colormap = ColorMaps.Plasma.GetColors(alpha);
                    break;
                case ColorMapTypes.Viridis_Sequential:
                    colormap = ColorMaps.Viridis.GetColors(alpha);
                    break;
                case ColorMapTypes.Inferno_Sequential:
                    colormap = ColorMaps.Inferno.GetColors(alpha);
                    break;
                case ColorMapTypes.Cividis_Sequential:
                    colormap = ColorMaps.Cividis.GetColors(alpha);
                    break;
                case ColorMapTypes.Blues_Sequential:
                    colormap = ColorMaps.Blues.GetColors(alpha);
                    break;
                case ColorMapTypes.BuGn_Sequential:
                    colormap = ColorMaps.BuGn.GetColors(alpha);
                    break;
                case ColorMapTypes.YlGnBu_Sequential:
                    colormap = ColorMaps.YlGnBu.GetColors(alpha);
                    break;
                case ColorMapTypes.YOrRd_Sequential:
                    colormap = ColorMaps.YOrRd.GetColors(alpha);
                    break;
                case ColorMapTypes.CoolWarm_Diverging:
                    colormap = ColorMaps.CoolWarm.GetColors(alpha);
                    break;
                case ColorMapTypes.RdYlBu_Diverging:
                    colormap = ColorMaps.RdYlBu.GetColors(alpha);
                    break;
                case ColorMapTypes.RdGy_Diverging:
                    colormap = ColorMaps.RdGy.GetColors(alpha);
                    break;
                case ColorMapTypes.RdBu_Diverging:
                    colormap = ColorMaps.RdBu.GetColors(alpha);
                    break;
                case ColorMapTypes.Cool_Sequential2:
                    colormap = ColorMaps.Cool.GetColors(alpha);
                    break;
                case ColorMapTypes.Hot_Sequential2:
                    colormap = ColorMaps.Hot.GetColors(alpha);
                    break;
                case ColorMapTypes.Bone_Sequential2:
                    colormap = ColorMaps.Bone.GetColors(alpha);
                    break;
                case ColorMapTypes.Binary_Sequential2:
                    colormap = ColorMaps.Binary.GetColors(alpha);
                    break;
                case ColorMapTypes.Turbo_Miscellaneous:
                    colormap = ColorMaps.Turbo.GetColors(alpha);
                    break;
                case ColorMapTypes.Twilight_Cyclic:
                    colormap = ColorMaps.Twilight.GetColors(alpha);
                    break;
                case ColorMapTypes.TwilightShifted_Cyclic:
                    colormap = ColorMaps.TwilightShifted.GetColors(alpha);
                    break;
                case ColorMapTypes.HSV_Cyclic:
                    colormap = ColorMaps.HSV.GetColors(alpha);
                    break;
                default:
                    colormap = ColorMaps.Plasma.GetColors(alpha);
                    break;

            }

            return colormap;
        }

        #region Perceptually Uniform Sequential colormaps
        private static class Plasma
        {
            public static Color[] GetColors(int alpha = 255)
            {
                Color[] colorRange = {
                    Color.FromArgb(alpha, 12, 7, 134),
                    Color.FromArgb(alpha, 27, 6, 140),
                    Color.FromArgb(alpha, 37, 5, 145),
                    Color.FromArgb(alpha, 47, 4, 149),
                    Color.FromArgb(alpha, 56, 4, 153),
                    Color.FromArgb(alpha, 66, 3, 157),
                    Color.FromArgb(alpha, 74, 2, 160),
                    Color.FromArgb(alpha, 82, 1, 163),
                    Color.FromArgb(alpha, 90, 0, 165),
                    Color.FromArgb(alpha, 100, 0, 167),
                    Color.FromArgb(alpha, 108, 0, 168),
                    Color.FromArgb(alpha, 115, 0, 168),
                    Color.FromArgb(alpha, 123, 2, 168),
                    Color.FromArgb(alpha, 130, 4, 167),
                    Color.FromArgb(alpha, 139, 9, 164),
                    Color.FromArgb(alpha, 146, 15, 162),
                    Color.FromArgb(alpha, 153, 20, 159),
                    Color.FromArgb(alpha, 159, 26, 155),
                    Color.FromArgb(alpha, 167, 33, 151),
                    Color.FromArgb(alpha, 173, 38, 146),
                    Color.FromArgb(alpha, 178, 44, 142),
                    Color.FromArgb(alpha, 184, 50, 137),
                    Color.FromArgb(alpha, 189, 55, 132),
                    Color.FromArgb(alpha, 195, 62, 127),
                    Color.FromArgb(alpha, 200, 68, 122),
                    Color.FromArgb(alpha, 205, 73, 117),
                    Color.FromArgb(alpha, 209, 79, 113),
                    Color.FromArgb(alpha, 215, 86, 108),
                    Color.FromArgb(alpha, 219, 91, 103),
                    Color.FromArgb(alpha, 223, 97, 99),
                    Color.FromArgb(alpha, 227, 103, 95),
                    Color.FromArgb(alpha, 230, 109, 90),
                    Color.FromArgb(alpha, 234, 116, 85),
                    Color.FromArgb(alpha, 237, 123, 81),
                    Color.FromArgb(alpha, 240, 129, 77),
                    Color.FromArgb(alpha, 243, 135, 72),
                    Color.FromArgb(alpha, 246, 143, 67),
                    Color.FromArgb(alpha, 248, 150, 63),
                    Color.FromArgb(alpha, 250, 157, 58),
                    Color.FromArgb(alpha, 251, 164, 54),
                    Color.FromArgb(alpha, 252, 172, 50),
                    Color.FromArgb(alpha, 253, 181, 45),
                    Color.FromArgb(alpha, 253, 188, 42),
                    Color.FromArgb(alpha, 253, 196, 39),
                    Color.FromArgb(alpha, 252, 204, 37),
                    Color.FromArgb(alpha, 250, 214, 36),
                    Color.FromArgb(alpha, 248, 223, 36),
                    Color.FromArgb(alpha, 245, 231, 38),
                    Color.FromArgb(alpha, 242, 240, 38),
                    Color.FromArgb(alpha, 239, 248, 33),
                };

                return colorRange;
            }
        }

        private static class Viridis
        {
            public static Color[] GetColors(int alpha = 255)
            {
                Color[] colorRange = {
                    Color.FromArgb(alpha, 68, 1, 84),
                    Color.FromArgb(alpha, 69, 8, 91),
                    Color.FromArgb(alpha, 71, 15, 98),
                    Color.FromArgb(alpha, 71, 22, 105),
                    Color.FromArgb(alpha, 72, 29, 111),
                    Color.FromArgb(alpha, 71, 37, 117),
                    Color.FromArgb(alpha, 71, 43, 122),
                    Color.FromArgb(alpha, 70, 49, 126),
                    Color.FromArgb(alpha, 68, 55, 129),
                    Color.FromArgb(alpha, 66, 62, 133),
                    Color.FromArgb(alpha, 64, 68, 135),
                    Color.FromArgb(alpha, 61, 74, 137),
                    Color.FromArgb(alpha, 59, 80, 138),
                    Color.FromArgb(alpha, 57, 85, 139),
                    Color.FromArgb(alpha, 54, 91, 140),
                    Color.FromArgb(alpha, 51, 96, 141),
                    Color.FromArgb(alpha, 49, 101, 141),
                    Color.FromArgb(alpha, 47, 106, 141),
                    Color.FromArgb(alpha, 44, 112, 142),
                    Color.FromArgb(alpha, 42, 117, 142),
                    Color.FromArgb(alpha, 40, 122, 142),
                    Color.FromArgb(alpha, 39, 126, 142),
                    Color.FromArgb(alpha, 37, 131, 141),
                    Color.FromArgb(alpha, 35, 137, 141),
                    Color.FromArgb(alpha, 33, 141, 140),
                    Color.FromArgb(alpha, 31, 146, 140),
                    Color.FromArgb(alpha, 30, 151, 138),
                    Color.FromArgb(alpha, 30, 156, 137),
                    Color.FromArgb(alpha, 31, 161, 135),
                    Color.FromArgb(alpha, 33, 166, 133),
                    Color.FromArgb(alpha, 36, 170, 130),
                    Color.FromArgb(alpha, 41, 175, 127),
                    Color.FromArgb(alpha, 48, 180, 122),
                    Color.FromArgb(alpha, 56, 185, 118),
                    Color.FromArgb(alpha, 64, 189, 114),
                    Color.FromArgb(alpha, 73, 193, 109),
                    Color.FromArgb(alpha, 85, 198, 102),
                    Color.FromArgb(alpha, 96, 201, 96),
                    Color.FromArgb(alpha, 107, 205, 89),
                    Color.FromArgb(alpha, 119, 208, 82),
                    Color.FromArgb(alpha, 131, 211, 75),
                    Color.FromArgb(alpha, 146, 215, 65),
                    Color.FromArgb(alpha, 159, 217, 56),
                    Color.FromArgb(alpha, 173, 220, 48),
                    Color.FromArgb(alpha, 186, 222, 39),
                    Color.FromArgb(alpha, 202, 224, 30),
                    Color.FromArgb(alpha, 215, 226, 25),
                    Color.FromArgb(alpha, 228, 227, 24),
                    Color.FromArgb(alpha, 241, 229, 28),
                    Color.FromArgb(alpha, 253, 231, 36),
                };
                return colorRange;
            }

        }

        private static class Inferno
        {
            public static Color[] GetColors(int alpha = 255)
            {
                Color[] colorRange = {
                    Color.FromArgb(alpha, 0, 0, 3),
                    Color.FromArgb(alpha, 1, 1, 11),
                    Color.FromArgb(alpha, 4, 3, 22),
                    Color.FromArgb(alpha, 9, 6, 33),
                    Color.FromArgb(alpha, 15, 9, 45),
                    Color.FromArgb(alpha, 23, 11, 59),
                    Color.FromArgb(alpha, 31, 12, 71),
                    Color.FromArgb(alpha, 39, 11, 82),
                    Color.FromArgb(alpha, 48, 10, 92),
                    Color.FromArgb(alpha, 59, 9, 100),
                    Color.FromArgb(alpha, 67, 10, 104),
                    Color.FromArgb(alpha, 75, 12, 107),
                    Color.FromArgb(alpha, 83, 14, 109),
                    Color.FromArgb(alpha, 91, 17, 110),
                    Color.FromArgb(alpha, 101, 21, 110),
                    Color.FromArgb(alpha, 109, 24, 110),
                    Color.FromArgb(alpha, 117, 27, 109),
                    Color.FromArgb(alpha, 125, 29, 108),
                    Color.FromArgb(alpha, 134, 33, 106),
                    Color.FromArgb(alpha, 142, 36, 104),
                    Color.FromArgb(alpha, 150, 38, 102),
                    Color.FromArgb(alpha, 158, 41, 99),
                    Color.FromArgb(alpha, 166, 44, 95),
                    Color.FromArgb(alpha, 175, 49, 91),
                    Color.FromArgb(alpha, 183, 52, 86),
                    Color.FromArgb(alpha, 190, 56, 82),
                    Color.FromArgb(alpha, 197, 61, 77),
                    Color.FromArgb(alpha, 205, 66, 71),
                    Color.FromArgb(alpha, 212, 72, 65),
                    Color.FromArgb(alpha, 218, 78, 59),
                    Color.FromArgb(alpha, 223, 84, 54),
                    Color.FromArgb(alpha, 229, 91, 48),
                    Color.FromArgb(alpha, 234, 100, 40),
                    Color.FromArgb(alpha, 238, 108, 34),
                    Color.FromArgb(alpha, 242, 116, 28),
                    Color.FromArgb(alpha, 245, 124, 21),
                    Color.FromArgb(alpha, 248, 135, 13),
                    Color.FromArgb(alpha, 249, 144, 8),
                    Color.FromArgb(alpha, 251, 153, 6),
                    Color.FromArgb(alpha, 251, 162, 8),
                    Color.FromArgb(alpha, 251, 172, 16),
                    Color.FromArgb(alpha, 251, 183, 28),
                    Color.FromArgb(alpha, 250, 193, 40),
                    Color.FromArgb(alpha, 248, 203, 52),
                    Color.FromArgb(alpha, 246, 213, 66),
                    Color.FromArgb(alpha, 243, 224, 86),
                    Color.FromArgb(alpha, 241, 233, 104),
                    Color.FromArgb(alpha, 241, 242, 125),
                    Color.FromArgb(alpha, 245, 248, 145),
                    Color.FromArgb(alpha, 252, 254, 164)};

                return colorRange;
            }

        }

        private static class Cividis
        {
            public static Color[] GetColors(int alpha = 255)
            {
                Color[] colorRange = {
                    Color.FromArgb(alpha, 0, 34, 77),
                    Color.FromArgb(alpha, 0, 38, 85),
                    Color.FromArgb(alpha, 0, 41, 94),
                    Color.FromArgb(alpha, 0, 44, 103),
                    Color.FromArgb(alpha, 0, 48, 112),
                    Color.FromArgb(alpha, 11, 51, 112),
                    Color.FromArgb(alpha, 24, 55, 111),
                    Color.FromArgb(alpha, 33, 59, 110),
                    Color.FromArgb(alpha, 40, 62, 109),
                    Color.FromArgb(alpha, 48, 66, 108),
                    Color.FromArgb(alpha, 54, 70, 108),
                    Color.FromArgb(alpha, 59, 73, 107),
                    Color.FromArgb(alpha, 65, 77, 107),
                    Color.FromArgb(alpha, 70, 80, 107),
                    Color.FromArgb(alpha, 76, 84, 108),
                    Color.FromArgb(alpha, 80, 88, 108),
                    Color.FromArgb(alpha, 85, 91, 109),
                    Color.FromArgb(alpha, 89, 95, 109),
                    Color.FromArgb(alpha, 95, 99, 110),
                    Color.FromArgb(alpha, 99, 102, 111),
                    Color.FromArgb(alpha, 104, 106, 112),
                    Color.FromArgb(alpha, 108, 109, 114),
                    Color.FromArgb(alpha, 112, 113, 115),
                    Color.FromArgb(alpha, 117, 117, 117),
                    Color.FromArgb(alpha, 121, 121, 119),
                    Color.FromArgb(alpha, 126, 125, 120),
                    Color.FromArgb(alpha, 131, 128, 120),
                    Color.FromArgb(alpha, 136, 133, 120),
                    Color.FromArgb(alpha, 141, 137, 120),
                    Color.FromArgb(alpha, 146, 140, 119),
                    Color.FromArgb(alpha, 151, 144, 118),
                    Color.FromArgb(alpha, 156, 148, 118),
                    Color.FromArgb(alpha, 162, 153, 116),
                    Color.FromArgb(alpha, 167, 157, 115),
                    Color.FromArgb(alpha, 172, 161, 113),
                    Color.FromArgb(alpha, 177, 165, 112),
                    Color.FromArgb(alpha, 183, 170, 109),
                    Color.FromArgb(alpha, 188, 174, 107),
                    Color.FromArgb(alpha, 194, 178, 105),
                    Color.FromArgb(alpha, 199, 182, 102),
                    Color.FromArgb(alpha, 204, 187, 99),
                    Color.FromArgb(alpha, 211, 192, 95),
                    Color.FromArgb(alpha, 216, 196, 91),
                    Color.FromArgb(alpha, 222, 201, 87),
                    Color.FromArgb(alpha, 227, 205, 82),
                    Color.FromArgb(alpha, 234, 211, 76),
                    Color.FromArgb(alpha, 239, 216, 70),
                    Color.FromArgb(alpha, 245, 220, 63),
                    Color.FromArgb(alpha, 251, 225, 54),
                    Color.FromArgb(alpha, 253, 231, 55)};

                return colorRange;
            }

        }

        #endregion

        #region Sequential colormaps
        private static class Blues
        {
            public static Color[] GetColors(int alpha = 255)
            {
                Color[] colorRange = {
                    Color.FromArgb(alpha, 247, 251, 255),
                    Color.FromArgb(alpha, 243, 248, 253),
                    Color.FromArgb(alpha, 239, 245, 252),
                    Color.FromArgb(alpha, 235, 243, 251),
                    Color.FromArgb(alpha, 231, 240, 249),
                    Color.FromArgb(alpha, 226, 237, 248),
                    Color.FromArgb(alpha, 222, 235, 247),
                    Color.FromArgb(alpha, 218, 232, 245),
                    Color.FromArgb(alpha, 215, 230, 244),
                    Color.FromArgb(alpha, 210, 227, 243),
                    Color.FromArgb(alpha, 206, 224, 241),
                    Color.FromArgb(alpha, 203, 222, 240),
                    Color.FromArgb(alpha, 199, 219, 239),
                    Color.FromArgb(alpha, 193, 217, 237),
                    Color.FromArgb(alpha, 186, 214, 234),
                    Color.FromArgb(alpha, 180, 211, 232),
                    Color.FromArgb(alpha, 173, 208, 230),
                    Color.FromArgb(alpha, 167, 206, 228),
                    Color.FromArgb(alpha, 160, 202, 225),
                    Color.FromArgb(alpha, 152, 199, 223),
                    Color.FromArgb(alpha, 144, 194, 222),
                    Color.FromArgb(alpha, 136, 190, 220),
                    Color.FromArgb(alpha, 128, 185, 218),
                    Color.FromArgb(alpha, 119, 180, 216),
                    Color.FromArgb(alpha, 111, 176, 214),
                    Color.FromArgb(alpha, 103, 171, 212),
                    Color.FromArgb(alpha, 97, 167, 210),
                    Color.FromArgb(alpha, 89, 162, 207),
                    Color.FromArgb(alpha, 83, 157, 204),
                    Color.FromArgb(alpha, 76, 153, 202),
                    Color.FromArgb(alpha, 70, 148, 199),
                    Color.FromArgb(alpha, 64, 144, 197),
                    Color.FromArgb(alpha, 58, 138, 193),
                    Color.FromArgb(alpha, 52, 132, 191),
                    Color.FromArgb(alpha, 47, 127, 188),
                    Color.FromArgb(alpha, 42, 122, 185),
                    Color.FromArgb(alpha, 36, 116, 182),
                    Color.FromArgb(alpha, 31, 111, 179),
                    Color.FromArgb(alpha, 27, 106, 175),
                    Color.FromArgb(alpha, 23, 101, 171),
                    Color.FromArgb(alpha, 19, 96, 167),
                    Color.FromArgb(alpha, 15, 90, 163),
                    Color.FromArgb(alpha, 11, 85, 159),
                    Color.FromArgb(alpha, 8, 80, 154),
                    Color.FromArgb(alpha, 8, 74, 146),
                    Color.FromArgb(alpha, 8, 68, 137),
                    Color.FromArgb(alpha, 8, 63, 130),
                    Color.FromArgb(alpha, 8, 58, 122),
                    Color.FromArgb(alpha, 8, 53, 114),
                    Color.FromArgb(alpha, 8, 48, 107),
                };

                return colorRange;
            }
        }

        private static class BuGn
        {
            public static Color[] GetColors(int alpha)
            {
                Color[] colorRange = {
                        Color.FromArgb(alpha, 247, 252, 253),
                        Color.FromArgb(alpha, 244, 250, 252),
                        Color.FromArgb(alpha, 241, 249, 251),
                        Color.FromArgb(alpha, 238, 248, 251),
                        Color.FromArgb(alpha, 235, 247, 250),
                        Color.FromArgb(alpha, 232, 246, 249),
                        Color.FromArgb(alpha, 229, 245, 249),
                        Color.FromArgb(alpha, 225, 243, 246),
                        Color.FromArgb(alpha, 221, 242, 243),
                        Color.FromArgb(alpha, 217, 240, 239),
                        Color.FromArgb(alpha, 213, 239, 237),
                        Color.FromArgb(alpha, 209, 237, 234),
                        Color.FromArgb(alpha, 205, 236, 231),
                        Color.FromArgb(alpha, 198, 233, 227),
                        Color.FromArgb(alpha, 189, 230, 221),
                        Color.FromArgb(alpha, 181, 227, 217),
                        Color.FromArgb(alpha, 173, 223, 212),
                        Color.FromArgb(alpha, 165, 220, 207),
                        Color.FromArgb(alpha, 155, 217, 202),
                        Color.FromArgb(alpha, 147, 213, 197),
                        Color.FromArgb(alpha, 139, 210, 191),
                        Color.FromArgb(alpha, 131, 206, 185),
                        Color.FromArgb(alpha, 123, 203, 179),
                        Color.FromArgb(alpha, 114, 199, 172),
                        Color.FromArgb(alpha, 106, 195, 166),
                        Color.FromArgb(alpha, 99, 192, 160),
                        Color.FromArgb(alpha, 93, 189, 153),
                        Color.FromArgb(alpha, 86, 185, 144),
                        Color.FromArgb(alpha, 80, 182, 137),
                        Color.FromArgb(alpha, 74, 179, 130),
                        Color.FromArgb(alpha, 68, 176, 122),
                        Color.FromArgb(alpha, 63, 172, 115),
                        Color.FromArgb(alpha, 57, 165, 106),
                        Color.FromArgb(alpha, 53, 160, 98),
                        Color.FromArgb(alpha, 48, 154, 90),
                        Color.FromArgb(alpha, 43, 149, 83),
                        Color.FromArgb(alpha, 38, 142, 73),
                        Color.FromArgb(alpha, 33, 137, 67),
                        Color.FromArgb(alpha, 27, 132, 63),
                        Color.FromArgb(alpha, 22, 127, 59),
                        Color.FromArgb(alpha, 16, 123, 55),
                        Color.FromArgb(alpha, 10, 117, 51),
                        Color.FromArgb(alpha, 4, 112, 47),
                        Color.FromArgb(alpha, 0, 107, 43),
                        Color.FromArgb(alpha, 0, 101, 40),
                        Color.FromArgb(alpha, 0, 93, 37),
                        Color.FromArgb(alpha, 0, 87, 35),
                        Color.FromArgb(alpha, 0, 80, 32),
                        Color.FromArgb(alpha, 0, 74, 29),
                        Color.FromArgb(alpha, 0, 68, 27)};
                return colorRange;
            }
        }

        private static class YlGnBu
        {
            public static Color[] GetColors(int alpha)
            {
                Color[] colorRange = {
                        Color.FromArgb(alpha, 255, 255, 217),
                        Color.FromArgb(alpha, 252, 253, 210),
                        Color.FromArgb(alpha, 249, 252, 204),
                        Color.FromArgb(alpha, 246, 251, 198),
                        Color.FromArgb(alpha, 243, 250, 191),
                        Color.FromArgb(alpha, 240, 249, 184),
                        Color.FromArgb(alpha, 237, 248, 178),
                        Color.FromArgb(alpha, 232, 246, 177),
                        Color.FromArgb(alpha, 226, 243, 177),
                        Color.FromArgb(alpha, 218, 240, 178),
                        Color.FromArgb(alpha, 213, 238, 178),
                        Color.FromArgb(alpha, 207, 236, 179),
                        Color.FromArgb(alpha, 201, 233, 179),
                        Color.FromArgb(alpha, 191, 230, 180),
                        Color.FromArgb(alpha, 178, 224, 182),
                        Color.FromArgb(alpha, 166, 220, 183),
                        Color.FromArgb(alpha, 155, 216, 184),
                        Color.FromArgb(alpha, 144, 211, 185),
                        Color.FromArgb(alpha, 130, 206, 186),
                        Color.FromArgb(alpha, 120, 202, 187),
                        Color.FromArgb(alpha, 110, 198, 189),
                        Color.FromArgb(alpha, 100, 195, 190),
                        Color.FromArgb(alpha, 91, 191, 192),
                        Color.FromArgb(alpha, 79, 187, 193),
                        Color.FromArgb(alpha, 69, 183, 195),
                        Color.FromArgb(alpha, 62, 179, 195),
                        Color.FromArgb(alpha, 56, 173, 195),
                        Color.FromArgb(alpha, 49, 166, 194),
                        Color.FromArgb(alpha, 44, 160, 193),
                        Color.FromArgb(alpha, 38, 154, 193),
                        Color.FromArgb(alpha, 32, 148, 192),
                        Color.FromArgb(alpha, 29, 142, 190),
                        Color.FromArgb(alpha, 30, 132, 186),
                        Color.FromArgb(alpha, 30, 124, 182),
                        Color.FromArgb(alpha, 31, 116, 178),
                        Color.FromArgb(alpha, 32, 108, 174),
                        Color.FromArgb(alpha, 33, 99, 170),
                        Color.FromArgb(alpha, 34, 91, 166),
                        Color.FromArgb(alpha, 34, 85, 163),
                        Color.FromArgb(alpha, 35, 78, 160),
                        Color.FromArgb(alpha, 35, 71, 157),
                        Color.FromArgb(alpha, 36, 64, 153),
                        Color.FromArgb(alpha, 36, 57, 150),
                        Color.FromArgb(alpha, 36, 51, 146),
                        Color.FromArgb(alpha, 31, 47, 136),
                        Color.FromArgb(alpha, 26, 43, 125),
                        Color.FromArgb(alpha, 21, 39, 116),
                        Color.FromArgb(alpha, 17, 36, 106),
                        Color.FromArgb(alpha, 12, 32, 97),
                        Color.FromArgb(alpha, 8, 29, 88)};
                return colorRange;
            }
        }

        private static class YOrRd
        {
            public static Color[] GetColors(int alpha = 255)
            {
                Color[] colorRange = {
                        Color.FromArgb(alpha, 255, 255, 204),
                        Color.FromArgb(alpha, 255, 252, 197),
                        Color.FromArgb(alpha, 255, 249, 190),
                        Color.FromArgb(alpha, 255, 246, 183),
                        Color.FromArgb(alpha, 255, 243, 176),
                        Color.FromArgb(alpha, 255, 240, 168),
                        Color.FromArgb(alpha, 255, 237, 161),
                        Color.FromArgb(alpha, 254, 234, 154),
                        Color.FromArgb(alpha, 254, 231, 147),
                        Color.FromArgb(alpha, 254, 227, 140),
                        Color.FromArgb(alpha, 254, 224, 133),
                        Color.FromArgb(alpha, 254, 221, 126),
                        Color.FromArgb(alpha, 254, 218, 120),
                        Color.FromArgb(alpha, 254, 213, 113),
                        Color.FromArgb(alpha, 254, 205, 105),
                        Color.FromArgb(alpha, 254, 199, 99),
                        Color.FromArgb(alpha, 254, 193, 92),
                        Color.FromArgb(alpha, 254, 187, 86),
                        Color.FromArgb(alpha, 254, 179, 78),
                        Color.FromArgb(alpha, 253, 174, 74),
                        Color.FromArgb(alpha, 253, 168, 71),
                        Color.FromArgb(alpha, 253, 162, 69),
                        Color.FromArgb(alpha, 253, 156, 66),
                        Color.FromArgb(alpha, 253, 149, 63),
                        Color.FromArgb(alpha, 253, 143, 61),
                        Color.FromArgb(alpha, 252, 136, 58),
                        Color.FromArgb(alpha, 252, 126, 55),
                        Color.FromArgb(alpha, 252, 114, 52),
                        Color.FromArgb(alpha, 252, 104, 49),
                        Color.FromArgb(alpha, 252, 94, 46),
                        Color.FromArgb(alpha, 252, 84, 43),
                        Color.FromArgb(alpha, 250, 75, 41),
                        Color.FromArgb(alpha, 246, 65, 38),
                        Color.FromArgb(alpha, 242, 57, 36),
                        Color.FromArgb(alpha, 238, 49, 34),
                        Color.FromArgb(alpha, 234, 41, 32),
                        Color.FromArgb(alpha, 229, 31, 29),
                        Color.FromArgb(alpha, 224, 24, 28),
                        Color.FromArgb(alpha, 218, 20, 30),
                        Color.FromArgb(alpha, 212, 16, 31),
                        Color.FromArgb(alpha, 207, 12, 33),
                        Color.FromArgb(alpha, 199, 7, 35),
                        Color.FromArgb(alpha, 193, 3, 36),
                        Color.FromArgb(alpha, 187, 0, 38),
                        Color.FromArgb(alpha, 177, 0, 38),
                        Color.FromArgb(alpha, 166, 0, 38),
                        Color.FromArgb(alpha, 156, 0, 38),
                        Color.FromArgb(alpha, 147, 0, 38),
                        Color.FromArgb(alpha, 137, 0, 38),
                        Color.FromArgb(alpha, 128, 0, 38)};


                return colorRange;
            }
        }
        #endregion

        #region Diverging colormaps
        private static class CoolWarm
        {
            public static Color[] GetColors(int alpha = 255)
            {
                Color[] colorRange = {
                    Color.FromArgb(alpha, 58, 76, 192),
                    Color.FromArgb(alpha, 64, 84, 199),
                    Color.FromArgb(alpha, 70, 93, 207),
                    Color.FromArgb(alpha, 76, 102, 214),
                    Color.FromArgb(alpha, 82, 110, 220),
                    Color.FromArgb(alpha, 90, 120, 227),
                    Color.FromArgb(alpha, 96, 128, 232),
                    Color.FromArgb(alpha, 103, 136, 237),
                    Color.FromArgb(alpha, 109, 144, 241),
                    Color.FromArgb(alpha, 117, 152, 246),
                    Color.FromArgb(alpha, 124, 160, 249),
                    Color.FromArgb(alpha, 131, 166, 251),
                    Color.FromArgb(alpha, 138, 173, 253),
                    Color.FromArgb(alpha, 145, 179, 254),
                    Color.FromArgb(alpha, 153, 186, 254),
                    Color.FromArgb(alpha, 160, 191, 254),
                    Color.FromArgb(alpha, 167, 196, 253),
                    Color.FromArgb(alpha, 174, 201, 252),
                    Color.FromArgb(alpha, 182, 206, 249),
                    Color.FromArgb(alpha, 188, 209, 246),
                    Color.FromArgb(alpha, 194, 212, 243),
                    Color.FromArgb(alpha, 200, 215, 239),
                    Color.FromArgb(alpha, 206, 217, 235),
                    Color.FromArgb(alpha, 213, 219, 229),
                    Color.FromArgb(alpha, 218, 220, 223),
                    Color.FromArgb(alpha, 223, 219, 217),
                    Color.FromArgb(alpha, 228, 216, 209),
                    Color.FromArgb(alpha, 233, 212, 201),
                    Color.FromArgb(alpha, 237, 208, 193),
                    Color.FromArgb(alpha, 240, 204, 185),
                    Color.FromArgb(alpha, 242, 199, 178),
                    Color.FromArgb(alpha, 244, 194, 170),
                    Color.FromArgb(alpha, 246, 187, 160),
                    Color.FromArgb(alpha, 247, 181, 152),
                    Color.FromArgb(alpha, 247, 174, 145),
                    Color.FromArgb(alpha, 246, 167, 137),
                    Color.FromArgb(alpha, 245, 158, 127),
                    Color.FromArgb(alpha, 243, 150, 120),
                    Color.FromArgb(alpha, 241, 142, 112),
                    Color.FromArgb(alpha, 238, 134, 105),
                    Color.FromArgb(alpha, 234, 125, 97),
                    Color.FromArgb(alpha, 230, 114, 89),
                    Color.FromArgb(alpha, 225, 104, 82),
                    Color.FromArgb(alpha, 220, 94, 75),
                    Color.FromArgb(alpha, 215, 84, 68),
                    Color.FromArgb(alpha, 207, 70, 61),
                    Color.FromArgb(alpha, 201, 59, 55),
                    Color.FromArgb(alpha, 194, 45, 49),
                    Color.FromArgb(alpha, 187, 26, 43),
                    Color.FromArgb(alpha, 179, 3, 38),
                };

                return colorRange;
            }
        }

        private static class RdYlBu
        {
            public static Color[] GetColors(int alpha = 255)
            {
                Color[] colorRange = {
                    Color.FromArgb(alpha, 165, 0, 38),
                    Color.FromArgb(alpha, 174, 9, 38),
                    Color.FromArgb(alpha, 184, 18, 38),
                    Color.FromArgb(alpha, 194, 28, 38),
                    Color.FromArgb(alpha, 204, 37, 38),
                    Color.FromArgb(alpha, 215, 49, 39),
                    Color.FromArgb(alpha, 221, 61, 45),
                    Color.FromArgb(alpha, 226, 73, 50),
                    Color.FromArgb(alpha, 232, 85, 56),
                    Color.FromArgb(alpha, 239, 99, 62),
                    Color.FromArgb(alpha, 244, 111, 68),
                    Color.FromArgb(alpha, 246, 124, 74),
                    Color.FromArgb(alpha, 247, 137, 79),
                    Color.FromArgb(alpha, 249, 149, 85),
                    Color.FromArgb(alpha, 251, 165, 92),
                    Color.FromArgb(alpha, 253, 176, 99),
                    Color.FromArgb(alpha, 253, 186, 108),
                    Color.FromArgb(alpha, 253, 196, 118),
                    Color.FromArgb(alpha, 253, 208, 129),
                    Color.FromArgb(alpha, 253, 218, 138),
                    Color.FromArgb(alpha, 254, 226, 147),
                    Color.FromArgb(alpha, 254, 232, 156),
                    Color.FromArgb(alpha, 254, 238, 166),
                    Color.FromArgb(alpha, 254, 245, 177),
                    Color.FromArgb(alpha, 254, 251, 186),
                    Color.FromArgb(alpha, 251, 253, 196),
                    Color.FromArgb(alpha, 245, 251, 207),
                    Color.FromArgb(alpha, 238, 248, 221),
                    Color.FromArgb(alpha, 232, 246, 232),
                    Color.FromArgb(alpha, 226, 243, 243),
                    Color.FromArgb(alpha, 217, 239, 246),
                    Color.FromArgb(alpha, 207, 234, 243),
                    Color.FromArgb(alpha, 194, 228, 239),
                    Color.FromArgb(alpha, 184, 223, 236),
                    Color.FromArgb(alpha, 174, 218, 233),
                    Color.FromArgb(alpha, 163, 210, 229),
                    Color.FromArgb(alpha, 150, 200, 224),
                    Color.FromArgb(alpha, 139, 191, 219),
                    Color.FromArgb(alpha, 128, 183, 214),
                    Color.FromArgb(alpha, 118, 174, 209),
                    Color.FromArgb(alpha, 108, 164, 204),
                    Color.FromArgb(alpha, 97, 151, 197),
                    Color.FromArgb(alpha, 88, 140, 191),
                    Color.FromArgb(alpha, 79, 129, 186),
                    Color.FromArgb(alpha, 69, 118, 180),
                    Color.FromArgb(alpha, 64, 103, 173),
                    Color.FromArgb(alpha, 60, 91, 167),
                    Color.FromArgb(alpha, 56, 78, 161),
                    Color.FromArgb(alpha, 52, 66, 155),
                    Color.FromArgb(alpha, 49, 54, 149)};


                return colorRange;
            }
        }

        private static class RdGy
        {
            public static Color[] GetColors(int alpha = 255)
            {
                Color[] colorRange = {
                    Color.FromArgb(alpha, 103, 0, 31),
                    Color.FromArgb(alpha, 117, 4, 33),
                    Color.FromArgb(alpha, 132, 9, 35),
                    Color.FromArgb(alpha, 147, 14, 38),
                    Color.FromArgb(alpha, 161, 18, 40),
                    Color.FromArgb(alpha, 178, 25, 43),
                    Color.FromArgb(alpha, 185, 39, 50),
                    Color.FromArgb(alpha, 192, 53, 56),
                    Color.FromArgb(alpha, 199, 67, 63),
                    Color.FromArgb(alpha, 208, 84, 71),
                    Color.FromArgb(alpha, 215, 98, 79),
                    Color.FromArgb(alpha, 221, 112, 89),
                    Color.FromArgb(alpha, 226, 125, 99),
                    Color.FromArgb(alpha, 232, 139, 110),
                    Color.FromArgb(alpha, 239, 155, 122),
                    Color.FromArgb(alpha, 244, 168, 134),
                    Color.FromArgb(alpha, 246, 178, 147),
                    Color.FromArgb(alpha, 248, 189, 161),
                    Color.FromArgb(alpha, 250, 202, 177),
                    Color.FromArgb(alpha, 251, 212, 190),
                    Color.FromArgb(alpha, 253, 221, 203),
                    Color.FromArgb(alpha, 253, 228, 214),
                    Color.FromArgb(alpha, 253, 235, 225),
                    Color.FromArgb(alpha, 254, 244, 238),
                    Color.FromArgb(alpha, 254, 251, 249),
                    Color.FromArgb(alpha, 251, 251, 251),
                    Color.FromArgb(alpha, 245, 245, 245),
                    Color.FromArgb(alpha, 238, 238, 238),
                    Color.FromArgb(alpha, 232, 232, 232),
                    Color.FromArgb(alpha, 226, 226, 226),
                    Color.FromArgb(alpha, 219, 219, 219),
                    Color.FromArgb(alpha, 212, 212, 212),
                    Color.FromArgb(alpha, 203, 203, 203),
                    Color.FromArgb(alpha, 195, 195, 195),
                    Color.FromArgb(alpha, 188, 188, 188),
                    Color.FromArgb(alpha, 179, 179, 179),
                    Color.FromArgb(alpha, 167, 167, 167),
                    Color.FromArgb(alpha, 157, 157, 157),
                    Color.FromArgb(alpha, 147, 147, 147),
                    Color.FromArgb(alpha, 137, 137, 137),
                    Color.FromArgb(alpha, 125, 125, 125),
                    Color.FromArgb(alpha, 112, 112, 112),
                    Color.FromArgb(alpha, 100, 100, 100),
                    Color.FromArgb(alpha, 89, 89, 89),
                    Color.FromArgb(alpha, 78, 78, 78),
                    Color.FromArgb(alpha, 65, 65, 65),
                    Color.FromArgb(alpha, 56, 56, 56),
                    Color.FromArgb(alpha, 46, 46, 46),
                    Color.FromArgb(alpha, 36, 36, 36),
                    Color.FromArgb(alpha, 26, 26, 26)};




                return colorRange;
            }
        }

        private static class RdBu
        {
            public static Color[] GetColors(int alpha = 255)
            {
                Color[] colorRange = {
                    Color.FromArgb(alpha, 103, 0, 31),
                    Color.FromArgb(alpha, 117, 4, 33),
                    Color.FromArgb(alpha, 132, 9, 35),
                    Color.FromArgb(alpha, 147, 14, 38),
                    Color.FromArgb(alpha, 161, 18, 40),
                    Color.FromArgb(alpha, 178, 25, 43),
                    Color.FromArgb(alpha, 185, 39, 50),
                    Color.FromArgb(alpha, 192, 53, 56),
                    Color.FromArgb(alpha, 199, 67, 63),
                    Color.FromArgb(alpha, 208, 84, 71),
                    Color.FromArgb(alpha, 215, 98, 79),
                    Color.FromArgb(alpha, 221, 112, 89),
                    Color.FromArgb(alpha, 226, 125, 99),
                    Color.FromArgb(alpha, 232, 139, 110),
                    Color.FromArgb(alpha, 239, 155, 122),
                    Color.FromArgb(alpha, 244, 168, 134),
                    Color.FromArgb(alpha, 246, 178, 147),
                    Color.FromArgb(alpha, 248, 189, 161),
                    Color.FromArgb(alpha, 250, 202, 177),
                    Color.FromArgb(alpha, 251, 212, 190),
                    Color.FromArgb(alpha, 252, 221, 202),
                    Color.FromArgb(alpha, 251, 226, 212),
                    Color.FromArgb(alpha, 250, 232, 221),
                    Color.FromArgb(alpha, 248, 238, 232),
                    Color.FromArgb(alpha, 247, 244, 242),
                    Color.FromArgb(alpha, 243, 245, 246),
                    Color.FromArgb(alpha, 235, 241, 244),
                    Color.FromArgb(alpha, 226, 237, 243),
                    Color.FromArgb(alpha, 219, 233, 241),
                    Color.FromArgb(alpha, 211, 230, 240),
                    Color.FromArgb(alpha, 201, 225, 237),
                    Color.FromArgb(alpha, 189, 218, 234),
                    Color.FromArgb(alpha, 174, 211, 230),
                    Color.FromArgb(alpha, 162, 205, 226),
                    Color.FromArgb(alpha, 149, 198, 223),
                    Color.FromArgb(alpha, 135, 190, 218),
                    Color.FromArgb(alpha, 116, 178, 211),
                    Color.FromArgb(alpha, 101, 168, 206),
                    Color.FromArgb(alpha, 85, 158, 201),
                    Color.FromArgb(alpha, 70, 148, 196),
                    Color.FromArgb(alpha, 61, 139, 191),
                    Color.FromArgb(alpha, 53, 129, 185),
                    Color.FromArgb(alpha, 47, 120, 181),
                    Color.FromArgb(alpha, 40, 111, 176),
                    Color.FromArgb(alpha, 33, 102, 172),
                    Color.FromArgb(alpha, 26, 90, 155),
                    Color.FromArgb(alpha, 21, 79, 141),
                    Color.FromArgb(alpha, 15, 69, 126),
                    Color.FromArgb(alpha, 10, 58, 111),
                    Color.FromArgb(alpha, 5, 48, 97)};

                return colorRange;
            }
        }
        #endregion

        #region Sequential 2-colormaps
        private static class Cool
        {
            public static Color[] GetColors(int alpha = 255)
            {
                Color[] colorRange = {
                    Color.FromArgb(alpha, 0, 255, 255),
                    Color.FromArgb(alpha, 5, 250, 255),
                    Color.FromArgb(alpha, 10, 245, 255),
                    Color.FromArgb(alpha, 15, 240, 255),
                    Color.FromArgb(alpha, 20, 235, 255),
                    Color.FromArgb(alpha, 26, 229, 255),
                    Color.FromArgb(alpha, 31, 224, 255),
                    Color.FromArgb(alpha, 36, 219, 255),
                    Color.FromArgb(alpha, 40, 214, 255),
                    Color.FromArgb(alpha, 47, 208, 255),
                    Color.FromArgb(alpha, 52, 203, 255),
                    Color.FromArgb(alpha, 56, 198, 255),
                    Color.FromArgb(alpha, 62, 193, 255),
                    Color.FromArgb(alpha, 67, 188, 255),
                    Color.FromArgb(alpha, 73, 182, 255),
                    Color.FromArgb(alpha, 78, 177, 255),
                    Color.FromArgb(alpha, 83, 172, 255),
                    Color.FromArgb(alpha, 88, 167, 255),
                    Color.FromArgb(alpha, 94, 161, 255),
                    Color.FromArgb(alpha, 99, 156, 255),
                    Color.FromArgb(alpha, 104, 151, 255),
                    Color.FromArgb(alpha, 109, 146, 255),
                    Color.FromArgb(alpha, 113, 141, 255),
                    Color.FromArgb(alpha, 120, 135, 255),
                    Color.FromArgb(alpha, 125, 130, 255),
                    Color.FromArgb(alpha, 130, 125, 255),
                    Color.FromArgb(alpha, 135, 120, 255),
                    Color.FromArgb(alpha, 141, 113, 255),
                    Color.FromArgb(alpha, 146, 109, 255),
                    Color.FromArgb(alpha, 151, 104, 255),
                    Color.FromArgb(alpha, 156, 98, 255),
                    Color.FromArgb(alpha, 161, 94, 255),
                    Color.FromArgb(alpha, 167, 88, 255),
                    Color.FromArgb(alpha, 172, 82, 255),
                    Color.FromArgb(alpha, 177, 78, 255),
                    Color.FromArgb(alpha, 182, 73, 255),
                    Color.FromArgb(alpha, 188, 66, 255),
                    Color.FromArgb(alpha, 193, 62, 255),
                    Color.FromArgb(alpha, 198, 56, 255),
                    Color.FromArgb(alpha, 203, 52, 255),
                    Color.FromArgb(alpha, 208, 47, 255),
                    Color.FromArgb(alpha, 214, 40, 255),
                    Color.FromArgb(alpha, 219, 36, 255),
                    Color.FromArgb(alpha, 224, 31, 255),
                    Color.FromArgb(alpha, 229, 25, 255),
                    Color.FromArgb(alpha, 235, 20, 255),
                    Color.FromArgb(alpha, 240, 15, 255),
                    Color.FromArgb(alpha, 245, 9, 255),
                    Color.FromArgb(alpha, 250, 5, 255),
                    Color.FromArgb(alpha, 255, 0, 255) };
                return colorRange;

            }
        }

        private static class Hot
        {
            public static Color[] GetColors(int alpha = 255)
            {
                Color[] colorRange = {
                    Color.FromArgb(alpha, 10, 0, 0),
                    Color.FromArgb(alpha, 23, 0, 0),
                    Color.FromArgb(alpha, 36, 0, 0),
                    Color.FromArgb(alpha, 49, 0, 0),
                    Color.FromArgb(alpha, 63, 0, 0),
                    Color.FromArgb(alpha, 78, 0, 0),
                    Color.FromArgb(alpha, 91, 0, 0),
                    Color.FromArgb(alpha, 105, 0, 0),
                    Color.FromArgb(alpha, 118, 0, 0),
                    Color.FromArgb(alpha, 133, 0, 0),
                    Color.FromArgb(alpha, 147, 0, 0),
                    Color.FromArgb(alpha, 160, 0, 0),
                    Color.FromArgb(alpha, 173, 0, 0),
                    Color.FromArgb(alpha, 186, 0, 0),
                    Color.FromArgb(alpha, 202, 0, 0),
                    Color.FromArgb(alpha, 215, 0, 0),
                    Color.FromArgb(alpha, 228, 0, 0),
                    Color.FromArgb(alpha, 241, 0, 0),
                    Color.FromArgb(alpha, 255, 2, 0),
                    Color.FromArgb(alpha, 255, 15, 0),
                    Color.FromArgb(alpha, 255, 28, 0),
                    Color.FromArgb(alpha, 255, 41, 0),
                    Color.FromArgb(alpha, 255, 54, 0),
                    Color.FromArgb(alpha, 255, 70, 0),
                    Color.FromArgb(alpha, 255, 83, 0),
                    Color.FromArgb(alpha, 255, 96, 0),
                    Color.FromArgb(alpha, 255, 110, 0),
                    Color.FromArgb(alpha, 255, 125, 0),
                    Color.FromArgb(alpha, 255, 138, 0),
                    Color.FromArgb(alpha, 255, 151, 0),
                    Color.FromArgb(alpha, 255, 165, 0),
                    Color.FromArgb(alpha, 255, 178, 0),
                    Color.FromArgb(alpha, 255, 193, 0),
                    Color.FromArgb(alpha, 255, 207, 0),
                    Color.FromArgb(alpha, 255, 220, 0),
                    Color.FromArgb(alpha, 255, 233, 0),
                    Color.FromArgb(alpha, 255, 249, 0),
                    Color.FromArgb(alpha, 255, 255, 10),
                    Color.FromArgb(alpha, 255, 255, 30),
                    Color.FromArgb(alpha, 255, 255, 50),
                    Color.FromArgb(alpha, 255, 255, 69),
                    Color.FromArgb(alpha, 255, 255, 93),
                    Color.FromArgb(alpha, 255, 255, 113),
                    Color.FromArgb(alpha, 255, 255, 132),
                    Color.FromArgb(alpha, 255, 255, 152),
                    Color.FromArgb(alpha, 255, 255, 176),
                    Color.FromArgb(alpha, 255, 255, 195),
                    Color.FromArgb(alpha, 255, 255, 215),
                    Color.FromArgb(alpha, 255, 255, 235),
                    Color.FromArgb(alpha, 255, 255, 255)};

                return colorRange;

            }
        }

        private static class Bone
        {
            public static Color[] GetColors(int alpha = 255)
            {
                Color[] colorRange = {
                    Color.FromArgb(alpha, 0, 0, 0),
                    Color.FromArgb(alpha, 4, 4, 6),
                    Color.FromArgb(alpha, 8, 8, 12),
                    Color.FromArgb(alpha, 13, 13, 18),
                    Color.FromArgb(alpha, 17, 17, 24),
                    Color.FromArgb(alpha, 22, 22, 31),
                    Color.FromArgb(alpha, 27, 27, 37),
                    Color.FromArgb(alpha, 31, 31, 43),
                    Color.FromArgb(alpha, 35, 35, 49),
                    Color.FromArgb(alpha, 41, 41, 57),
                    Color.FromArgb(alpha, 45, 45, 63),
                    Color.FromArgb(alpha, 49, 49, 69),
                    Color.FromArgb(alpha, 54, 54, 75),
                    Color.FromArgb(alpha, 58, 58, 81),
                    Color.FromArgb(alpha, 63, 63, 88),
                    Color.FromArgb(alpha, 68, 68, 94),
                    Color.FromArgb(alpha, 72, 72, 101),
                    Color.FromArgb(alpha, 77, 76, 107),
                    Color.FromArgb(alpha, 82, 82, 114),
                    Color.FromArgb(alpha, 86, 88, 118),
                    Color.FromArgb(alpha, 91, 94, 122),
                    Color.FromArgb(alpha, 95, 100, 127),
                    Color.FromArgb(alpha, 99, 106, 131),
                    Color.FromArgb(alpha, 105, 113, 136),
                    Color.FromArgb(alpha, 109, 119, 141),
                    Color.FromArgb(alpha, 113, 125, 145),
                    Color.FromArgb(alpha, 118, 131, 149),
                    Color.FromArgb(alpha, 123, 139, 155),
                    Color.FromArgb(alpha, 127, 145, 159),
                    Color.FromArgb(alpha, 132, 151, 163),
                    Color.FromArgb(alpha, 136, 157, 168),
                    Color.FromArgb(alpha, 140, 163, 172),
                    Color.FromArgb(alpha, 146, 170, 177),
                    Color.FromArgb(alpha, 150, 176, 182),
                    Color.FromArgb(alpha, 154, 182, 186),
                    Color.FromArgb(alpha, 159, 188, 191),
                    Color.FromArgb(alpha, 164, 195, 196),
                    Color.FromArgb(alpha, 170, 200, 200),
                    Color.FromArgb(alpha, 177, 205, 205),
                    Color.FromArgb(alpha, 183, 209, 209),
                    Color.FromArgb(alpha, 190, 213, 213),
                    Color.FromArgb(alpha, 198, 219, 219),
                    Color.FromArgb(alpha, 205, 223, 223),
                    Color.FromArgb(alpha, 212, 227, 227),
                    Color.FromArgb(alpha, 219, 232, 232),
                    Color.FromArgb(alpha, 227, 237, 237),
                    Color.FromArgb(alpha, 234, 241, 241),
                    Color.FromArgb(alpha, 241, 246, 246),
                    Color.FromArgb(alpha, 248, 250, 250),
                    Color.FromArgb(alpha, 255, 255, 255)};


                return colorRange;

            }
        }

        private static class Binary
        {
            public static Color[] GetColors(int alpha = 255)
            {
                Color[] colorRange = {
                    Color.FromArgb(alpha, 255, 255, 255),
                    Color.FromArgb(alpha, 250, 250, 250),
                    Color.FromArgb(alpha, 245, 245, 245),
                    Color.FromArgb(alpha, 240, 240, 240),
                    Color.FromArgb(alpha, 235, 235, 235),
                    Color.FromArgb(alpha, 229, 229, 229),
                    Color.FromArgb(alpha, 224, 224, 224),
                    Color.FromArgb(alpha, 219, 219, 219),
                    Color.FromArgb(alpha, 214, 214, 214),
                    Color.FromArgb(alpha, 208, 208, 208),
                    Color.FromArgb(alpha, 203, 203, 203),
                    Color.FromArgb(alpha, 198, 198, 198),
                    Color.FromArgb(alpha, 193, 193, 193),
                    Color.FromArgb(alpha, 188, 188, 188),
                    Color.FromArgb(alpha, 182, 182, 182),
                    Color.FromArgb(alpha, 177, 177, 177),
                    Color.FromArgb(alpha, 172, 172, 172),
                    Color.FromArgb(alpha, 167, 167, 167),
                    Color.FromArgb(alpha, 161, 161, 161),
                    Color.FromArgb(alpha, 156, 156, 156),
                    Color.FromArgb(alpha, 151, 151, 151),
                    Color.FromArgb(alpha, 146, 146, 146),
                    Color.FromArgb(alpha, 141, 141, 141),
                    Color.FromArgb(alpha, 135, 135, 135),
                    Color.FromArgb(alpha, 130, 130, 130),
                    Color.FromArgb(alpha, 125, 125, 125),
                    Color.FromArgb(alpha, 120, 120, 120),
                    Color.FromArgb(alpha, 113, 113, 113),
                    Color.FromArgb(alpha, 109, 109, 109),
                    Color.FromArgb(alpha, 104, 104, 104),
                    Color.FromArgb(alpha, 98, 98, 98),
                    Color.FromArgb(alpha, 94, 94, 94),
                    Color.FromArgb(alpha, 88, 88, 88),
                    Color.FromArgb(alpha, 82, 82, 82),
                    Color.FromArgb(alpha, 78, 78, 78),
                    Color.FromArgb(alpha, 73, 73, 73),
                    Color.FromArgb(alpha, 66, 66, 66),
                    Color.FromArgb(alpha, 62, 62, 62),
                    Color.FromArgb(alpha, 56, 56, 56),
                    Color.FromArgb(alpha, 52, 52, 52),
                    Color.FromArgb(alpha, 47, 47, 47),
                    Color.FromArgb(alpha, 40, 40, 40),
                    Color.FromArgb(alpha, 36, 36, 36),
                    Color.FromArgb(alpha, 31, 31, 31),
                    Color.FromArgb(alpha, 25, 25, 25),
                    Color.FromArgb(alpha, 20, 20, 20),
                    Color.FromArgb(alpha, 15, 15, 15),
                    Color.FromArgb(alpha, 9, 9, 9),
                    Color.FromArgb(alpha, 5, 5, 5),
                    Color.FromArgb(alpha, 0, 0, 0)};

                return colorRange;

            }
        }
        #endregion

        #region Miscellaneous colormaps
        private static class Turbo
        {
            public static Color[] GetColors(int alpha = 255)
            {
                Color[] colorRange = {
                        Color.FromArgb(alpha, 48, 18, 59),
                        Color.FromArgb(alpha, 54, 33, 95),
                        Color.FromArgb(alpha, 59, 47, 127),
                        Color.FromArgb(alpha, 63, 61, 156),
                        Color.FromArgb(alpha, 66, 75, 181),
                        Color.FromArgb(alpha, 69, 91, 206),
                        Color.FromArgb(alpha, 70, 104, 224),
                        Color.FromArgb(alpha, 70, 117, 237),
                        Color.FromArgb(alpha, 70, 130, 248),
                        Color.FromArgb(alpha, 66, 145, 254),
                        Color.FromArgb(alpha, 60, 157, 253),
                        Color.FromArgb(alpha, 52, 170, 248),
                        Color.FromArgb(alpha, 43, 182, 239),
                        Color.FromArgb(alpha, 35, 194, 228),
                        Color.FromArgb(alpha, 27, 207, 212),
                        Color.FromArgb(alpha, 23, 217, 199),
                        Color.FromArgb(alpha, 24, 225, 186),
                        Color.FromArgb(alpha, 30, 232, 175),
                        Color.FromArgb(alpha, 44, 239, 157),
                        Color.FromArgb(alpha, 59, 244, 141),
                        Color.FromArgb(alpha, 77, 249, 124),
                        Color.FromArgb(alpha, 97, 252, 108),
                        Color.FromArgb(alpha, 116, 254, 92),
                        Color.FromArgb(alpha, 139, 254, 75),
                        Color.FromArgb(alpha, 155, 253, 64),
                        Color.FromArgb(alpha, 169, 251, 57),
                        Color.FromArgb(alpha, 182, 247, 53),
                        Color.FromArgb(alpha, 197, 239, 51),
                        Color.FromArgb(alpha, 209, 232, 52),
                        Color.FromArgb(alpha, 221, 224, 54),
                        Color.FromArgb(alpha, 231, 215, 56),
                        Color.FromArgb(alpha, 239, 205, 57),
                        Color.FromArgb(alpha, 247, 192, 57),
                        Color.FromArgb(alpha, 251, 181, 55),
                        Color.FromArgb(alpha, 253, 169, 50),
                        Color.FromArgb(alpha, 254, 155, 45),
                        Color.FromArgb(alpha, 252, 137, 38),
                        Color.FromArgb(alpha, 250, 122, 31),
                        Color.FromArgb(alpha, 246, 107, 24),
                        Color.FromArgb(alpha, 241, 93, 19),
                        Color.FromArgb(alpha, 234, 80, 13),
                        Color.FromArgb(alpha, 226, 66, 9),
                        Color.FromArgb(alpha, 217, 56, 6),
                        Color.FromArgb(alpha, 208, 47, 4),
                        Color.FromArgb(alpha, 197, 38, 2),
                        Color.FromArgb(alpha, 182, 28, 1),
                        Color.FromArgb(alpha, 169, 21, 1),
                        Color.FromArgb(alpha, 154, 14, 1),
                        Color.FromArgb(alpha, 139, 9, 1),
                        Color.FromArgb(alpha, 122, 4, 2)};
                return colorRange;
            }
        }
        #endregion

        #region cyclic
        private static class Twilight
        {
            public static Color[] GetColors(int alpha = 255)
            {
                Color[] colorRange = {
                        Color.FromArgb(alpha, 225, 216, 226),
                        Color.FromArgb(alpha, 219, 216, 223),
                        Color.FromArgb(alpha, 210, 213, 218),
                        Color.FromArgb(alpha, 195, 206, 212),
                        Color.FromArgb(alpha, 180, 199, 206),
                        Color.FromArgb(alpha, 164, 190, 202),
                        Color.FromArgb(alpha, 150, 181, 198),
                        Color.FromArgb(alpha, 137, 172, 196),
                        Color.FromArgb(alpha, 125, 162, 194),
                        Color.FromArgb(alpha, 115, 153, 193),
                        Color.FromArgb(alpha, 107, 142, 191),
                        Color.FromArgb(alpha, 102, 131, 189),
                        Color.FromArgb(alpha, 98, 120, 187),
                        Color.FromArgb(alpha, 96, 108, 183),
                        Color.FromArgb(alpha, 95, 97, 180),
                        Color.FromArgb(alpha, 94, 84, 174),
                        Color.FromArgb(alpha, 93, 72, 167),
                        Color.FromArgb(alpha, 92, 60, 159),
                        Color.FromArgb(alpha, 90, 46, 148),
                        Color.FromArgb(alpha, 87, 35, 134),
                        Color.FromArgb(alpha, 80, 26, 117),
                        Color.FromArgb(alpha, 73, 21, 100),
                        Color.FromArgb(alpha, 64, 17, 84),
                        Color.FromArgb(alpha, 56, 16, 69),
                        Color.FromArgb(alpha, 50, 17, 59),
                        Color.FromArgb(alpha, 50, 18, 55),
                        Color.FromArgb(alpha, 57, 17, 57),
                        Color.FromArgb(alpha, 68, 18, 63),
                        Color.FromArgb(alpha, 80, 20, 68),
                        Color.FromArgb(alpha, 93, 23, 73),
                        Color.FromArgb(alpha, 107, 26, 77),
                        Color.FromArgb(alpha, 120, 31, 79),
                        Color.FromArgb(alpha, 134, 38, 80),
                        Color.FromArgb(alpha, 145, 46, 80),
                        Color.FromArgb(alpha, 154, 55, 79),
                        Color.FromArgb(alpha, 164, 66, 79),
                        Color.FromArgb(alpha, 172, 77, 80),
                        Color.FromArgb(alpha, 179, 89, 82),
                        Color.FromArgb(alpha, 185, 101, 86),
                        Color.FromArgb(alpha, 190, 113, 91),
                        Color.FromArgb(alpha, 195, 126, 100),
                        Color.FromArgb(alpha, 198, 139, 110),
                        Color.FromArgb(alpha, 201, 152, 123),
                        Color.FromArgb(alpha, 204, 164, 138),
                        Color.FromArgb(alpha, 207, 176, 154),
                        Color.FromArgb(alpha, 211, 188, 173),
                        Color.FromArgb(alpha, 216, 198, 190),
                        Color.FromArgb(alpha, 221, 208, 207),
                        Color.FromArgb(alpha, 224, 214, 218),
                        Color.FromArgb(alpha, 225, 216, 225)};


                return colorRange;
            }
        }

        private static class TwilightShifted
        {
            public static Color[] GetColors(int alpha = 255)
            {
                Color[] colorRange = {
                        Color.FromArgb(alpha, 47, 19, 55),
                        Color.FromArgb(alpha, 52, 16, 63),
                        Color.FromArgb(alpha, 59, 17, 75),
                        Color.FromArgb(alpha, 68, 19, 91),
                        Color.FromArgb(alpha, 77, 23, 108),
                        Color.FromArgb(alpha, 84, 31, 127),
                        Color.FromArgb(alpha, 89, 41, 141),
                        Color.FromArgb(alpha, 91, 52, 153),
                        Color.FromArgb(alpha, 93, 66, 164),
                        Color.FromArgb(alpha, 94, 78, 171),
                        Color.FromArgb(alpha, 94, 91, 177),
                        Color.FromArgb(alpha, 95, 103, 182),
                        Color.FromArgb(alpha, 97, 114, 185),
                        Color.FromArgb(alpha, 100, 126, 188),
                        Color.FromArgb(alpha, 104, 136, 190),
                        Color.FromArgb(alpha, 111, 148, 192),
                        Color.FromArgb(alpha, 120, 157, 193),
                        Color.FromArgb(alpha, 130, 167, 195),
                        Color.FromArgb(alpha, 143, 177, 197),
                        Color.FromArgb(alpha, 156, 186, 200),
                        Color.FromArgb(alpha, 173, 195, 204),
                        Color.FromArgb(alpha, 188, 202, 209),
                        Color.FromArgb(alpha, 202, 209, 215),
                        Color.FromArgb(alpha, 215, 215, 221),
                        Color.FromArgb(alpha, 223, 217, 225),
                        Color.FromArgb(alpha, 225, 216, 222),
                        Color.FromArgb(alpha, 222, 211, 213),
                        Color.FromArgb(alpha, 218, 203, 198),
                        Color.FromArgb(alpha, 214, 193, 181),
                        Color.FromArgb(alpha, 209, 183, 164),
                        Color.FromArgb(alpha, 205, 170, 146),
                        Color.FromArgb(alpha, 202, 158, 131),
                        Color.FromArgb(alpha, 199, 145, 116),
                        Color.FromArgb(alpha, 196, 132, 105),
                        Color.FromArgb(alpha, 193, 120, 96),
                        Color.FromArgb(alpha, 188, 107, 89),
                        Color.FromArgb(alpha, 182, 95, 84),
                        Color.FromArgb(alpha, 175, 82, 81),
                        Color.FromArgb(alpha, 168, 71, 80),
                        Color.FromArgb(alpha, 160, 61, 79),
                        Color.FromArgb(alpha, 150, 50, 79),
                        Color.FromArgb(alpha, 139, 42, 80),
                        Color.FromArgb(alpha, 127, 34, 80),
                        Color.FromArgb(alpha, 114, 29, 78),
                        Color.FromArgb(alpha, 101, 25, 75),
                        Color.FromArgb(alpha, 86, 21, 70),
                        Color.FromArgb(alpha, 74, 19, 65),
                        Color.FromArgb(alpha, 62, 17, 60),
                        Color.FromArgb(alpha, 53, 17, 56),
                        Color.FromArgb(alpha, 47, 20, 54)};

                return colorRange;
            }
        }

        private static class HSV
        {
            public static Color[] GetColors(int alpha = 255)
            {
                Color[] colorRange = {
                        Color.FromArgb(alpha, 255, 0, 0),
                        Color.FromArgb(alpha, 255, 29, 0),
                        Color.FromArgb(alpha, 255, 59, 0),
                        Color.FromArgb(alpha, 255, 88, 0),
                        Color.FromArgb(alpha, 255, 118, 0),
                        Color.FromArgb(alpha, 255, 153, 0),
                        Color.FromArgb(alpha, 255, 183, 0),
                        Color.FromArgb(alpha, 255, 212, 0),
                        Color.FromArgb(alpha, 253, 241, 0),
                        Color.FromArgb(alpha, 232, 255, 0),
                        Color.FromArgb(alpha, 202, 255, 0),
                        Color.FromArgb(alpha, 173, 255, 0),
                        Color.FromArgb(alpha, 143, 255, 0),
                        Color.FromArgb(alpha, 114, 255, 0),
                        Color.FromArgb(alpha, 78, 255, 0),
                        Color.FromArgb(alpha, 49, 255, 0),
                        Color.FromArgb(alpha, 19, 255, 0),
                        Color.FromArgb(alpha, 2, 255, 11),
                        Color.FromArgb(alpha, 0, 255, 45),
                        Color.FromArgb(alpha, 0, 255, 74),
                        Color.FromArgb(alpha, 0, 255, 104),
                        Color.FromArgb(alpha, 0, 255, 133),
                        Color.FromArgb(alpha, 0, 255, 163),
                        Color.FromArgb(alpha, 0, 255, 198),
                        Color.FromArgb(alpha, 0, 255, 228),
                        Color.FromArgb(alpha, 0, 252, 255),
                        Color.FromArgb(alpha, 0, 222, 255),
                        Color.FromArgb(alpha, 0, 187, 255),
                        Color.FromArgb(alpha, 0, 157, 255),
                        Color.FromArgb(alpha, 0, 128, 255),
                        Color.FromArgb(alpha, 0, 98, 255),
                        Color.FromArgb(alpha, 0, 69, 255),
                        Color.FromArgb(alpha, 0, 33, 255),
                        Color.FromArgb(alpha, 3, 8, 255),
                        Color.FromArgb(alpha, 25, 0, 255),
                        Color.FromArgb(alpha, 54, 0, 255),
                        Color.FromArgb(alpha, 90, 0, 255),
                        Color.FromArgb(alpha, 119, 0, 255),
                        Color.FromArgb(alpha, 149, 0, 255),
                        Color.FromArgb(alpha, 178, 0, 255),
                        Color.FromArgb(alpha, 208, 0, 255),
                        Color.FromArgb(alpha, 243, 0, 255),
                        Color.FromArgb(alpha, 255, 0, 236),
                        Color.FromArgb(alpha, 255, 0, 207),
                        Color.FromArgb(alpha, 255, 0, 177),
                        Color.FromArgb(alpha, 255, 0, 142),
                        Color.FromArgb(alpha, 255, 0, 112),
                        Color.FromArgb(alpha, 255, 0, 82),
                        Color.FromArgb(alpha, 255, 0, 53),
                        Color.FromArgb(alpha, 255, 0, 23)};

                return colorRange;
            }
        }
        #endregion
    }
}

