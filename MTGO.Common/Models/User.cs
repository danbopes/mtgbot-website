//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MTGO.Common.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class User
    {
        public User()
        {
            this.MtgoLinks = new HashSet<MtgoLink>();
        }
    
        public int Id { get; set; }
        public bool Admin { get; set; }
        public string TwitchUsername { get; set; }
        public string SignupIpAddress { get; set; }
        public System.DateTime Created { get; set; }
    
        public virtual ICollection<MtgoLink> MtgoLinks { get; set; }
    }
}
