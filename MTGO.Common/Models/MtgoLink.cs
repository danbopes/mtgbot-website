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
    
    public partial class MtgoLink
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string MtgoUsername { get; set; }
        public int MtgoId { get; set; }
        public bool Confirmed { get; set; }
        public string ConfirmKey { get; set; }
    
        public virtual User User { get; set; }
    }
}
