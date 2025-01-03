﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sylves
{
    public class HexPrismCellType : ICellType
    {
        private static HexPrismCellType ftInstance = new HexPrismCellType(HexOrientation.FlatTopped);
        private static HexPrismCellType ptInstance = new HexPrismCellType(HexOrientation.PointyTopped);

        private readonly HexOrientation orientation;
        private readonly CellCorner[] corners;
        private readonly CellDir[] dirs;
        private readonly CellRotation[] rotations;
        private readonly CellRotation[] rotationsAndReflections;

        private HexPrismCellType(HexOrientation orientation)
        {
            dirs = Enumerable.Range(0, 6).Select(x => (CellDir)x).Concat(new[] { (CellDir)PTHexPrismDir.Forward, (CellDir)PTHexPrismDir.Back }).ToArray();
            corners = Enumerable.Range(0, 12).Select(x => (CellCorner)x).ToArray();
            rotations = Enumerable.Range(0, 6).Select(x => (CellRotation)x).ToArray();
            rotationsAndReflections = rotations.Concat(Enumerable.Range(0, 6).Select(x => (CellRotation)~x)).ToArray();
            this.orientation = orientation;
        }

        public static HexPrismCellType Get(HexOrientation orientation) => orientation == HexOrientation.FlatTopped ? ftInstance : ptInstance;

        public HexOrientation Orientation => orientation;


        private bool IsAxial(CellDir dir) => ((PTHexPrismDir)dir).IsAxial();

        public IEnumerable<CellCorner> GetCellCorners() => corners;
        public IEnumerable<CellDir> GetCellDirs() => dirs;

        public CellDir? Invert(CellDir dir) => (CellDir)((PTHexPrismDir)dir).Inverted();

        public IList<CellRotation> GetRotations(bool includeReflections = false) => includeReflections ? rotationsAndReflections: rotations;

        public CellRotation Multiply(CellRotation a, CellRotation b) => ((HexRotation)a) * b;

        public CellRotation Invert(CellRotation a) => ((HexRotation)a).Invert();
        
        public CellRotation GetIdentity() => HexRotation.Identity;

        public CellDir Rotate(CellDir dir, CellRotation rotation)
        {
            return IsAxial(dir) ? dir : (CellDir)((HexRotation)rotation * (PTHexDir)dir);
        }

        public CellCorner Rotate(CellCorner corner, CellRotation rotation)
        {
            var hexCorner = (PTHexCorner)((int)corner % 6);
            hexCorner = (HexRotation)rotation * hexCorner;
            return (CellCorner)(hexCorner + ((int)corner >= 6 ? 6 : 0));
        }

        public void Rotate(CellDir dir, CellRotation rotation, out CellDir resultDir, out Connection connection)
        {
            if (dir == (CellDir)PTHexPrismDir.Forward)
            {
                resultDir = dir;
                var r = (int)rotation;
                var rot = r < 0 ? ~r : r;
                connection = new Connection { Mirror = r < 0, Rotation = rot, Sides = 6 };
            }
            else if(dir == (CellDir)PTHexPrismDir.Back)
            {
                resultDir = dir;
                var r = (int)rotation;
                // In this case the front and back faces do not share the same line of symmetry for what Connection.Mirror means.
                // I.e. The x-mirror of (TriangleDir)(0) != the 180 rotation of (TriangleDir)(0), unlike 
                // So some extra rotation is needed to keep back and front consistent
                if (r < 0 && orientation == HexOrientation.FlatTopped)
                {
                    throw new System.NotImplementedException();
                }
                else
                {
                    var rot = (6 - (r < 0 ? ~r : r)) % 6;
                    connection = new Connection { Mirror = r < 0, Rotation = rot, Sides = 6 };
                }
            }
            else
            {
                resultDir = (CellDir)((HexRotation)rotation * (PTHexDir)dir);
                var isMirror = (int)rotation < 0;
                connection = new Connection { Mirror = isMirror, Rotation = isMirror ? 2 : 0, Sides = 4 };
            }
        }

        public bool TryGetRotation(CellDir fromDir, CellDir toDir, Connection connection, out CellRotation rotation)
        {
            if (fromDir == (CellDir)PTHexPrismDir.Forward)
            {
                if (fromDir != toDir)
                {
                    rotation = default;
                    return false;
                }
                rotation = (CellRotation)(connection.Mirror ? ~connection.Rotation : connection.Rotation);
                return true;
            }
            else if (fromDir == (CellDir)PTHexPrismDir.Back)
            {
                if (fromDir != toDir)
                {
                    rotation = default;
                    return false;
                }
                // See comment in Rotate
                if (connection.Mirror && orientation == HexOrientation.FlatTopped)
                {
                    throw new System.NotImplementedException();
                }
                else
                {
                    var ir = (6 - connection.Rotation) % 6;
                    rotation = (CellRotation)(connection.Mirror ? ~ir : ir);
                    return true;
                }
            }
            else
            {
                if(IsAxial(toDir))
                {
                    rotation = default;
                    return false;
                }
                if (connection.Mirror && connection.Rotation == 2)
                {
                    var delta = ((int)toDir + (int)fromDir) % 6 + 6;
                    rotation = (CellRotation)~(delta % 6);
                    return true;
                }
                if (!connection.Mirror && connection.Rotation == 0)
                {
                    var delta = ((int)toDir - (int)fromDir) % 6 + 6;
                    rotation = (CellRotation)(delta % 6);
                    return true;
                }
                rotation = default;
                return false;
            }
        }

        public CellRotation ReflectY => orientation == HexOrientation.FlatTopped ? HexRotation.FTReflectY : HexRotation.PTReflectY;
        public CellRotation ReflectX => orientation == HexOrientation.FlatTopped ? HexRotation.FTReflectX : HexRotation.PTReflectX;
        public CellRotation RotateCW => HexRotation.RotateCW;
        public CellRotation RotateCCW => HexRotation.RotateCCW;

        public Matrix4x4 GetMatrix(CellRotation cellRotation) => ((HexRotation)cellRotation).ToMatrix(orientation);

        public Vector3 GetCornerPosition(CellCorner corner)
        {
            var flatCorner = (int)corner % 6;
            var flatPosition = orientation == HexOrientation.FlatTopped ? ((FTHexCorner)flatCorner).GetPosition() : ((PTHexCorner)flatCorner).GetPosition();
            return flatPosition + (((int)corner) >= 6 ? new Vector3(0, 0, 0.5f) : new Vector3(0, 0, -0.5f));
        }


        public string Format(CellRotation rotation) => NGonCellType.Format(rotation, 6);
        public string Format(CellDir dir) => orientation == HexOrientation.FlatTopped ? ((FTHexPrismDir)dir).ToString() : ((PTHexPrismDir)dir).ToString();
        public string Format(CellCorner corner)
        {
            var flatCorner = (int)corner % 6;
            return ((int)corner >= 6 ? "Forward" : "Back") +
                (orientation == HexOrientation.FlatTopped ? ((FTHexCorner)flatCorner).ToString() : ((PTHexCorner)flatCorner).ToString());
        }

    }
}
