using StackExchange.Redis;
using System.Drawing;
using UKMCAB.Data.Models.LegislativeAreas;

namespace UKMCAB.Data.Models
{
    public class UserNote : IEquatable<UserNote>
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public DateTime DateTime { get; set; }
        public string Note { get; set; }

        public override bool Equals(object? obj) => Equals(obj as UserNote);

        public bool Equals(UserNote other)
        {
            if (other is null) return false;

            if (ReferenceEquals(this, other)) return true;

            if (other.GetType() != GetType()) return false;

            return other is not null &&
            UserId == other.UserId &&
            UserName == other.UserName &&
            UserRole == other.UserRole &&
            Note == other.Note;
        }
        public override int GetHashCode() => (
            UserId, UserName, UserRole, Note).GetHashCode();

        public static bool operator ==(UserNote userNote, UserNote other)
        {
            if (userNote is null)
            {
                if (other is null)
                {
                    return true;
                }
                return false;
            }
            return userNote.Equals(other);
        }

        public static bool operator !=(UserNote userNote, UserNote other) => !(userNote == other);
    }
}