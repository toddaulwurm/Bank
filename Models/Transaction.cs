using System;
using System.ComponentModel.DataAnnotations;
namespace Bank.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId {get;set;}
        [Required(ErrorMessage ="Needs Amount!!")]
        public decimal Amount {get;set;}

        public DateTime CreatedAt {get;set;} = DateTime.Now;

        public int UserId {get;set;}
        public User User {get;set;}
    }
}