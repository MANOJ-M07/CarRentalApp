//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class Cost
    {
        public int CostID { get; set; }
        public int RentID { get; set; }
        public int KmsCovered { get; set; }
        public decimal Price { get; set; }
        public Nullable<decimal> Tax { get; set; }
        public decimal TotalCost { get; set; }
    
        public virtual Rental Rental { get; set; }
    }
}
