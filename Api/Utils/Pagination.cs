namespace Api.Utils
{
    public class Pagination<T>
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages => (int)System.Math.Ceiling((decimal)TotalItems / Limit);
        public IEnumerable<T> Items { get; set; }
    }
}
