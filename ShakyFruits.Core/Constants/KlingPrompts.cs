namespace ShakyFruits.Core.Constants
{
    public static class KlingPrompts
    {
        // Metinleri buraya sabitliyoruz. Yarın promptu değiştirmek istersen sadece burayı elleyeceksin!
        public const string SINGLE_FRUIT_FIX = "Cinematic 4K 3D video of the single exact fruit character from the reference image, dancing to the exact motions in the reference video. Preserve the original fruit texture and character design.";

        public const string MULTIPLE_FRUITS_FIX = "Cinematic 4K 3D video of the multiple fruit characters from the reference image, dancing to the exact motions in the reference video. Preserve the shapes of all fruits and their relative positions.";

        // Dışarıdan bool değerini alıp doğru metni dönen merkezi metot
        public static string GetFixPrompt(bool isMultipleFruits)
        {
            return isMultipleFruits ? MULTIPLE_FRUITS_FIX : SINGLE_FRUIT_FIX;
        }
    }
}