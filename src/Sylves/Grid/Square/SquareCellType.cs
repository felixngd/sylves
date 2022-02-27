﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Sylves
{
    /// <summary>
    /// Handles cell information about squares.
    /// This is a a customized version of NGonCellType and behaves virtually identically.
    /// </summary>
    public class SquareCellType : ICellType
    {
        private static readonly SquareCellType instance = new SquareCellType();

        private static readonly CellDir[] allCellDirs = new[]
        {
            (CellDir) SquareDir.Right,
            (CellDir) SquareDir.Up,
            (CellDir) SquareDir.Left,
            (CellDir) SquareDir.Down,
        };

        private static readonly CellRotation[] allRotations = new[]
        {
            (CellRotation) 0,
            (CellRotation) 1,
            (CellRotation) 2,
            (CellRotation) 3,
        };

        private static readonly CellRotation[] allRotationsAndReflections = new[]
        {
            (CellRotation) 0,
            (CellRotation) 1,
            (CellRotation) 2,
            (CellRotation) 3,
            (CellRotation) ~0,
            (CellRotation) ~1,
            (CellRotation) ~2,
            (CellRotation) ~3,
        };

        public static SquareCellType Instance => instance;

        private SquareCellType(){}

        public IEnumerable<CellDir> GetCellDirs() => allCellDirs;

        public CellDir? Invert(CellDir dir) => (CellDir)((SquareDir)dir).Inverted();

        // Rotations

        public IList<CellRotation> GetRotations(bool includeReflections = false) => includeReflections ? allRotationsAndReflections : allRotations;

        public CellRotation Multiply(CellRotation a, CellRotation b) => (a * (SquareRotation)b);

        public CellRotation Invert(CellRotation a) => ((SquareRotation)a).Invert();

        public CellRotation GetIdentity() => SquareRotation.Identity;

        public CellDir Rotate(CellDir dir, CellRotation rotation)
        {
            var squareRotation = (SquareRotation)rotation;
            var squareDir = (SquareDir)dir;
            return (CellDir)(squareRotation * squareDir);
        }

        public void Rotate(CellDir dir, CellRotation rotation, out CellDir resultDir, out Connection connection)
        {
            resultDir = Rotate(dir, rotation);
            connection = new Connection { Mirror = ((SquareRotation)rotation).IsReflection };
        }

        public Matrix4x4 GetMatrix(CellRotation cellRotation)
        {
            return ((SquareRotation)cellRotation).ToMatrix();
        }
    }
}