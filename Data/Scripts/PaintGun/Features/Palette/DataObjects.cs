﻿using System;
using Digi.PaintGun.Utilities;
using ProtoBuf;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace Digi.PaintGun.Features.Palette
{
    [Flags]
    public enum OddAxis
    {
        NONE = 0,
        X = 1,
        Y = 2,
        Z = 4
    }

    [ProtoContract(UseProtoMembersOnly = true)]
    public struct MirrorPlanes
    {
        [ProtoMember(1)]
        public readonly int? X;

        [ProtoMember(2)]
        public readonly int? Y;

        [ProtoMember(3)]
        public readonly int? Z;

        public bool HasMirroring => X.HasValue || Y.HasValue || Z.HasValue;

        public MirrorPlanes(IMyCubeGrid grid)
        {
            if(grid.XSymmetryPlane.HasValue)
                X = grid.XSymmetryPlane.Value.X;
            else
                X = null;

            if(grid.YSymmetryPlane.HasValue)
                Y = grid.YSymmetryPlane.Value.Y;
            else
                Y = null;

            if(grid.ZSymmetryPlane.HasValue)
                Z = grid.ZSymmetryPlane.Value.Z;
            else
                Z = null;
        }
    }

    public struct PaintMaterial
    {
        public readonly Vector3? ColorMask;
        public readonly MyStringHash? Skin;

        public PaintMaterial(Vector3? colorMask, MyStringHash? skin)
        {
            ColorMask = colorMask;
            Skin = skin;
        }

        public PaintMaterial(SerializedPaintMaterial material)
        {
            ColorMask = null;

            if(material.ColorMaskPacked.HasValue)
                ColorMask = ColorExtensions.UnpackHSVFromUint(material.ColorMaskPacked.Value);

            Skin = null;
            if(material.SkinIndex.HasValue)
                Skin = PaintGunMod.Instance.Palette.GetSkinInfo(material.SkinIndex.Value).SubtypeId;
        }

        public bool PaintEquals(BlockMaterial blockMaterial)
        {
            return (!ColorMask.HasValue || Utils.ColorMaskEquals(ColorMask.Value, blockMaterial.ColorMask))
                && (!Skin.HasValue || Skin.Value.Equals(blockMaterial.Skin));
        }

        public bool PaintEquals(PaintMaterial paintMaterial)
        {
            return (!ColorMask.HasValue || !paintMaterial.ColorMask.HasValue || Utils.ColorMaskEquals(ColorMask.Value, paintMaterial.ColorMask.Value))
                && (!Skin.HasValue || !paintMaterial.Skin.HasValue || Skin.Value.Equals(paintMaterial.Skin.Value));
        }

        public bool PaintEquals(IMySlimBlock block)
        {
            return (!ColorMask.HasValue || Utils.ColorMaskEquals(ColorMask.Value, block.ColorMaskHSV)) && (!Skin.HasValue || Skin.Value.Equals(block.SkinSubtypeId));
        }

        public override string ToString()
        {
            return $"{(ColorMask.HasValue ? ColorMask.Value.ToString() : "(NoColor)")}/{(Skin.HasValue ? Skin.Value.String : "(NoSkin)")}";
        }
    }

    public struct BlockMaterial
    {
        public readonly Vector3 ColorMask;
        public readonly MyStringHash Skin;

        public BlockMaterial(Vector3 colorMask, MyStringHash skin)
        {
            ColorMask = colorMask;
            Skin = skin;
        }

        public BlockMaterial(IMySlimBlock block)
        {
            ColorMask = block.ColorMaskHSV;
            Skin = block.SkinSubtypeId;
        }

        public BlockMaterial(SerializedBlockMaterial material)
        {
            ColorMask = ColorExtensions.UnpackHSVFromUint(material.ColorMaskPacked);

            var skin = PaintGunMod.Instance.Palette.GetSkinInfo(material.SkinIndex);
            if(skin == null)
                throw new ArgumentException($"BlockMaterial :: Unknown skin index={material.SkinIndex}");

            Skin = skin.SubtypeId;
        }

        public bool MaterialEquals(BlockMaterial material)
        {
            return Skin == material.Skin && Utils.ColorMaskEquals(ColorMask, material.ColorMask);
        }

        public override string ToString()
        {
            return $"{ColorMask.ToString()}/{Skin.String}";
        }
    }

    [ProtoContract]
    public struct SerializedPaintMaterial
    {
        [ProtoMember(1)]
        public readonly uint? ColorMaskPacked;

        [ProtoMember(2)]
        public readonly int? SkinIndex;

        public SerializedPaintMaterial(uint? colorMaskPacked, int? skinIndex)
        {
            ColorMaskPacked = colorMaskPacked;
            SkinIndex = skinIndex;
        }

        public SerializedPaintMaterial(PaintMaterial material)
        {
            ColorMaskPacked = null;
            if(material.ColorMask.HasValue)
                ColorMaskPacked = material.ColorMask.Value.PackHSVToUint();

            SkinIndex = null;
            if(material.Skin.HasValue)
            {
                var skin = PaintGunMod.Instance.Palette.GetSkinInfo(material.Skin.Value);

                if(skin == null)
                    throw new ArgumentException($"Skin {material.Skin.ToString()} does not exist!");

                SkinIndex = skin.Index;
            }
        }

        public override string ToString()
        {
            return $"{(ColorMaskPacked.HasValue ? Utils.ColorMaskToHSVText(ColorExtensions.UnpackHSVFromUint(ColorMaskPacked.Value)) : "(NoColor)")}/{(SkinIndex.HasValue ? SkinIndex.Value.ToString() : "(NoSkinIndex)")}";
        }
    }

    [ProtoContract]
    public struct SerializedBlockMaterial
    {
        [ProtoMember(1)]
        public readonly uint ColorMaskPacked;

        [ProtoMember(2)]
        public readonly int SkinIndex;

        public SerializedBlockMaterial(uint colorMaskPacked, int skinIndex)
        {
            ColorMaskPacked = colorMaskPacked;
            SkinIndex = skinIndex;
        }

        public SerializedBlockMaterial(BlockMaterial material)
        {
            ColorMaskPacked = material.ColorMask.PackHSVToUint();

            var skin = PaintGunMod.Instance.Palette.GetSkinInfo(material.Skin);

            if(skin == null)
                throw new ArgumentException($"Skin {material.Skin.ToString()} does not exist!");

            SkinIndex = skin.Index;
        }

        public override string ToString()
        {
            return $"{Utils.ColorMaskToHSVText(ColorExtensions.UnpackHSVFromUint(ColorMaskPacked))}/{SkinIndex.ToString()}";
        }
    }
}