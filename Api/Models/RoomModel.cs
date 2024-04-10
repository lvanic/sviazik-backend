using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    [Table("Rooms")]
    public class RoomModel : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        //room type 1: public, 2: private, 3 saved, 4: one by one
        //userLimit
        [Column]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar(500)")]
        public string Description { get; set; }

        public string? Image { get; set; }

        public List<UserModel> Users { get; set; }

        [InverseProperty("AdminRooms")]
        public List<UserModel> Admins { get; set; }

        public List<JoinedRoomModel> JoinedUsers { get; set; }

        public CallRoomModel CallRoom { get; set; }

        public List<MessageModel> Messages { get; set; }
    }
}
