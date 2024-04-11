using Mail;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MoviesDBManager.Models
{
    public class GroupEmail
    {
        public List<int> SelectedUsers { get; set; } = new List<int>();

        [Display(Name = "Sujet"), Required(ErrorMessage = "Obligatoire")]
        public string Subject { get; set; }

        [Display(Name = "Message"), Required(ErrorMessage = "Obligatoire")]
        public string Message { get; set; }

        public void Send()
        {
            foreach (int userId in SelectedUsers)
            {
                User user = DB.Users.Get(userId);
                string personalizedMessage = Message.Replace("[Nom]", user.GetFullName(true)).Replace("\r\n", @"<br>");
                SMTP.SendEmail(user.GetFullName(), user.Email, Subject, personalizedMessage);
            }
        }
    }
}