namespace MTOGO.Web.Models.Restaurant
{
    public class MenuItem
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public byte[] Image { get; set; }

        public Restaurant? Restaurant { get; set; }
    }
}
