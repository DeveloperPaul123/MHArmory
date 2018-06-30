﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MHArmory.Core.DataStructures
{
    public enum ArmorPieceType
    {
        Head,
        Chest,
        Gloves,
        Waist,
        Legs,
    }

    public interface IArmorPieceDefense
    {
        int Base { get; }
        int Max { get; }
        int Augmented { get; }
    }

    public interface IArmorPieceResistances
    {
        int Fire { get; }
        int Water { get; }
        int Thunder { get; }
        int Ice { get; }
        int Dragon { get; }
    }

    public enum Gender
    {
        None,
        Male,
        Female,
        Both,
    }

    public interface IArmorPieceAttributes
    {
        Gender RequiredGender { get; }
    }

    public interface IArmorPieceAssets
    {
        string ImageMale { get; }
        string ImageFemale { get; }
    }

    public interface IArmorPiece
    {
        int Id { get; }
        string Name { get; }
        ArmorPieceType Type { get; }
        int Rarity { get; }
        IArmorPieceDefense Defense { get; }
        IArmorPieceResistances Resistances { get; }
        IArmorPieceAttributes Attributes { get; }
        int[] Slots { get; }
        IAbility[] Abilities { get; }
        IArmorPieceAssets Assets { get; }
    }
}