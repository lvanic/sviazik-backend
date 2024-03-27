using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Api.Models
{
    [Table("Users")]
    public class UserModel : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("username")]
        [MaxLength(255)]
        public string Username { get; set; }

        [Column("email")]
        [MaxLength(255)]
        public string Email { get; set; }

        [Column("password")]
        [JsonIgnore]
        public string Password { get; set; }


        public List<RoomModel> Rooms { get; set; }

        [InverseProperty("Admins")]
        public List<RoomModel> AdminRooms { get; set; }

        public List<ConnectedUserModel> Connections { get; set; }

        public List<JoinedRoomModel> JoinedRooms { get; set; }

        public UserPeerModel Peer { get; set; }

        public List<MessageModel> Messages { get; set; }
    }
}
