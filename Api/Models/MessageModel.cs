using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    [Table("Messages")]
    public class MessageModel : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column]
        public string Text { get; set; }
        public MessageType? MessageType { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public UserModel User { get; set; }

        [ForeignKey("Room")]
        public int RoomId { get; set; }
        public RoomModel Room { get; set; }
        public string? Attacment { get; set; }
        public AttachmentType? AttachmentType { get; set; }
    }

    public enum AttachmentType
    {
        None,
        Image,
        Video
    }
    public enum MessageType
    {
        None,
        System,
        User
    }
}
