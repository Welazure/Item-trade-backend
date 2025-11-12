namespace Trading.Dto
{
    public class ProfileDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime RegisteredAt { get; set; }
        public int ItemsCount { get; set; }
        public int ActiveBookingsCount { get; set; }
        public string Role { get; set; }
    }
}
