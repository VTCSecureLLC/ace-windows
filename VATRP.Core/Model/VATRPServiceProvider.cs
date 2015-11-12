using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;

namespace VATRP.Core.Model
{
    [Table(Name = "PROVIDERS")]
    public class VATRPServiceProvider
    {

        #region Properties

        [Column(IsPrimaryKey = true, DbType = "NVARCHAR(50) NOT NULL ", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public string Label { get; set; }

        [Column]
        public string ImagePath { get; set; }

        #endregion

        #region Methods

        public VATRPServiceProvider()
        {
        }

        #endregion
    }
}
