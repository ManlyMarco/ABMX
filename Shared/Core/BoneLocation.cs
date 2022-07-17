namespace KKABMX.Core
{
    /// <summary>
    /// Specifies what part of a character the bone is a part of.
    /// Needed to differentiate multiple identical accessories and accessories with bone names identical to main body.
    /// </summary>
    public enum BoneLocation
    {
        /// <summary>
        /// Location unknown, likely because data from an old ABMX version was loaded.
        /// Includes everything under BodyTop, including accessories.
        /// This will be replaced with the correct bone location if the bone is found by the controller.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// The character's body, including the head but excluding accessories.
        /// </summary>
        BodyTop = 1,
        /// <summary>
        /// Enum values beyond this point all refer to accessories.
        /// When MoreAccessories is used, values above Accessory19 are possible.
        /// </summary>
        Accessory = 10,
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        Accessory0 = 10,
        Accessory1 = 11,
        Accessory2 = 12,
        Accessory3 = 13,
        Accessory4 = 14,
        Accessory5 = 15,
        Accessory6 = 16,
        Accessory7 = 17,
        Accessory8 = 18,
        Accessory9 = 19,
        Accessory10 = 20,
        Accessory11 = 21,
        Accessory12 = 22,
        Accessory13 = 23,
        Accessory14 = 24,
        Accessory15 = 25,
        Accessory16 = 26,
        Accessory17 = 27,
        Accessory18 = 28,
        Accessory19 = 29,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
