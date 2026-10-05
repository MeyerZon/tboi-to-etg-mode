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
            if (Data == null)
            {
                // Alexandria leaves its half-built prefab active in the scene when it fails; as a live
                // PlayerController it throws every frame and drags the camera away, so remove it.
                GameObject stray = GameObject.Find(StoredName);
                if (stray != null) Object.Destroy(stray);
                ETGModConsole.Log("Isaac Mode: Isaac could not be registered, see the CharAPI error above.");
                return;
            }

            Identity = Data.identity;
            CentreSprites();
            Loader.SetupCustomBreachAnimation(StoredName, "select_idle", 6, tk2dSpriteAnimationClip.WrapMode.Loop);
            Loader.SetupCustomBreachAnimation(StoredName, "select_choose", 6, tk2dSpriteAnimationClip.WrapMode.Once);
        }

        /// <summary>
        /// Alexandria puts each frame's lower-left corner on the player, which is right for a
        /// 16 px wide sprite. Shift every frame sideways so its centre stays where that one would be.
        /// </summary>
        private static void CentreSprites()
        {
            if (Data.collection == null) return;
            string prefix = Data.nameShort + "_";
            foreach (tk2dSpriteDefinition def in Data.collection.spriteDefinitions)
            {
                if (def == null || def.name == null || !def.name.StartsWith(prefix)) continue;
                float width = def.position1.x - def.position0.x;
                Vector3 shift = new Vector3((1f - width) / 2f, 0f, 0f);
                def.position0 += shift;
                def.position1 += shift;
                def.position2 += shift;
                def.position3 += shift;
                def.boundsDataCenter += shift;
                def.untrimmedBoundsDataCenter += shift;
            }
        }

        public static bool IsIsaac(this PlayerController player)
        {
            return Registered && player != null && player.characterIdentity == Identity;
        }
    }
}
