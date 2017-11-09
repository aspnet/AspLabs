// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNet.WebHooks.Storage
{
    /// <summary>
    /// Defines the WebHook registration data model for rows stored in Microsoft SQL.
    /// </summary>
    [Table("WebHooks")]
    public class Registration : IRegistration
    {
        /// <inheritdoc />
        [Key]
        [StringLength(256)]
        [Column(Order = 0)]
        public string User { get; set; }

        /// <inheritdoc />
        [Key]
        [StringLength(64)]
        [Column(Order = 1)]
        public string Id { get; set; }

        /// <inheritdoc />
        [Required]
        public string ProtectedData { get; set; }

        /// <summary>
        /// Gets or sets a unique row version.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is the pattern for row version.")]
        [Timestamp]
        public byte[] RowVer { get; set; }
    }
}
