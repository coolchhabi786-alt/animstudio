using System;
using System.Collections.Generic;

namespace AnimStudio.SharedKernel
{
    /// <summary>
    /// Represents a paginated list of items.
    /// </summary>
    /// <typeparam name="T">The type of the items in the list.</typeparam>
    public class PaginatedList<T>
    {
        /// <summary>
        /// Gets the items in the current page.
        /// </summary>
        public IReadOnlyList<T> Items { get; }

        /// <summary>
        /// Gets the total count of items across all pages.
        /// </summary>
        public int TotalCount { get; }

        /// <summary>
        /// Gets the current page number (1-based).
        /// </summary>
        public int PageNumber { get; }

        /// <summary>
        /// Gets the size of each page.
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedList{T}"/> class.
        /// </summary>
        /// <param name="items">The items in the current page.</param>
        /// <param name="totalCount">The total count of items across all pages.</param>
        /// <param name="pageNumber">The current page number.</param>
        /// <param name="pageSize">The size of each page.</param>
        public PaginatedList(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        /// <summary>
        /// Calculates the total number of pages.
        /// </summary>
        /// <returns>The total number of pages.</returns>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// Indicates whether there is a previous page.
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// Indicates whether there is a next page.
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;
    }
}