using Trading.Models;

namespace Trading.Dto
{
    public class BookingDetailsDto
    {
        public Guid Id { get; set; }
        public Guid ItemId { get; set; }
        public string ItemName { get; set; }
        public DateTime BookedAt { get; set; }
        public bool IsActive { get; set; }
        
        // Details of the user who booked the item
        public ProfileDto Booker { get; set; } 
        
        // Contact details of the item's owner
        public UserContactDto ItemOwner { get; set; } 
    }
}
