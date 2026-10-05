using Alexandria.CharacterAPI;
using UnityEngine;

namespace IsaacMode.Character
{
    /// <summary>Registers Isaac as a Gungeoneer through Alexandria's CharacterAPI (CHR-1, CHR-2).</summary>
    public static class IsaacCharacter
    {
        /// <summary>Embedded resource folder holding characterdata.txt and the sprite folders.</summary>
        private const string ResourcePath = "IsaacMode/Resources/Characters/Isaac";

        /// <summary>Where the select stand is placed in the Breach.</summary>
        private static readonly Vector3 FoyerPosition = new Vector3(12.3f, 21.3f);

        /// <summary>Internal prefab name Alexandria derives from "name short" in characterdata.txt.</summary>
        private const string StoredName = "PlayerIsaac";

        public static CustomCharacterData Data { get; private set; }

        /// <summary>Isaac's <see cref="PlayableCharacters"/> value; only meaningful once <see cref="Init"/> succeeded.</summary>
        public static PlayableCharacters Identity { get; private set; }

        public static bool Registered
        {
            get { return Data != null; }
        }

        public static void Init()
        {
            Data = Loader.BuildCharacter(ResourcePath, Plugin.GUID, FoyerPosition, false, Vector3.zero);
            if (Data == null) return;

            Identity = Data.identity;
            Loader.SetupCustomBreachAnimation(StoredName, "select_idle", 6, tk2dSpriteAnimationClip.WrapMode.Loop);
            Loader.SetupCustomBreachAnimation(StoredName, "select_choose", 6, tk2dSpriteAnimationClip.WrapMode.Once);
        }

        public static bool IsIsaac(this PlayerController player)
        {
            return Registered && player != null && player.characterIdentity == Identity;
        }
    }
}
