﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylves
{
    /// <summary>
    /// IGrid contains a lot of methods.
    /// This class contains default implementations for several of these methods,
    /// in terms of more fundamental methods of the grid.
    /// These are not extension methods as the grids may implement their own implementations
    /// to which have specific functionality or are more performant.
    /// </summary>
    internal static class DefaultGridImpl
    {
        public static IEnumerable<CellDir> GetCellDirs(IGrid grid, Cell cell)
        {
            return grid.GetCellType(cell).GetCellDirs();
        }

        #region Topology
        public static bool TryMoveByOffset(IGrid grid, Cell startCell, Vector3Int startOffset, Vector3Int destOffset, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            // TODO: Do parallel transport
            throw new NotImplementedException();
        }

        public static bool ParallelTransport(IGrid srcGrid, Cell srcStartCell, Cell srcDestCell, IGrid destGrid, Cell destStartCell, CellRotation startRotation, out Cell destCell, out CellRotation destRotation)
        {
            destCell = destStartCell;
            destRotation = startRotation;

            var path = srcGrid.FindBasicPath(srcStartCell, srcDestCell);
            if(path == null)
            {
                return false;
            }
            var checkCellTypes = !srcGrid.IsSingleCellType || !destGrid.IsSingleCellType;
            ICellType cellType = null;
            if(!checkCellTypes && (cellType = srcGrid.GetCellTypes().First()) != destGrid.GetCellTypes().First())
            {
                return false;
            }
            foreach(var (srcCell, srcDir) in path)
            {
                // Check both src/dest are on compatible cell types
                if(checkCellTypes && (cellType = srcGrid.GetCellType(srcCell)) != destGrid.GetCellType(destCell))
                {
                    return false;
                }

                // Move dest dir 
                var destDir = cellType.Rotate(srcDir, destRotation);
                if(!destGrid.TryMove(destCell, destDir, out destCell, out var inverseDir, out var connection))
                {
                    return false;
                }

                // Figure out new rotation
                // TODO
            }
            return true;
        }


        public static IEnumerable<(Cell, CellDir)> FindBasicPath(IGrid grid, Cell startCell, Cell destCell)
        {
            // TODO: Do Dijkstra's algorithm
            throw new NotImplementedException();
        }

        #endregion


        // Default impl supports no bounds,
        // just returns null representing bounds that covers the whole grid.
        #region Bounds
        public static IBound GetBound(IGrid grid, IEnumerable<Cell> cells)
        {
            return null;
        }

        public static IGrid BoundBy(IGrid grid, IBound bound)
        {
            return grid;
        }

        public static IBound IntersectBounds(IGrid grid, IBound bound, IBound other)
        {
            return null;
        }
        public static IBound UnionBounds(IGrid grid, IBound bound, IBound other)
        {
            return null;
        }
        public static IEnumerable<Cell> GetCellsInBounds(IGrid grid, IBound bound)
        {
            return grid.GetCells();
        }
        #endregion
    }
}
