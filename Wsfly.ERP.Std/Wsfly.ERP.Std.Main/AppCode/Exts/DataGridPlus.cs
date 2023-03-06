using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Wsfly.ERP.Std.AppCode.Exts
{
    public static class DataGridPlus
    {
        /// <summary>
        /// 获取DataGrid控件单元格
        /// </summary>
        /// <param name="dataGrid">DataGrid控件</param>
        /// <param name="rowIndex">单元格所在的行号</param>
        /// <param name="columnIndex">单元格所在的列号</param>
        /// <returns>指定的单元格</returns>
        public static DataGridCell GetCell(this DataGrid dataGrid, int rowIndex, int columnIndex)
        {
            DataGridRow rowContainer = dataGrid.GetRow(rowIndex);

            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                if (presenter == null) return null;
                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
                if (cell == null)
                {
                    dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[columnIndex]);
                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
                }
                return cell;
            }

            return null;
        }

        /// <summary>
        /// 获取DataGrid控件单元格
        /// </summary>
        /// <param name="dataGrid">DataGrid控件</param>
        /// <param name="rowIndex">单元格所在的行号</param>
        /// <param name="columnIndex">单元格所在的列号</param>
        /// <returns>指定的单元格</returns>
        public static DataGridCell GetCell(this DataGrid dataGrid, int rowIndex, string columnName)
        {
            DataGridRow rowContainer = dataGrid.GetRow(rowIndex);
            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                if (presenter == null) return null;

                for (int i = 0; i < presenter.Items.Count; i++)
                {
                    DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(i);
                    if (cell == null)
                    {
                        dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[i]);
                        cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(i);
                    }

                    if (cell != null && cell.Column.SortMemberPath.Equals(columnName))
                    {
                        return cell;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 获取DataGrid的行
        /// </summary>
        /// <param name="dataGrid">DataGrid控件</param>
        /// <param name="rowIndex">DataGrid行号</param>
        /// <returns>指定的行号</returns>
        public static DataGridRow GetRow(this DataGrid dataGrid, int rowIndex)
        {
            if (rowIndex < 0) return null;

            try
            {
                DataGridRow rowContainer = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex);
                if (rowContainer == null && dataGrid.Items.Count > 0)
                {
                    dataGrid.UpdateLayout();
                    dataGrid.ScrollIntoView(dataGrid.Items[rowIndex]);
                    rowContainer = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex);
                }
                return rowContainer;
            }
            catch { }

            return null;
        }

        /// <summary>
        /// 设置行，第一个单元格编辑模式
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="rowIndex"></param>
        public static void SetRowFirstCellEdit(DataGrid dataGrid, int rowIndex)
        {
            for (int i = 0; i < dataGrid.Columns.Count; i++)
            {
                DataGridCell cell = dataGrid.GetCell(rowIndex, i);
                if (cell == null) continue;
                if (i == 0 || i == 1) continue;

                //是否过滤的列
                if (!string.IsNullOrWhiteSpace(cell.Column.SortMemberPath))
                {
                    //列名
                    string cellName = cell.Column.SortMemberPath.ToUpper();
                    //是否过滤
                    if (AppGlobal.List_TableEditFilterCells.Contains(cellName.ToUpper())) continue;
                }

                //标记编辑模式
                cell.Focus();
                cell.IsEditing = true;
                break;
            }
        }

        /// <summary>
        /// 设置DataGrid的行可编辑
        /// </summary>
        /// <param name="dataGrid">DataGrid控件</param>
        /// <param name="rowIndex">DataGrid行号</param>
        public static void SetRowEdit(this DataGrid dataGrid, int rowIndex, bool editAll = true)
        {
            //与焦点单元格冲突 故取消设置整行编辑；在单元格得到焦点时出现编辑框；
            return;

            bool isFirst = true;

            for (int i = 0; i < dataGrid.Columns.Count; i++)
            {
                DataGridCell cell = dataGrid.GetCell(rowIndex, i);
                if (cell == null) continue;
                if (i == 0 || i == 1) continue;

                //是否过滤的列
                if (!editAll && !string.IsNullOrWhiteSpace(cell.Column.SortMemberPath))
                {
                    //列名
                    string cellName = cell.Column.SortMemberPath.ToUpper();
                    //是否过滤
                    if (AppGlobal.List_TableEditFilterCells.Contains(cellName.ToUpper())) continue;
                }

                //是否聚焦
                if (cell.Visibility == System.Windows.Visibility.Visible && isFirst)
                {
                    isFirst = false;
                    cell.Focus();
                }

                //标记编辑模式
                cell.IsEditing = true;
            }
        }

        /// <summary>
        /// 设置DataGrid的单元格禁用
        /// </summary>
        /// <param name="dataGrid">DataGrid控件</param>
        /// <param name="rowIndex">DataGrid行号</param>
        public static void SetRowDisable(this DataGrid dataGrid, int rowIndex)
        {
            for (int i = 0; i < dataGrid.Columns.Count; i++)
            {
                DataGridCell cell = dataGrid.GetCell(rowIndex, i);
                if (cell == null) continue;
                if (cell.Column.SortMemberPath.Equals("IsSelected")) continue;
                cell.IsEnabled = false;
            }
        }

        /// <summary>
        /// 设置DataGrid的单元格禁用
        /// </summary>
        /// <param name="dataGrid">DataGrid控件</param>
        /// <param name="rowIndex">DataGrid行号</param>
        public static void SetRowEnable(this DataGrid dataGrid, int rowIndex)
        {
            for (int i = 0; i < dataGrid.Columns.Count; i++)
            {
                DataGridCell cell = dataGrid.GetCell(rowIndex, i);
                if (cell == null) continue;
                cell.IsEnabled = false;
            }
        }

        /// <summary>
        /// 设置DataGrid的行不可编辑
        /// </summary>
        /// <param name="dataGrid">DataGrid控件</param>
        /// <param name="rowIndex">DataGrid行号</param>
        public static void SetRowReadOnly(this DataGrid dataGrid, int rowIndex)
        {
            for (int i = 0; i < dataGrid.Columns.Count; i++)
            {
                DataGridCell cell = dataGrid.GetCell(rowIndex, i);
                if (cell == null) continue;
                cell.IsEditing = false;
            }
        }

        /// <summary>
        /// 获取父可视对象中第一个指定类型的子可视对象
        /// </summary>
        /// <typeparam name="T">可视对象类型</typeparam>
        /// <param name="parent">父可视对象</param>
        /// <returns>第一个指定类型的子可视对象</returns>
        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            try
            {
                T child = default(T);
                if (parent == null) return child;
                int numVisuals = VisualTreeHelper.GetChildrenCount(parent);

                for (int i = 0; i < numVisuals; i++)
                {
                    Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                    child = v as T;
                    if (child == null)
                    {
                        child = GetVisualChild<T>(v);
                    }
                    if (child != null)
                    {
                        break;
                    }
                }
                return child;
            }
            catch { }

            return null;
        }
    }
}
