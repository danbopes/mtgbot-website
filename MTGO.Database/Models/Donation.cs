using System;

namespace MTGO.Database.Models
{
    public class Donation
    {
        public Donation()
        {
            Amount = 0D;
        }
    
        public virtual int Id { get; protected set; }
        public virtual string TxnId { get; set; }
        public virtual string Email { get; set; }
        public virtual string Username { get; set; }
        public virtual double Amount { get; set; }
        public virtual DateTime DateTime { get; set; }
    }
}
