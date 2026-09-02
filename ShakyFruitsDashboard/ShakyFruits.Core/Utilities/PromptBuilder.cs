using System.Collections.Generic;
using ShakyFruits.Core.Enums;

namespace ShakyFruits.Core.Utilities
{
    public static class PromptBuilder
    {
        public static string BuildSocialMediaPrompt(List<string> fruits, string danceStyle, SocialPlatform platform)
        {
            var platformName = platform.ToString();
            var fruitNames = string.Join(" and ", fruits);

            return $@"I made {fruitNames} do the {danceStyle} dance.
Write a viral caption for this video for {platformName}.

CRITICAL RULES:
1. The entire response (caption and hashtags) MUST be strictly in English.
2. Keep it very short and punchy (maximum 2 sentences).
3. Use 2 or 3 relevant emojis.
4. End with a short question to encourage comments (Call-To-Action).
5. Provide EXACTLY 5 hashtags at the very end. Do not exceed 5 hashtags.
6. Return ONLY the final text ready to copy-paste. No intro, no outro, no explanations.";
        }
    }
}