using System;
using System.Collections.Generic;
using System.Text;

namespace TeduCoreApp.Utilities.Dtos
{
    public abstract class PagedResultBase
    {
        public int CurrentPage { get; set; }
        /// <summary>
        /// tổng số bản ghi
        /// </summary>
        public int PageCount
        {
            get
            {
                var pageCount = (double)RowCount / PageSize;
                return (int)Math.Ceiling(pageCount);
            }
        }
        public int PageSize { get; set; }
        public int RowCount { get; set; }

        public int FirstRowOnPage
        {
            //get firstRow on page
            get { return (CurrentPage - 1) * PageSize + 1; }
        }

        public int LastRowOnPage
        {
            get { return Math.Min(CurrentPage * PageSize,RowCount); }
        }
    }
}
