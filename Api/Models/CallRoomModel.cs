using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    [Table("CallRooms")]
    public class CallRoomModel : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public List<UserPeerModel> PeersUsers { get; set; }

        [ForeignKey("Room")]
        public int RoomId { get; set; }
        public RoomModel Room { get; set; }
    }
}
