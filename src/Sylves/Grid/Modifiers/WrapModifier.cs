﻿using System;
#if UNITY
using UnityEngine;
#endif

namespace Sylves
{
    /// <summary>
    /// Turns any bounded grid into a grid which connects back on itself when you leave the grounds. 
    /// This is done via a canonicalize method that is responsible for replacing cells that are outside of the bounds.
    /// </summary>
    public class WrapModifier : BaseModifier
    {
        private readonly Func<Cell, Cell?> canonicalize;
        private readonly IGrid unboundedUnderlying;
        public WrapModifier(IGrid underlying, Func<Cell, Cell?> canonicalize) : base(underlying)
        {
            unboundedUnderlying = underlying.Unbounded;
            this.canonicalize = canonicalize;
        }
        protected override IGrid Rebind(IGrid underlying)
        {
            return new WrapModifier(underlying, canonicalize);
        }

        public Cell? Canonicalize(Cell cell) => canonicalize(cell);

        #region Relatives
        // It no longer makes sense to consider an unbounded variant of this.
        public override IGrid Unbounded => this;
        #endregion

        #region Topology
        public override bool TryMove(Cell cell, CellDir dir, out Cell dest, out CellDir inverseDir, out Connection connection)
        {
            if(!unboundedUnderlying.TryMove(cell, dir, out var dest1, out inverseDir, out connection))
            {
                dest = default;
                return false;
            }
            var dest2 = canonicalize(dest1);
            dest = dest2 ?? default;
            return dest2 != null;
        }

        public override bool TryMoveByOffset(Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            if(!unboundedUnderlying.TryMoveByOffset(startCell, startOffset, destOffset, startRotation, out var dest1, out destRotation))
            {
                destCell = default;
                return false;
            }
            var dest2 = canonicalize(dest1);
            destCell = dest2 ?? default;
            return dest2 != null;
        }
        #endregion
    }
}
