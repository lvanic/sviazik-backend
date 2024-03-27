using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    [Table("JoinedRooms")]
    public class JoinedRoomModel : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column]
        public string SocketId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public UserModel User { get; set; }

        [ForeignKey("Room")]
        public int RoomId { get; set; }
        public RoomModel Room { get; set; }

        // Добавление составного ключа для пользователя и socket_id (предположим, что это составной ключ)
        // [Key]
        // [Column(Order = 1)]
        // public int UserId { get; set; }

        // [Key]
        // [Column(Order = 2)]
        // public string SocketId { get; set; }
    }
}
