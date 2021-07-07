using System.Text.Json;

namespace API.Helpers
{

    public static class HttpExtensions
    {
        public static void AddPaginationHeader(this Microsoft.AspNetCore.Http.HttpResponse response, int currentPage,
            int itemsPerPage, int totalItems, int totalPages)
        {
            var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);

            response.Headers.Add("Pagination", JsonSerializer.Serialize(paginationHeader));

            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }
    }
}
