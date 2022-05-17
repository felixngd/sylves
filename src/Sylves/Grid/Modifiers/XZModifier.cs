﻿using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif


namespace Sylves
{
    /// <summary>
    /// Converts a IGrid based in the XY plane to one
    /// in the XZ plane. It does this by rotating Y+ to Z+  (and Z+ to Y-).
    /// This is different from a transform in that it doesn't rotate the cells, it applies XZCellModifier to them.
    /// </summary>
    public class XZModifier : TransformModifier
    {
        private static readonly Matrix4x4 RotateYZ = new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, -1, 0, 0), new Vector4(0, 0, 0, 1));
        private static readonly Matrix4x4 RotateZY = new Matrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, 0, -1, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 0, 1));

        private readonly ICellType cellType;
        private readonly ICellType[] cellTypes;

        public XZModifier(IGrid underlying) : base(underlying, RotateYZ)
        {
            if(underlying.IsSingleCellType)
            {
                cellType = XZCellModifier.Get(underlying.GetCellTypes().Single());
                cellTypes = new[] { cellType };

            }
            else
            {
                cellType = null;
                cellTypes = underlying.GetCellTypes().Select(XZCellModifier.Get).ToArray();
            }
        }

        public override TRS GetTRS(Cell cell) => new TRS(this.GetCellCenter(cell));

        public override IEnumerable<ICellType> GetCellTypes()
        {
            return cellTypes;
        }

        public override ICellType GetCellType(Cell cell)
        {
            return cellType ?? XZCellModifier.Get(Underlying.GetCellType(cell));
        }

        protected override IGrid Rebind(IGrid underlying)
        {
            return new XZModifier(underlying);
        }

        public override bool FindCell(Matrix4x4 matrix, out Cell cell, out CellRotation rotation)
        {
            return Underlying.FindCell(RotateZY * matrix * RotateYZ, out cell, out rotation);
        }

        public override bool ParallelTransport(IGrid aGrid, Cell aSrcCell, Cell aDestCell, Cell srcCell, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            // TODO: Need more permanent solution for this.
            // The default behaviour doesn't work as it uses one underlying, one not, but they have different cell types.
            if(aGrid is XZModifier other)
            {
                return Underlying.ParallelTransport(other.Underlying, aSrcCell, aDestCell, srcCell, startRotation, out destCell, out destRotation);
            }
            else
            {
                return base.ParallelTransport(aGrid, aSrcCell, aDestCell, srcCell, startRotation, out destCell, out destRotation);
            }
        }
    }
}